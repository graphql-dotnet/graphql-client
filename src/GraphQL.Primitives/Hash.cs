using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace GraphQL;

internal static class Hash
{
    private static SHA256? _sha256;

    internal static string Compute(string query)
    {
        int expected = Encoding.UTF8.GetByteCount(query);
        byte[]? inputBytes = ArrayPool<byte>.Shared.Rent(expected);
        int written = Encoding.UTF8.GetBytes(query, 0, query.Length, inputBytes, 0);
        Debug.Assert(written == expected, (string)$"Encoding.UTF8.GetBytes returned unexpected bytes: {written} instead of {expected}");

        var shaShared = Interlocked.Exchange(ref _sha256, null) ?? SHA256.Create();

#if NET5_0_OR_GREATER
        Span<byte> bytes = stackalloc byte[32];
        if (!shaShared.TryComputeHash(inputBytes.AsSpan().Slice(0, written), bytes, out int bytesWritten)) // bytesWritten ignored since it is always 32
            throw new InvalidOperationException("Too small buffer for hash");
#else
        byte[] bytes = shaShared.ComputeHash(inputBytes, 0, written);
#endif

        ArrayPool<byte>.Shared.Return(inputBytes);
        Interlocked.CompareExchange(ref _sha256, shaShared, null);

#if NET5_0_OR_GREATER
        return Convert.ToHexString(bytes);
#else
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (byte item in bytes)
        {
            builder.Append(item.ToString("x2"));
        }

        return builder.ToString();
#endif
    }
}
