using System.ComponentModel.DataAnnotations;

namespace CallTrackMVP.Web.Models;

public class AppUser
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad Soyad zorunludur")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "User"; // "Admin" or "User"

    public ICollection<CallLog> CallLogs { get; set; } = new List<CallLog>();
}
