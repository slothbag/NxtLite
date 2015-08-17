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
    app.quit();
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

  //noinspection JSUnresolvedFunction
  prc.stdout.setEncoding('utf8');
  prc.stdout.on('data', function (data) {
      var str = data.toString()
      var lines = str.split(/(\r?\n)/g);
      console.log(lines.join(""));
  });

  prc.on('close', function (code) {
      console.log('process exit code ' + code);
      app.quit();
  });

  // Create the browser window.
  mainWindow = new BrowserWindow({width: 1360, height: 768, "node-integration": false, title: "DApp Store"});

  // and load the index.html of the app.
  mainWindow.loadUrl('http://127.0.0.1:1234');

  // Emitted when the window is closed.
  mainWindow.on('closed', function() {
    // Dereference the window object, usually you would store windows
    // in an array if your app supports multi windows, this is the time
    // when you should delete the corresponding element.
    mainWindow = null;
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