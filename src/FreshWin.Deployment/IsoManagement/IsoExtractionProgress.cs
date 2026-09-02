namespace FreshWin.Deployment.IsoManagement
{
    public readonly record struct IsoExtractionProgress(long BytesCopied, long TotalBytes)
    {
        public double Percent => TotalBytes == 0 ? 0 : (double)BytesCopied / TotalBytes * 100;
    }
}
