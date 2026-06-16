# IronLeveldb

[![NuGet version](https://badge.fury.io/nu/IronLeveldb.svg)](https://badge.fury.io/nu/IronLeveldb)

A leveldb implementation in C#, targeting .NET Standard 2.0 (works on .NET Framework 4.6.1+, .NET Core / .NET 5+).

 *This project is still under hard working and was built to read leveldb instances on Azure Blob Storage originally,
 thus, only partial of READ functions were implemented at the moment.*

## Build the project

 The whole solution builds and tests run cross-platform (Linux, macOS, Windows) on the .NET SDK.

 The end-to-end tests create genuine leveldb databases using the native `leveldb` library
 (through the `leveldb/c.h` C API) and then read them back with IronLeveldb, so the native
 library must be available when running the tests:

 * Ubuntu/Debian: `sudo apt-get install -y libleveldb-dev`
 * macOS: `brew install leveldb`
 * Windows: provide `leveldb.dll` on the `PATH`

 ```
 git clone https://github.com/tg123/IronLeveldb.git

 dotnet restore
 dotnet build

 dotnet test IronLeveldb.Test/IronLeveldb.Test.csproj
 ```

## How to use

 * Install
   ```
   Install-Package IronLeveldb -Pre
   ```

 * Open a local directory

   ```
   var db = IronLeveldbBuilder.BuildFromPath(dbpath)
   ```

 * Open a directory on Azure Blob

   ```
   var container = new BlobContainerClient(connectionString, "mycontainer");
   var db = new AzureBlobFolderStorage(container, "leveldb_directory").Build();
   ```
   More info about blob storage at <https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet>

## Roadmap

  // TBD
