# Anlu.Save — Cross-platform save engine for Unity

> Author: **Franco Esposito** · Part of the Anlu Packages collection

A generic, cross-platform save engine. You give it your own save model; it persists it
reliably on any target — PC, mobile, WebGL portals (Poki, CrazyGames), and (via a custom
storage backend) consoles. It knows nothing about what you save: gold, stats, unlocked
maps, settings — it's all just `T` to the engine.

The design rests on one idea: **saving is two problems, not one.**

- **Layer A — the persistence engine** (this release): take a blob, persist it reliably
  and portably. Content-agnostic.
- **Layer B — scene-object capture** (future): walk marked objects (`SaveableEntity`) and
  build the blob. Layer B sits on top of Layer A, never the other way around.

This package ships Layer A. Layer B is designed and reserved for a later release.

---

## Folder structure

```
Anlu.Save/
├── package.json
├── README.md · AUTHORS.md · CHANGELOG.md · LICENSE.md
├── Runtime/
│   ├── Core/                                   Anlu.Save.Core.asmdef (no external deps)
│   │   ├── ISaveService.cs                     ISaveService / ISaveService<T>
│   │   ├── SaveService.cs                      the engine (POCO, testable)
│   │   ├── IVersionedSave.cs
│   │   ├── ISaveMigration.cs
│   │   ├── SaveMigrationRunner.cs              object-chain migration
│   │   ├── SaveKeys.cs                         shared constants (no magic strings)
│   │   ├── Config/SaveConfigSO.cs              designer-tunable config
│   │   ├── Serialization/
│   │   │   ├── ISerializer.cs
│   │   │   └── JsonUtilitySerializer.cs        default, zero deps
│   │   ├── Storage/
│   │   │   ├── ISaveStorage.cs                 async-first storage contract
│   │   │   ├── FileStorage.cs                  atomic tmp→rename + .bak
│   │   │   ├── PlayerPrefsStorage.cs
│   │   │   ├── WebGLStorage.cs                 idbfs + FS.syncfs
│   │   │   ├── EncryptedStorage.cs             AES + HMAC decorator
│   │   │   └── IEncryptionKeyProvider.cs
│   │   ├── Integrity/ChecksumUtil.cs           HMAC-SHA256 helpers
│   │   └── Policy/
│   │       ├── WritePolicy.cs                  debounce (POCO)
│   │       └── SaveFlushBehaviour.cs           flush on pause/focus/quit
│   ├── Plugins/WebGL/AnluSaveSyncFs.jslib      IndexedDB flush bridge
│   └── Serialization.Newtonsoft/               Anlu.Save.Newtonsoft.asmdef (opt-in)
│       └── NewtonsoftSerializer.cs
├── Tests/Editor/                               Anlu.Save.Tests.Editor.asmdef
└── Samples~/BasicSave/                         PlayerSaveData + BasicSaveSample
```

**One engine, swappable parts.** The Core assembly has zero external dependencies. Storage,
serialization, and migration are all injected — you pick the parts per platform without
touching the engine.

---

## Quick start (5 minutes)

**1. Define your save model.** Implement `IVersionedSave` and mark it `[Serializable]`.

```csharp
using System;
using System.Collections.Generic;
using Anlu.Save;

[Serializable]
public class MySave : IVersionedSave
{
    public int schemaVersion = 1;
    public int gold;
    public List<string> unlockedMaps = new();

    public int SchemaVersion { get => schemaVersion; set => schemaVersion = value; }
}
```

**2. Wire the engine (manual DI, in your bootstrap).**

```csharp
using Anlu.Save;
using Anlu.Save.Policy;
using Anlu.Save.Serialization;
using Anlu.Save.Storage;

var storage    = new FileStorage("Saves");                 // atomic + backup
var serializer = new JsonUtilitySerializer();
var migrations = new SaveMigrationRunner<MySave>(currentVersion: 1);

var save = new SaveService<MySave>(storage, serializer, migrations, key: "player", debounceSeconds: 1f);

gameObject.AddComponent<SaveFlushBehaviour>().Bind(save); // flush on pause/focus/quit
await save.LoadAsync();                                    // Data is never null after this
```

**3. Mutate and mark dirty.** The write policy batches changes into one debounced write.

```csharp
save.Data.gold += 50;
save.MarkDirty();          // schedules a debounced write
// await save.FlushAsync(); // or force an immediate write (e.g. a "Save" button)
```

That's it. Progress survives alt-tab, mobile suspend, and browser-tab close.

---

## Public API

### `ISaveService<T>`

| Member | Description |
|---|---|
| `T Data` | Live save data. Never null after `LoadAsync`. |
| `bool IsLoaded` | True once `LoadAsync` completed at least once. |
| `event Action<T> Loaded` | Fired after load + migration. |
| `event Action<T> Saved` | Fired after each successful write. |
| `Task<T> LoadAsync()` | Loads and migrates; creates a default if missing/corrupt. |
| `void MarkDirty()` | Schedules a debounced write. |
| `void Tick(float dt)` | Advances the debounce; driven by `SaveFlushBehaviour`. |
| `Task FlushAsync()` | Writes immediately. |
| `Task DeleteAsync()` | Deletes the save; resets `Data` to a fresh default. |

### `ISaveStorage` (async-first)

`Task<bool> ExistsAsync(key)` · `Task<byte[]> LoadAsync(key)` · `Task SaveAsync(key, data)` · `Task DeleteAsync(key)`

| Implementation | Use it for | Notes |
|---|---|---|
| `FileStorage` | PC, mobile, base for WebGL | Atomic write, rotating `.bak`. Falls back to backup on read failure. |
| `PlayerPrefsStorage` | Settings, small saves | Base64 over PlayerPrefs. Works everywhere including WebGL. |
| `WebGLStorage` | Browser portals | Wraps `FileStorage`, calls `FS.syncfs` so idbfs actually persists. |
| `EncryptedStorage` | Anti-tampering | AES-256 + HMAC decorator over any inner storage. |

> **WebGL is single-threaded.** Storage implementations must never use `Task.Run`; the async
> signature exists so console/cloud backends can be genuinely async, and so the API is uniform.

### `ISerializer`

`string Serialize<T>(T)` · `T Deserialize<T>(string)` — `JsonUtilitySerializer` (default) or
`NewtonsoftSerializer` (opt-in; needs `com.unity.nuget.newtonsoft-json`).

---

## Schema migration

Migration is a **chain of objects**, not a `switch`. Add a migration = add a class; the engine
never changes (Open/Closed).

```csharp
public class AddPrestigeV1ToV2 : ISaveMigration<MySave>
{
    public int FromVersion => 1;
    public void Apply(MySave data)
    {
        data.prestige = 0;      // new field default
        data.SchemaVersion = 2; // MUST advance the version
    }
}

var migrations = new SaveMigrationRunner<MySave>(
    currentVersion: 2,
    migrations: new ISaveMigration<MySave>[] { new AddPrestigeV1ToV2() });
```

The runner walks from the document's version to `currentVersion`, applying each step. A save
newer than the build is clamped down with a warning; a version with no explicit step is treated
as additive (adopts the current version).

> **JsonUtility limitation:** migrations run on the deserialized typed object, so they handle
> additive fields and value transforms well, but not structural rewrites of arbitrary JSON. For
> that, use the Newtonsoft serializer.

---

## Encryption

`EncryptedStorage` is a decorator. You supply the keys — the library never hardcodes them.

```csharp
public class MyKeys : IEncryptionKeyProvider
{
    public byte[] GetEncryptionKey() => /* 32 bytes, AES-256 */;
    public byte[] GetMacKey()        => /* 32 bytes, distinct from the encryption key */;
}

ISaveStorage storage = new EncryptedStorage(new FileStorage("Saves"), new MyKeys());
```

On-disk format: `[IV 16][cipher…][hmac 32]`, encrypt-then-MAC. Tampered or corrupt payloads fail
the HMAC check and are rejected (the engine then loads a fresh default).

> This is not DRM. The key lives on the client, so it stops casual JSON editing, not a determined
> attacker. Real anti-cheat needs a server-authoritative backend.

---

## ScriptableObjects

| SO | Configures |
|---|---|
| `SaveConfigSO` | `SlotKey` (save file/key name), `DebounceSeconds` (write debounce), `KeepBackup` (rotating `.bak`). Read it in your bootstrap to build the `SaveService`. |

---

## Extension examples

**Encrypted WebGL save with Newtonsoft:**

```csharp
ISaveStorage storage    = new EncryptedStorage(new WebGLStorage("Saves"), new MyKeys());
ISerializer  serializer = new NewtonsoftSerializer(); // needs the versionDefine active
var save = new SaveService<MySave>(storage, serializer, new SaveMigrationRunner<MySave>(1));
```

**Custom storage backend (e.g. a console SDK):** implement `ISaveStorage` and inject it. The
engine, serializer, migrations, and write policy stay untouched — that's the whole point of the
abstraction.

---

## File reference

| File | Responsibility |
|---|---|
| `ISaveService.cs` | Non-generic lifecycle + generic `ISaveService<T>` contract. |
| `SaveService.cs` | The engine: load/migrate, debounced + immediate write, events. |
| `IVersionedSave.cs` | Minimal `SchemaVersion` contract for save models. |
| `ISaveMigration.cs` / `SaveMigrationRunner.cs` | Object-chain schema migration. |
| `Serialization/*` | `ISerializer` + `JsonUtilitySerializer` (Newtonsoft in its own assembly). |
| `Storage/*` | `ISaveStorage` + the four backends + `IEncryptionKeyProvider`. |
| `Integrity/ChecksumUtil.cs` | HMAC-SHA256 compute + constant-time compare. |
| `Policy/WritePolicy.cs` | Debounce timer (POCO). |
| `Policy/SaveFlushBehaviour.cs` | Ticks the debounce; flushes on pause/focus/quit. |
| `Config/SaveConfigSO.cs` | Designer-tunable slot, debounce, backup. |
| `Plugins/WebGL/AnluSaveSyncFs.jslib` | `FS.syncfs` bridge for browser persistence. |

---

## FAQ

**Why is the storage async when file IO is synchronous today?** Because IndexedDB (WebGL) and
console SDKs are genuinely async. A synchronous API would force a rewrite the day you port. The
sync backends just complete immediately.

**Why is `SaveService` a POCO instead of a MonoBehaviour?** So it's unit-testable and lifecycle
concerns stay separate. `SaveFlushBehaviour` is the thin MonoBehaviour bridge that drives it.

**Why doesn't the engine capture my scene objects automatically?** That's Layer B — a separate
concern with genuinely hard problems (runtime-spawned objects, destroyed objects, stable GUIDs).
It ships later as `Anlu.Save.SceneObjects` on top of this engine, without changing it.

**Why JsonUtility by default?** Zero dependencies and fast. When you need dictionaries or
polymorphism, flip on the Newtonsoft module — the versionDefine only compiles it when the package
is installed.

**Does encryption make my save secure?** It makes it tamper-evident and not casually editable. It
is not server-grade security; the key is on the client.
