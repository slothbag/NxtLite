using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NxtLite.api
{
	public partial class APIProcessor
	{
		private bool GetStatus(Dictionary<string, string> dParams)
		{
			JObject jo = new JObject();
			JObject jResult = new JObject();
			
			//can return (firstrun, single, multi)
			if (Nodes._nodes.Count == 0)
				jResult.Add("mode", "firstrun");
			else if (Nodes._nodes.Count == 1) {
				jResult.Add("mode", "single");
				jResult.Add("address", Nodes._nodes[0].address);
			}
			else
				jResult.Add("mode", "multi");
			
			jo.Add("result", jResult);
			this._data_for_remote = jo.ToString();
			
			return true;
		}
	}
}
