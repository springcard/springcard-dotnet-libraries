# SpringCard (public) .NET libraries

This repository stores all the .NET libraries that are referenced by SpringCard's SDK and related software.

## Documentation

All these libraries are self-documented using Doxygen.

Official documantions are published by SpringCard on [https://docs.springcard.com/](https://docs.springcard.com/).

## License

```text
This software is part of SPRINGCARD SDKs

Redistribution and use in source (source code) and binary (object code) forms, with or without modification, are permitted provided that the following conditions are met :

1. Redistributed source code or object code shall be used only in conjunction with products (hardware devices) either manufactured, distributed or developed by SPRINGCARD,
2. Redistributed source code, either modified or un-modified, must retain the above copyright notice, this list of conditions and the disclaimer below,
3. Redistribution of any modified code must be clearly identified "Code derived from original SPRINGCARD copyrighted source code", with a description of the modification and the name of its author,
4. Redistributed object code must reproduce the above copyright notice, this list of conditions and the disclaimer below in the documentation and/or other materials provided with the distribution,
5. The name of SPRINGCARD may not be used to endorse or promote products derived from this software or in any other form without specific prior written permission from SPRINGCARD.

THIS SOFTWARE IS PROVIDED BY SPRINGCARD "AS IS". SPRINGCARD SHALL NOT BE LIABLE FOR INFRINGEMENTS OF THIRD PARTIES RIGHTS BASED ON THIS SOFTWARE.

ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.

SPRINGCARD DOES NOT WARRANT THAT THE FUNCTIONS CONTAINED IN THIS SOFTWARE WILL MEET THE USER'S REQUIREMENTS OR THAT THE OPERATION OF IT WILL BE UNINTERRUPTED OR ERROR-FREE.

IN NO EVENT SHALL SPRINGCARD BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```

## Available libraries

### Bluetooth

A library to access BLE devices from a WPF application.

### PC/SC Helpers

Various libraries to work with smart cards.

### PC/SC

Winscard wrapper for .NET. Gives access to PC/SC readers and smart cards. 

### Utils

Platform-agnostic utilities.

### Windows

Utilities for the Windows platform.

## Target platforms

All the libraries target .NET Framework 4.8 (`net48`).

Some of the libraries target the .NET Core 6 (`net60`) as well.

## Building the libraries

## Net48 targets

Every library comes with its project for the Microsoft Visual Studio IDE (.sln and .csproj) in a subdirectory named `net48`.

Projects have been created with Visual Studio 2017, then updated with Visual Studio 2019, and finally have updated with Visual Studio 2022.

Building from the command line is easy using `MSBUILD.EXE`. Use the `BUILD-NET48.CMD` file at the root to build all the projects in a row.

### Net60 targets

Every library comes with its project for the Microsoft Visual Studio IDE (.sln and .csproj) in a subdirectory named `net60`.

Projects have been created with Visual Studio 2022.

Building from the command line is easy using `DOTNET.EXE`. Use the `BUILD-NET60.CMD` file at the root to build all the projects in a row.

