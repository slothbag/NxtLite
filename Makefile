# Dependancies
# ============
# mono
# genisoimage
# wine
# inno setup (iscc.exe)
# dmg (libdmg-hfsplus)
# wget, unzip

electron_version = v0.31.0
PROJECT = NxtLite
VERSION = 0.12
ISCC = "inno/ISCC.exe"

none:

#Build C# Project
#================
nxtlite:
	xbuild /p:Configuration=Release NxtLite.sln
	
#Download Electron for desired platforms
#=======================================
getelectron:
	mkdir -p electron
	#wget -P electron/ https://github.com/atom/electron/releases/download/$(electron_version)/electron-$(electron_version)-linux-ia32.zip
	wget -P electron/ https://github.com/atom/electron/releases/download/$(electron_version)/electron-$(electron_version)-win32-ia32.zip
	wget -P electron/ https://github.com/atom/electron/releases/download/$(electron_version)/electron-$(electron_version)-darwin-x64.zip
	#unzip -d electron/linux/ electron/electron-$(electron_version)-linux-ia32.zip
	unzip -d electron/win32/ electron/electron-$(electron_version)-win32-ia32.zip
	unzip -d electron/darwin/ electron/electron-$(electron_version)-darwin-x64.zip
	wget -P electron/ https://github.com/atom/electron/releases/download/$(electron_version)/electron-$(electron_version)-linux-x64.zip
	unzip -d electron/linux/ electron/electron-$(electron_version)-linux-x64.zip

#Build libdmg-hfsplus
#====================
getdmg:
	git clone https://github.com/hamstergene/libdmg-hfsplus
	cd libdmg-hfsplus && cmake . && make
	libdmg-hfsplus/dmg/dmg --help

#Create OSX dmg package
#======================
darwin:
	rm build/darwin -rf
	mkdir -p build/darwin/Electron.app/Contents/Resources/core/assets
	cp electron/darwin/* build/darwin -R
	rm build/darwin/LICENSE
	rm build/darwin/version
	cp Info.plist build/darwin/Electron.app/Contents
	cp nxtlite.icns build/darwin/Electron.app/Contents/Resources
	rm build/darwin/Electron.app/Contents/Resources/default_app -rf
	mkdir build/darwin/Electron.app/Contents/Resources/app
	cp gui_app/* build/darwin/Electron.app/Contents/Resources/app
	cp NxtLite/bin/Release/* build/darwin/Electron.app/Contents/Resources/core
	rm build/darwin/Electron.app/Contents/Resources/core/NxtLite.exe.config
	cp NxtLite/assets/* build/darwin/Electron.app/Contents/Resources/core/assets -R
	mv build/darwin/Electron.app build/darwin/NxtLite.app

	genisoimage -D -V "$(PROJECT) $(VERSION)" -no-pad -r -apple -o build/$(PROJECT)-$(VERSION)-uncompressed.dmg build/darwin/
	libdmg-hfsplus/dmg/dmg dmg build/$(PROJECT)-$(VERSION)-uncompressed.dmg build/$(PROJECT)-$(VERSION).dmg
	rm build/$(PROJECT)-$(VERSION)-uncompressed.dmg

#Create Windows Inno Setup package
#=================================
win32:
	rm build/win32 -rf
	mkdir -p build/win32/core/assets
	cp electron/win32/* build/win32/ -R
	mv build/win32/electron.exe build/win32/nxtlite.exe
	cp NxtLite/bin/Release/* build/win32/core
	rm build/win32/core/NxtLite.exe.config
	cp NxtLite/assets/* build/win32/core/assets -R
	rm build/win32/resources/default_app -rf
	mkdir build/win32/resources/app
	cp gui_app/* build/win32/resources/app
	cp NxtLite/nxtlite.ico build/win32/core
	cp icon32.png build/win32/core
	wine $(ISCC) /Obuild /F$(PROJECT)-$(VERSION)_setup NxtLite.iss

#Create Linux tar.gz
#===================
linux:
	rm build/linux -rf
	mkdir -p build/linux/core/assets
	cp electron/linux/* build/linux/ -R
	mv build/linux/electron build/linux/nxtlite
	cp NxtLite/bin/Release/* build/linux/core
	rm build/linux/core/NxtLite.exe.config
	cp NxtLite/assets/* build/linux/core/assets -R
	rm build/linux/resources/default_app -rf
	mkdir build/linux/resources/app
	cp gui_app/* build/linux/resources/app
	cp icon32.png build/linux/core
	tar -cvjf build/$(PROJECT)-$(VERSION).tar.bz2 --transform s/linux/$(PROJECT)-$(VERSION)/ -C build linux/ 

all: nxtlite darwin win32 linux

clean:
	rm NxtLite/bin -rf
	rm NxtLite/obj -rf
	rm build/ -rf

cleanelectron:
	rm electron/ -rf
