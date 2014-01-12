using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reactive.Linq;

namespace RunInTray
{
	// Class for handling process stdout and stderr.
	public class ProcessOutput : IDisposable
	{
		private StringBuilder outputString = new StringBuilder();
		private IObservable<string> stdout;

		private List<IDisposable> disposables = new List<IDisposable>();

		// TODO: stderr
		public ProcessOutput(Process process)
		{
			var stdoutObservable = Observable.FromEventPattern<DataReceivedEventHandler, DataReceivedEventArgs>(
					h => process.OutputDataReceived += h,
					h => process.OutputDataReceived -= h)
				// Get the string data from the event.
				// Also, add back the newline that is stripped.
				.Select(e => e.EventArgs.Data + Environment.NewLine);

			// Use Replay so we don't lose output when the window isn't open.
			var connectableStdout = stdoutObservable.Replay();
			disposables.Add(connectableStdout.Connect());

			stdout = connectableStdout;

			// Start async I/O.
			process.BeginOutputReadLine();
		}

		public void Dispose()
		{
			foreach (var disposable in disposables)
			{
				disposable.Dispose();
			}
		}

		// Observable that contains the contents of the output.
		public IObservable<string> GetOutput()
		{
			return stdout;
		}
	}
}
