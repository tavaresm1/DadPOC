Project: CruiseSurvey — Blazor Server app at /home/mtavares/Git/CruiseSurvey/
                                                                                                                                                                      
  3-Step Wizard Flow:                                          
                                                                                                                                                                      
  1. Personal Info — collects first/last name, email, age range, cruise ship name, departure date, and number of nights (all validated)                               
  2. 10 Survey Questions — star ratings (1-5) with optional comments for each:                                                                                        
    - Overall Experience, Cabin & Accommodations, Dining, Entertainment, Staff & Service, Cleanliness, Shore Excursions, Value for Money, Embarkation/Debarkation,    
  Likelihood to Recommend                                                                                                                                             
  3. Completion — shows a summary with star ratings, overall average, and the path to the saved text file                                                             
                                                                                                                                                                      
  Key files:                                                                                                                                                          
  - Models/SurveyModel.cs — data models with validation attributes                                                                                                    
  - Services/SurveyService.cs — builds a formatted summary and writes it to SurveyResults/ as a .txt file                                                             
  - Pages/Index.razor — the full wizard UI with progress bar                                             
  - wwwroot/css/site.css — ocean-themed, clean, responsive design                                                                                                     
                                                                                                                                                                      
  Output: Each completed survey saves to SurveyResults/survey_{LastName}_{FirstName}_{timestamp}.txt with a nicely formatted report including all ratings, comments,  
  average score, and sentiment analysis.                                                                                                                              
                                                                                                                                                                      
  ---                                                                                                                                                                 
  To run it, you'll need the .NET 8 SDK installed. Install it with:
                                                                                                                                                                      
  wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh && bash /tmp/dotnet-install.sh --channel 8.0
  export DOTNET_ROOT=$HOME/.dotnet                                                                                                                                    
  export PATH=$PATH:$HOME/.dotnet                                                                                                                                     
                                                                                                                                                                      
  Then add those export lines to your ~/.bashrc so they persist. After that:                                                                                          
                                                                                                                                                                      
  cd ~/Git/CruiseSurvey                                                                                                                                               
  dotnet run                                                                                                                                                          
                                                                                                                                                                      
  The app will be available at https://localhost:5001 (or http://localhost:5000).   