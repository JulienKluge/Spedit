/*
**
*/
#if defined _INTERFACE_included
 #endinput
#endif
#define _INTERFACE_included



/*
#if !defined REQUIRE_PLUGIN
public __pl_INTERFACE_SetNTVOptional()
{
	MarkNativeAsOptional("");
}
#endif
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
