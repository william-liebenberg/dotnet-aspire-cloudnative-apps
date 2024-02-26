public static class FileSizeFormatterExtensions
{
    static readonly string[] suf = [" Bytes", " KB", " MB", " GB", " TB", " PB", " EB"]; //Longs run out around EB

    //static readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

    public static string FormatSize(this int bytes)
    {
        return FormatSize((long)bytes);
    }

    public static string FormatSize(this long byteCount)
    {
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }
}