using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

public class Utils {
	public static string getAppDirectory {
		get {
			string appdir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string boatnet_appdir = Path.Combine(appdir, "NxtLite");
			return boatnet_appdir;
		}
	}
	public static bool doRESTfullApi(string url, out string result) {
		WebRequest webRequest;
		WebResponse webResp;
		result = null;
		
		try {
			webRequest = WebRequest.Create(url);
			webResp = webRequest.GetResponse();
		
			//string responseText;
			using (var reader = new System.IO.StreamReader(webResp.GetResponseStream(), System.Text.Encoding.UTF8))
	        {
	            result = reader.ReadToEnd();
	        }
		}
		catch (Exception ex) {
			Console.WriteLine("Error in doRESTfullApi: " + ex.Message);
			return false;
		}
		
		return true;
	}
}
		
static class MyExtensions {
	public static byte[] ReadAllBytes(this BinaryReader reader) {
	    const int bufferSize = 4096;
	    using (var ms = new MemoryStream())
	    {
	        byte[] buffer = new byte[bufferSize];
	        int count;
	        while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
	            ms.Write(buffer, 0, count);
	        return ms.ToArray();
	    }
	}
	
	public static IDictionary<string, string> ToDictionary(this NameValueCollection col)
	{
	    IDictionary<string, string> dict = new Dictionary<string, string>();
	    foreach (var k in col.AllKeys)
	    { 
	        dict.Add(k, col[k]);
	    }  
	    return dict;
	}
}