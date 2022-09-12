<div>

# UdonExplorer [![GitHub](https://img.shields.io/github/license/Varneon/UdonExplorer?color=blue&label=License&style=flat)](https://github.com/Varneon/UdonExplorer/blob/main/LICENSE) [![GitHub Repo stars](https://img.shields.io/github/stars/Varneon/UdonExplorer?style=flat&label=Stars)](https://github.com/Varneon/UdonExplorer/stargazers) [![GitHub all releases](https://img.shields.io/github/downloads/Varneon/UdonExplorer/total?color=blue&label=Downloads&style=flat)](https://github.com/Varneon/UdonExplorer/releases) [![GitHub tag (latest SemVer)](https://img.shields.io/github/v/tag/Varneon/UdonExplorer?color=blue&label=Release&sort=semver&style=flat)](https://github.com/Varneon/UdonExplorer/releases/latest)

</div>

### Unity Editor extension for easily exploring all UdonBehaviours in your Unity scene

---

Udon Explorer is an editor window that allows you to see all of the Udon behaviours in your scene at a glance and detailed information about them like their sync mode, execution order, propgram source, program source file size, etc. and the listing can be sorted based on any one of these columns.

By right clicking an UdonBehaviour you can really quickly access any of the following methods:
* Open Udon Graph *(Udon Graph Programs Only)*
* Open/Select U# Source C# Script *(UdonSharp Only)*
* Select Program Source
* Select Serialized Program Asset

![UdonExplorer](https://user-images.githubusercontent.com/26690821/162178484-05b12fdd-6c5e-4e3c-acbd-7e0b740584da.png)


---

# How to use UdonExplorer
## Prerequisites:
Make sure that the VRCSDK3 is already imported in your project:
* [VRCSDK3](https://vrchat.com/download/sdk3-worlds)

## Installation via Unity Package Manager (git):
1. Navigate to your toolbar: `Window` > `Package Manager` > `[+]` > `Add package from git URL...` and type in: `https://github.com/Varneon/UdonExplorer.git?path=/Packages/com.varneon.udonexplorer`
2. Open UdonExplorer by navigating to `Varneon` > `UdonExplorer` on your Unity editor's toolbar

## Installation via [VRChat Creator Companion](https://vcc.docs.vrchat.com/):
1. Download the the repository's .zip [here](https://github.com/Varneon/UdonExplorer/archive/refs/heads/main.zip)
2. Unpack the .zip somewhere
3. In VRChat Creator Companion, navigate to `Settings` > `User Packages` > `Add`
4. Navigate to the unpacked folder, `com.varneon.udonexplorer` and click `Select Folder`
5. `Udon Explorer` should now be visible under `Local User Packages` in the project view in VRChat Creator Companion

## Installation via Unitypackage:
1. Download latest UdonExplorer from [here](https://github.com/Varneon/UdonExplorer/releases/latest)
2. Import the downloaded .unitypackage into your Unity project
3. Open UdonExplorer by navigating to `Varneon` > `UdonExplorer` on your Unity editor's toolbar
