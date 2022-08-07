using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using LEDPiLib.DataItems;
using Newtonsoft.Json;

namespace LEDPiProcessor
{
    class LEDPiProcessor
    {
        static void Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<string>(
                    "--parameter",
                    description: "a playlist in json format"),
                new Option<string>(
                    "--playlist",
                    description: "a playlist in json format"),
                new Option<bool>(
                    "--showframerate",
                    description: "show frame rates"),
            };

            rootCommand.TreatUnmatchedTokensAsErrors = false;
            rootCommand.Description = "";
            rootCommand.Handler = CommandHandler.Create<string, string, bool>(Execute);
            rootCommand.InvokeAsync(args);
        }

        private static void Execute(string playlist, string parameter, bool showframerate)
        {
            if (!string.IsNullOrEmpty(playlist))
            {
                using StreamReader r = new StreamReader(playlist);
                parameter = r.ReadToEnd();
            }

            ModulePlaylist mp = JsonConvert.DeserializeObject<ModulePlaylist>(parameter);

            try
            {
                LEDPIProcessorKit kit = new LEDPIProcessorKit();
                kit.RunModule(mp, true, showframerate);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
