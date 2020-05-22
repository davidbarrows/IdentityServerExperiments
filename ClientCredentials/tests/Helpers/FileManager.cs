using System;
using System.IO;
using System.Linq;

namespace Quickstart.Tests.Helpers
{
    public static class FileManager
    {
        private static string GetAppUrlLine(string propertiesPath)
        {
            // this works if there's only one applicationUrl in the file;
            // not tested for other possibilities (yet)

            var launchSettingsPath = $"{propertiesPath}\\launchSettings.json";

            if (!File.Exists(launchSettingsPath))
            {
                throw new Exception($"{launchSettingsPath} does not exist!");
            }

            var lines = File.ReadAllLines(launchSettingsPath);
            var appUrl = lines.FirstOrDefault(x => x.Contains("applicationUrl"));

            if (string.IsNullOrEmpty(appUrl))
            {
                throw new Exception("Could not get applicationUrl from launchSettings.json");
            }

            return appUrl;
        }

        public static string GetAppUrlFromLaunchSettings(string propertiesPath)
        {
            var appUrlLine = GetAppUrlLine(propertiesPath);

            var urlIndex = appUrlLine?.IndexOf("http");

            if (urlIndex == null || urlIndex <= 0)
            {
                throw new Exception("Could not extract URL from launchSettings.json");
            }

            var url = appUrlLine.Substring(Convert.ToInt32(urlIndex)).Replace("\"", "");

            return url;
        }
    }
}
