# IronLeveldb

[![NuGet version](https://badge.fury.io/nu/IronLeveldb.svg)](https://badge.fury.io/nu/IronLeveldb)

A leveldb implementation in C#, targeting .NET Standard 1.3 (Frameworks 4.6+, Core 1.0+).

 *This project is still under hard working and was built to read leveldb instances on Azure Blob Storage originally,
 thus, only partial of READ functions were implemented at the moment.*
 
## Build the project
 
 The main project can be built using dotnet on any platform, 
 but the test project is now targeting net46 due to some dependencies,
 which can only build and run on Windows.
 
 ```
 git clone https://github.com/tg123/IronLeveldb.git
 
 dotnet restore
 dotnet build
 
 dotnet test IronLeveldb.Test\IronLeveldb.Test.csproj
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
   var storageAccount = CloudStorageAccount.Parse(
        CloudConfigurationManager.GetSetting("StorageConnectionString"));

   var blobClient = storageAccount.CreateCloudBlobClient();

   var container = blobClient.GetContainerReference("mycontainer");

   var folder = container.GetDirectoryReference("leveldb_directory");
   var db = new IronLeveldbOptions
   {
        Storge = new AzureBlobFolderStorage(folder)
   }.Build()
   ```
   More info about blob storage at <https://docs.microsoft.com/en-us/azure/storage/storage-dotnet-how-to-use-blobs>
 
## Roadmap

  // TBD
