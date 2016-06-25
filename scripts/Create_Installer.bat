@echo off
cls
Title Creating MediaPortal mvCentral Installer

:: Check for modification
REM svn status ..\source | findstr "^M"
REM if ERRORLEVEL 1 (
REM    echo No modifications in source folder.
REM ) else (
REM    echo There are modifications in source folder. Aborting.
REM    pause
REM    exit 1
REM )

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
    :: 64-bit
    set PROGS=%programfiles(x86)%
    goto CONT
:32BIT
    set PROGS=%ProgramFiles%
:CONT

IF NOT EXIST "%PROGS%\Team MediaPortal\MediaPortal\" SET PROGS=C:

:: Get version from DLL
FOR /F "tokens=1-3" %%i IN ('Tools\sigcheck.exe "..\mvCentral\bin\Release\mvCentral.dll"') DO ( IF "%%i %%j"=="File version:" SET version=%%k )

:: trim version
SET version=%version:~0,-1%

:: Temp xmp2 file
copy ..\MPEI\mvCentral.xmp2 ..\MPEI\mvCentralTemp.xmp2

:: Sed "mvCentral-{VERSION}.xml" from xmp2 file
Tools\sed.exe -i "s/mvCentral-{VERSION}.xml/mvCentral-%version%.xml/g" ..\MPEI\mvCentralTemp.xmp2

:: Build MPE1
"%PROGS%\Team MediaPortal\MediaPortal\MPEMaker.exe" ..\MPEI\mvCentralTemp.xmp2 /B /V=%version% /UpdateXML

:: Cleanup
del ..\MPEI\mvCentralTemp.xmp2

:: Sed "mvCentral-{VERSION}.mpe1" from mvCentral.xml
Tools\sed.exe -i "s/mvCentral-{VERSION}.MPE1/mvCentral-%version%.mpe1/g" ..\MPEI\mvCentral-%version%.xml

:: Parse version (Might be needed in the futute)
FOR /F "tokens=1-4 delims=." %%i IN ("%version%") DO ( 
    SET major=%%i
    SET minor=%%j
    SET build=%%k
    SET revision=%%l
)

:: Rename MPE1
if exist "..\MPEI\Release\mvCentral-%major%.%minor%.%build%.%revision%.mpe1" del "..\MPEI\Release\mvCentral-%major%.%minor%.%build%.%revision%.mpe1"
rename ..\MPEI\Release\mvCentral-MAJOR.MINOR.BUILD.REVISION.mpe1 "mvCentral-%major%.%minor%.%build%.%revision%.mpe1"


