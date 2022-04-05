@echo off
pushd %0\..
set ROOTDIR=%CD%

rem 
rem Find DOTNET
rem
set DOTNET=dotnet.exe

:work

@mkdir _output
@mkdir _output\net60
@mkdir _libraries
@mkdir _libraries\net60

rem 
rem Build the library that contains Logger, Translation, etc
rem 
echo Building main utility library
cd utils\net60
%DOTNET% build --configuration Release
cd %ROOTDIR%
copy _output\net60\*.dll _libraries\net60

rem 
rem Now we can build the PC/SC library
rem 
echo Building PC/SC library
cd pcsc\net60
%DOTNET% build --configuration Release
cd %ROOTDIR%
copy _output\net60\*.dll _libraries\net60

rem 
rem And finally the PC/SC card helpers libraries
rem 
echo Building PC/SC helper libraries
cd pcsc-helpers\net60
%DOTNET% build --configuration Release
cd %ROOTDIR%
copy _output\net60\*.dll _libraries\net60

:end
popd

