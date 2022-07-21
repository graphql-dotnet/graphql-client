using System.Buffers;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace GraphQL.Client.Serializer.SystemTextJson;

public static class ConverterHelperExtensions
{
    public static object ReadNumber(this ref Utf8JsonReader reader)
    {
        if (reader.TryGetInt32(out int i))
            return i;
        else if (reader.TryGetInt64(out long l))
            return l;
        else if (reader.TryGetDouble(out double d))
            return reader.TryGetBigInteger(out var bi) && bi != new BigInteger(d)
                ? bi
                : (object)d;
        else if (reader.TryGetDecimal(out decimal dd))
            return reader.TryGetBigInteger(out var bi) && bi != new BigInteger(dd)
                ? bi
                : (object)dd;

        throw new NotImplementedException($"Unexpected Number value. Raw text was: {reader.GetRawString()}");
    }

    public static bool TryGetBigInteger(this ref Utf8JsonReader reader, out BigInteger bi) => BigInteger.TryParse(reader.GetRawString(), out bi);

    public static string GetRawString(this ref Utf8JsonReader reader)
    {
        var byteArray = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan.ToArray();
        return Encoding.UTF8.GetString(byteArray);
    }
}
