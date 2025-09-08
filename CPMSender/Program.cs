using CPMSender;
using CPMSender.Models;
using CPMSender.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddDbContext<PosttradeDbContext>(opt=>
    opt
        .UseNpgsql(Environment.GetEnvironmentVariable("POSTTRADE_DB"))
        .UseSnakeCaseNamingConvention());
builder.Services.Configure<BotConfig>(builder.Configuration.GetSection("BotConfig"));
builder.Services.AddSerilog();
builder.Services.AddScoped<ICpmRepo, CpmRepo>();
builder.Services.AddHttpClient<IIndexSender, IndexSender>(opt =>
{
    opt.BaseAddress = new Uri(IndexSender.BaseAddress);
    opt.DefaultRequestHeaders.TryAddWithoutValidation("APIKEY", Environment.GetEnvironmentVariable("API_KEY"));
});

builder.Services.AddQuartz(options =>
{
    var jobKey = JobKey.Create(nameof(IndexJob));

    options.AddJob<IndexJob>(jobKey)
        .AddTrigger(trigger => trigger
            .ForJob(jobKey)
            .WithIdentity($"{nameof(IndexJob)}-trigger")
            .WithCronSchedule("0 0 10-20 * * ?", x => 
                x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")))
        );
});
builder.Services.AddQuartzHostedService();



var host = builder.Build();
host.Run();