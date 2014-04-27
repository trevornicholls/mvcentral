@echo off
IF EXIST mvCentral_UNMERGED.dll del mvCentral_UNMERGED.dll
ren mvCentral.dll mvCentral_UNMERGED.dll
ilmerge /out:mvCentral.dll mvCentral_UNMERGED.dll nlog.dll cornerstone.dll /targetplatform:v4,"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0" /allowdup
del mvCentral_UNMERGED.dll
