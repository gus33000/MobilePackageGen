@echo off

msbuild /m /t:restore,tocbs:publish,tocbsffunopool:publish,tocbsvhdxnopool:publish /p:Platform=x64 /p:RuntimeIdentifier=win-x64 /p:PublishDir="%CD%\publish\artifacts\win-x64\ToCBS" /p:PublishSingleFile=true /p:PublishTrimmed=false /p:Configuration=Release ToCBS\ToCBS.sln

msbuild /m /t:restore,tospkg:publish,tospkgffu:publish,tospkgvhdx:publish /p:Platform=x64 /p:RuntimeIdentifier=win-x64 /p:PublishDir="%CD%\publish\artifacts\win-x64\ToSPKG" /p:PublishSingleFile=true /p:PublishTrimmed=false /p:Configuration=Release ToSPKG\ToSPKG.sln