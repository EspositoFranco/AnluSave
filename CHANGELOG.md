# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-07-17

Initial scaffold. Layer A (the persistence engine) only. Layer B (scene-object
persistence via a saveable-entity/GUID pattern) is designed but deferred to a future
release.

### Added
- `ISaveService<T>` — generic save service over the game's own versioned model.
- `SaveService<T>` — POCO engine orchestrating serializer + storage + migrations + write policy.
- `ISaveStorage` — async-first storage abstraction. Implementations:
  - `FileStorage` — atomic write (tmp → rename) + rotating `.bak` backup in `persistentDataPath`.
  - `PlayerPrefsStorage` — Base64 payload over PlayerPrefs; good for settings and small saves.
  - `WebGLStorage` — file storage + `FS.syncfs` flush to IndexedDB for browser builds.
  - `EncryptedStorage` — AES-256-CBC + HMAC-SHA256 (encrypt-then-MAC) decorator over any storage.
- `ISerializer` — `JsonUtilitySerializer` (default, zero deps) and `NewtonsoftSerializer`
  (opt-in via `com.unity.nuget.newtonsoft-json`, gated by the `ANLU_SAVE_USE_NEWTONSOFT` versionDefine).
- `IVersionedSave` + `ISaveMigration<T>` + `SaveMigrationRunner<T>` — object-chain schema migration (Open/Closed).
- `WritePolicy` — testable debounce; `SaveFlushBehaviour` — flush on pause/focus/quit.
- `SaveConfigSO` — designer-tunable slot key, debounce, and backup toggle.
- `IEncryptionKeyProvider` + `ChecksumUtil` — consumer-supplied keys, HMAC integrity helpers.
- Basic Save sample and EditMode test suite (migration, file storage, encryption tamper-detection, write policy).
