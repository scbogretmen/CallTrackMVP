namespace CallTrackMVP.Web.Models.ViewModels;

public class UserListViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int CallLogCount { get; set; }
}
