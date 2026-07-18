// Bridge WebGL: fuerza el volcado del FS virtual (idbfs) a IndexedDB.
// Sin este sync, lo "guardado" en persistentDataPath se pierde al cerrar la pestaña.
mergeInto(LibraryManager.library, {
  AnluSaveSyncFs: function () {
    try {
      FS.syncfs(false, function (err) {
        if (err) console.error('[Anlu.Save] FS.syncfs error:', err);
      });
    } catch (e) {
      console.error('[Anlu.Save] FS.syncfs exception:', e);
    }
  }
});
