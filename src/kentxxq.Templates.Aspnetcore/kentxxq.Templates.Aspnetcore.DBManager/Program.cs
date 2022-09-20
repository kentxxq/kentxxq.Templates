using System.Reflection;
using Microsoft.Extensions.Configuration;
using SqlSugar;


IConfiguration config = new ConfigurationBuilder()
    .AddUserSecrets(typeof(Program).Assembly)
    .Build();


var db = new SqlSugarClient(new ConnectionConfig
{
    ConnectionString = config["Database:Connectionstring"],
    DbType = DbType.MySql,
    IsAutoCloseConnection = true
});


db.DbMaintenance.CreateDatabase();

#pragma warning disable CS8602 // 解引用可能出现空引用。
var types = Assembly
    .LoadFrom("kentxxq.Templates.Aspnetcore.DB.dll") //如果 .dll报错，可以换成 xxx.exe 有些生成的是exe 
    .GetTypes()
    .Where(it => it.FullName.StartsWith("kentxxq.Templates.Aspnetcore")) //命名空间过滤，当然你也可以写其他条件过滤
    .ToArray(); //断点调试一下是不是需要的Type，不是需要的在进行过滤
#pragma warning restore CS8602 // 解引用可能出现空引用。

db.CodeFirst.SetStringDefaultLength(200).InitTables(types); //根据types创建表