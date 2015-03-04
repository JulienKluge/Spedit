/*
**
*/
#if defined _INTERFACE_included
 #endinput
#endif
#define _INTERFACE_included



public Extension __ext_INTERFACE = 
{
	name = "",
	file = ".ext",
#if defined AUTOLOAD_EXTENSIONS
	autoload = 1,
#else
	autoload = 0,
#endif
#if defined REQUIRE_EXTENSIONS
	required = 1,
#else
	required = 0,
#endif
};
