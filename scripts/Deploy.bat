@echo off
cls
Title Deploying MediaPortal mvCentral (RELEASE)
cd ..

if "%programfiles(x86)%XXX"=="XXX" goto 32BIT
	:: 64-bit
	set PROGS=%programfiles(x86)%
	goto CONT
:32BIT
	set PROGS=%ProgramFiles%
:CONT

copy /y "mvCentral\bin\Release\mvCentral.dll" "%PROGS%\Team MediaPortal\MediaPortal\plugins\Windows\"

cd scripts