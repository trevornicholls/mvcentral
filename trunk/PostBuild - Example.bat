if %2 == Debug goto debug

copy G:\Development\mvcentral\mvCentral\bin\Release\mvCentral.* D:\HTPC\MediaPortal\plugins\Windows /Y
copy G:\Development\mvcentral\Tester\bin\Release\ConfigTester.* D:\HTPC\MediaPortal\plugins\Windows /Y

goto endPostBuild

:debug

copy G:\Development\mvcentral\mvCentral\bin\Debug\mvCentral.* D:\HTPC\MediaPortal\plugins\Windows /Y
copy G:\Development\mvcentral\Tester\bin\Debug\ConfigTester.* D:\HTPC\MediaPortal\plugins\Windows /Y
:endPostBuild


