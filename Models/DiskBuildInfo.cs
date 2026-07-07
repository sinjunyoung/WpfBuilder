namespace WpfBuilder.Models;

public class DiskBuildInfo
{
    public required string TargetFileName { get; init; }

    public required string OriginalMd5 { get; init; }

    public required ulong XdeltaOffset { get; init; }

    public required uint XdeltaSize { get; init; }

    public ulong BgOffset { get; init; } = 0;

    public uint BgSize { get; init; } = 0;
}