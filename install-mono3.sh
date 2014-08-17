#!/bin/bash

# get build tools
sudo apt-get install git autoconf automake libtool g++ gettext mono-gmcs
# get dev libs
sudo apt-get install libglib2.0-dev libpng12-dev libx11-dev
sudo apt-get install libfreetype6-dev libfontconfig1-dev
sudo apt-get install libtiff5-dev libjpeg8-dev libgif-dev libexif-dev

# clone mono repos
git clone git://github.com/mono/mono.git
git clone git://github.com/mono/libgdiplus.git3

# compile and install lidgdiplus
cd libgdiplus
./autogen.sh --prefix=/usr/local
make
sudo make install

# compile mono
cd ../mono

# get mono submodules
git submodule init
git submodule update --recursive

./autogen.sh --prefix=/usr/local
make get-monolite-latest
make EXTERNAL_MCS=${PWD}/mcs/class/lib/monolite/gmcs.exe

# finally install mono
sudo make install
