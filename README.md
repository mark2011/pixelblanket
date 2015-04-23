# pixelblanket
raspberry pi tv browser (photos, etc) in f#

## Functions

* display one photo per minute from the /home/pi/Pictures folder. Photos
are expected to be jpg files with a resolution of 1920 x 1080. At the
beginning of the minute, the current time is displayed in the corner
of the screen
* respond to left, right, and rewind buttons on the remote control
(requires a cec-enabled tv). Left and right change the photo by one
position, rewind starts back at the beginning

## Operating Instructions
cd PixelBlanket
./run.sh

## Development
I have not succeeded in making an f# development environment on the pi,
so I use MonoDevelop on wheezy/amd64 and copy the files to the pi. This repo
has captured bin/Debug so that it can be cloned to the pi and run without
building.
