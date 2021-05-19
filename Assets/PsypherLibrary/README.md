# Support Library V0.1

**Authors** 
 - Kashyap Singha Phukan
 - Tsewang Pintso

---

- *This is a custom support library for unity projects developed in Psypher Interactive.*
- *This library contains many extension functions of commonly used classes and data types. Details to be added later.*
- *The dependencies for this library are mentioned bellow*
	- DoTween
	- Better Streaming Assets
	- JsonDotNet
	- Unity UI Extension
	- SharpZip 
	- InternetReachabilityVerifier
	- Stan's Ultimate Mobile Plugin [*Not included. Please install before importing this library*]

Above mentioned libraries belongs to their respective owner, we have not altered anything and are only used in few of our classes.  
[Thanks for making our life a little easier]

---
## How to Add this library to unity projects

- You can copy the entire PsypherLibrary inside your Assets folder but we suggest making this repo as submodule to your project repo. So that you will be always updated with the latest codes.
- Relative path should be always - Assets/PsypherLibrary [*Namespace dependencies*]
- Repo link: https://gitlab.com/psypher-interactive/psypher-rnd/psypherlibrary

## How to Edit/Update this Support Library

- Create an empty Unity project.
- Clone this repo inside the Assets Folder.
- Compile once and Start Editing.
---

## Category System

Category system is a JSON based data driven UI system. It can dynamically fill contents in your UIs based on your supplied JSONs.
- To Enable, add "ENABLE_CATEGORY" to the [Scripting Define Symbols*] field in project settings.

## Purchase Manager `[Todo]`

- Activates the unity's in-app purchase services
- To Enable, add "ENABLE_PURCHASE" to the [Scripting Define Symbols*] field in project settings. `[Todo]`

## Native Utilities

- Native Popup
- Local Notification Helper
- GPGS Helpers
    - GPlayLoginHelper
    - GPlayAchievementHelper
    - GPlayLeaderboardHelper
- GameCenter Helpers
    - GCenterLoginHelper
    - GCenterAchievementHelper
    - GCenterLeaderboardHelper

**How to Add Native Helper Utilities**
- Add stan's Ultimate Mobile plugin. link: https://assetstore.unity.com/packages/tools/integration/ultimate-mobile-pro-130345
- To Enable, add "ENABLE_NUTILS" to the [Scripting Define Symbols*] field in project settings.
- To Enable Game Center, add "ENABLE_NGCENTER" to the [Scripting Define Symbols*] field in project settings.
- To Enable Google play game services, add "ENABLE_NGPGS" to the [Scripting Define Symbols*] field in project settings.

## UI Helper Scripts

UI Helper classes helps in creating and managing UI Panels.

**Defined Panels**

These panels are pre-defined and can be used to make life little more easier. They have respective base prefab associated with them which are stored in the *Resources* folder of this Psypher Library (not to be confused with Asset->Resources).

- UILoader
- UISceneLoader
- UISplashLoader
- UIToastNotification
- UIPopupBox

These pre-defined panels can be override with new assets depending on the working project. The new overriden panels should be placed in the first scene [*or the scene where it is intended to use for the first time*] for the system to pick the updated ones. Use the given prefabs to create a copy and then edit/update as needed.

## Ads System `[WIP]`

This ad system is a middle man for the app and any 3rd party ads provider sdk. This system also have demo ads, which can be used during development time. Simply use the DemoAdsInitializer script from the demo folder [listed below*]

**Features**

- Ad manager
    - Multiple Ads sdk support
    - Request Ads
    - Show ads
    - Rewards Ads
    
- Demo [Mock] Ads
    - Supports all ads type, use it during development before any ad SDK is integrated. This gives the game designer to preview how ads are placed and impacting user engagement beforehand. *All the related demo code are inside [PsypherLibray] > [SupportLibrary] > [MetaGameSystems] > [AdsSystem] > [DemoAdsSystem]

## Analytics System `[Todo]`

This system helps to keep the statistical data. Before any 3rd party analytical systems are added, using this system can store important gameplay data to local files during development. 
    
