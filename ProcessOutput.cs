using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunInTray
{
	// Observable that produces a single value when the object is disposed.
	class DisposeObservable : IDisposable, IObservable<Unit>
	{
		private List<IObserver<Unit>> observers = new List<IObserver<Unit>>();

		// Subscribe an observer.
		public IDisposable Subscribe(IObserver<Unit> observer)
		{
			observers.Add(observer);
			return Disposable.Create(() => observers.Remove(observer));
		}

		// Dispose the object, producing a value.
		public void Dispose()
		{
			// Observers may remove themselves during OnComplete, so make a copy of the array.
			var localObservers = observers.ToArray();

			// Produce value on and complete all observers.
			foreach (var observer in localObservers)
			{
				observer.OnNext(Unit.Default);
				observer.OnCompleted();
			}
		}
	}

	// Class for handling process stdout and stderr.
	public class ProcessOutput : IDisposable
	{
		private IObservable<string> stdout;
		private IObservable<string> stderr;

		// Observable that will fire when we're disposed.
		private DisposeObservable disposeObservable = new DisposeObservable();
		private CompositeDisposable disposables = new CompositeDisposable();

		public ProcessOutput(Process process, string friendlyName)
		{
			// Fire dispose observable when we are disposed.
			disposables.Add(disposeObservable);

			stdout = CreateOutputObservable(process, "OutputDataReceived");
			stderr = CreateOutputObservable(process, "ErrorDataReceived");

			// Make sure the log directory exists.
			var logPath = GetLogPath(friendlyName);
			Directory.CreateDirectory(Path.GetDirectoryName(logPath));

			// Create log file with standard utf8 encoding.
			var logWriter = new StreamWriter(logPath, false, Encoding.UTF8);

			// Register observer to write the data to the log file.
			stdout.Merge(stderr).Subscribe(
				s => logWriter.Write(s),
				() => logWriter.Dispose()
				);

			// Start async I/O.
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
		}

		public static string GetLogBaseDir()
		{
			// Put logs in the user's local application data folder.
			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"RunInTray");
		}

		// Get the location for the logfile of a process.
		private string GetLogPath(string friendlyName)
		{
			// Sanitise friendly name for use as a filename.
			friendlyName = friendlyName.Replace("\"", "'");
			friendlyName = friendlyName.Replace("<", "");
			friendlyName = friendlyName.Replace(">", "");
			friendlyName = friendlyName.Replace("|", "");
			friendlyName = friendlyName.Replace(":", "");
			friendlyName = friendlyName.Replace("*", "");
			friendlyName = friendlyName.Replace("?", "");
			friendlyName = friendlyName.Replace("\\", "-");
			friendlyName = friendlyName.Replace("/", "-");

			// Use current timestamp to prevent overwriting previous logs.
			var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

			return Path.Combine(
				GetLogBaseDir(),
				friendlyName,
				timestamp + ".log");
		}

		public void Dispose()
		{
			disposables.Dispose();
		}

		// Observable that contains the contents of the output.
		public IObservable<string> GetOutput()
		{
			return stdout;
		}
		public IObservable<string> GetErrorOutput()
		{
			return stderr;
		}

		// Create an observable for an output stream.
		IObservable<string> CreateOutputObservable(object target, string eventName)
		{
			var observable = Observable.FromEventPattern<DataReceivedEventArgs>(target, eventName)
				// Get the string data from the event.
				// Also, add back the newline that is stripped.
				.Select(e => e.EventArgs.Data + Environment.NewLine)
				// Produce values until we are disposed.
				.TakeUntil(disposeObservable)
				;

			// Use Replay so we don't lose output when the window isn't open.
			var replay = observable.Replay();
			disposables.Add(replay.Connect());

			return replay;
		}
	}
}
