using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CallTrackMVP.Web.Models;

/// <summary>
/// Bir çağrı kaydına yapılan her güncelleme ayrı kayıt olarak tutulur.
/// </summary>
public class CallLogUpdate
{
    public int Id { get; set; }

    public int CallLogId { get; set; }
    [ForeignKey(nameof(CallLogId))]
    public CallLog CallLog { get; set; } = null!;

    [Required(ErrorMessage = "Güncellenen tarih zorunludur")]
    [DataType(DataType.Date)]
    public DateTime GuncellenenTarih { get; set; }

    [Required(ErrorMessage = "Güncellenen çağrı saati zorunludur")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Geçerli saat formatı HH:mm olmalıdır")]
    [StringLength(5)]
    public string GuncellenenCagriSaat { get; set; } = string.Empty;

    [Required(ErrorMessage = "Güncelleme nedeni zorunludur")]
    [StringLength(500)]
    public string GuncellemeNedeni { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
