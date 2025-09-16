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
            dt = new DateTime(dt.Year, dt.Month, dt.Day);
            var startTime = dt-TimeSpan.FromMinutes(30);
            
            var cpm = await cpmRepo.GetLatest30MinCpmAsync(context.CancellationToken);
            
            var prices = new List<double>();

            while (startTime < dt)
            {
                var currCpm = cpm.Where(c => 
                    c.TradeTime.Year == startTime.Year && 
                    c.TradeTime.Month == startTime.Month &&
                    c.TradeTime.Day == startTime.Day && 
                    c.TradeTime.Hour == startTime.Hour &&
                    c.TradeTime.Minute == startTime.Minute)
                    .ToList();

                prices.Add(currCpm.Count == 0
                    ? cpm.First(x => x.TradeTime < startTime).Price
                    : currCpm.Average(x => x.Price));


                startTime +=  TimeSpan.FromMinutes(1);
            }
            

            await sender.SendCpmAsync(new CurrentPriceOfMarket()
            {
                Amount = 0,
                TradeTime = dt,
                Price = prices.Average(),
            },context.CancellationToken);
            
            logger.LogInformation("Completed index job");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
}