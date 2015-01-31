using System;
using System.Collections.Generic;

namespace NxtLite.api
{
	public partial class APIProcessor
	{
		public delegate bool ProcessAPI(Dictionary<string, string> dParams);
		private Dictionary<string, ProcessAPI> methodmap;
		
		private string _data_for_remote;
		
		public string dataForRemote {
			get {
				return this._data_for_remote;
			}
		}
		
		public APIProcessor()
		{
			this.initMethodMap();
		}
		
		private void initMethodMap() {
			methodmap = new Dictionary<string, ProcessAPI>();
			methodmap.Add("getstatus", this.GetStatus);
			methodmap.Add("setmode", this.SetMode);
			methodmap.Add("addnode", this.AddNode);
			methodmap.Add("getpeers", this.GetPeers);
		}
		
		public bool processAPI(string method, Dictionary<string, string> dParams) {
			bool retVal;
			
			if (methodmap.ContainsKey(method)) {
				retVal = methodmap[method].Invoke(dParams);
				if (retVal)
					return true;
				else
					return false;
			}
			else {
				this._data_for_remote = "{\"error\":\"Unknown method\"}";
				return false;
			}
		}
	}
}
