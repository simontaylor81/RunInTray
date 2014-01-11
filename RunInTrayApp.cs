using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunInTray
{
	// Entry point class.
	static class RunInTrayApp
	{
		public static string Title { get { return "Run In Tray"; } }

		[STAThread]
		static void Main(string[] args)
		{
			var controller = new SingleInstanceController();
			controller.Run(args);
		}
	}
}
