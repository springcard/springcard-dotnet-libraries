@echo off
pushd %0\..
set ROOTDIR=%CD%

rem 
rem Find MSBUILD
rem
setlocal enableextensions 
for /F "tokens=*" %%F in ('vswhere.exe -latest -requires Microsoft.Component.MSBuild -property installationPath') do set MSBUILD_DIR=%%F
set MSBUILD_DIR="%MSBUILD_DIR%\MSBuild\Current\Bin"
set MSBUILD=%MSBUILD_DIR%\msbuild.exe
%MSBUILD%

@mkdir _output
@mkdir _libraries
@mkdir _libraries\net48

rem 
rem Build the library that contains Logger, Translation, etc
rem 
echo Building main Windows utility library
cd utils\net48
%MSBUILD% /target:Build /property:Configuration=Release
cd %ROOTDIR%
copy _output\net48\*.dll _libraries\net48

rem
rem The Windows library uses WPF forms
rem Therefore we must use msbuild, not dotnet
rem 
echo Building Windows utility library
cd windows\net48
%MSBUILD% /target:Build /property:Configuration=Release
cd %ROOTDIR%
copy _output\net48\*.dll _libraries\net48

rem 
rem The Bluetooth library is required by PC/SC 'ZeroDriver' library for BLE devices
rem 
echo Building Bluetooth library
cd bluetooth\net48
%MSBUILD% /target:Build /property:Configuration=Release
cd %ROOTDIR%
copy _output\net48\*.dll _libraries\net48

rem 
rem Now we can build the PC/SC library
rem 
echo Building PC/SC library
cd pcsc\net48
%MSBUILD% /target:Build /property:Configuration=Release
cd %ROOTDIR%
copy _output\net48\*.dll _libraries\net48

rem 
rem And finally the PC/SC card helpers libraries
rem 
echo Building PC/SC helper libraries
cd pcsc-helpers\net48
%MSBUILD% /target:Build /property:Configuration=Release
cd %ROOTDIR%
copy _output\net48\*.dll _libraries\net48

popd
pause
