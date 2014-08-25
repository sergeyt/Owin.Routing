@ECHO OFF
@set THISPATH=%~dp0
@set THISFILENAME=%~n0
@set SRCDIR=%THISPATH%\src

if not defined FrameworkDir (
  set FrameworkDir=%SystemRoot%\Microsoft.NET\Framework64\
)

if not defined FrameworkVersion (
  set FrameworkVersion=v4.0.30319
)

@set MSBUILDCMD=%FrameworkDir%%FrameworkVersion%\msbuild.exe
@set MSBUILD_OPTS=/l:FileLogger,Microsoft.Build.Engine;logfile=build.log;append=false

if "%1"=="clean" (
	echo CLEAN...CLEAN...CLEAN...CLEAN...CLEAN...CLEAN...
	%MSBUILDCMD% .build\dcnexus.msbuild /target:clean
	exit /b
)

%MSBUILDCMD% %MSBUILD_OPTS% .build\main.msbuild

:END