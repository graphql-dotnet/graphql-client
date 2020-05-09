using System;
using System.Numerics;
using System.Text.Json;

namespace GraphQL.Client.Serializer.SystemTextJson
{
    public static class ConverterHelperExtensions
    {
        public static object ReadNumber(this JsonElement value)
        {
            if (value.TryGetInt32(out int i))
                return i;
            else if (value.TryGetInt64(out long l))
                return l;
            else if (BigInteger.TryParse(value.GetRawText(), out var bi))
                return bi;
            else if (value.TryGetDouble(out double d))
                return d;
            else if (value.TryGetDecimal(out decimal dd))
                return dd;

            throw new NotImplementedException($"Unexpected Number value. Raw text was: {value.GetRawText()}");
        }
    }
}
