using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reactive.Linq;

namespace RunInTray
{
	public partial class LogForm : Form
	{
		private ProcessOutput output;
		private IDisposable disposable;

		public LogForm(ProcessOutput output)
		{
			InitializeComponent();
			this.output = output;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// Subscribe to output sequence.
			disposable = output.GetOutput()
				.ObserveOn(logTextBox)
				.Subscribe(c =>
				{
					// Append output to text box.
					logTextBox.AppendText(c);
				});
		}
	}
}
