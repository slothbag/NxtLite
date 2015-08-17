using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NxtLite 
{
	internal sealed class Program
	{
		public delegate void NoParamMethod();
		
		[STAThread]
		private static void Main(string[] args)
		{
			//create the app config directory
			if (!System.IO.Directory.Exists(Utils.getAppDirectory)) {
				System.IO.Directory.CreateDirectory(Utils.getAppDirectory);
			}
			else {
				//try load existing settings
				Nodes.LoadFromFile();
				Nodes.ScanLatestBlockHeight();
			}
			
			//start the Core
			var core = new WebServer.WebServer();
			core.Run();

			while (true)
				System.Threading.Thread.Sleep(50);
		}

		//Nodes.SaveToDisk();
	}
}


