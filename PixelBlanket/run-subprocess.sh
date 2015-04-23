#!/bin/bash

$1 0<&0 &
APP_PID=$!

while [ 1 == 1 ]; do
    ps $PPID > /dev/null
    if [ "$?" != "0" ]; then
        kill -9 $APP_PID
        exit
    fi
    sleep 5
done
