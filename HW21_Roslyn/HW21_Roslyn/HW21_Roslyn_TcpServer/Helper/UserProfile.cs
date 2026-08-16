using HW21_Roslyn_TcpServer.Attributes;

namespace HW21_Roslyn_TcpServer.Helper
{
    [GenerateBinarySerializer]
    public partial class UserProfile
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}