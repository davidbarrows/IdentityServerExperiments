namespace Quickstart.Tests.Helpers
{
    public class WebAppWrapper
    {
        public string ProjectPath { get; set; }
        public string OutputPath { get; set; }
        public string PropertiesPath { get; set; }
        public string UriString { get; set; }
        public int HttpsPort { get; set; }
        public ProcessWrapper ProcWrapper { get; set; }
    }
}
