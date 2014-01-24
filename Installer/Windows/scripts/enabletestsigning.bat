@echo off
bcdedit -set loadoptions DISABLE_INTEGRITY_CHECKS
bcdedit -set TESTSIGNING ON
bcdedit|findstr testsigning|findstr Yes > NUL || goto :error
exit 0
:error
echo error
exit 1