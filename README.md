# LabExtended
[![Build](https://github.com/marchellc/LabExtended/actions/workflows/dotnet.yml/badge.svg)](https://github.com/marchellc/LabExtended/actions/workflows/dotnet.yml)

LabExtended is an unofficial extension for the [LabAPI](https://github.com/northwood-studios/LabAPI) modding framework for [SCP Secret Laboratory](https://scpslgame.com), developed by [Northwood Studios](https://github.com/northwood-studios).  
The sole reason for the existence of this project is the fact that LabAPI lacks a lot of functionality when compared to Exiled *(which is just bloated with random stuff)*, 
so we're trying to fix that by providing developers with clear and simple to use APIs.  

These APIs include, but are not limited to:
- Custom Ammo
- Custom Items *(including Firearms & Usable Items)*
- Custom Teams
- Custom Roles
- Voice Chat API *(with threaded voice message modifications!)*
- Hint API *(yes, another Hint API ..)*
- Settings API *(a simple Server-Specific-Settings wrapper)*
- Remote Admin API *(adds custom dummy actions and objects)*
- More events *(also some events which already exist in LabAPI, but lack* ***a lot*** *of properties)*
- A fully-custom Commands API  
.. and much more!

# Documentation
Every method in the project *should* be documented, but extended descriptions can be found in the [Wiki](https://github.com/marchellc/LabExtended/wiki)!  
If there's something still unclear, create an issue and I'll try to answer.

# Installation
Grab the assembly from the [Releases](https://github.com/marchellc/LabExtended/releases) page and put it in the server's plugin directory!  
**Do not remove the zero before the plugin name, it's required to be loaded first!**

## Dependencies
- [NVorbis](https://github.com/NVorbis/NVorbis)
  - Required only if you intend to use the audio API.
- [Harmony](https://github.com/pardeike/Harmony)
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
  - Should be a part of the server's dependencies.

# Contributing
I do not limit contributions at all, if you see something that may be wrong or needs an improvement, create an issue and explain it.  
Any request is welcome, no matter how stupid it may seem!
