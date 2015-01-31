using System;
using System.Collections.Generic;

namespace NxtLite.api
{
	public partial class APIProcessor
	{
		private bool AddNode(Dictionary<string, string> dParams)
		{
			//can return (firstrun, single, multi)
			
			if (!dParams.ContainsKey("address")) {
				this._data_for_remote = "{\"error\":\"Missing address\"}";
				return false;
			}
				
			string ipaddress = dParams["address"];
			
			Nodes.AddNodes(ipaddress);
			Nodes.SaveToDisk();
			this._data_for_remote = "{\"result\":\"ok\",\"data\":\""+ipaddress+"\"}";
			return true;
		}
	}
}
