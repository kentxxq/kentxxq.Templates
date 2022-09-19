using System.Reflection;
using SqlSugar;

var db = new SqlSugarClient(new ConnectionConfig
{
    ConnectionString =
        "Server=yourhost;Port=3306;Database=k_webapi;Username=username;Password=password;MinimumPoolSize=5;maximumpoolsize=50;",
    DbType = DbType.MySql,
    IsAutoCloseConnection = true
});


db.DbMaintenance.CreateDatabase();

var types = Assembly
    .LoadFrom("kentxxq.Templates.Aspnetcore.dll") //如果 .dll报错，可以换成 xxx.exe 有些生成的是exe 
    .GetTypes().Where(it => it.FullName.StartsWith("kentxxq.Templates.Aspnetcore")) //命名空间过滤，当然你也可以写其他条件过滤
    .ToArray(); //断点调试一下是不是需要的Type，不是需要的在进行过滤

db.CodeFirst.SetStringDefaultLength(200).InitTables(types); //根据types创建表