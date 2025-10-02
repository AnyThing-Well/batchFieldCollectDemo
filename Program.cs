using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddTransient<TranslateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var users = new List<User>
{
    new User { Name = "张三", Country = "中国", IsEnable = EnableEnum.Enable},
    new User { Name = "李四", Country = "美国", IsEnable = EnableEnum.Disable },
    new User { Name = "王五", Country = "英国", IsEnable = EnableEnum.Enable},
    new User { Name = "赵六", Country = "法国", IsEnable = EnableEnum.Disable },
    new User { Name = "田七", Country = "德国", IsEnable = EnableEnum.Enable},
};


app.MapGet("/weatherforecast", async ([FromServices] TranslateService translateService) =>
{
    var res1 = await translateService
        .BuildCollect(users)
        .MapTranslation(a => a.IsEnable.GetDescription(), (a, translated) => a.IsEnableName = translated)
        .ExecuteAsync();

    //自动生成 Setter
    var res2 = await translateService
        .BuildCollect(users)
        .MapTranslation(a => a.IsEnable.GetDescription(), t => t.IsEnableName)
        .ExecuteAsync();
    return new { res1, res2 };
})
.WithName("GetWeatherForecast");

app.Run();

public class User
{
    public string Name { get; set; } = null!;

    public string Country { get; set; } = null!;

    public EnableEnum IsEnable { get; set; }

    public string IsEnableName { get; set; } = null!;
}
