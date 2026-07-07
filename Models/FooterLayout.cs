using System.Text;

namespace WpfBuilder.Models;

public static class FooterLayout
{
    public const int TargetFileNameSize = 260;
    public const int OriginalMd5Size = 33;
    public const int WindowTitleSize = 100;
    public const int MaxDisks = 8;

    public const uint Magic = 0x50544348;

    public const int DiskInfoSize = TargetFileNameSize + OriginalMd5Size + 8 + 4 + 8 + 4;

    public const int FooterSize = WindowTitleSize + 4 + (MaxDisks * DiskInfoSize) + 4;

    public static byte[] EncodeFixedUtf8(string text, int fixedSize)
    {
        var raw = Encoding.UTF8.GetBytes(text ?? "");

        if (raw.Length >= fixedSize)
            throw new ArgumentException($"'{text}' 이(가) {fixedSize}바이트 제한을 넘음 (UTF-8 {raw.Length}바이트). null terminator 자리 포함 필요.");

        var buf = new byte[fixedSize];
        Array.Copy(raw, buf, raw.Length);

        return buf;
    }
}