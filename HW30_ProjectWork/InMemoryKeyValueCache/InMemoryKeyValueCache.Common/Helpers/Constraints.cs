namespace InMemoryKeyValueCache.Common.Helpers
{
    public static class Constraints
    {
        public static int TotalBytes = 4096;
        public static int RentTotalBytes = TotalBytes * 2;
        public static int MaxClientSemaphore = 10;
    }
}
