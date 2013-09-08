SoundCloudFS
============

A Dokan FUSE system that mounts your soundcloud stream

# Operation #

To have this work you are going to need to have Dokan installed. Dokan is a driver set for windows that allows [FUSE](http://fuse.sourceforge.net/ "FUSE") like operation on windows systems.

In addition you will need to get a soundcloud API
key and info. You can get this here:

<http://soundcloud.com/you/apps/new>

You will then need to make a file in the root of you project
(make sure that it is always put into the same directory that your executable is running)

This file needs to be called `api_info.txt` and needs to contain the 4 following

```
clientid
client secret
your username
your password
```

* * *


Once you have that you run the application and watch to see if a drive called "DOKAN" appears.

![Dokan][Dokan]
[Dokan]: http://i.imgur.com/sgGpBc8.png

going into that directory you will see a folder called "stream" this contains your soundcloud timeline.

![Dokan][FS]
[FS]: http://i.imgur.com/PB06GKr.png

# Warnings #

Doing this proabbly is against soundcloud's API TOS. I only made this as a POC.

It is also worth noting Dokan's COM system is not the most stable in the world. when using this tool please make sure you don't have anything importent unsaved. If used incorrectly Dokan can BSOD your entire computer.
