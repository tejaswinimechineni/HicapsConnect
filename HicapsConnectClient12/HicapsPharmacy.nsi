;NSIS Modern User Interface
;Basic Example Script

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"
  !include "servicelib.nsh"
  !include "nsProcess.nsh"
;--------------------------------
;General
!define ProductVersion "1.0.3.32"
!define BuildVersion "1.0.3.32.X20"
!define InstallerVersion "1.0.3.32.X20.1"

  ;Name and file
  Name "Pharmacy for HICAPS Connect ${ProductVersion}"
LicenseForceSelection checkbox "I Accept"


VIAddVersionKey "ProductName" "Pharmacy for HICAPS Connect ${ProductVersion}"                                   
VIAddVersionKey  "Comments" "HICAPS Connect Pharmacy for PMS applications"                                          ;Default installation folder
VIAddVersionKey  "CompanyName" "HICAPS Pty Ltd."                
VIAddVersionKey  "LegalTrademarks" "HICAPS Connect is a trademark of HICAPS Pty Ltd."  
VIAddVersionKey  "LegalCopyright" "Copyright © 2009"                                    ;Get installation folder from registry if available
VIAddVersionKey  "FileDescription" "HICAPS Connect"        
VIAddVersionKey  "FileVersion" ${ProductVersion}                                             
VIProductVersion ${ProductVersion} 


BrandingText "HICAPS Connect Pharmacy ${ProductVersion}"

OutFile "HICAPSConnectPharmacyInstall_${InstallerVersion}.exe"

InstallDir "$PROGRAMFILES\HICAPSConnect"

InstallDirRegKey HKCU "Software\HicapsConnectPharmacy" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel user
  
 ; Attempt to give the UAC plug-in a user process and an admin process.
Function .OnInit
 
UAC_Elevate:
    UAC::RunElevated 
    StrCmp 1223 $0 UAC_ElevationAborted ; UAC dialog aborted by user?
    StrCmp 0 $0 0 UAC_Err ; Error?
    StrCmp 1 $1 0 UAC_Success ;Are we the real deal or just the wrapper?
    Quit
 
UAC_Err:
    MessageBox mb_iconstop "Unable to elevate, error $0"
    Abort
 
UAC_ElevationAborted:
    # elevation was aborted, run as normal?
    MessageBox mb_iconstop "This installer requires admin access, aborting!"
    Abort
 
UAC_Success:
    StrCmp 1 $3 +4 ;Admin?
    StrCmp 3 $1 0 UAC_ElevationAborted ;Try again?
    MessageBox mb_iconstop "This installer requires admin access, try again"
    goto UAC_Elevate 
 
FunctionEnd 
Function .OnInstFailed
    UAC::Unload ;Must call unload!
FunctionEnd
 
Function .OnInstSuccess
    UAC::Unload ;Must call unload!
FunctionEnd


;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "license.txt"
  ;!insertmacro MUI_PAGE_COMPONENTS
  ;!insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
   # These indented statements modify settings for MUI_PAGE_FINISH
  ;  !define MUI_FINISHPAGE_NOAUTOCLOSE
  ;  !define MUI_FINISHPAGE_RUN
  ; !define MUI_FINISHPAGE_RUN_CHECKED
   ; !define MUI_FINISHPAGE_RUN_TEXT "Start service now"
  ;  !define MUI_FINISHPAGE_RUN_FUNCTION "LaunchLink"
    ;    !define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
   ; !define MUI_FINISHPAGE_SHOWREADME $INSTDIR\LastMinutesNotes.txt

 !insertmacro MUI_PAGE_FINISH



  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
  Icon "HicapsConnectIcon6.ico"
  UninstallIcon "HicapsConnectIcon6.ico"


;Installer Sections
Section "Client Section" SecClient

Processes::KillProcess "HicapsConnectClient12.exe" $R0
Processes::KillProcess "HicapsConnectPharmacy.exe" $R0


   SetOutPath "$INSTDIR"
     File "c:\HicapsConnect\obfuscatedbin\HicapsConnectClient12\HICAPSConnectPharmacy.exe"
     File "c:\HicapsConnect\bin\System.Windows.Controls.Input.Toolkit.dll"
 	 File "c:\HicapsConnect\bin\System.Windows.Controls.Layout.Toolkit.dll"
	 File "c:\HicapsConnect\bin\WpfControls.dll"
	 File "c:\HicapsConnect\bin\WPFToolkit.dll"
     File "c:\HicapsConnect\bin\Microsoft.ReportViewer.*.dll"

  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\HICAPSConnectPharmacyUninstall.exe"


IfFileExists "$INSTDIR\HicapsConnectService.exe" +3 0
DetailPrint "HicapsConnect not installed, Aborting install"
Abort
	
SectionEnd

Section "Start Menu Shortcuts"
;File "c:\HicapsConnect\PharmacyBin\*.ico"

SetShellVarContext all
CreateDirectory "$SMPROGRAMS\HICAPSConnect"
Delete /REBOOTOK "$SMPROGRAMS\HICAPSConnect\HicapsConnectPharmacy.lnk"
CreateShortCut "$SMPROGRAMS\HICAPSConnect\HICAPS Connect Pharmacy.lnk" "$INSTDIR\HicapsConnectPharmacy.exe" "" "$INSTDIR\HicapsConnectPharmacy.exe" 0

; Uninstaller
CreateShortCut "$SMPROGRAMS\HICAPSConnect\HICAPS Connect Pharmacy Uninstaller.lnk" "$INSTDIR\HICAPSConnectPharmacyUninstall.exe" "" "$INSTDIR\HicapsConnectPharmacyUninstall.exe" 0

     
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\HicapsConnectPharmacy" \
                 "DisplayName" "HicapsConnectPharmacy :- Pharmacy client for HICAPS Connect"
WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\HicapsConnectPharmacy" \
                 "UninstallString" "$\"$INSTDIR\HicapsConnectPharmacyUninstall.exe$\""
SectionEnd

;--------------------------------
;Descriptions

  ;Language strings
  LangString DESC_SecClient ${LANG_ENGLISH} "This will install the HICAPS Connect Pharmacy"
  LangString DESC_SecServer ${LANG_ENGLISH} "HICAPS Connect Pharmacy for your practice"

  ;Assign language strings to sections
  !insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
    !insertmacro MUI_DESCRIPTION_TEXT ${SecClient} $(DESC_SecClient)
    !insertmacro MUI_DESCRIPTION_TEXT ${SecServer} $(DESC_SecServer)
  !insertmacro MUI_FUNCTION_DESCRIPTION_END
  

;--------------------------------
;Uninstaller Section


  
Section "Uninstall"

  ;ADD YOUR OWN FILES HERE...
  SetRebootFlag true
  
  Delete /REBOOTOK "$INSTDIR\HicapsConnectClient12.*"
  Delete /REBOOTOK "$INSTDIR\HicapsConnectPharmacy.*"
  Delete /REBOOTOK "$INSTDIR\System.Windows.Controls.Input.Toolkit.dll"
  Delete /REBOOTOK "$INSTDIR\System.Windows.Controls.Layout.Toolkit.dll"
  Delete /REBOOTOK "$INSTDIR\WpfControls.dll"
  Delete /REBOOTOK "$INSTDIR\WPFToolkit.dll"
  Delete /REBOOTOK "$INSTDIR\Microsoft.ReportViewer.*.dll"
  
  Delete /REBOOTOK "$SMPROGRAMS\HicapsConnect\HICAPS Connect Pharmacy.lnk"
  
IfRebootFlag 0 noreboot
  MessageBox MB_YESNO "A reboot is required to finish the un-installation. Do you wish to reboot now?" IDNO noreboot
    Reboot
noreboot:

SectionEnd