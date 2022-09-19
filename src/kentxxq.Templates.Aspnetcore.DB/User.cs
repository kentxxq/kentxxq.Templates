using SqlSugar;

namespace kentxxq.Templates.Aspnetcore.DB;

public class User
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    [SugarColumn]
    public string Username { get; set; } = null!;

    [SugarColumn]
    public string Password { get; set; } = null!;

    [SugarColumn]
    public DateTime? LastLoginTime { get; set; }
}