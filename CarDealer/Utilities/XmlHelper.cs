namespace CarDealer.Utilities;

using System.Text;
using System.Xml.Serialization;

/// <summary>
/// Lightweight wrapper class for XML serialization deserialization using System.Xml.Serialization.
/// </summary>

public class XmlHelper
{
    /// <summary>
    /// Generic method for deserialization of XML strings.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="inputXml"></param>
    /// <param name="rootName"></param>
    /// <returns>Object instance of type T.</returns>
    /// 
    public T Deserialize<T>(string inputXml, string rootName)
    {
        XmlRootAttribute rootAttribute = new XmlRootAttribute(rootName);

        XmlSerializer serializer = new XmlSerializer(typeof(T), rootAttribute);

        using (TextReader reader = new StringReader(inputXml))
            return (T)serializer.Deserialize(reader)!;
    }

    /// <summary>
    /// Generic method for XML serialization of objects.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="rootName"></param>
    /// <returns>String containing serialized object.</returns>
    /// 
    public string Serialize<T>(T obj, string rootName)
    {
        XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

        namespaces.Add(string.Empty, string.Empty);

        XmlRootAttribute rootAttribute = new XmlRootAttribute(rootName);

        XmlSerializer serializer = new XmlSerializer(typeof(T), rootAttribute);

        StringBuilder stringBuilder = new StringBuilder();

        using (TextWriter writer = new StringWriter(stringBuilder))
        {
            serializer.Serialize(writer, obj, namespaces);

            return stringBuilder.ToString().TrimEnd();
        }
    }
}
