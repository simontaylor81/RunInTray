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

		// Close all processes, forcefully if necessary.
		public void CloseAll()
		{
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
			return processes.Select(p => p.ProcessName);
		}

		// Do we have any running processes?
		public bool HasProcesses()
		{
			return processes.Any();
		}

		private List<Process> processes = new List<Process>();
	}
}
