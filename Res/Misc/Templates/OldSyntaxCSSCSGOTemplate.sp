#pragma semicolon 1

#define DEBUG

#define PLUGIN_AUTHOR ""
#define PLUGIN_VERSION "0.00"

#include <sourcemod>
#include <sdktools>
#include <cstrike>
//#include <sdkhooks>

enum CS_GameMod
{
	Game_Css,
	Game_Csgo,
	Game_Indeterminate,
}
CS_GameMod g_Game = Game_Indeterminate;

public Plugin:myinfo = 
{
	name = "",
	author = PLUGIN_AUTHOR,
	description = "",
	version = PLUGIN_VERSION,
	url = ""
};

public OnPluginStart()
{
	new String:gameName[MAX_NAME_LENGTH]; //name length is enough...
	GetGameFolderName(gameName, MAX_NAME_LENGTH);
	if (StrEqual(gameName, "cstrike", false)) { g_Game = Game_Css; }
	else if (StrEqual(gameName, "csgo", false)) { g_Game = Game_Csgo; }
}
