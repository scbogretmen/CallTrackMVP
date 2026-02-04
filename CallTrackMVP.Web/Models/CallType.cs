using System.ComponentModel.DataAnnotations;

namespace CallTrackMVP.Web.Models;

public class CallType
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad zorunludur")]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
}
