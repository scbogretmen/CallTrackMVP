using System.ComponentModel.DataAnnotations;

namespace CallTrackMVP.Web.Models.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Ad Soyad zorunludur")]
    [StringLength(200)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
    [StringLength(100)]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rol zorunludur")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = "User";
}
