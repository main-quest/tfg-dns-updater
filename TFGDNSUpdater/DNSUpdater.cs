using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace TFGDNSUpdater
{
	class DNSUpdater
	{
		public const string IP_TOKEN = "{ip}";

		readonly string _URLTemplate;

		// 30 mins
		const int SLEEP_ON_SUCCESS = 30 * 60 * 1000;
		// Retry after 1 min
		const int SLEEP_ON_FAILURE = 1 * 60 * 1000;

		readonly string[] PROVIDERS = new string[]
		{
				"http://myexternalip.com/raw",
				"https://checkip.amazonaws.com/",
				"https://api.ipify.org",
				"http://bot.whatismyipaddress.com/",
				"http://ipinfo.io/ip"
		};


		public DNSUpdater(string uRLTemplate)
		{
			_URLTemplate = uRLTemplate;
		}


		public void Start()
		{
			var t = new Thread(Loop);
			t.Start();
		}

		void Loop()
		{
			while (true)
			{
				string log = $"[{nameof(DNSUpdater)}] ";
				int toSleep = SLEEP_ON_FAILURE;
				if (TryGetExternalIP(out string ip))
				{
					log += $"Retrieved external ip: {ip}; ";
					if (RequestUpdate(ip, out string response))
					{
						// Commented: No need to clutter on success
						//log += $"Successfully updated DNS: {response}";
						log += $"Successfully updated DNS";
						toSleep = SLEEP_ON_SUCCESS;
					}
					else
						log += $"Failed to update DNS:\r\n{response}";
				}
				else
					log += $"Failed to retrieve external ip";

				Console.WriteLine(log);
				Thread.Sleep(toSleep);
			}
		}

		bool TryGetExternalIP(out string ip)
		{

			string result = null;
			bool success = false;
			using (var client = CreateWebClient())
			{
				foreach (var url in PROVIDERS)
				{
					try
					{
						result = client.DownloadString(url);
					}
					catch
					{
						result = null;
					}

					if (string.IsNullOrEmpty(result))
						continue;

					if (!Regex.IsMatch(result, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
						continue;

					success = true;
					break;
				}
			}
			ip = result;

			return success;
		}

		bool RequestUpdate(string ip, out string response)
		{
			string url = _URLTemplate.Replace(IP_TOKEN, ip);
			response = "{Unknown}";
			try
			{
				using (var client = CreateWebClient())
				{
					response = client.DownloadString(url);

					if (response.Contains("<ErrCount>0</ErrCount>") && response.Contains("<Done>true</Done>"))
						return true;
				}
			}
			catch
			{
			}

			return false;
		}

		WebClient CreateWebClient()
		{
			var client = new WebClient();
			client.Headers["User-Agent"] = "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) " +
				"(compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

			return client;
		}
	}
}
