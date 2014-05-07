@echo off
%~dp0..\adb\adb.exe kill-server >NUL
%~dp0..\adb\adb.exe -d shell getprop|findstr model|findstr Glass >NUL || goto :error
%~dp0..\adb\adb.exe kill-server >NUL
exit 0
:error
%~dp0..\adb\adb.exe kill-server >NUL
exit 1