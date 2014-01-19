# Run In Tray

## Overview

Have you ever wanted to run a Windows console application in the
background without the intrusive console window? Probably not.
But I did, so I wrote this application. It's very simple, poorly tested,
and probably doesn't work.

Put simply, it runs console application in the system tray. The tray
menu lets you kill applications, and view their stdout & stderr. Output
is also redirected to log files.

## Launching Applications

Apps can be launched interactively or from the command line.

To launch interactively, right click on the tray icon and select 'Run App'.
Then navigate to the executable/script to launch. Command line arguments
are not supported when launching interactively.

To launch on the command line, simply add the command to run to the end
of the RunInTray.exe command line. All arguments are passed on to the
launched process. If you run multiple apps this way, they are consolidated
under one instance of RunInTray, so only one icon is shown in the tray.

## Controlling Apps

Each running app has its own sub-menu in the main tray menu. From here
you can kill the process (graceful exit is attempted via CTRL+C, but if
this fails the process is forcefully killed), and view its output.

The tray menu also has options for killing all running processes, and
exiting the application (which also kills all running processes).