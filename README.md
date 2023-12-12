# SQLite Key-Value Store for Unity
[Key-Value Store](https://github.com/gilzoide/unity-key-value-store) implementation backed by the [SQLite](https://sqlite.org) database engine.


## Features
- Supported platforms: Windows, macOS, iOS, tvOS, Android and WebGL
- Custom serialization of complex objects (class/struct types), using Key-Value Store's serialization system
- Supports running `PRAGMA` statements for customization of the database
- Supports running `VACUUM` statements manually


## Dependencies
- [Key-Value Store](https://github.com/gilzoide/unity-key-value-store): interface used by this implementation, which also provides custom object serialization out of the box.


## How to install
Either:
- Install using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with the following URL:
  ```
  https://github.com/gilzoide/unity-key-value-store-sqlite.git
  ```
- Clone this repository or download a snapshot of it directly inside your project's `Assets` or `Packages` folder.


## Basic usage
```cs
using Gilzoide.KeyValueStore.Sqlite;
using UnityEngine;

// 1. Instantiate you SqliteKeyValueStore
var kvs = new SqliteKeyValueStore(Application.persistentDataPath + "/MySaveFile.db");


// 2. Set/Get/Delete values
kvs.SetBool("finishedTutorial", true);
kvs.SetString("username", "gilzoide");

Debug.Log("Checking if values exist: " + kvs.HasKey("username"));
Debug.Log("Getting values: " + kvs.GetInt("username"));
Debug.Log("Getting values with fallback: " + kvs.GetString("username", "default username"));
// Like C# Dictionary, this idiom returns a bool if the key is found
if (kvs.TryGetString("someKey", out string foundValue))
{
    Debug.Log("'someKey' exists: " + foundValue);
}

kvs.DeleteKey("someKey");


// 3. Dispose of the SqliteKeyValueStore when done
// This ensures the database connection gets closed correctly
kvs.Dispose();
```