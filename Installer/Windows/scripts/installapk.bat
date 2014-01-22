@echo off
%~dp0..\adb\adb.exe install %~dp0..\raceyourself.apk
%~dp0..\adb\adb.exe kill-server >NUL
exit 0
:error
%~dp0..\adb\adb.exe kill-server >NUL
exit 1