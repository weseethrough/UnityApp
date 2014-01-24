@echo off
bcdedit|findstr testsigning|findstr Yes > NUL || goto :error
exit 0
:error
exit 1