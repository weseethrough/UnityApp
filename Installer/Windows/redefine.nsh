!ifmacrondef _Define
    !ifndef Define
        !define Define `!insertmacro _Define`
        !macro _Define _Name _Value 
            !ifdef ${_Name}
                !if `${${_Name}}` == ``
                    !undef ${_Name}
                    !define ${_Name} `${_Value}`
                !else if `${${_Name}}` == `!insertmacro _Define ${_Name}`
                    !undef ${_Name}
                    !define ${_Name} `${_Value}`
                !endif
            !else
                !define ${_Name} `${_Value}`
            !endif
        !macroend
    !endif
!endif
 
!ifmacrondef _ReDefine
    !ifndef ReDefine
        !define ReDefine `!insertmacro _ReDefine`
        !macro _ReDefine _Name _Value 
            !ifdef ${_Name}
                !undef ${_Name}
            !endif
            !insertmacro _Define ${_Name} `${_Value}`
        !macroend
    !endif
!endif
 
!ifndef DefineOnce
    !define DefineOnce `!insertmacro _DefineOnce`
    !macro _DefineOnce _Name
        ${Define} ${_Name} `${Define} ${_Name}`
    !macroend
!endif
 
!ifndef ReDefineOnce
    !define ReDefineOnce `!insertmacro _ReDefineOnce`
    !macro _ReDefineOnce _Name
        ${ReDefine} ${_Name} `${ReDefine} ${_Name}`
    !macroend
!endif