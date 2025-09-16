using CPMSender.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CPMSender.Services;

public interface ICpmRepo
{
    Task<IEnumerable<CurrentPriceOfMarket>> GetLatest30MinCpmAsync(CancellationToken cancellationToken = default);
}

public class CpmRepo(PosttradeDbContext dbContext, IOptions<BotConfig> conf) : ICpmRepo
{
    private string BuildQuery()
    {
        return $@"
        SELECT trade_time, price, amount
        FROM (
            SELECT trade_time, price, amount, 1 as source_order
            FROM ""Indiquote""
            WHERE instrument_instrument_id = {conf.Value.InstrumentId} 
              AND trade_time >= NOW() - INTERVAL '30 minutes'
            
            UNION ALL
            
            SELECT trade_time, price, amount, 2 as source_order
            FROM ""Indiquote""
            WHERE instrument_instrument_id = {conf.Value.InstrumentId} 
              AND trade_time < NOW() - INTERVAL '30 minutes'
            ORDER BY trade_time DESC
            LIMIT 1
        ) data
        ORDER BY source_order, trade_time DESC"
            .ToString();
    }

    public async Task<IEnumerable<CurrentPriceOfMarket>> GetLatest30MinCpmAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Database
            .SqlQueryRaw<CurrentPriceOfMarket>(BuildQuery())
            .ToListAsync(cancellationToken);
}