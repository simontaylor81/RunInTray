using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RunInTray
{
	public class RunInTrayForm : Form
	{
		private NotifyIcon trayIcon;
		private ContextMenuStrip trayMenu;
		private ProcessList processes = new ProcessList();

		public RunInTrayForm()
		{
			// Create tray menu.
			trayMenu = new ContextMenuStrip();
			trayMenu.Opening += OnMenuOpening;

			// Create a tray icon. In this example we use a
			// standard system icon for simplicity, but you
			// can of course use your own custom icon too.
			trayIcon = new NotifyIcon();
			trayIcon.Text = RunInTrayApp.Title;
			trayIcon.Icon = new Icon(SystemIcons.Application, 16, 16);

			// Add menu to tray icon and show it.
			trayIcon.ContextMenuStrip = trayMenu;
			trayIcon.Visible = true;
		}

		protected override void OnLoad(EventArgs e)
		{
			Visible = false; // Hide form window.
			ShowInTaskbar = false; // Remove from taskbar.

			base.OnLoad(e);

			// Skip the first arg (the process name).
			if (!ProcessCommandLine(Environment.GetCommandLineArgs().Skip(1)))
			{
				// If the user launched the app to run a process, they probably don't want the
				// app hanging around after if it failed, so quit immediately.
				Application.Exit();
			}
		}

		// If an app was specified on the commandline, run it.
		public bool ProcessCommandLine(IEnumerable<string> args)
		{
			if (args.Any())
			{
				try
				{
					processes.RunProcess(args.First(), args.Skip(1).ToArray());
				}
				catch (ProcessException ex)
				{
					MessageBox.Show(ex.Message, RunInTrayApp.Title);
					return false;
				}
			}

			return true;
		}

		// Contruct the tray menu on demand before it is opened.
		private void OnMenuOpening(object sender, System.ComponentModel.CancelEventArgs eventArgs)
		{
			// Clear existing entries.
			trayMenu.Items.Clear();

			// Add sub-menu for each running process.
			processes.GetNames().ForEach((processName, index) =>
				{
					trayMenu.Items.Add(new ToolStripMenuItem(processName, null,
						new ToolStripMenuItem("Kill", null, new EventHandler((o, e) => processes.Close(index))),
						new ToolStripMenuItem("Output", null, new EventHandler((o, e) => ShowProcessOutput(index)))
					));
				});

			if (processes.HasProcesses())
			{
				trayMenu.Items.Add(new ToolStripSeparator());
			}

			trayMenu.Items.Add("Run App", null, OnRunApp);
			trayMenu.Items.Add("Kill All", null, OnKillAll)
				.Enabled = processes.HasProcesses();
			trayMenu.Items.Add("Exit", null, OnExit);
		}

		private void OnExit(object sender, EventArgs e)
		{
			if (processes.HasProcesses())
			{
				// Warn if processes are running.
				var message = "The following processes are still running. Are you sure you wish to exit?\n  "
					+ string.Join("\n  ", processes.GetNames());
				var result = MessageBox.Show(message, RunInTrayApp.Title, MessageBoxButtons.YesNo);
				if (result == DialogResult.No)
				{
					return;
				}
				
				// Quit all running processes.
				processes.CloseAll();
			}

			Application.Exit();
		}

		private void OnKillAll(object sender, EventArgs e)
		{
			if (processes.HasProcesses() &&
				MessageBox.Show(
					"This will kill all running processes. Are you sure?",
					RunInTrayApp.Title,
					MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				processes.CloseAll();
			}
		}

		private void OnRunApp(object sender, EventArgs e)
		{
			// Ask the user to pick the application.
			var dialog = new OpenFileDialog();
			dialog.InitialDirectory = "C:\\";
			dialog.Filter = "All files (*.*)|*.*";
			dialog.RestoreDirectory = true;

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				processes.RunProcess(dialog.FileName, Enumerable.Empty<string>());
			}
		}

		private void ShowProcessOutput(int index)
		{
			var logForm = new LogForm(processes.GetProcessOutput(index));
			logForm.Show();
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				// Release the icon resource.
				trayIcon.Dispose();
			}

			base.Dispose(isDisposing);
		}
	}
}
