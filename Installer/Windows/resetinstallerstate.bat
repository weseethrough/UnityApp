@echo off
reg delete "HKEY_CURRENT_USER\Software\Race Yourself" || goto :error
exit 0
:error
pause
exit 1