namespace MobilePackageGen.Wof
{
    [Flags]
    public enum COMPRESS_ALGORITHM : uint
    {
        COMPRESS_ALGORITHM_INVALID = 0,
        COMPRESS_ALGORITHM_NULL = 1,
        COMPRESS_ALGORITHM_MSZIP = 2,
        COMPRESS_ALGORITHM_XPRESS = 3,
        COMPRESS_ALGORITHM_XPRESS_HUFF = 4,
        COMPRESS_ALGORITHM_LZMS = 5,
        COMPRESS_ALGORITHM_MAX = 6,
        COMPRESS_RAW = 1 << 29
    }
}
