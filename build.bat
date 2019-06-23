@echo off

cls
dotnet build FSharp.Codecs.Redis.sln -c Release
if ERRORLEVEL 1 (
	echo Error building FSharp.Codecs.Redis
	exit /b 1
) 

dotnet pack FSharp.Codecs.Redis -c Release
if ERRORLEVEL 1 (
	echo Error creating package for FSharp.Codecs.Redis
	exit /b 1
)

