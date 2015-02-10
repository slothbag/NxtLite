using System;
using Eto.Forms;
using Eto.Drawing;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace NxtLite 
{
	
	class MyForm : Form
	{
		
		public Delegate CoreShutDownMethod;
		
		public static bool firstload = true;
		
	    public MyForm()
	    {
	        // sets the client (inner) size of the window for your content
	        //this.ClientSize = new Eto.Drawing.Size(1280, 768);
	        this.ClientSize = new Eto.Drawing.Size(1200,600);
			
	        if (Eto.EtoEnvironment.Platform.IsWindows && !this.IsRunningOnMono())
			{
				this.SetIconUnderWindows();
	        }
			
	        this.Title = "NxtLite";
	        
	        this.Closing += e_OnFormClosing;

	        if (Eto.EtoEnvironment.Platform.IsMac)
	        	this.Menu = new MenuBar();
	       
	        WebView wv = new WebView();
	        
	        //disable IE context menu
	        wv.BrowserContextMenuEnabled = false;
	 
	        //intercept external links before loading a new page
	        wv.DocumentLoading += e_OnDocumentLoading;
	        
	        wv.DocumentLoaded += e_OnDocumentLoaded;
	        
	        var layout = new DynamicLayout();
	        layout.Padding = new Padding(0);
	        layout.BeginHorizontal();
	        layout.Add(wv, xscale: true);
	        layout.EndHorizontal();
	        Content = layout;
	        
	        wv.Url = new Uri("http://localhost:1234");
	    }
	    
	    static void e_OnFormClosing(object sender, object e) {
	    	MyForm me = (MyForm)sender;
	    	if (me.CoreShutDownMethod != null)
	    		me.CoreShutDownMethod.DynamicInvoke();
	    }
	    
	   	static void e_OnDocumentLoading(object sender, WebViewLoadingEventArgs e)
	    {
	    	WebView wv = (WebView)sender;
	    	
	    	if (e.Uri.Host == "") {}
	    	else if (e.Uri.Host == "ieframe.dll") {
	    		e.Cancel = true;
	    		wv.LoadHtml(new MemoryStream(Encoding.UTF8.GetBytes("<html><body>Could not reach NxtLite core</body></html>")));
	    	}
	    	else if (e.Uri.Host != "127.0.0.1" && e.Uri.Host != "localhost") {
	    		e.Cancel = true;
	    		Application.Instance.Open(e.Uri.ToString());
	    	}
		}
	   	
	   	static void e_OnDocumentLoaded(object sender, WebViewLoadedEventArgs e) {
			//WebView wv = (WebView)sender;
	   	}
	   	
	   	private void SetIconUnderWindows()
		{
	   		System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().Location);			
			object formhandler = this.Handler.GetType().GetProperty("Control").GetValue(this.Handler, null);
			formhandler.GetType().GetProperty("Icon").SetValue(formhandler, icon, null);
		}
	   	
	   	public bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}
	   	
	    public void ShutDown() {
	   		Application.Instance.Invoke(Application.Instance.Quit);
	   	}
	}

	internal sealed class Program
	{
		public delegate void NoParamMethod();
		
		[STAThread]
		private static void Main(string[] args)
		{
			//create the app config directory
			if (!System.IO.Directory.Exists(Utils.getAppDirectory)) {
				System.IO.Directory.CreateDirectory(Utils.getAppDirectory);
			}
			else {
				//try load existing settings
				Nodes.LoadFromFile();
				Nodes.ScanLatestBlockHeight();
			}
			
			//manually enable localStorage for osx
			Eto.Style.Add<Eto.Mac.Forms.Controls.WebViewHandler>(null, h =>
			{
				var webView = h.Control;
				var path = new MonoMac.Foundation.NSString("~/.config/NxtLite");
				MonoMac.ObjCRuntime.Messaging.void_objc_msgSend_IntPtr(webView.Preferences.Handle, MonoMac.ObjCRuntime.Selector.GetHandle("_setLocalStorageDatabasePath:"), path.Handle);
				
				MonoMac.ObjCRuntime.Messaging.void_objc_msgSend_bool(webView.Preferences.Handle, MonoMac.ObjCRuntime.Selector.GetHandle("setLocalStorageEnabled:"), true);
				MonoMac.ObjCRuntime.Messaging.void_objc_msgSend_bool(webView.Preferences.Handle, MonoMac.ObjCRuntime.Selector.GetHandle("setDatabasesEnabled:"), true);
				
				h.Control.UIDelegate = new MyUIDelegate { Handler = h };
			});
			
			var app = new Application();
			var myForm = new MyForm();
			
			//start the Core
			var core = new WebServer.WebServer();
			core.Run();
			
			//set up app closing event
			app.Terminating += e_OnAppTerminating;
						
			//Then start the Gui which will connect to it
			app.Run(myForm);
		}
		
		static void e_OnAppTerminating(object sender, object e) {
			Nodes.SaveToDisk();
		}
	}
	
	public class MyUIDelegate : Eto.Mac.Forms.Controls.WebViewHandler.UIDelegate
	{
		[MonoMac.Foundation.Export("webView:frame:exceededDatabaseQuotaForSecurityOrigin:database:")]
		public void WebView (MonoMac.WebKit.WebView WebView, MonoMac.WebKit.WebFrame frame, IntPtr origin, MonoMac.Foundation.NSString databaseIdentifier)
		{
			MonoMac.Foundation.NSNumber quota = new MonoMac.Foundation.NSNumber(5 * 1024 * 1024);
			MonoMac.ObjCRuntime.Messaging.void_objc_msgSend_IntPtr(origin, MonoMac.ObjCRuntime.Selector.GetHandle("setQuota:"), quota.Handle);
		}
	}
}


