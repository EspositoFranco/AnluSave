using System;
using UnityEngine;

namespace Anlu.Save.Policy
{
    /// <summary>
    /// Puente de ciclo de vida: hace avanzar el debounce del servicio cada frame y fuerza un flush
    /// en los momentos de riesgo (pausa, pérdida de foco, salida). Ese comportamiento de
    /// "no perder progreso en WebGL/móvil" vive acá una sola vez, no en cada juego.
    /// </summary>
    public sealed class SaveFlushBehaviour : MonoBehaviour
    {
        private ISaveService _service;

        /// <summary>Inyecta el servicio a manejar. Llamalo desde tu bootstrap tras crear el SaveService.</summary>
        public void Bind(ISaveService service) => _service = service;

        private void Update() => _service?.Tick(Time.unscaledDeltaTime);

        private void OnApplicationPause(bool paused)
        {
            if (paused) FlushNow();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) FlushNow();
        }

        private void OnApplicationQuit() => FlushNow();

        private async void FlushNow()
        {
            if (_service == null) return;
            try { await _service.FlushAsync(); }
            catch (Exception e) { Debug.LogError($"[Anlu.Save] Flush de ciclo de vida falló: {e}"); }
        }
    }
}
