using System.IO;
using System.Security.Cryptography;
using WpfBuilder.Models;

namespace WpfBuilder.Services;

public static class PatchBuilder
{
    public static void Build(string stubExePath, string outputExePath, string windowTitle, List<DiskInput> disks, string? iconPath = null, IProgress<double>? progress = null)
    {
        if (disks.Count == 0)
            throw new ArgumentException("디스크(xdelta)가 최소 1개는 있어야 함");

        File.Copy(stubExePath, outputExePath, overwrite: true);

        if (!string.IsNullOrWhiteSpace(iconPath) && File.Exists(iconPath))
            IconEmbedder.EmbedIcon(outputExePath, iconPath);

        progress?.Report(0.1);

        ulong currentOffset = (ulong)new FileInfo(outputExePath).Length;
        var diskInfos = new List<DiskBuildInfo>();

        using (var outStream = new FileStream(outputExePath, FileMode.Append, FileAccess.Write))
        {
            for (int i = 0; i < disks.Count; i++)
            {
                var disk = disks[i];
                var xdeltaBytes = File.ReadAllBytes(disk.XdeltaFilePath);

                string md5 = "";

                if (!string.IsNullOrEmpty(disk.OriginalFilePathForMd5) && File.Exists(disk.OriginalFilePathForMd5))
                    md5 = CalcMd5(disk.OriginalFilePathForMd5);

                ulong xdeltaOffset = currentOffset;

                outStream.Write(xdeltaBytes, 0, xdeltaBytes.Length);
                currentOffset += (ulong)xdeltaBytes.Length;

                ulong bgOffset = 0;
                uint bgSize = 0;

                if (!string.IsNullOrEmpty(disk.BgImagePath) && File.Exists(disk.BgImagePath))
                {
                    var bgBytes = File.ReadAllBytes(disk.BgImagePath);
                    bgOffset = currentOffset;
                    bgSize = (uint)bgBytes.Length;
                    outStream.Write(bgBytes, 0, bgBytes.Length);
                    currentOffset += (ulong)bgBytes.Length;
                }

                diskInfos.Add(new DiskBuildInfo
                {
                    TargetFileName = disk.TargetFileName,
                    OriginalMd5 = md5,
                    XdeltaOffset = xdeltaOffset,
                    XdeltaSize = (uint)xdeltaBytes.Length,
                    BgOffset = bgOffset,
                    BgSize = bgSize,
                });

                progress?.Report(0.1 + (double)(i + 1) / disks.Count * 0.8);
            }

            var footerInfo = new FooterBuildInfo
            {
                WindowTitle = windowTitle,
                Disks = diskInfos,
            };

            var footerBytes = FooterWriter.Build(footerInfo);

            outStream.Write(footerBytes, 0, footerBytes.Length);
        }

        progress?.Report(1.0);
    }

    public static string CalcMd5(string filePath)
    {
        using var md5 = MD5.Create();
        using var fs = File.OpenRead(filePath);
        var hash = md5.ComputeHash(fs);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}