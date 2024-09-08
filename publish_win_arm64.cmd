@echo off

msbuild /m /t:restore,tospkg:publish,tospkgffu:publish,tospkgvhdx:publish,tocbs:publish,tocbsffunopool:publish,tocbsvhdxnopool:publish /p:Platform=arm64 /p:RuntimeIdentifier=win-arm64 /p:PublishDir="%CD%\publish\artifacts\win-arm64\CLI" /p:PublishSingleFile=true /p:PublishTrimmed=false /p:Configuration=Release MobilePackageGen.sln