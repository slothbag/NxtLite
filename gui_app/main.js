var app = require('app');  // Module to control application life.
var BrowserWindow = require('browser-window');  // Module to create native browser window.

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is GCed.
var mainWindow = null;
var prc = null;
var savenodes_done = false;
var coreshutdown_done = false;
var core_ready = false;

// Quit when all windows are closed.
app.on('window-all-closed', function() {
  // On OS X it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform != 'darwin') {
    app.quit();
  }
});

app.on('will-quit', function(e) {
    if (!savenodes_done) {
        e.preventDefault(); 
        SaveNodes();
        return;
    }
    if (!coreshutdown_done) {
        e.preventDefault(); 
        prc.kill();
        return;
    }
});

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
app.on('ready', function() {
  var path;
  if (require('fs').existsSync('NxtLite/bin/Debug/NxtLite.exe'))
    path = "NxtLite/bin/Debug/NxtLite.exe";
  else if (require('fs').existsSync('core/NxtLite.exe'))
    path = "core/NxtLite.exe";
  else if (require('fs').existsSync(__dirname + '/../core/NxtLite.exe'))
    path = __dirname + "/../core/NxtLite.exe";
  else if (require('fs').existsSync('NxtLite/bin/Release/NxtLite.exe'))
    path = "NxtLite/bin/Release/NxtLite.exe";

  var command;
  var args = [];

  if (require('os').platform() == 'win32')
    command = path;
  else {
    command = "mono";
    args.push(path);
  }

  //spawn the Core
  var spawn = require('child_process').spawn;
    prc = spawn(command, args);

  //noinspection JSUnresolvedFunction
  prc.stdout.setEncoding('utf8');
  prc.stdout.on('data', function (data) {
      var str = data.toString();

      if (str.substring(0, 23) == 'API server listening on')
        core_ready = true;

      var lines = str.split(/(\r?\n)/g);
      console.log(lines.join(""));
  });

  prc.on('close', function (code) {
      console.log('process exit code ' + code);
      //NxtLite backend already closed, close Gui
      coreshutdown_done = true;
      //cant save nodes now, flag it as done
      savenodes_done = true;
      app.quit();
  });

  // Create the browser window.
  mainWindow = new BrowserWindow({width: 1450, height: 768, "node-integration": false, title: "NxtLite", icon: "core/icon32.png" });
  mainWindow.setMenu(null);
  mainWindow.loadUrl('file://' + __dirname + '/init.html');
  //mainWindow.toggleDevTools();

  // and load the index.html of the app.
  LoadUrlIfDaemonReady();

  // Emitted when the window is closed.
  mainWindow.on('closed', function() {
    mainWindow = null;
  });

  mainWindow.on('page-title-updated', function(e) {
    e.preventDefault();
  });

  mainWindow.webContents.on('will-navigate', function(evnt, url) {
    if (url.indexOf("127.0.0.1:1234") >= 0) {
      //this is a local request, let it happen
    }
    else {
      //var newWindow = new BrowserWindow({width: 800, height: 600, "node-integration": false});
      //newWindow.loadUrl(url);
      require('shell').openExternal(url);
      evnt.preventDefault();
    }
  });
});

function LoadUrlIfDaemonReady() {
  if (core_ready)
    mainWindow.loadUrl('http://127.0.0.1:1234');
  else
    setTimeout(LoadUrlIfDaemonReady, 100);
}

function SaveNodes() {
  var http = require('http');

  var options = {
    host: '127.0.0.1',
    port: '1234',
    path: '/api/savenodes',
    method: 'POST',
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
      'Content-Length': 0
    }
  };

  http.request(options, function(res) {
    //got response from savenodes call, now exit gui (causing backend to close)
    console.log("Nodes Saved.")
    savenodes_done = true;
    app.quit();
  }).end();

}
