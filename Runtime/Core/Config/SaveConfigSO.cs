using UnityEngine;

namespace Anlu.Save
{
    /// <summary>
    /// Configuración tuneable por diseñador para el motor de guardado. Data-only: no contiene
    /// lógica, solo valores que el bootstrap lee para construir el <c>SaveService</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "SaveConfig", menuName = "Anlu/Save/Save Config", order = 0)]
    public sealed class SaveConfigSO : ScriptableObject
    {
        [Header("Slot")]
        [Tooltip("Nombre lógico del slot de guardado (archivo o clave de PlayerPrefs).")]
        [SerializeField] private string _slotKey = SaveKeys.DefaultSlot;

        [Header("Política de escritura")]
        [Tooltip("Segundos de calma tras un cambio antes de escribir a disco (debounce).")]
        [SerializeField, Min(0f)] private float _debounceSeconds = 1f;

        [Tooltip("Mantener un backup rotativo (.bak) para recuperar ante corrupción del principal.")]
        [SerializeField] private bool _keepBackup = true;

        /// <summary>Nombre lógico del slot de guardado.</summary>
        public string SlotKey => _slotKey;

        /// <summary>Segundos de debounce antes de escribir.</summary>
        public float DebounceSeconds => _debounceSeconds;

        /// <summary>Si se mantiene un backup rotativo (.bak).</summary>
        public bool KeepBackup => _keepBackup;
    }
}
