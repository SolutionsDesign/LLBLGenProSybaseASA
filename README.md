## Sybase iAnywhere (ASA) Driver / DQE for LLBLGen Pro

This is the repository for the Sybase iAnywhere (ASA) Driver and Dynamic Query Engine (DQE) for LLBLGen Pro v5.x. It contains the sourcecode for the driver and DQE so users who move to v5.x and 
need to support Sybase ASA can continue to do so. Starting with v5.0, LLBLGen Pro doesn't officially support Sybase ASA anymore, and as a courtesy to our existing customers
we have published the sourcecode for the driver and DQE here on GitHub under the flexible MIT license.

### Compatiblity
The sourcecode available here compiles against LLBLGen Pro v5.0.x assemblies. If you encounter code breaking changes, please file an issue on this repository so we can look into this. 

### Support on this sourcecode
While LLBLGen Pro officially doesn't support Sybase ASA anymore, we strive to keep this code compileable against the latest LLBLGen Pro version, if feasible. As we no longer have access to any Sybase ASA database nor ADO.NET provider for ASA, we can't test the code ourselves (and which is also the reason why we no longer support it officially). Please file an issue here if you run into an issue with compiling the code. We can't fix bugs / issues in the code for you as we can't test it on live databases anymore. However if you want to fix an issue yourself but need advice how to do so, e.g. because you're unfamiliar with how things work internally in the system, please ask the question as an issue here on GitHub and we'll try to help you as best as we can. 

### Templates
The Sybase ASA templates are still shipped with the LLBLGen Pro installer as it's otherwise perhaps a bit cumbersome to add a custom built driver / dqe to the system. The amount of templates specific for a database is very small so this isn't a big deal. 

### Features of the driver / DQE

The driver supports: 

-   All features of iAnywhere v8.x or higher are supported except Java based types
    and proxy tables
-   Owners `SYS`, `dbo`, `SA_DEBUG`, `rs_systabgroup` are
    filtered out.
-   `(Long)varbit` bitarrays are mapped to `string`.
-   Users should specify the database service name for the service to
    connect to, not the server name (or IP address) the database service
    runs on.
-   Multiple schemas per catalog.

### Compiling the Driver
To compile the driver, make sure the references in the SybaseAsaDBDriver csproj file are updated and point to the LLBLGen Pro v5.0.x designer installation. The driver dll is self-signed. Compiling is simple: just compile the csproj file. 

#### Deploying the driver
To use the driver, place the compiled dll it in the following folder: `<llblgen pro installation folder>\Drivers\SybaseASA`. 
Additionally, copy the file `driver.config` from the Driver folder into this folder. 

To see whether the driver is loaded by the designer, start the designer and go to `Tools->View Loaded External Types...` If the Sybase ASA driver is shown, it's loaded and usable. 

#### Driver ID
Drivers in LLBLGen Pro use a GUID as 'ID'. This ID is hardcoded and is specified in the driver.config file and the DBDriver class. As this ID is also used in the templates
it's key you keep this ID the same as it is today. If you fork this codebase and change that ID, templates shipped with the designer won't work with the driver. 

The ID for Sybase ASA is: `A3076322-977C-4e28-BFF4-F25ED096D1DB`

### Compiling the Dynamic Query Engine (DQE)
To compile the DQE, make sure the references in the SybaseAsaDQE csproj are updated and point to the ORMSupportClasses dll. The DQE dll is self-signed. The ORM Support classes package is [also available on nuget](https://www.nuget.org/packages/SD.LLBLGen.Pro.ORMSupportClasses/), for easy referencing.

#### Deploying the DQE
To use the DQE, just reference the compiled dll. See for more info about compiling the generated code and using a DQE, the [Compiling your code](http://www.llblgen.com/documentation/5.0/LLBLGen%20Pro%20RTF/Using%20the%20generated%20code/gencode_compiling.htm) topic in the 
[LLBLGen Pro Runtime Framework documentation](http://www.llblgen.com/documentation/5.0/LLBLGen%20Pro%20RTF/index.htm). 

### License
The sourcecode in this repository is licensed to you under the MIT license, given below.

```
The MIT License (MIT)

Copyright (c) 2016 Solutions Design bv

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```