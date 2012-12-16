@echo off
IF EXIST mvCentral_UNMERGED.dll del mvCentral_UNMERGED.dll
ren mvCentral.dll mvCentral_UNMERGED.dll
ilmerge /out:mvCentral.dll mvCentral_UNMERGED.dll nlog.dll cornerstone.dll Lucene.Net.dll
