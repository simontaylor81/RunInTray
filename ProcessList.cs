using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RunInTray
{
	class ProcessList
	{
		// Run a process.
		public void RunProcess(string path)
		{
			// Run the file with a hidden window.
			var startInfo = new ProcessStartInfo(path);
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.UseShellExecute = true;

			processes.Add(Process.Start(startInfo));
		}

		// Close a process, forcefully if necessary.
		public void Close(int index)
		{
			var process = processes[index];
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

			process.Close();
			processes.RemoveAt(index);
		}

		// Close all processes, forcefully if necessary.
		public void CloseAll()
		{
			RemoveExited();

			foreach (var process in processes)
			{
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

				process.Close();
			}

			processes.Clear();
		}

		// Get the friendly names for each process.
		public IEnumerable<string> GetNames()
		{
			RemoveExited();
			return processes.Select(p => p.ProcessName);
		}

		// Do we have any running processes?
		public bool HasProcesses()
		{
			RemoveExited();
			return processes.Any();
		}

		// Remove any exited process from the list.
		private void RemoveExited()
		{
			// Close and remove exited processes.
			processes.RemoveAll(p =>
				{
					if (p.HasExited)
					{
						p.Close();
						return true;
					}
					return false;
				});
		}

		private List<Process> processes = new List<Process>();
	}
}
