using System;
using System.Collections.Generic;

namespace NxtLite.api
{
	public partial class APIProcessor
	{
		private bool SetMode(Dictionary<string, string> dParams)
		{
			//can return (firstrun, single, multi)
			if (dParams.ContainsKey("mode")) {
				if (dParams["mode"] == "multi") {
					Nodes.nxtpeersImportFromPE();
					Nodes.SaveToDisk();
				}
			}
			
			this._data_for_remote = "{\"result\":\"ok\"}";
			return true;
		}
	}
}
