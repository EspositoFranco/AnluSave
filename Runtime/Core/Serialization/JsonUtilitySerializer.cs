using UnityEngine;

namespace Anlu.Save.Serialization
{
    /// <summary>
    /// Serializador por defecto sobre <see cref="JsonUtility"/>: rápido y sin dependencias.
    /// Limitaciones: no serializa diccionarios, polimorfismo ni propiedades (solo campos públicos
    /// o <c>[SerializeField]</c>). Para eso usá el módulo Newtonsoft.
    /// </summary>
    public sealed class JsonUtilitySerializer : ISerializer
    {
        private readonly bool _prettyPrint;

        /// <summary>Crea el serializador. <paramref name="prettyPrint"/> indenta el JSON (útil en debug).</summary>
        public JsonUtilitySerializer(bool prettyPrint = false) => _prettyPrint = prettyPrint;

        /// <inheritdoc />
        public string Serialize<T>(T value) => JsonUtility.ToJson(value, _prettyPrint);

        /// <inheritdoc />
        public T Deserialize<T>(string data) => JsonUtility.FromJson<T>(data);
    }
}
