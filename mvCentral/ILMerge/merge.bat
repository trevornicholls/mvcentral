@echo off

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
    :: 64-bit
    set PROGS=%programfiles(x86)%
    goto CONT
:32BIT
    set PROGS=%ProgramFiles%
:CONT

if exist mvCentral_UNMERGED.dll del mvCentral_UNMERGED.dll
ren mvCentral.dll mvCentral_UNMERGED.dll 
REM ilmerge.exe /out:mvCentral.dll mvCentral_UNMERGED.dll Nlog.dll cornerstone.dll /target:dll /targetplatform:"v4,%PROGS%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /wildcards
ilmerge.exe /out:mvCentral.dll mvCentral_UNMERGED.dll Nlog.dll cornerstone.dll /targetplatform:v4,"%PROGS%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /allowdup
del mvCentral_UNMERGED.dll
