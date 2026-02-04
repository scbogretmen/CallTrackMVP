using System.ComponentModel.DataAnnotations.Schema;

namespace CallTrackMVP.Web.Models;

/// <summary>
/// Kullanıcının bir çağrı kaydındaki güncellemeleri okuduğunu/gördüğünü işaretler.
/// </summary>
public class CallLogAcknowledgment
{
    public int UserId { get; set; }
    public int CallLogId { get; set; }
    /// <summary>Kullanıcının gördüğü en son güncelleme Id'si</summary>
    public int LastAcknowledgedUpdateId { get; set; }

    [ForeignKey(nameof(UserId))]
    public AppUser User { get; set; } = null!;

    [ForeignKey(nameof(CallLogId))]
    public CallLog CallLog { get; set; } = null!;
}
