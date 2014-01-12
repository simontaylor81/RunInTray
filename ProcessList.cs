using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RunInTray
{
	class ProcessException : Exception
	{
		public ProcessException(string message = null, Exception inner = null)
			: base(message, inner)
		{
		}
	}

	// Information we store about a running process.
	struct ProcessInfo
	{
		public Process process;			// Process object
		public ProcessOutput output;	// Combined stdout and stderr
	}

	class ProcessList
	{
		// Run a process.
		public void RunProcess(string path, IEnumerable<string> args)
		{
			// Combine args into single string.
			var quotedArgs = args.Select(arg => ArgvQuote(arg, false));
			var argString = string.Join(" ", quotedArgs);

			// Run the file with a hidden window.
			var startInfo = new ProcessStartInfo(path, argString);
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;

			try
			{
				// Start the process.
				var process = Process.Start(startInfo);
				var output = new ProcessOutput(process);
				processes.Add(new ProcessInfo() { process = process, output = output });
			}
			catch (System.ComponentModel.Win32Exception ex)
			{
				throw new ProcessException("Failed to launch process '" + path + "'", ex);
			}
			catch (System.IO.FileNotFoundException ex)
			{
				throw new ProcessException("Failed to launch process '" + path + "'", ex);
			}
		}

		// Close a process, forcefully if necessary.
		public void Close(int index)
		{
			var process = processes[index].process;
			if (process.CloseMainWindow())
			{
				// Successfully sent close message, wait for the process to exit.
				if (!process.WaitForExit(2000))
				{
					// Process is not responding -- kill it.
					process.Kill();
				}
			}
			else
			{
				// Could not close the app nicely, so kill it.
				process.Kill();
			}

			processes[index].output.Dispose();
			process.Close();
			processes.RemoveAt(index);
		}

		// Close all processes, forcefully if necessary.
		public void CloseAll()
		{
			RemoveExited();

			foreach (var processInfo in processes)
			{
				var process = processInfo.process;
				if (process.CloseMainWindow())
				{
					// Successfully sent close message, wait for the process to exit.
					if (!process.WaitForExit(2000))
					{
						// Process is not responding -- kill it.
						process.Kill();
					}
				}
				else
				{
					// Could not close the app nicely, so kill it.
					process.Kill();
				}

				processInfo.output.Dispose();
				process.Close();
			}

			processes.Clear();
		}

		// Get the friendly names for each process.
		public IEnumerable<string> GetNames()
		{
			RemoveExited();
			return processes.Select(pi => pi.process.ProcessName + " " + pi.process.StartInfo.Arguments);
		}

		// Do we have any running processes?
		public bool HasProcesses()
		{
			RemoveExited();
			return processes.Any();
		}

		public ProcessOutput GetProcessOutput(int index)
		{
			return processes[index].output;
		}

		// Remove any exited process from the list.
		private void RemoveExited()
		{
			// Close and remove exited processes.
			processes.RemoveAll(pi =>
				{
					if (pi.process.HasExited)
					{
						pi.output.Dispose();
						pi.process.Close();
						return true;
					}
					return false;
				});
		}

		/* Converted to C# from:
		 * http://blogs.msdn.com/b/twistylittlepassagesallalike/archive/2011/04/23/everyone-quotes-arguments-the-wrong-way.aspx

		Routine Description:
			This routine appends the given argument to a command line such
			that CommandLineToArgvW will return the argument string unchanged.
			Arguments in a command line should be separated by spaces; this
			function does not add these spaces.

		Arguments:
			Argument - Supplies the argument to encode.
			Force - Supplies an indication of whether we should quote
					the argument even if it does not contain any characters that would
					ordinarily require quoting.
		*/
		private string ArgvQuote(string argument, bool force)
		{
			// Unless we're told otherwise, don't quote unless we actually
			// need to do so --- hopefully avoid problems if programs won't
			// parse quotes properly
			if (!force &&
				!string.IsNullOrEmpty(argument) &&
				argument.IndexOfAny(new[] {' ', '\t', '\n', '\v', '"'}) == -1)
			{
				return argument;
			}
			else
			{
				var commandLine = new StringBuilder();
				commandLine.Append('"');

				for (int i = 0; ; i++)
				{
					int NumberBackslashes = 0;

					while (i != argument.Length && argument[i] == '\\')
					{
						++i;
						++NumberBackslashes;
					}

					if (i == argument.Length)
					{
						// Escape all backslashes, but let the terminating
						// double quotation mark we add below be interpreted
						// as a metacharacter.
						commandLine.Append('\\', NumberBackslashes * 2);
						break;
					}
					else if (argument[i] == '"')
					{
						// Escape all backslashes and the following
						// double quotation mark.
						commandLine.Append('\\', NumberBackslashes * 2 + 1);
						commandLine.Append(argument[i]);
					}
					else
					{
						// Backslashes aren't special here.
						commandLine.Append('\\', NumberBackslashes);
						commandLine.Append(argument[i]);
					}
				}
	
				commandLine.Append('"');
				return commandLine.ToString();
			}
		}

		private List<ProcessInfo> processes = new List<ProcessInfo>();
	}
}
