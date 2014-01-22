@echo off
%~dp0..\devcon\devcon.exe find USB* | findstr "Android" > NUL || goto :error
exit 0
:error
exit 1