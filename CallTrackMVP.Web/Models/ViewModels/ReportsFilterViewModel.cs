using System.ComponentModel.DataAnnotations;

namespace CallTrackMVP.Web.Models.ViewModels;

public class ReportsFilterViewModel
{
    [Display(Name = "Başlangıç Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? TarihBaslangic { get; set; }

    [Display(Name = "Bitiş Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? TarihBitis { get; set; }

    [Display(Name = "Teknisyen Adı")]
    public string? TeknisyenAdi { get; set; }

    [Display(Name = "Çağrı Türü")]
    public string? CagriTuru { get; set; }

    [Display(Name = "Çağrı No")]
    public string? CagriNo { get; set; }
}
