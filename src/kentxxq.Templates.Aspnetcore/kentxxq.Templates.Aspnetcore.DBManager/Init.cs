using System.Reflection;
using kentxxq.Templates.Aspnetcore.DB;
using Spectre.Console;
using SqlSugar;

namespace kentxxq.Templates.Aspnetcore.DBManager;

public class Init
{
    private readonly SqlSugarClient _client;

    public Init(SqlSugarClient client)
    {
        _client = client;
    }

    /// <summary>
    /// 同步数据结构
    /// </summary>
    public void FirstTimeCreate()
    {
        AnsiConsole.MarkupLine("[green]开始创建数据库[/]");
        _client.DbMaintenance.CreateDatabase();

#pragma warning disable CS8602 // 解引用可能出现空引用。
        var types = Assembly
            .LoadFrom("kentxxq.Templates.Aspnetcore.DB.dll") //如果 .dll报错，可以换成 xxx.exe 有些生成的是exe 
            .GetTypes()
            .Where(it => it.FullName.StartsWith("kentxxq.Templates.Aspnetcore")) //命名空间过滤，当然你也可以写其他条件过滤
            .ToArray(); //断点调试一下是不是需要的Type，不是需要的在进行过滤
#pragma warning restore CS8602 // 解引用可能出现空引用。
        AnsiConsole.MarkupLine("[green]开始创建表格[/]");
        _client.CodeFirst.SetStringDefaultLength(200).InitTables(types); //根据types创建表
    }

    /// <summary>
    /// 重新初始化
    /// </summary>
    public void ReCreate()
    {
        _client.DbMaintenance.CreateDatabase();
        var tableNameList = _client.DbMaintenance.GetTableInfoList().Select(t => t.Name);
        foreach (var tableName in tableNameList)
        {
            AnsiConsole.MarkupLine($"[red]正在删除{tableName}[/]");
            _client.DbMaintenance.DropTable(tableName);
        }

        FirstTimeCreate();
        InitTableData();
    }

    private void InitTableData()
    {
        int count;
        AnsiConsole.Markup($"初始化表数据-{nameof(User)} ");
        var initUser = new User { Username = "ken", Password = "ken", LastLoginTime = DateTime.Now };
        count = _client.Insertable(initUser).ExecuteCommand();
        AnsiConsole.MarkupLine($"[green]{count}[/]条");

        AnsiConsole.Markup($"初始化表数据-{nameof(Address)}");
        var initAddresses = new List<Address>
        {
            new() { UserAddress = "qcd1", Uid = 1, Name = "ken", Phone = "123" },
            new() { UserAddress = "qcd2", Uid = 1, Name = "ken", Phone = "123" }
        };
        count = _client.Insertable(initAddresses).ExecuteCommand();
        AnsiConsole.MarkupLine($"[green]{count}[/]条");
    }
}