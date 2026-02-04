using CallTrackMVP.Web.Models;

namespace CallTrackMVP.Web.Services;

public interface IExcelExportService
{
    byte[] ExportCallLogsToExcel(IEnumerable<CallLog> logs);
}
