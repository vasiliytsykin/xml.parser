using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ConsoleApplication
{

	public class XmlParser
	{
		public IEnumerable<XElement> Read(string path)
		{
			var xDoc = XDocument.Load(path);
			return xDoc.Root.Element("flats").Elements("flat");
		}

		private IEnumerable<FormUrlEncodedContent> PrepareContent(IEnumerable<XElement> flats)
		{
			Dictionary<string, string> fields = new Dictionary<string, string> ();
			int counter = 0;
			foreach (var flat in flats) 
			{
				foreach (var prop in flat.Elements()) 
				{
					string key = String.Format ("flats[{0}][{1}]", counter, prop.Name);
					fields [key] = prop.Value;
				}
				counter++;
				if (counter % 50 == 0) 
				{
					yield return new FormUrlEncodedContent (fields);
					fields = new Dictionary<string, string> ();
				}
			}

			yield return new FormUrlEncodedContent (fields);
		}

		public void SendData(string url, string path)
		{
			var dataChunks = PrepareContent (Read (path));
			var client = new HttpClient ();
			Console.WriteLine("transfer started");
			foreach (var chunk in dataChunks) 
			{
				var t = client.PostAsync (url, chunk).Result;
				Console.WriteLine (t.Content.ReadAsStringAsync ().Result);
			}

			Console.WriteLine("transfer complete");
		}
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			Stopwatch sw = Stopwatch.StartNew ();
			var path = "doc/exp.xml";
			var parser = new XmlParser();
			parser.SendData(@"http://vvzakharov.ru/xml/", path);

			Console.WriteLine (sw.Elapsed);
		}
	}
}