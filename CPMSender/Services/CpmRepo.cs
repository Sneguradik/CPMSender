using CPMSender.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CPMSender.Services;

public interface ICpmRepo
{
    Task<CurrentPriceOfMarket?> GetLatestCpmAsync(CancellationToken cancellationToken = default);
}

public class CpmRepo(PosttradeDbContext dbContext, IOptions<BotConfig> conf) : ICpmRepo
{
    private string BuildQuery() =>
        $"select trade_time, price, amount from \"Indiquote\" where instrument_instrument_id in ({conf.Value.InstrumentId}) order by  trade_time desc limit 1;"
            .ToString();

    public async Task<CurrentPriceOfMarket?> GetLatestCpmAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Database
            .SqlQueryRaw<CurrentPriceOfMarket>(BuildQuery())
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
}