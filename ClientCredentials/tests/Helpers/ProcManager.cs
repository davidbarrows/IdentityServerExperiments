using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Quickstart.Tests.Helpers
{
    // see: https://stackoverflow.com/questions/31065699/how-to-kill-the-application-that-is-using-a-tcp-port-in-c

    public static class ProcManager
    {
        public static void KillByPort(int port)
        {
            var processes = GetAllProcesses();
            if (processes.Any(p => p.Port == port))
                try
                {
                    Process.GetProcessById(processes.First(p => p.Port == port).Pid).Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            else
            {
                Console.WriteLine("No process to kill!");
            }
        }

        public static List<Prc> GetAllProcesses()
        {
            var pStartInfo = new ProcessStartInfo
            {
                FileName = "netstat.exe",
                Arguments = "-a -n -o",
                WindowStyle = ProcessWindowStyle.Maximized,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new Process()
            {
                StartInfo = pStartInfo
            };
            process.Start();

            var soStream = process.StandardOutput;

            var output = soStream.ReadToEnd();
            if (process.ExitCode != 0)
                throw new Exception("failed to execute 'netstat' process");

            var result = new List<Prc>();

            var lines = Regex.Split(output, "\r\n");
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("Proto"))
                    continue;

                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var len = parts.Length;
                if (len > 2)
                    result.Add(new Prc
                    {
                        Protocol = parts[0],
                        Port = int.Parse(parts[1].Split(':').Last()),
                        Pid = int.Parse(parts[len - 1])
                    });


            }
            return result;
        }

        // see https://github.com/Tomamais/StartAndStopDotNetCoreApp
        public static void PublishWebApp(string projectPath, string outputPath)
        {
            var process = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = projectPath,
                    FileName = "dotnet",
                    Arguments = $"publish -o {outputPath}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            process.Close();
            process.Dispose();
        }

        public static ProcessWrapper RunWebApp(string projectPath, string outputPath, string urls)
        {
            var processWrapper = new ProcessWrapper();

            var process = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = outputPath,
                    FileName = "dotnet",
                    Arguments = $"{projectPath.Split(@"\")[projectPath.Split(@"\").Length - 1]}.dll --urls \"{urls}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    
                }
            };

            process.OutputDataReceived += processWrapper.proc_DataReceived;
            process.ErrorDataReceived += processWrapper.proc_DataReceived;
            process.Exited += processWrapper.proc_ProcessExited;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            processWrapper.WebAppProcess = process;

            return processWrapper;
        }
    }
}
