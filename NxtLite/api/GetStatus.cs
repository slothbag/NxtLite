using System;
using System.Collections.Generic;

namespace NxtLite.api
{
	public partial class APIProcessor
	{
		private bool GetStatus(Dictionary<string, string> dParams)
		{
			//can return (firstrun, single, multi)
			if (Nodes._nodes.Count == 0)
				this._data_for_remote = "{\"result\":\"firstrun\"}";
			else if (Nodes._nodes.Count == 1)
				this._data_for_remote = "{\"result\":\"single\"}";
			else
				this._data_for_remote = "{\"result\":\"multi\"}";
			return true;
		}
	}
}
