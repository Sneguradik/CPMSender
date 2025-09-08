using Quartz;

namespace CPMSender.Services;

public class IndexJob(IIndexSender sender, ICpmRepo cpmRepo, ILogger<IndexJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("Executing index job");
            var cpm = await cpmRepo.GetLatestCpmAsync(context.CancellationToken);
            if (cpm == null)
            {
                logger.LogWarning("Current price of market is null.");
                return;
            }

            await sender.SendCpmAsync(cpm, context.CancellationToken);
            
            logger.LogInformation("Completed index job");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
}