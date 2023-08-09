
<h1 align="center">
    MadeNPlay editor
</h1> 


<p align="center">
    Editor for games based on the madenplay platform. MadeNPlay is a platform for creating multiplayer games in unity.
</p>

<p align="center">
    <a href="./LICENSE">
        <img src="https://img.shields.io/github/license/Edward-Khaymanov/MadeNPlay-editor?label=license&style=for-the-badge" /> 
    </a>
</p>

## Requirements

- Netcode for GameObjects 1.4.0
- com.unity.nuget.newtonsoft-json


## Getting started

- place the MadeNPlayEditor folder in your assets folder
- click `Window/MadeNPlay/Setup`
- create a script with the name `GameHandler` and the following method

```c#

public void SetupGame(LobbyData lobbyData)
{

}

```

- add a script to a new gameobject and make a prefab out of it
- use it as the starting point of your game


## Export

To export you need:

- take all your network objects and add it to addressables with the label specified in `CONSTANTS.NETWORK_PREFAB_LABEL`
- add your prefab with the `GameHandler` script to addressables as well, but only with the label specified in `CONSTANTS.GAME_HANDLER_LABEL`
- build your project 


Click `Window/External Resource Loader/Show`

- Click build 
- Click copy dll and in the menu that opens, go to `buildFolderName/projectName_Data/Managed/youProjectDll` (default Assembly-CSharp)
- Open resources folder and check that your dll and addressable data folder are in place

Click `Window/Game creator`

- in the window that opens, fill in all the fields
- to create teams, click `Create/MadeNPlay/New Team` in Project explorer- â ïîëå `Teams` äîáàâüòå ýòè êîìàíäû
- in the `Build directory' field, specify the location of the addressables data folder
- in the `Dll path` field, specify the location of your dll


## Roadmap

- Custom file format
- Simplification of the game export stage
- Support netcode for gameobjects version above 1.4.0
- Support dedicated servers


## Related

[MadeNPlay client](https://github.com/Edward-Khaymanov/MadeNPlay-client)
