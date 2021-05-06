using LEDPiLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LEDPiApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LEDPiModuleController : ControllerBase
    {
        private readonly ILogger<LEDPiModuleController> _logger;

        private const string killParameter = " pkill -f \"LEDPiProcessor\"";

        private const string processParameter =
            "dotnet ../Processor/LEDPiProcessor.dll --led-no-hardware-pulse --module {0} --parametertext {1}";


        public LEDPiModuleController(ILogger<LEDPiModuleController> logger)
        {
            _logger = logger;
        }
            
        [HttpPost("[action]")]
        public IActionResult Process(LEDPIProcessorBase.LEDModules module, string scrollingText)
        {
            killParameter.Bash();

            Thread.Sleep(new TimeSpan(0, 0, 0, 1));

            Task.Run(() =>
            {
                String.Format(processParameter, (int)module, scrollingText).Bash();

                while (true)
                {
                    Thread.Sleep(new TimeSpan(0, 0, 0, 0, 750));
                }
            });

            return Ok();
        }

        [HttpGet]
        public IEnumerable<LEDModule> Get()
        {
            return Enum.GetValues(typeof(LEDPIProcessorBase.LEDModules)).Cast<LEDPIProcessorBase.LEDModules>().Select(c => new LEDModule(){Id = (int)c, Name = c.ToString()});
        }
    }

    public static class ShellHelper
    {
        public static Process Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            process.StandardOutput.ReadToEnd();

            return process;
        }
    }
}
