@echo off

set MSBUILD="%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"

set missingPackages=
if not exist packages\ (
    set missingPackages=YES
)

if not defined MSBUILD echo error: can't find MSBuild.exe & goto :eof
if not exist %MSBUILD% echo error: "%MSBUILD%": not found & goto :eof

echo Cleaning
%MSBUILD% VMM.sln /target:Clean /p:Configuration=Release /nologo /m

if defined missingPackages (
	echo Restoring Nuget packages
) else (
	echo Building
)
%MSBUILD% VMM.sln /p:Configuration=Release /nologo /m

if defined missingPackages (
	echo Building
	%MSBUILD% VMM.sln /p:Configuration=Release /nologo /m
)

echo Collecting binaries

rmdir bin /s/q >nul 2>&1

xcopy VMM\bin\Release\VMM.exe bin\*.* > nul
xcopy VMM\bin\Release\VMM.exe.config bin\*.* > nul
xcopy VMM\bin\Release\*.dll bin\*.* > nul

echo Done

:eof