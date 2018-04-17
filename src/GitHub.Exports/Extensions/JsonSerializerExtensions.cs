using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text;

namespace GitHub.Extensions
{
    public static class JsonSerializerExtensions
    {
        static JsonSerializationStrategy strategy = new JsonSerializationStrategy();
        public static StringContent SerializeForRequest<T>(this T model)
        {
            var json = model.ToJson(true);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static string ToJson<T>(this T model, bool toLowerCaseProps = false)
        {
            if (toLowerCaseProps)
                return SimpleJson.SerializeObject(model, strategy);
            return SimpleJson.SerializeObject(model);
        }

        public static T FromJson<T>(this string json, bool fromLowerCaseProps = false)
        {
            if (fromLowerCaseProps)
                return SimpleJson.DeserializeObject<T>(json, strategy);
            return SimpleJson.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Convert from PascalCase to camelCase.
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        static string ToJsonPropertyName(string propertyName)
        {
            Guard.ArgumentNotEmptyString(propertyName, nameof(propertyName));
            int i = 0;
            while (i < propertyName.Length && char.IsUpper(propertyName[i]))
                i++;
            return propertyName.Substring(0, i).ToLowerInvariant() + propertyName.Substring(i);
        }

        class JsonSerializationStrategy : PocoJsonSerializerStrategy
        {
            protected override string MapClrMemberNameToJsonFieldName(string clrPropertyName)
            {
                return ToJsonPropertyName(clrPropertyName);
            }
        }
    }
}
