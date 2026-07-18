# Authors

**Franco Esposito** — original author and maintainer

## Package
Anlu.Save — generic, cross-platform save engine for Unity. Persists any versioned
model (`ISaveService<T>`) through swappable storage backends (file/JSON, PlayerPrefs,
WebGL/IndexedDB, AES+HMAC encrypted), pluggable serialization (JsonUtility by default,
Newtonsoft opt-in), object-chain schema migration, and a reliable write policy
(debounce + flush on pause/focus/quit + atomic write + rotating backup). The same game
code runs in editor, standalone, mobile, and web portals without a single change.
