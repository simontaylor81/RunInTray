﻿using System;
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
				.Subscribe(s => AddText(s, false));

			// Subscribe to error sequence.
			disposable = output.GetErrorOutput()
				.ObserveOn(logTextBox)
				.Subscribe(s => AddText(s, true));
		}

		// Add text to the text box.
		private void AddText(string s, bool isError)
		{
			// Save previous selection.
			var prevSelectionStart = logTextBox.SelectionStart;
			var prevSelectionLength = logTextBox.SelectionLength;
			bool atEnd = logTextBox.SelectionStart == logTextBox.TextLength;
			var prevLength = logTextBox.TextLength;

			// Append output to text box.
			// Don't use AppendText as it messes with the scroll position.
			logTextBox.AppendText(s);

			// Set colour for new text.
			logTextBox.SelectionStart = prevLength;
			logTextBox.SelectionLength = s.Length;
			logTextBox.SelectionColor = isError ? Color.DarkRed : Color.Black;

			if (atEnd)
			{
				// Stay at end if we were previously.
				logTextBox.SelectionStart = logTextBox.TextLength;
			}
			else
			{
				// Otherwise, restore previous selection.
				logTextBox.SelectionStart = prevSelectionStart;
				logTextBox.SelectionLength = prevSelectionLength;
			}
		}
	}
}
