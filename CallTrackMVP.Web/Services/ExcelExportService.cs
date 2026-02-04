using ClosedXML.Excel;
using CallTrackMVP.Web.Models;

namespace CallTrackMVP.Web.Services;

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportCallLogsToExcel(IEnumerable<CallLog> logs)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Çağrı Kayıtları");

        var headers = new[] { "Sıra No", "Çağrı No", "Çağrı Türü", "Teknisyen Adı", "Tarih", "Mevcut Çağrı Saat", "Oluşturan", "Güncelleme Adedi", "Oluşturulma Tarihi" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        int row = 2;
        foreach (var log in logs)
        {
            worksheet.Cell(row, 1).Value = log.SiraNo;
            worksheet.Cell(row, 2).Value = log.CagriNo;
            worksheet.Cell(row, 3).Value = log.CagriTuru;
            worksheet.Cell(row, 4).Value = log.TeknisyenAdi;
            worksheet.Cell(row, 5).Value = log.Tarih.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 6).Value = log.MevcutCagriSaat;
            worksheet.Cell(row, 7).Value = log.CreatedByUser?.FullName ?? "";
            worksheet.Cell(row, 8).Value = log.Updates?.Count ?? 0;
            worksheet.Cell(row, 9).Value = log.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream, false);
        return stream.ToArray();
    }
}
