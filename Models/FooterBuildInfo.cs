namespace WpfBuilder.Models;

public class FooterBuildInfo
{
    public required string WindowTitle { get; init; }

    public required List<DiskBuildInfo> Disks { get; init; }
}