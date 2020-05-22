using System;
using System.Diagnostics;
using System.Text;

namespace Quickstart.Tests.Helpers
{
    public class ProcessWrapper
    {
        public ProcessWrapper()
        {
            LoggingOutput = new StringBuilder();
        }

        public Process WebAppProcess { get; set; }
        public StringBuilder LoggingOutput { get; set; }

        public void proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            LoggingOutput.AppendLine(e.Data);
        }

        public void proc_ProcessExited(object sender, EventArgs e)
        {
            LoggingOutput.AppendLine("Process exited at " + System.DateTime.UtcNow);
        }
    }
}
