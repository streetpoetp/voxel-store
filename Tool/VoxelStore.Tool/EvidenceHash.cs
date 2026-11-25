using System.Security.Cryptography;
using System.Text;

namespace VoxelStore.Tool;

internal static class EvidenceHash
{
    private static readonly HashSet<string> TextExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".yaml",
            ".yml",
            ".json",
            ".md",
            ".cs",
            ".ps1",
            ".props",
            ".csproj",
            ".asmdef",
            ".meta",
            ".unity",
            ".asset",
            ".txt"
        };

    public static string HashFile(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        if (TextExtensions.Contains(Path.GetExtension(path)))
            bytes = NormalizeLineEndings(bytes);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    public static bool TextFileEquals(string path, string expected)
    {
        if (!File.Exists(path))
            return false;

        byte[] actualBytes = NormalizeLineEndings(File.ReadAllBytes(path));
        byte[] expectedBytes = NormalizeLineEndings(Encoding.UTF8.GetBytes(expected));
        return actualBytes.AsSpan().SequenceEqual(expectedBytes);
    }

    private static byte[] NormalizeLineEndings(byte[] bytes)
    {
        if (Array.IndexOf(bytes, (byte)'\r') < 0)
            return bytes;

        using var stream = new MemoryStream(bytes.Length);
        for (int i = 0; i < bytes.Length; i++)
        {
            byte value = bytes[i];
            if (value != (byte)'\r')
            {
                stream.WriteByte(value);
                continue;
            }

            if (i + 1 < bytes.Length && bytes[i + 1] == (byte)'\n')
                i++;
            stream.WriteByte((byte)'\n');
        }

        return stream.ToArray();
    }
}
