using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunInTray
{
	// Class that makes sure there's only ever a single instance of the application.
	class SingleInstanceController : WindowsFormsApplicationBase
	{
		public SingleInstanceController()
		{
			IsSingleInstance = true;

			StartupNextInstance += startupNextInstance;
		}

		private void startupNextInstance(object sender, StartupNextInstanceEventArgs e)
		{
			var form = (RunInTrayForm)MainForm;
			form.ProcessCommandLine(e.CommandLine);
		}

		protected override void OnCreateMainForm()
		{
			MainForm = new RunInTrayForm();
		}
	}
}
