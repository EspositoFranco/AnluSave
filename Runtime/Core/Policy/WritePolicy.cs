using UnityEngine;

namespace Anlu.Save.Policy
{
    /// <summary>
    /// Política de escritura con debounce: agrupa muchas marcas de "sucio" en una sola escritura
    /// tras <see cref="DebounceSeconds"/> de calma. POCO testeable, sin dependencia de escena.
    /// </summary>
    public sealed class WritePolicy
    {
        private float _timer;

        /// <summary>Segundos de calma antes de que un cambio dispare la escritura.</summary>
        public float DebounceSeconds { get; }

        /// <summary>True si hay cambios pendientes de escribir.</summary>
        public bool IsDirty { get; private set; }

        /// <summary>Crea la política. <paramref name="debounceSeconds"/> se clampea a &gt;= 0.</summary>
        public WritePolicy(float debounceSeconds) => DebounceSeconds = Mathf.Max(0f, debounceSeconds);

        /// <summary>Marca que hay cambios y reinicia el contador de debounce.</summary>
        public void MarkDirty()
        {
            IsDirty = true;
            _timer = DebounceSeconds;
        }

        /// <summary>Limpia el estado sucio tras una escritura exitosa.</summary>
        public void Clear() => IsDirty = false;

        /// <summary>Avanza el debounce. Devuelve true cuando el temporizador venció y toca escribir.</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsDirty) return false;
            _timer -= deltaTime;
            return _timer <= 0f;
        }
    }
}
