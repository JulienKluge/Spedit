/*
**
*/
#if defined _INTERFACE_included
 #endinput
#endif
#define _INTERFACE_included



/*
public __pl_INTERFACE_SetNTVOptional()
{
	MarkNativeAsOptional("");
}
*/

public SharedPlugin __pl_INTERFACE =
{
	name = "",
	file = ".smx",
	#if defined REQUIRE_PLUGIN
	required = 1,
	#else
	required = 0,
	#endif
};
