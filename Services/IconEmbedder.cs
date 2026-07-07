using System.IO;
using System.Runtime.InteropServices;

namespace WpfBuilder.Services;

public static class IconEmbedder
{
    private const int RT_ICON = 3;
    private const int RT_GROUP_ICON = 14;
    private const ushort LANG_NEUTRAL = 0;
    private const int GROUP_ICON_RESOURCE_ID = 101;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr BeginUpdateResource(string pFileName, [MarshalAs(UnmanagedType.Bool)] bool bDeleteExistingResources);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool UpdateResource(IntPtr hUpdate, IntPtr lpType, IntPtr lpName, ushort wLanguage, byte[] lpData, uint cbData);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool EndUpdateResource(IntPtr hUpdate, [MarshalAs(UnmanagedType.Bool)] bool fDiscard);

    private static IntPtr MakeIntResource(int id) => (IntPtr)id;

    public static void EmbedIcon(string exePath, string icoPath)
    {
        var icoBytes = File.ReadAllBytes(icoPath);

        using var ms = new MemoryStream(icoBytes);
        using var br = new BinaryReader(ms);

        ushort reserved = br.ReadUInt16();
        ushort type = br.ReadUInt16();
        ushort count = br.ReadUInt16();
        if (type != 1)
            throw new InvalidDataException($"'{icoPath}' 는 올바른 .ico 파일이 아님 (type={type})");

        var entries = new List<(byte w, byte h, byte colors, byte res, ushort planes, ushort bitCount, uint bytesInRes, uint imageOffset)>();
        for (int i = 0; i < count; i++)
        {
            byte w = br.ReadByte();
            byte h = br.ReadByte();
            byte colors = br.ReadByte();
            byte res = br.ReadByte();
            ushort planes = br.ReadUInt16();
            ushort bitCount = br.ReadUInt16();
            uint bytesInRes = br.ReadUInt32();
            uint imageOffset = br.ReadUInt32();
            entries.Add((w, h, colors, res, planes, bitCount, bytesInRes, imageOffset));
        }

        IntPtr hUpdate = BeginUpdateResource(exePath, false);
        if (hUpdate == IntPtr.Zero)
            throw new InvalidOperationException($"BeginUpdateResource 실패 (Win32 에러 {Marshal.GetLastWin32Error()})");

        try
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var (w, h, colors, res, planes, bitCount, bytesInRes, imageOffset) = entries[i];
                var imageData = new byte[bytesInRes];
                Array.Copy(icoBytes, (int)imageOffset, imageData, 0, (int)bytesInRes);

                if (!UpdateResource(hUpdate, MakeIntResource(RT_ICON), MakeIntResource(i + 1),
                        LANG_NEUTRAL, imageData, (uint)imageData.Length))
                {
                    throw new InvalidOperationException(
                        $"RT_ICON 등록 실패 (idx={i}, Win32 에러 {Marshal.GetLastWin32Error()})");
                }
            }

            using var groupMs = new MemoryStream();
            using var bw = new BinaryWriter(groupMs);
            bw.Write(reserved);
            bw.Write(type);
            bw.Write(count);
            for (int i = 0; i < entries.Count; i++)
            {
                var (w, h, colors, res, planes, bitCount, bytesInRes, imageOffset) = entries[i];
                bw.Write(w);
                bw.Write(h);
                bw.Write(colors);
                bw.Write(res);
                bw.Write(planes);
                bw.Write(bitCount);
                bw.Write(bytesInRes);
                bw.Write((ushort)(i + 1));
            }
            bw.Flush();
            var groupBytes = groupMs.ToArray();

            if (!UpdateResource(hUpdate, MakeIntResource(RT_GROUP_ICON), MakeIntResource(GROUP_ICON_RESOURCE_ID),
                    LANG_NEUTRAL, groupBytes, (uint)groupBytes.Length))
            {
                throw new InvalidOperationException(
                    $"RT_GROUP_ICON 등록 실패 (Win32 에러 {Marshal.GetLastWin32Error()})");
            }

            if (!EndUpdateResource(hUpdate, false))
                throw new InvalidOperationException(
                    $"EndUpdateResource 실패 (Win32 에러 {Marshal.GetLastWin32Error()})");
        }
        catch
        {
            EndUpdateResource(hUpdate, true);
            throw;
        }
    }
}