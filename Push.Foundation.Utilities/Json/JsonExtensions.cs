using System.IO;
using Newtonsoft.Json;

namespace Push.Foundation.Utilities.Json
{
    public static class JsonExtensions
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();
        public static Formatting Formatting = Formatting.Indented;


        public static string SerializeToJson(this object input)
        {
            var stringWriter = new StringWriter();
            input.SerializeToJson(stringWriter);
            return stringWriter.ToString();
        }

        public static void SerializeToJson(this object input, TextWriter textWriter)
        {
            var writer = new JsonTextWriter(textWriter) { Formatting = Formatting };
            var serializer = JsonSerializer.Create(SerializerSettings);
            serializer.Serialize(writer, input);
            writer.Flush();
        }
    }
}
