# SpringCard (public) .NET libraries

This repository stores all the .NET libraries that are referenced by SpringCard's SDK and related software.

## Legal disclaimer

THE SDK IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

## License

This software is part of SPRINGCARD SDKs

Redistribution and use in source (source code) and binary (object code) forms, with or without modification, are permitted provided that the following conditions are met :

1. Redistributed source code or object code shall be used only in conjunction with products (hardware devices) either manufactured, distributed or developed by SPRINGCARD,
2. Redistributed source code, either modified or un-modified, must retain the above copyright notice, this list of conditions and the disclaimer below,
3. Redistribution of any modified code must be clearly identified "Code derived from original SPRINGCARD copyrighted source code", with a description of the modification and the name of its author,
4. Redistributed object code must reproduce the above copyright notice, this list of conditions and the disclaimer below in the documentation and/or other materials provided with the distribution,
5. The name of SPRINGCARD may not be used to endorse or promote products derived from this software or in any other form without specific prior written permission from SPRINGCARD.

Please read to `LICENSE.TXT` for the complete license statement. Please always place a copy of the `LICENSE.TXT` file together with the redistributed source code or object code.

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

## Documentation

All the libraries are self-documented using Doxygen.

Official documentation is published by SpringCard on [docs.springcard.com/](https://docs.springcard.com/).

## How to contact us

Retrieve all our contact details on [www.springcard.com/contact](https://www.springcard.com/en/contact).
