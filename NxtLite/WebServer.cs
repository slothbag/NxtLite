using System;
using System.Threading;
using System.Reflection;
using System.IO;
using Mono.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace NxtLite.WebServer
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private List<thread_info> _threads = new List<thread_info>();
        
        public struct thread_info {
        	public HttpListenerContext ctx;
        	public ManualResetEvent reset_event;
        }

        public WebServer()
        {		
        	string prefix = "http://+:1234/";
            this._listener.Prefixes.Add(prefix);
            this._listener.Start();
        }

        public void Run()
        {
        	//Start listening in new thread (GetContext blocks)
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("API server listening on 127.0.0.1:1234...");
                try
                {
                    while (_listener.IsListening)
                    {
                    	thread_info new_thread_info = new thread_info();
                    	new_thread_info.ctx = _listener.GetContext();
                    	
                    	new_thread_info.reset_event = new ManualResetEvent(false);

                    	//Console.WriteLine("Starting thread " + this._threads.Count);
                    	ThreadPool.QueueUserWorkItem(new WaitCallback(GotRequest), new_thread_info);

                    	this._threads.Add(new_thread_info);
                    	
                    	//check status of finished threads
                    	for (int a=0;a<this._threads.Count;a++) {
                    		if (this._threads[a].reset_event.WaitOne(0)) {
                    			//Console.WriteLine("Closing thread " + a);
                    			this._threads.RemoveAt(a);	
                    		}
                    	}
                    }
                }
                catch { }
            });
        }
        
        public void GotRequest(Object stateinfo) {
        	WebServer.thread_info state_data = (WebServer.thread_info)stateinfo;
            HttpListenerContext ctx = state_data.ctx;
            
			//check for /nxt
			if (ctx.Request.Url.LocalPath.Length == 4 && ctx.Request.Url.LocalPath.Substring(0,4) == "/nxt") {
				tryTunnel(ctx);
			}
			//check for api
			else if (ctx.Request.Url.LocalPath.Length >= 4 && ctx.Request.Url.LocalPath.Substring(0,4) == "/api") {
				string method = ctx.Request.Url.LocalPath.Substring(5);
					
				//Get the params
				Dictionary<string,string> data = new Dictionary<string,string>();
				if (ctx.Request.QueryString.AllKeys.Length > 0) {
					if (ctx.Request.QueryString.AllKeys[0] != null)
						data = (Dictionary<string,string>)ctx.Request.QueryString.ToDictionary();					
				}
				
				var apiproc = new api.APIProcessor();
				apiproc.processAPI(method, data);
				sendResponse(ctx, apiproc.dataForRemote);
			}
			else {
				sendUI(ctx.Request.Url.LocalPath, ctx);
			}
				            
            bool bWaitingToKill = true;
            while (bWaitingToKill) {
	            try
	            {
	                ctx.Response.OutputStream.Close();
	                bWaitingToKill = false;
	            }
	            catch {
	            	Thread.Sleep(10);
	            }
            }
            
            state_data.reset_event.Set();
        }

        public void tryTunnel(HttpListenerContext ctx) {
        	bool hadSuccess = false;
        	bool getBestPeer = false;
        	
        	if (ctx.Request.HttpMethod == "POST")
        		return;
			
			while (!hadSuccess) {
	    		Nodes.Node tmpNode = Nodes.getNxtPeerToUse(getBestPeer);
	    		if (tmpNode == null) {
	    			sendError(ctx, "No peers to use");
	    			return;
	    		}
	    		
	    		tmpNode.connection_attempts++;
	    		
	        	hadSuccess = tunnelAPI(ctx, tmpNode);
	        	getBestPeer = true;
			}
        }
        
        public bool tunnelAPI(HttpListenerContext ctx, Nodes.Node publicNode) {

    		string querystring = ctx.Request.RawUrl;
    		string address = publicNode.address;
    		System.Net.WebResponse webresp = null;
    			
    		try {
	        	System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create("http://" + address + ":7876" + querystring); 
	        	wr.Timeout = 20000;
	        	var sw = new System.Diagnostics.Stopwatch();
	        	sw.Start();
	        	webresp = wr.GetResponse();
	        	sw.Stop();
	        	publicNode.latency = sw.ElapsedMilliseconds;
    		}
    		catch (Exception ex) {
				publicNode.reachable = false;
				publicNode.last_error = "Could not connect";
				publicNode.connection_failures++;
				publicNode.consecutive_errors++;
				return false;
    		}
    		
        	Stream NodeResponseStream = webresp.GetResponseStream();
        	
        	using (StreamReader sr = new StreamReader(NodeResponseStream)) {
        		string response_data = sr.ReadToEnd();
        		
        		//check for unknown block response, probably means non sync'd node
        		if (querystring.Substring(0,26) == "/nxt?requestType=getBlock&" && response_data == "{\"errorCode\":5,\"errorDescription\":\"Unknown block\"}") {
        		    publicNode.block_sync_failures ++;
        		    publicNode.consecutive_errors ++;
        		    publicNode.last_error = "Unknown block";
        		    //now need to connect to high priority node to try again
        		    
        		    return false;
        		}
        	
        		publicNode.last_reachable = publicNode.last_checked;
        		bool hasBlockHeight = false;
        		
        		if (querystring.Substring(0,37) == "/nxt?requestType=getBlockchainStatus&") {
        			try {
	        			JObject joGetBlockResponse = JObject.Parse(response_data);
	        			publicNode.last_block = joGetBlockResponse.SelectToken("lastBlock").ToString();
	        			publicNode.block_height = int.Parse(joGetBlockResponse.SelectToken("numberOfBlocks").ToString())-1;
	        			publicNode.time = int.Parse(joGetBlockResponse.SelectToken("time").ToString());
	        			publicNode.version = joGetBlockResponse.SelectToken("version").ToString();
	        			hasBlockHeight = true;
        			}
        			catch {}
       			}
        		
        		if (querystring.Substring(0,26) == "/nxt?requestType=getState&") {
        			try {
	        			JObject joGetBlockResponse = JObject.Parse(response_data);
	        			publicNode.last_block = joGetBlockResponse.SelectToken("lastBlock").ToString();
	        			publicNode.block_height = int.Parse(joGetBlockResponse.SelectToken("numberOfBlocks").ToString())-1;
	        			publicNode.time = int.Parse(joGetBlockResponse.SelectToken("time").ToString());
	        			publicNode.version = joGetBlockResponse.SelectToken("version").ToString();
	        			hasBlockHeight = true;
        			}
        			catch {}
       			}

        		if (hasBlockHeight) {
        			if (publicNode.block_height < Nodes.latest_block_height) {
	        		    publicNode.block_sync_failures ++;
	        		    publicNode.consecutive_errors ++;
	        		    publicNode.last_error = "Not fully sync'd";        				
        			}
        				
        		}
        			
        		
        		publicNode.consecutive_errors = 0;
        		Nodes.SetLatestBlockHeight(publicNode.block_height);
        		
        		byte[] response_bytes = System.Text.Encoding.UTF8.GetBytes(response_data);
        		try {
        			ctx.Response.OutputStream.Write(response_bytes,0,response_bytes.Length);
        		}
        		catch {}
        	}
        	
        	//try cleanup
        	try {
	        	webresp.Close();
	        	ctx.Response.Close();
        	}
        	catch {}
        	
        	//first get the request from local client
			//System.IO.StreamReader reader = new System.IO.StreamReader(ctx.Request.InputStream, System.Text.Encoding.UTF8);
			//string data = reader.ReadToEnd();
			//reader.Close();
			
			//if (data == "")
			//	return false;
			
			//convert local request to byte array
			//byte[] request_bytes = System.Text.Encoding.UTF8.GetBytes(data);
			
			//connect to remote node
			//System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();
        	//tcpClient.Connect("213.46.57.77",7876);
        	
        	//Stream ByteStream = tcpClient.GetStream();
        	//StreamWriter sw = new StreamWriter(ByteStream);
        	
        	//read the response from remote node
        	//using (StreamReader sr = new StreamReader(ByteStream)) {
        	//	string response_data = sr.ReadToEnd();
        	//	byte[] response_bytes = System.Text.Encoding.UTF8.GetBytes(response_data);
        	//	ctx.Response.OutputStream.Write(response_bytes,0,response_bytes.Length);
        	//}
        	
        	return true;
        }
        
        public bool sendUI(string url, HttpListenerContext ctx) {
        	
        	if (url.IndexOf("?") >= 0)
        		url = url.Substring(0, url.IndexOf("?") - 1);
        	
        	Console.WriteLine("Sending: " + url);
        	
            if (url == "/") {
       			sendResource(ctx, "/index.html");
            }
        	else {
        		sendResource(ctx, url);
        		
        	}
        	
        	return true;
        }
        
        public void Stop()
        {   
        	try {
            	_listener.Stop();
        	}
        	catch {}
        	
            _listener.Close();
            
            //wait for all threads to finish
            Console.WriteLine("Waiting for threads to finish");
            bool bActiveThreadsFound = false;
            int active_threads;
            do {
				active_threads = 0;
	            for (int a=0;a<this._threads.Count;a++) {
	            	if (this._threads[a].reset_event.WaitOne(0) == false) {
	          			bActiveThreadsFound = true;
						active_threads ++;
	            	}
	            }
				Console.WriteLine(active_threads + " active threads");
            	Thread.Sleep(100);
            } while (bActiveThreadsFound);
            Console.WriteLine("Threads finished");
        }
        
        public void sendResource(HttpListenerContext ctx, string filename) {
        	
        	DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        	
        	byte[] fileData = null;
        	if (dir.Parent.Parent.GetDirectories("assets").Length > 0)
        		dir = dir.Parent.Parent.GetDirectories("assets")[0];
        	else if (dir.GetDirectories("assets").Length > 0)
        		dir = dir.GetDirectories("assets")[0];

    		string test = Path.Combine(dir.FullName, filename.Substring(1));
    		if (File.Exists(test))
    			fileData = File.ReadAllBytes(test);
    		else {
    			sendError(ctx, "File not found");
        		return;
    		}
    		
        	/*else {
        		Assembly assem;
        		try {
        			assem = Assembly.Load("Assets");
        		}
        		catch (Exception ex) {
        			Console.WriteLine("Could not load Assets resource file");
	        		sendError(ctx, "File not found");
	        		return;        			
        		}
			    
        		//get the file from Assets file
				Stream s = assem.GetManifestResourceStream("Assets.ui." + filename);
				System.IO.BinaryReader br = new System.IO.BinaryReader(s);
				fileData = br.ReadAllBytes();
				br.Close();
				s.Close();
				
				if (fileData == null) {
	        		sendError(ctx, "File not found");
	        		return;
				}
        	}*/

        	if (filename.Substring(filename.Length - 4) == "html")
        		ctx.Response.Headers.Add("Content-Type","text/html");
        	if (filename.Substring(filename.Length - 3) == "css")
        		ctx.Response.Headers.Add("Content-Type","text/css");
        	if (filename.Substring(filename.Length - 2) == "js")
        		ctx.Response.Headers.Add("Content-Type","application/javascript");
        	if (filename.Substring(filename.Length - 3) == "png")
        		ctx.Response.Headers.Add("Content-Type","image/png");
        	
        	//if its the NRS.js, inject our own JS hooks
        	if (filename == "/js/nrs.js") {
        		string tmpFileData = System.Text.Encoding.UTF8.GetString(fileData);
        		//tmpFileData = tmpFileData.Replace("NRS.init();","$.getScript('nxtlite.js');if (typeof nxtliteready === 'undefined') return;NRS.init();");
        		tmpFileData = tmpFileData.Replace("NRS.init();","$.getScript('nxtlite.js');");
        		fileData = System.Text.Encoding.UTF8.GetBytes(tmpFileData);
        	}
        	
        	try {
        		ctx.Response.ContentLength64 = fileData.Length;
				ctx.Response.OutputStream.Write(fileData, 0, fileData.Length);
        	}
        	catch {
        		//TODO: do something here
        	}
        }
        
        private void sendError(HttpListenerContext ctx, string message) {
        	message = "{\"result\":null,\"error\":\"" + message + "\"}";
        	byte[] message_bytes = System.Text.Encoding.UTF8.GetBytes(message);
        	ctx.Response.OutputStream.Write(message_bytes,0,message_bytes.Length);
        }
        
        private void sendResponse(HttpListenerContext ctx, string message) {
        	byte[] message_bytes = System.Text.Encoding.UTF8.GetBytes(message);
        	ctx.Response.OutputStream.Write(message_bytes,0,message_bytes.Length);
        }
    }
}
