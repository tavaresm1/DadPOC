using System.Diagnostics;

namespace CruiseSurvey.Services;

public class DockerSqlServerService
{
    private const string ContainerName = "cruise-survey-mssql";
    private const string SaPassword = "CruiseSurvey2024!";
    private const int HostPort = 1433;

    public static string ConnectionString =>
        $"Server=localhost,{HostPort};Database=CruiseSurveyDb;User Id=sa;Password={SaPassword};TrustServerCertificate=True;";

    public static async Task EnsureRunningAsync(ILogger logger)
    {
        if (await IsContainerRunning())
        {
            logger.LogInformation("MSSQL container '{Container}' is already running.", ContainerName);
            return;
        }

        if (await ContainerExists())
        {
            logger.LogInformation("Starting existing MSSQL container '{Container}'...", ContainerName);
            await RunDockerCommand($"start {ContainerName}");
        }
        else
        {
            logger.LogInformation("Creating and starting MSSQL container '{Container}'...", ContainerName);
            await RunDockerCommand(
                $"run -d --name {ContainerName} " +
                $"-e \"ACCEPT_EULA=Y\" " +
                $"-e \"MSSQL_SA_PASSWORD={SaPassword}\" " +
                $"-p {HostPort}:1433 " +
                $"mcr.microsoft.com/mssql/server:2022-latest");
        }

        logger.LogInformation("Waiting for MSSQL to be ready...");
        await WaitForSqlServerReady(logger);
        logger.LogInformation("MSSQL is ready and accepting connections.");
    }

    private static async Task<bool> IsContainerRunning()
    {
        var output = await RunDockerCommand($"inspect -f \"{{{{.State.Running}}}}\" {ContainerName}", throwOnError: false);
        return output.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<bool> ContainerExists()
    {
        var output = await RunDockerCommand($"ps -a -q -f name=^{ContainerName}$", throwOnError: false);
        return !string.IsNullOrWhiteSpace(output);
    }

    private static async Task WaitForSqlServerReady(ILogger logger, int maxRetries = 30)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            await Task.Delay(2000);
            var result = await RunDockerCommand(
                $"exec {ContainerName} /opt/mssql-tools2/bin/sqlcmd -S localhost -U sa -P \"{SaPassword}\" -Q \"SELECT 1\" -C",
                throwOnError: false);

            if (result.Contains("1"))
            {
                return;
            }

            logger.LogInformation("MSSQL not ready yet, retrying ({Attempt}/{Max})...", i + 1, maxRetries);
        }

        throw new TimeoutException($"MSSQL did not become ready after {maxRetries} attempts.");
    }

    private static async Task<string> RunDockerCommand(string arguments, bool throwOnError = true)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (throwOnError && process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Docker command failed: docker {arguments}\n{error}");
        }

        return output + error;
    }
}
