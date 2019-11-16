#region Using

using System.Diagnostics;
using System.Threading;
using Emotion.Common;
using Emotion.Test.Helpers;

#endregion

namespace Emotion.Test
{
    public class LinkedRunner
    {
        /// <summary>
        /// The process id of the runner.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The arguments the linked runner was run with.
        /// </summary>
        public string Args { get; private set; }

        private Process _parentProcess = Process.GetCurrentProcess();

        private Process _runnerProcess;
        private string _output;
        private string _errorOutput;

        public LinkedRunner(string args)
        {
            Args = args;

            var prep = new ProcessStartInfo();
            if (_parentProcess.MainModule != null) prep.FileName = _parentProcess.MainModule.FileName;
            prep.Arguments = $"testRunId={Runner.TestRunId} {args}";
            prep.RedirectStandardOutput = true;
            prep.RedirectStandardError = true;
            _runnerProcess = Process.Start(prep);

            if (_runnerProcess == null)
            {
                Runner.Log.Error("Couldn't start linked runner with args {args}", CustomMSource.TestRunner);
                return;
            }

            Id = $"{_runnerProcess.Id}";
            _runnerProcess.OutputDataReceived += _runnerProcess_OutputDataReceived;
            _runnerProcess.ErrorDataReceived += _runnerProcess_ErrorDataReceived;

            _runnerProcess.BeginOutputReadLine();
            _runnerProcess.BeginErrorReadLine();
        }

        private void _runnerProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _errorOutput += $"{e.Data}\n";
        }

        private void _runnerProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _output += $"{e.Data}\n";
        }

        public int WaitForFinish(out string output, out string errorOutput)
        {
            _runnerProcess.WaitForExit(1000 * 60 * 3);
            output = _output;
            errorOutput = _errorOutput;
            return _runnerProcess.ExitCode;
        }
    }
}