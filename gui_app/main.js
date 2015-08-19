var app = require('app');  // Module to control application life.
var BrowserWindow = require('browser-window');  // Module to create native browser window.

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is GCed.
var mainWindow = null;

// Quit when all windows are closed.
app.on('window-all-closed', function() {
  // On OS X it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform != 'darwin') {
    SaveAndQuit();
  }
});

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
app.on('ready', function() {

  //spawn the Core
  var spawn = require('child_process').spawn;
  var prc;
  if (require('fs').existsSync('NxtLite.exe'))
    prc = spawn('NxtLite.exe');
  else
    prc = spawn('NxtLite\\bin\\Release\\NxtLite.exe');

  //noinspection JSUnresolvedFunction
  prc.stdout.setEncoding('utf8');
  prc.stdout.on('data', function (data) {
      var str = data.toString()
      var lines = str.split(/(\r?\n)/g);
      console.log(lines.join(""));
  });

  prc.on('close', function (code) {
      console.log('process exit code ' + code);
      //NxtLite backend already closed, close Gui
      app.quit();
  });

  // Create the browser window.
  mainWindow = new BrowserWindow({width: 1450, height: 768, "node-integration": false, title: "NxtLite", icon: "icon32.png"});
  mainWindow.setMenu(null);

  // and load the index.html of the app.
  mainWindow.loadUrl('http://127.0.0.1:1234');

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

function SaveAndQuit() {
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
    app.quit();
  }).end();
}