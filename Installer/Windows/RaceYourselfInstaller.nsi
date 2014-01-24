;Race Yourself beta installer
;Based off Welcome/Finish Page Example Script by Joost Verburg

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"
  !include "redefine.nsh"
  !include "getwinver.nsh"
  !include "x64.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Race Yourself Beta"
  OutFile "RaceYourselfBeta.exe"
  InstallDir $TEMP\RaceYourselfBeta

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !define MUI_PAGE_CUSTOMFUNCTION_PRE ResumingCheck
  !insertmacro MUI_PAGE_WELCOME
  !define MUI_PAGE_CUSTOMFUNCTION_PRE LicenseCheck
  !insertmacro MUI_PAGE_LICENSE "License.txt"
  !define MUI_PAGE_CUSTOMFUNCTION_PRE ResumingCheck
  ${ReDefine} MUI_LICENSEPAGE_TEXT_TOP "Changelog"
  ${ReDefine} MUI_LICENSEPAGE_TEXT_BOTTOM " "
  ${ReDefine} MUI_LICENSEPAGE_BUTTON "Install"
  ${ReDefine} MUI_PAGE_HEADER_TEXT  "Changelog"
  ${ReDefine} MUI_PAGE_HEADER_SUBTEXT "Notable changes in this version"
  !insertmacro MUI_PAGE_LICENSE "changelog.txt"
  !define MUI_FINISHPAGE_NOAUTOCLOSE
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
; Functions
  Function LicenseCheck
    ReadRegStr $0 HKCU "Software\Race Yourself\Installer" "License"
    ${If} $0 == "agreed"
      Abort
    ${EndIf}
    ReadRegStr $0 HKCU "Software\Race Yourself\Installer" "Resuming"
    ${If} $0 == "yes"
      Abort
    ${EndIf}
  FunctionEnd

  Function ResumingCheck
    ReadRegStr $0 HKCU "Software\Race Yourself\Installer" "Resuming"
    ${If} $0 == "yes"
      Abort
    ${EndIf}
  FunctionEnd

  Function .onRebootFailed
    MessageBox MB_OK|MB_ICONSTOP "Reboot failed. Please reboot manually." /SD IDOK
   FunctionEnd

;--------------------------------
; Instfiles sections
  Section "Main Section" Main
    WriteRegStr HKCU "Software\Race Yourself\Installer" "License" "agreed"
    DetailPrint "Decompressing files.."
    DeleteRegValue HKCU "Software\Race Yourself\Installer" "Resuming"

    SetDetailsPrint none
      RmDir /r $INSTDIR
      SetOutPath $INSTDIR
      File /r "adb"
      File "raceyourself.apk"
      File /r "devcon"
      File /r "dpinst"
      File /r "scripts"
    SetDetailsPrint both

    checkglass:
    DetailPrint "Looking for Google Glass device.."
    nsExec::Exec '$INSTDIR\scripts\hasusbglass.bat'
    Pop $0
    IntCmp $0 0 hasusbglass
    DetailPrint " Google Glass device not detected."
    MessageBox MB_OKCANCEL "Please plug your Google Glass into a USB port" IDOK checkglass
      Abort

    hasusbglass:
    DetailPrint "  Google Glass device detected."

    DetailPrint "Connecting to Google Glass device.."
    nsExec::Exec '$INSTDIR\scripts\hasadbglass.bat'
    Pop $0
    IntCmp $0 0 aok
    DetailPrint " Google Glass device is not available through adb."
 
    DetailPrint "Installing Google Glass USB drivers.."
    nsExec::Exec '$INSTDIR\scripts\hasdriver.bat'
    Pop $0
    IntCmp $0 0 reinstall
    goto install
    reinstall:
    DetailPrint "  Removing old drivers.."
    nsExec::Exec 'wmic path win32_systemdriver where name="Android USB Driver" call delete'
    nsExec::Exec 'wmic path win32_systemdriver where name="WinUsb" call delete'

    install:
    # If win8 and (right) driver not installed, reboot if test signing not enabled
    ${GetWindowsVersion} $R0
    DetailPrint "  OS: Windows $R0"
    StrCmp $R0 "8" windows8
    StrCmp $R0 "8.1" windows8
    goto installdriver
    windows8:
    ${If} ${RunningX64}
    ${DisableX64FSRedirection}
    ${Endif}
    nsExec::Exec '$INSTDIR\scripts\testsigningon.bat'
    Pop $0
    IntCmp $0 0 installdriver
    MessageBox MB_YESNO "Windows 8 and above requires a reboot to debug mode to install the Google Glass USB driver.$\nDo you want to reboot now?" IDYES reboot
      DetailPrint "Please install the Google Glass drivers manually and try again"
      MessageBox MB_OK "Please install the Google Glass drivers manually and try again" /SD IDOK
      Abort
    reboot:  
    nsExec::Exec '$INSTDIR\scripts\enabletestsigning.bat'
    Pop $0
    IntCmp $0 0 rebooting
    MessageBox MB_OK|MB_ICONSTOP "Could not enable driver test-signing!$\nPlease get in touch with Race Yourself support" /SD IDOK
    Abort
    rebooting:
    WriteRegStr HKCU "Software\Race Yourself\Installer" "Rebooting" "yes"
    WriteRegStr HKCU "Software\Race Yourself\Installer" "Resuming" "yes"
    WriteRegStr "HKLM" "SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" "Program Name" "$EXEPATH"
    DetailPrint "Rebooting.."
    Reboot

    installdriver:
    DetailPrint "  Waiting on driver installer.."
    SetDetailsPrint none
    ${If} ${RunningX64}
    ExecWait '$INSTDIR\dpinst\dpinst64.exe /path $INSTDIR\dpinst'
    ${Else}
    ExecWait '$INSTDIR\dpinst\dpinst.exe /path $INSTDIR\dpinst'
    ${EndIf}
    SetDetailsPrint both
    DetailPrint "  Driver installer completed."
    DetailPrint "Connecting to Google Glass device.."
    nsExec::Exec '$INSTDIR\scripts\hasadbglass.bat'
    Pop $0
    IntCmp $0 0 aok
    DetailPrint "  Could not contact device after driver install."
    MessageBox MB_OK|MB_ICONSTOP "Could not install Google Glass USB driver.$\nPlease install the Google Glass drivers manually and try again" /SD IDOK
    Abort

    aok:
    DetailPrint "  Connection to Google Glass device established."
    DetailPrint "Installing Race Yourself Beta to Google Glass device (may take a few minutes).."
    nsExec::Exec '$INSTDIR\scripts\installapk.bat'
    Pop $0
    IntCmp $0 0 apkinstalled
    DetailPrint "  Installation failed!"
    MessageBox MB_OK|MB_ICONSTOP "Installation of the Race Yourself Beta app failed!$\nPlease get in touch with Race Yourself support" /SD IDOK
    Abort
    apkinstalled:
    DetailPrint "Race Yourself Beta installed on Glass!"
    ReadRegStr $0 HKCU "Software\Race Yourself\Installer" "Rebooting"
    ${If} $0 == "yes"
      # Cleanup
      nsExec::Exec '$INSTDIR\scripts\disabletestsigning.bat'
      DeleteRegValue HKCU "Software\Race Yourself\Installer" "Rebooting"
      MessageBox MB_YESNO "We recommend that you reboot once more to disable the driver debug mode.$\nDo you want to reboot now?" IDNO noreboot
      Reboot
      noreboot:
    ${EndIf}

    done:    
# TODO re-enable integrity
  SectionEnd
