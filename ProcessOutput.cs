using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace RunInTray
{
	// Class for handling process stdout and stderr.
	public class ProcessOutput
	{
		private StreamReader stdout;
		private StringBuilder outputString = new StringBuilder();
		private ReplaySubject<char> subject = new ReplaySubject<char>();

		// TODO: stderr
		public ProcessOutput(StreamReader stdout)
		{
			this.stdout = stdout;

			// Asynchronously read from the stream readers and build up the result string.
			Task.Run(() =>
				{
					//string str = stdout.ReadToEnd();
					//Console.WriteLine(str);
					while (!stdout.EndOfStream)
					{
						int c = stdout.Read();
						if (c == -1)
						{
							break;
						}
						//outputString.Append((char)c);
						subject.OnNext((char)c);
					}
					//Console.WriteLine(outputString.ToString());
					subject.OnCompleted();
				});
		}

		// Observable that contains the contents of the output.
		public IObservable<char> GetOutput()
		{
			return subject;
		}

		// Get the output so far.
		//public string GetOutput()
		//{
		//	return outputString.ToString();
		//}
	}
}
