cd "C:\Users\Patrick Urban\source\repos\LEDPiPrivate\LEDPiLib"
dotnet clean ..\LEDPiProcessor
dotnet build ..\LEDPiProcessor\LEDPiProcessor.csproj --no-incremental --configuration Release
dotnet publish ..\LEDPiProcessor -r linux-arm --configuration Release
scp -r ..\LEDPiProcessor\bin\Release\netcoreapp3.1\linux-arm\publish\* pi@192.168.73.96:/home/pi/LEDPi/Processor/
