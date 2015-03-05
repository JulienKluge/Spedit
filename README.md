# Spedit
SPEdit - A lightweight sourcepawn editor.

Compiling instructions:
So fine you downloaded the project and it wont compile didn't it?
Then you thought: "well i should have read the readme before" - am I right so far? ^^ 
Well, lets go step to step to let you compile it (VS 2013 Desktop):
 - Save the SOLUTION as an .sln file.
 - re-open the solution (with the .sln file of course)
 - right click on the project (solution explorer) and klick on "Manage NuGet Packages..."
 - uninstall all packages
 - open the nuget console (TOOLS-->NuGet Package Manager-->Package Manager Console)
 - Type in following commands and execute them (one after another)
 Install-Package MahApps.Metro
 Install-Package AvalonDock
 Install-Package AvalonEdit
 - 7th: congratulations you can compile it now

The Editor is crashing right after execution, what did I wrong?
Nothing, there is a lack of files it needs to work. Copy them from the official builds or recreate them by hand...
