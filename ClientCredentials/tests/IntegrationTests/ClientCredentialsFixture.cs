using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net.Http;
using System.Text.RegularExpressions;
using Quickstart.Tests.Helpers;

namespace Quickstart.Tests.IntegrationTests
{
    public class ClientCredentialsFixture : IDisposable
    {
        public HttpClient IdentityServerHttpClient;
        public WebAppWrapper IdentityServerWrapper;

        public HttpClient WebApiHttpClient;
        public WebAppWrapper WebApiWrapper;

        public ClientCredentialsFixture()
        {
            // path setup
            var assembly = Assembly.GetExecutingAssembly();
            var solutionRoot = Directory.GetParent(assembly.Location)?.Parent?.Parent?.Parent?.Parent;
            var srcDir = solutionRoot?.GetDirectories().SingleOrDefault(x => x.Name == "src");
            var testsDir = solutionRoot?.GetDirectories().SingleOrDefault(x => x.Name == "tests");

            // IdentityServer setup

            // wrap all parameters, publish and start IdentityServer web app -
            // runs in a Process & listens on the port specified in launchSettings.json
            IdentityServerWrapper = GetWebAppWrapper(srcDir, testsDir, "IdentityServer");

            // Web API setup

            // wrap all parameters, publish and start the Api web app -
            // runs in a Process & listens on the port specified in launchSettings.json
            WebApiWrapper = GetWebAppWrapper(srcDir, testsDir, "Api");
        }

        public void Dispose()
        {
            Teardown(IdentityServerWrapper);
            Teardown(WebApiWrapper);
        }

        private WebAppWrapper GetWebAppWrapper(DirectoryInfo srcDir, DirectoryInfo testsDir, string projectName)
        {
            if (srcDir == null || testsDir == null)
            {
                throw new Exception("srcDir and/or testsDir is null");
            }

            var srcDirFullName = srcDir.FullName;
            var testsDirFullName = testsDir.FullName;
            var projectPath = $"{srcDirFullName}\\{projectName}";
            var outputPath = $"{testsDirFullName}\\output\\{projectName}";
            var propertiesPath = $"{projectPath}\\Properties";
            var uriString = FileManager.GetAppUrlFromLaunchSettings(propertiesPath);
            var httpsPort = Convert.ToInt32(Regex.Match(uriString, @"\d+").Value);

            var wrapper = new WebAppWrapper
            {
                ProjectPath = projectPath,
                OutputPath = outputPath,
                PropertiesPath = propertiesPath,
                UriString = uriString,
                HttpsPort = httpsPort
            };

            // teardown, publish and run the identity server web app in a process
            Setup(wrapper);

            return wrapper;
        }

        public void Setup(WebAppWrapper webAppWrapper)
        {
            // ensure nothing is running on the port
            ProcManager.KillByPort(webAppWrapper.HttpsPort);

            // ensure the output directory to which we'll publish is empty
            DirManager.DeleteAndRecreate(webAppWrapper.OutputPath);

            // publish and run the specified webApp
            webAppWrapper.ProcWrapper = PublishAndRun(webAppWrapper);
        }

        public void Teardown(WebAppWrapper webAppWrapper)
        {
            // (we don't delete the output here in case the tester wants to look at the logs)

            // dump the log
            var logFilename = $"{webAppWrapper.OutputPath}\\{GetNewLogfileName()}";
            File.WriteAllText(logFilename, webAppWrapper.ProcWrapper.LoggingOutput.ToString());

            // ensure nothing is running on the port
            ProcManager.KillByPort(webAppWrapper.HttpsPort);
        }

        private string GetNewLogfileName()
        {
            var now = DateTime.Now;
            var logFilename =
                $"{now.Year}_{now.Month.ToString().PadLeft(2, '0')}_{now.Day.ToString().PadLeft(2, '0')}_{now.Hour.ToString().PadLeft(2, '0')}{now.Minute.ToString().PadLeft(2, '0')}{now.Second.ToString().PadLeft(2, '0')}_testRun.txt";
            return logFilename;
        }

        private ProcessWrapper PublishAndRun(WebAppWrapper parms)
        {
            // publish the web app to better control processes
            ProcManager.PublishWebApp(parms.ProjectPath, parms.OutputPath);

            // start process to ensure its port is actually listening
            var processWrapper = ProcManager.RunWebApp(parms.ProjectPath, parms.OutputPath, parms.UriString);

            return processWrapper;
        }

        public void RestartIdentityServer()
        {
            // restart IdentityServer - creates a new process after it's been killed by child tests
            IdentityServerWrapper.ProcWrapper = ProcManager.RunWebApp(IdentityServerWrapper.ProjectPath, IdentityServerWrapper.OutputPath, IdentityServerWrapper.UriString);
        }

        public void RestartWebApi()
        {
            // restart IdentityServer - creates a new process after it's been killed by child tests
            WebApiWrapper.ProcWrapper = ProcManager.RunWebApp(WebApiWrapper.ProjectPath, WebApiWrapper.OutputPath, WebApiWrapper.UriString);
        }
    }
}
