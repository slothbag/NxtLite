using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;

namespace NxtLite
{
	public static class Nodes
	{
		public class Node {
			public string address;
			public DateTime last_checked;
			public DateTime last_reachable;
			public int block_height;
			public string last_block;
			public bool reachable;
			public int time;
			public string version;
			public int block_sync_failures;
			public int connection_failures;
			public int connection_attempts;
			public int consecutive_errors;
			public string last_error;
			public bool onFork;
			public long latency;
			public int rank;
			public bool banned;
			
			public Node(string address) {
				this.address = address;
			}
		}
		
		public static List<Nodes.Node> _nodes = new List<Nodes.Node>();
		public static int[] block_height_history = new int[3];
		
		public static int latest_block_height = 0;
		
		public static bool AddNodes(string address) {
			_nodes.Add(new Node(address));
			
			return true;
		}
		
		public static bool SaveToDisk() {
			string nodefilepath = Path.Combine(Utils.getAppDirectory, "nodes.dat");
			
			string json = JsonConvert.SerializeObject(_nodes);
			File.WriteAllText(nodefilepath, json);
			
			return true;
		}
		
		public static bool LoadFromFile() {
			string nodefilepath = Path.Combine(Utils.getAppDirectory, "nodes.dat");
			List<Nodes.Node> nodeList = new List<Nodes.Node>();
			
			//Load NxtNodes json
			if (!File.Exists(nodefilepath))
			    return true;
			
			StreamReader re = new StreamReader(nodefilepath);
			JsonTextReader reader = new JsonTextReader(re);
			
			JsonSerializer se = new JsonSerializer();
			JArray parsedData = (JArray)se.Deserialize(reader);
			
			if (parsedData != null) {
				foreach (JObject jo in parsedData) {
					Nodes.Node tmpNode = jo.ToObject<Nodes.Node>();
					_nodes.Add(tmpNode);
				}
			}
			
    		reader.Close();
    		re.Close();

			return true;
		}
		
		public static void nxtpeersImportFromPE() {
			string apiresult;
			bool retVal = Utils.doRESTfullApi("http://www.peerexplorer.com/api_openapi", out apiresult);
			
			if (!retVal)
				return;
						
			JObject jo = JObject.Parse(apiresult);
			foreach (JToken tmpjtoken in jo["peers"]) {
				//check IP doesnt exist first
				if (Nodes.findPeerByIP(tmpjtoken.ToString()) == null) {
					Nodes.Node nxtnode = new Nodes.Node(tmpjtoken.ToString());
					_nodes.Add(nxtnode);
				}
			}
		}
		
		public static Nodes.Node findPeerByIP(string ip) {
			Nodes.Node retNode = null;
			foreach (Nodes.Node tmpNode in _nodes) {
				if (tmpNode.address == ip) {
					retNode = tmpNode;
					break;
				}
			}
			return retNode;
		}
		
		public static Nodes.Node getNxtPeerToUse(bool getBest = false) {
			
			List<Nodes.Node> lNodes1;
			List<Nodes.Node> lNodes2;
			List<Nodes.Node> lNodes = new List<Nodes.Node>();

			bool single_node = false;
			if (Nodes._nodes.Count == 1)
				single_node = true;
			
			lock (_nodes) {
				if (getBest) {
					lNodes1 =
						(from p in _nodes
						where p.latency > 0
						where (single_node || p.consecutive_errors == 0)
						orderby p.latency
						select p).ToList<Nodes.Node>();
					lNodes2 = 	
						(from p in _nodes
						where (single_node || p.consecutive_errors == 0)
						orderby p.latency
						select p).ToList<Nodes.Node>();
					
					lNodes.AddRange(lNodes1);
					lNodes.AddRange(lNodes2);
				}
				else {
					lNodes1 =
						(from p in _nodes
						where (single_node || p.consecutive_errors == 0)
						orderby p.last_checked
						select p).ToList<Nodes.Node>();
					
					lNodes.AddRange(lNodes1);
				}
				
				if (lNodes.Count() == 0)
					return null;
				
				lNodes.First().last_checked = DateTime.Now;
			}
	
			return lNodes.First();
		}
		
		public static int ScanLatestBlockHeight() {
			foreach (Node tmpNode in _nodes) {
				if (tmpNode.block_height > latest_block_height) {
					latest_block_height = tmpNode.block_height;
					break;
				}
			}
			return latest_block_height;
		}
		
		public static void SetLatestBlockHeight(int block_height) {
			if (block_height_history.Max() - block_height_history.Min() < 3) {
				if (block_height_history.Max() > latest_block_height) {
					latest_block_height = block_height_history.Max();
				}
			}

			//cycle the block history queue
			block_height_history[0] = block_height_history[1];
			block_height_history[1] = block_height_history[2];
			block_height_history[2] = block_height;

			return;
		}
		
		public static void ClearAll() {
			_nodes.Clear();
			Nodes.SaveToDisk();
		}
	}
}
