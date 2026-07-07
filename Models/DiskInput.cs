namespace WpfBuilder.Models;

public class DiskInput
{
    public required string TargetFileName { get; init; }

    public required string XdeltaFilePath { get; init; }

    public string? OriginalFilePathForMd5 { get; init; }

    public string? BgImagePath { get; init; }
}