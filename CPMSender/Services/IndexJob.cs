using CPMSender.Models;
using Quartz;

namespace CPMSender.Services;

public class IndexJob(IIndexSender sender, ICpmRepo cpmRepo, ILogger<IndexJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("Executing index job");
            var dt = DateTime.UtcNow;
            dt = new DateTime(dt.Year, dt.Month, dt.Day,  dt.Hour, dt.Minute, 0);
            
            
            var cpm = await cpmRepo.GetLatest30MinCpmAsync(dt,context.CancellationToken);

            await sender.SendCpmAsync(new CurrentPriceOfMarket()
            {
                Amount = 0,
                TradeTime = dt,
                Price = CountCpm(cpm, dt)
            },context.CancellationToken);
            
            logger.LogInformation("Completed index job");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
    
    private static double CountCpm(IEnumerable<CurrentPriceOfMarket> cpm, DateTime dt)
    {
        var startTime = dt.AddMinutes(-30).ToUniversalTime();
        var endTime = dt.ToUniversalTime();
            
        var prices = new List<CurrentPriceOfMarket>();

        while (startTime < endTime)
        {
            var currCpm = cpm.Where(c => 
                    c.TradeTime.Year == startTime.Year && 
                    c.TradeTime.Month == startTime.Month &&
                    c.TradeTime.Day == startTime.Day && 
                    c.TradeTime.Hour == startTime.Hour &&
                    c.TradeTime.Minute == startTime.Minute)
                .ToList();

            var pr = currCpm.Count == 0
                ? prices.LastOrDefault()?.Price??cpm.First(x => x.TradeTime < startTime).Price
                : currCpm.Average(x => x.Price);
            prices.Add(new CurrentPriceOfMarket(){ Price = pr, TradeTime = startTime });
        
            startTime +=  TimeSpan.FromMinutes(1);
        }
    
        return Math.Round(prices.Average(x=>x.Price), 4);

    }
}