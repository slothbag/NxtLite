using System;
using System.Collections.Generic;

namespace NxtLite.api
{
	public partial class APIProcessor
	{
		private bool Reset(Dictionary<string, string> dParams)
		{
			Nodes.ClearAll();
			
			this._data_for_remote = "{\"result\":\"ok\"}";
			return true;
		}
	}
}
