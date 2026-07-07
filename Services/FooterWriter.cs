using System.IO;
using WpfBuilder.Models;

namespace WpfBuilder.Services;

public static class FooterWriter
{
    public static byte[] Build(FooterBuildInfo info)
    {
        if (info.Disks.Count == 0 || info.Disks.Count > FooterLayout.MaxDisks)
            throw new ArgumentException($"디스크 개수는 1~{FooterLayout.MaxDisks}개여야 함 (현재 {info.Disks.Count}개)");

        using var ms = new MemoryStream(FooterLayout.FooterSize);
        using var bw = new BinaryWriter(ms);

        bw.Write(FooterLayout.EncodeFixedUtf8(info.WindowTitle, FooterLayout.WindowTitleSize));
        bw.Write((uint)info.Disks.Count);

        for (int i = 0; i < FooterLayout.MaxDisks; i++)
        {
            if (i < info.Disks.Count)
            {
                var d = info.Disks[i];
                bw.Write(FooterLayout.EncodeFixedUtf8(d.TargetFileName, FooterLayout.TargetFileNameSize));
                bw.Write(FooterLayout.EncodeFixedUtf8(d.OriginalMd5, FooterLayout.OriginalMd5Size));
                bw.Write(d.XdeltaOffset);
                bw.Write(d.XdeltaSize);
                bw.Write(d.BgOffset);
                bw.Write(d.BgSize);
            }
            else
                bw.Write(new byte[FooterLayout.DiskInfoSize]);
        }

        bw.Write(FooterLayout.Magic);
        bw.Flush();

        var result = ms.ToArray();

        if (result.Length != FooterLayout.FooterSize)
            throw new InvalidOperationException(
                $"footer 크기 불일치! 계산값={FooterLayout.FooterSize}, 실제={result.Length}. C++ footer.h랑 레이아웃 다시 맞춰봐야 함.");

        return result;
    }
}