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

		public class PeerBlockHeightHistory {
			private int last_agreed_block_height = 0;

			private struct PeerBlockHeight {
				public string ip_address;
				public int block_height;
			}

			private PeerBlockHeight[] History = new PeerBlockHeight[3];

			public int AgreedBlockHeight {
				get { return this.last_agreed_block_height; }
			}

			private bool TryUpdateToLatestBlockHeight() {
				int min = 0;
				int max = 0;
				for (int a = 0; a < History.Length; a++) {
					//if the history buffer is not full then no agreement yet
					if (History[a].block_height == 0)
						return false;
					
					if (min == 0 || History[a].block_height < min)
						min = History[a].block_height;
					if (max == 0 || History[a].block_height > max)
						max = History[a].block_height;
				}
				if (max - min < 3 && max > this.last_agreed_block_height) {
					this.last_agreed_block_height = max;
					return true;
				}
				else
					return false;
			}
				
			public void Add(string ip_address, int block_height) {
				bool bFound = false;
				for (int a = 0; a < History.Length; a++) {
					if (History[a].ip_address == ip_address) {
						History[a].block_height = block_height;
						bFound = true;
					}
				}
				if (!bFound) {
					//not found, add it
					History[0] = History[1];
					History[1] = History[2];
					History[2] = new PeerBlockHeight() {ip_address = ip_address, block_height = block_height};
				}

				if (this.TryUpdateToLatestBlockHeight())
					Console.WriteLine("Last block set to " + this.AgreedBlockHeight);
			}
		}
		
		public static List<Nodes.Node> _nodes = new List<Nodes.Node>();
		public static PeerBlockHeightHistory peer_block_height_history = new PeerBlockHeightHistory();
		
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
		
		public static void SetLatestBlockHeight(string ip_address, int block_height) {
			peer_block_height_history.Add(ip_address, block_height);
			return;
		}
		
		public static void ClearAll() {
			_nodes.Clear();
			Nodes.SaveToDisk();
		}
	}
}
