namespace FreshWin.Deployment.IsoManagement.Exceptions
{
    public class IsoFileNotUdfException(FileInfo isoPath) : Exception($"The ISO file is not UDF-formatted: {isoPath.FullName}");
}
