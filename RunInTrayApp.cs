using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RunInTray
{
	public class RunInTrayApp : Form
	{
		[STAThread]
		public static void Main()
		{
			Application.Run(new RunInTrayApp());
		}

		private NotifyIcon trayIcon;
		private ContextMenuStrip trayMenu;
		private ProcessList processes = new ProcessList();

		public RunInTrayApp()
		{
			// Create tray menu.
			trayMenu = new ContextMenuStrip();
			trayMenu.Opening += OnMenuOpening;

			// Create a tray icon. In this example we use a
			// standard system icon for simplicity, but you
			// can of course use your own custom icon too.
			trayIcon = new NotifyIcon();
			trayIcon.Text = "Run-in-tray";
			trayIcon.Icon = new Icon(SystemIcons.Application, 16, 16);

			// Add menu to tray icon and show it.
			trayIcon.ContextMenuStrip = trayMenu;
			trayIcon.Visible = true;
		}

		// Contruct the tray menu on demand before it is opened.
		private void OnMenuOpening(object sender, System.ComponentModel.CancelEventArgs eventArgs)
		{
			// Clear existing entries.
			trayMenu.Items.Clear();

			// Add sub-menu for each running process.
			processes.GetNames().ForEach((processName, index) =>
				{
					var item = new ToolStripMenuItem(processName, null,
						new ToolStripMenuItem("Kill", null, new EventHandler((o, e) => processes.Close(index)))
					);
					trayMenu.Items.Add(item);
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

		protected override void OnLoad(EventArgs e)
		{
			Visible = false; // Hide form window.
			ShowInTaskbar = false; // Remove from taskbar.

			base.OnLoad(e);
		}

		private void OnExit(object sender, EventArgs e)
		{
			if (processes.HasProcesses())
			{
				// Warn if processes are running.
				var message = "The following processes are still running. Are you sure you wish to exit?\n  "
					+ string.Join("\n  ", processes.GetNames());
				var result = MessageBox.Show(message, "Run in tray", MessageBoxButtons.YesNo);
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
					"Run in tray",
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
				processes.RunProcess(dialog.FileName);
			}
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
