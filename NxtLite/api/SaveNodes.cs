using System;
using System.Collections.Generic;

namespace NxtLite.api
{
	public partial class APIProcessor
	{
		private bool SaveNodes(Dictionary<string, string> dParams)
		{
			Nodes.SaveToDisk();

			this._data_for_remote = "{\"result\":\"ok\"}";
			return true;
		}
	}
}
