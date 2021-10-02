cd "C:\Users\patri\source\repos\LEDPiPrivate\LEDPiLib"
dotnet clean ..\LEDPiProcessor
dotnet build ..\LEDPiProcessor\LEDPiProcessor.csproj --no-incremental --configuration Release
dotnet publish ..\LEDPiProcessor -r linux-arm --configuration Release
scp -r ..\LEDPiProcessor\bin\Release\netcoreapp3.1\linux-arm\publish\* pi@ledpi.local:/home/pi/LEDPi/Processor/
