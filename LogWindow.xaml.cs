using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace RunInTray
{
	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow : Window
	{
		private ProcessOutput output;
		private CompositeDisposable disposables = new CompositeDisposable();

		public ProcessOutput ProcessOutput { get { return output; } }

		public LogWindow(ProcessOutput output, string title)
		{
			InitializeComponent();
			this.output = output;
			this.Title = title + " - Output";
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			// Subscribe to combined output and error sequence.
			disposables.Add(output.GetOutput()
				.Merge(output.GetErrorOutput())
				.ObserveOn(logTextBox)
				.Subscribe(AddLogText));

			// Set focus to the text box so the caret is visible.
			logTextBox.Focus();
		}

		// Add text to the text box.
		private void AddLogText(string s)
		{
			bool bAutoScroll = logTextBox.CaretIndex == logTextBox.Text.Length;

			logTextBox.AppendText(s);

			// If the cursor is at the end of the text, automatically scroll to show the new content.
			if (bAutoScroll)
			{
				logTextBox.ScrollToEnd();
				logTextBox.CaretIndex = logTextBox.Text.Length;
			}
		}
	}
}
