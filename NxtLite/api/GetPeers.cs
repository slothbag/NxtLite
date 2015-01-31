using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace NxtLite.api
{
	public partial class APIProcessor
	{
		private bool GetPeers(Dictionary<string, string> dParams)
		{
			
			var lNodes = from p in Nodes._nodes
						where p.consecutive_errors == 0
						orderby p.latency
						select p;
			
			var lNodes2 = from p in Nodes._nodes
						where p.consecutive_errors > 0
						orderby p.latency
						select p;
			
			List<Nodes.Node> fullList = new List<Nodes.Node>();
			fullList.AddRange(lNodes);
			fullList.AddRange(lNodes2);
			
			JObject jo = new JObject();
			JArray ja = JArray.FromObject(fullList);
			jo.Add("result", ja);
			
			this._data_for_remote = jo.ToString();
			return true;
		}
	}
}
