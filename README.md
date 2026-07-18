# Anlu Save (`com.anlu.save`)

Motor de guardado (Save) reutilizable y multiplataforma para Unity. Genérico sobre tu
modelo de datos, con storage intercambiable (PC / WebGL / móvil / consolas), migración
encadenada, serialización pinchable y política de escritura confiable.

> 🚧 En construcción — scaffolding inicial.

## Arquitectura (norte)

El Save son **dos problemas separados**:

- **Capa A — Motor de persistencia** (v1): toma un blob y lo guarda de forma confiable en
  cualquier plataforma. Agnóstico del contenido.
- **Capa B — Captura de estado de escena** (v2): recorre objetos marcados (`SaveableEntity`)
  y arma el blob. Se apoya sobre la Capa A, nunca al revés.

## Contratos principales (v1)

- `ISaveService<T>` — genérico sobre tu modelo de save.
- `ISaveStorage` — async-first. `FileStorage`, `PlayerPrefsStorage`, `WebGLStorage`, `EncryptedStorage`.
- `ISerializer` — `JsonUtilitySerializer` (default) · `NewtonsoftSerializer` (opcional, versionDefine).
- `ISaveMigration` — cadena de migraciones registrables (Open/Closed).
- `WritePolicy` — debounce + flush on pause/focus/quit + escritura atómica + backup.

## Licencia

Franco Esposito.
