using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFGDNSUpdater
{
	class Program
	{
		const string CFG_FILE = "config.cfg";
		const string CFG_FILE_PROPERTY_URLTEMPLATE = "URLTEMPLATE";
		const string CFG_FILE_PROPERTY_URLTEMPLATE_PREVALUE = CFG_FILE_PROPERTY_URLTEMPLATE + "=";


		static void Main(string[] args)
		{

			if (!File.Exists(CFG_FILE))
			{
				File.WriteAllLines(
					CFG_FILE,
					new string[]
					{
						$"# Put the string '{DNSUpdater.IP_TOKEN}' (without quotes) where the ip needs to be inserted. Example: http://ddns.example.com/mydomain/myusername?ip={DNSUpdater.IP_TOKEN}",
						CFG_FILE_PROPERTY_URLTEMPLATE_PREVALUE
					}
				);

				Console.WriteLine("Error: CFG file not found: A default one was written and needs to be modified");
				Console.Read();
				return;
			}

			// Remove comments
			var lines = new List<string>(File.ReadAllLines(CFG_FILE));
			lines.RemoveAll(l => l.StartsWith("#"));

			var line = lines.Find(l => l.StartsWith(CFG_FILE_PROPERTY_URLTEMPLATE_PREVALUE));
			if (line == null)
				throw new InvalidOperationException($"Couldn't find CFG property {CFG_FILE_PROPERTY_URLTEMPLATE}");

			string urlTemplate = line.Substring(CFG_FILE_PROPERTY_URLTEMPLATE_PREVALUE.Length);
			var updater = new DNSUpdater(urlTemplate);
			updater.Start();

			Console.Read();
		}
	}
}
