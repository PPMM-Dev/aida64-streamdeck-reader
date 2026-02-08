using Aida64SDPlugin.Core;
using BarRaider.SdTools;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Aida64SDPlugin
{
	internal class Program
	{
		[SupportedOSPlatform("windows")]
		static async Task Main(string[] args)
		{
			//Uncomment and attach to the Aida64SdReader process for debuging
			//while (!System.Diagnostics.Debugger.IsAttached) { System.Threading.Thread.Sleep(100);}

			await Aida64Plugin.Instance.Start();

			Logger.Instance.LogMessage(TracingLevel.INFO, "Watcher started");
			Console.WriteLine("Watcher started");
			SDWrapper.Run(args);

			Thread.Sleep(Timeout.Infinite); //Change for a cleaner way to exit
		}
	}
}
