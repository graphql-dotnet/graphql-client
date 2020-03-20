using System;
using System.Linq;
using System.Reflection;
using GraphQL.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GraphQL.Client.LocalExecution
{
    public class GraphQLEnumConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var enumString = ((Enum)value).ToString("G");
                var memberName = value.GetType()
                    .GetMember(enumString, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault()?.Name;
                if (string.IsNullOrEmpty(memberName))
                {
                    if (!AllowIntegerValues)
                        throw new JsonSerializationException($"Integer value {value} is not allowed.");
                    writer.WriteValue(value);
                }
                else
                {
                    writer.WriteValue(StringUtils.ToConstantCase(memberName));
                }
            }
        }
    }

}
