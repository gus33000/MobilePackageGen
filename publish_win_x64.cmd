@echo off

msbuild /m /t:restore,cbstoreg:publish,secwim2wim:publish,mobilepackagegen:publish /p:Platform=x64 /p:RuntimeIdentifier=win-x64 /p:PublishDir="%CD%\publish\artifacts\win-x64" /p:PublishSingleFile=true /p:PublishTrimmed=false /p:Configuration=Release /p:IncludeNativeLibrariesForSelfExtract=true MobilePackageGen.sln

mkdir %CD%\publish\artifacts\win-x64-symbols
move %CD%\publish\artifacts\win-x64\*.pdb %CD%\publish\artifacts\win-x64-symbols\