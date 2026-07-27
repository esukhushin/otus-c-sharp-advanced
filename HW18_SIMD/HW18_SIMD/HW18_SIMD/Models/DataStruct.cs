namespace HW18_SIMD.Models
{
    public ref struct DataStruct<T>
    {
        public ReadOnlySpan<T> Command { get; set; }
        public ReadOnlySpan<T> Key { get; set; }
        public ReadOnlySpan<T> Value { get; set; }
    }
}
