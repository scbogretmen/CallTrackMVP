using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CallTrackMVP.Web.Models;

public class CallLog
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Sıra No zorunludur")]
    public int SiraNo { get; set; }

    [Required(ErrorMessage = "Çağrı No zorunludur")]
    [StringLength(50)]
    public string CagriNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Çağrı Türü zorunludur")]
    [StringLength(50)]
    public string CagriTuru { get; set; } = string.Empty; // Web, Telefon, Eposta

    [Required(ErrorMessage = "Teknisyen Adı zorunludur")]
    [StringLength(200)]
    public string TeknisyenAdi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih zorunludur")]
    [DataType(DataType.Date)]
    public DateTime Tarih { get; set; }

    [Required(ErrorMessage = "Mevcut Çağrı Saati zorunludur")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Geçerli saat formatı HH:mm olmalıdır (örn: 09:30)")]
    [StringLength(5)]
    public string MevcutCagriSaat { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? GuncellenenTarih { get; set; }

    [RegularExpression(@"^$|^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Geçerli saat formatı HH:mm olmalıdır")]
    [StringLength(5)]
    public string? GuncellenenCagriSaat { get; set; }

    [StringLength(500)]
    public string? GuncellemeNedeni { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedByUserId { get; set; }
    [ForeignKey(nameof(CreatedByUserId))]
    [ValidateNever]
    public AppUser CreatedByUser { get; set; } = null!;

    [NotMapped]
    public DateTime TarihDateOnly => Tarih.Date;

    /// <summary>
    /// Tüm güncelleme geçmişi (birden fazla güncelleme desteklenir)
    /// </summary>
    public ICollection<CallLogUpdate> Updates { get; set; } = new List<CallLogUpdate>();
}
