# pixelblanket
raspberry pi tv browser (photos, etc) in f#

## Functions

* display one photo per minute from the /home/pi/Pictures folder. Photos
are expected to be jpg files with a resolution of 1920 x 1080. At the
beginning of the minute, the current time is displayed in the corner
of the screen.
Any user or data input during the last 25 seconds of the minute period will
suppress moving to the next photo
* display clock and ring chimes on the quarter hour
* respond to left, right, and rewind buttons on the remote control
(requires a cec-enabled tv). Left and right change the photo by one
position, rewind starts back at the beginning
* runs on raspbian/pi and wheezy/linux with photos centered on
non-hdmi 1080 screen resolutions and the with left/right/home keypad keys
mapping to left/right/rewind functions

## Operating Instructions
```
cd PixelBlanket
./run.sh
```

## Development
I have not succeeded in making an f# development environment on the pi,
so I use MonoDevelop on wheezy/amd64 and copy the files to the pi. This repo
has captured bin/Debug so that it can be cloned to the pi and run without
building.
