using kentxxq.Templates.Aspnetcore.DBManager;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using SqlSugar;

IConfiguration config = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly)
    .Build();


var db = new SqlSugarClient(new ConnectionConfig
{
    ConnectionString = config["Database:ConnectionString"],
    DbType = (DbType)int.Parse(config["Database:DbType"] ?? "0"),
    IsAutoCloseConnection = true
});

var init = new Init(db);


var fruit = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[red]请小心[/]选择对数据库的操作?")
        .AddChoices("首次初始化", "重新初始化"));

switch (fruit)
{
    case "首次初始化":
        init.FirstTimeCreate();
        break;
    case "重新初始化":
        init.ReCreate();
        break;
    default:
        AnsiConsole.MarkupLine("无此选项");
        break;
}