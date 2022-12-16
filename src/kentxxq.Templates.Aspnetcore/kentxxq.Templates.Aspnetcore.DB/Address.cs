using SqlSugar;

namespace kentxxq.Templates.Aspnetcore.DB;

[SugarTable(tableName: nameof(Address), tableDescription: "地址")]
public class Address
{
    [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
    public int Id { get; set; }

    [SugarColumn] public string UserAddress { get; set; } = null!;

    [SugarColumn] public string Name { get; set; } = null!;

    [SugarColumn] public string Phone { get; set; } = null!;

    [SugarColumn] public int Uid { get; set; }
}