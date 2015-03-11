!include "MUI2.nsh"
!include "DotNetChecker.nsh"
!include "FileAssociation.nsh"

Name "SPEdit"
OutFile "SPEdit Installer.exe"

InstallDir $APPDATA\spedit

RequestExecutionLevel admin

!define SHCNE_ASSOCCHANGED 0x8000000
!define SHCNF_IDLIST 0

!define MUI_ABORTWARNING
!define MUI_ICON "icon.ico"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"




Section "Program" prog01
SectionIn 1 RO
SetOutPath $INSTDIR

!insertmacro CheckNetFramework 45

File Spedit.exe
File MahApps.Metro.dll
File ICSharpCode.AvalonEdit.dll
File System.Windows.Interactivity.dll
File Xceed.Wpf.AvalonDock.dll
File Xceed.Wpf.AvalonDock.Themes.Metro.dll
File GPLv3.txt

IfFileExists $INSTDIR\options_0.dat OptionsExist OptionsDoesNotExist
OptionsExist:
Delete $INSTDIR\options_0.dat
OptionsDoesNotExist:

CreateDirectory "$INSTDIR\sourcepawn"
CreateDirectory "$INSTDIR\sourcepawn\errorfiles"
CreateDirectory "$INSTDIR\sourcepawn\scripts"
CreateDirectory "$INSTDIR\sourcepawn\temp"
CreateDirectory "$INSTDIR\sourcepawn\templates"
CreateDirectory "$INSTDIR\sourcepawn\configs"
CreateDirectory "$INSTDIR\sourcepawn\configs\sm_one_seven"
CreateDirectory "$INSTDIR\sourcepawn\configs\sm_one_six"

File /r ".\sourcepawn"

WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit" "DisplayName" "SPEdit - A lightweight sourcepawn editor"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit" "UninstallString" "$INSTDIR\uninstall.exe"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit" "InstallLocation" "$INSTDIR"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit" "DisplayIcon" "$INSTDIR\Spedit.exe"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit" "Publisher" "Julien Kluge"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit" "DisplayVersion" "0.40.0-beta"
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit" "NoModify" 1
WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit" "NoRepair" 1

WriteUninstaller $INSTDIR\uninstall.exe
SectionEnd




Section "File Association (.sp)" prog02
SectionIn 1
${registerExtension} "$INSTDIR\Spedit.exe" ".sp" "Sourcepawn Script"
System::Call 'Shell32::SHChangeNotify(i ${SHCNE_ASSOCCHANGED}, i ${SHCNF_IDLIST}, i 0, i 0)'
SectionEnd




Section "File Association (.inc)" prog03
SectionIn 1
${registerExtension} "$INSTDIR\Spedit.exe" ".inc" "Sourcepawn Include-File"
System::Call 'Shell32::SHChangeNotify(i ${SHCNE_ASSOCCHANGED}, i ${SHCNF_IDLIST}, i 0, i 0)'
SectionEnd



Section "Desktop Shortcut" prog04
SectionIn 1
CreateShortCut "$DESKTOP\SPEdit.lnk" "$INSTDIR\Spedit.exe" ""
SectionEnd



Section "Uninstall"

Delete $INSTDIR\uninstall.exe

Delete $INSTDIR\Spedit.exe
Delete $INSTDIR\MahApps.Metro.dll
Delete $INSTDIR\ICSharpCode.AvalonEdit.dll
Delete $INSTDIR\System.Windows.Interactivity.dll
Delete $INSTDIR\Xceed.Wpf.AvalonDock.dll
Delete $INSTDIR\Xceed.Wpf.AvalonDock.Themes.Metro.dll
Delete $INSTDIR\GPLv3.txt
Delete $INSTDIR\*.dat
RMDir /r $INSTDIR\sourcepawn
RMDir $INSTDIR

DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\spedit"

${unregisterExtension} ".sp" "Sourcepawn Script"
${unregisterExtension} ".inc" "Sourcepawn Include-File"
System::Call 'Shell32::SHChangeNotify(i ${SHCNE_ASSOCCHANGED}, i ${SHCNF_IDLIST}, i 0, i 0)'
 
SectionEnd