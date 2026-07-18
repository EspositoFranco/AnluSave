namespace Anlu.Save.Serialization
{
    /// <summary>
    /// Convierte el documento de guardado a texto y viceversa. Pinchable: default
    /// <see cref="JsonUtilitySerializer"/>; inyectá Newtonsoft o binario si lo necesitás.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>Serializa <paramref name="value"/> a texto.</summary>
        string Serialize<T>(T value);

        /// <summary>Deserializa <paramref name="data"/> al tipo <typeparamref name="T"/>.</summary>
        T Deserialize<T>(string data);
    }
}
