#!/bin/sh
mono "/Applications/Natural Docs/NaturalDocs.exe" "/Volumes/G-Drive/Projekte_Unity/NaturalDocs5.6/Assets/NaturalDocs/Editor/Config" -r
kill -9 $(ps -p $(ps -p $PPID -o ppid=) -o ppid=)
