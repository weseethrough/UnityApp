@echo off
driverquery|findstr /c:"Android USB Driver" > NUL || goto :error
exit 0
:error
exit 1