using Newtonsoft.Json;

namespace Anlu.Save.Serialization
{
    /// <summary>
    /// Serializador opcional sobre Newtonsoft.Json: soporta diccionarios, polimorfismo y propiedades
    /// (lo que JsonUtility no hace). Este assembly solo compila cuando el paquete
    /// com.unity.nuget.newtonsoft-json está instalado (versionDefine ANLU_SAVE_USE_NEWTONSOFT).
    /// </summary>
    public sealed class NewtonsoftSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings;

        /// <summary>Crea el serializador con settings opcionales (default: ignora nulos, sin indentar).</summary>
        public NewtonsoftSerializer(JsonSerializerSettings settings = null)
            => _settings = settings ?? new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };

        /// <inheritdoc />
        public string Serialize<T>(T value) => JsonConvert.SerializeObject(value, _settings);

        /// <inheritdoc />
        public T Deserialize<T>(string data) => JsonConvert.DeserializeObject<T>(data, _settings);
    }
}
