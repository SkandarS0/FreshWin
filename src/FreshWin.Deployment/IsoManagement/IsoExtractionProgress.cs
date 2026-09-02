namespace FreshWin.Deployment.IsoManagement
{
    public readonly record struct IsoExtractionProgress(long BytesCopied, long TotalBytes)
    {
        public double Percent => TotalBytes <= 0 ? 0 : Math.Min(100d, (double)BytesCopied / TotalBytes * 100d);
    }
}
