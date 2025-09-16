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
        var instId = conf.Value.InstrumentId;

        return $@"
        WITH p AS (
          SELECT {instId} AS inst,
                 (NOW() AT TIME ZONE 'UTC') - INTERVAL '30 minutes' AS t0
        )
        -- последние 30 минут
        SELECT i.trade_time, i.price, i.amount
        FROM ""Indiquote"" i
        JOIN p ON i.instrument_instrument_id = p.inst
        WHERE i.trade_time >= p.t0

        UNION ALL

        -- последнее значение ПЕРЕД интервалом
        SELECT pv.trade_time, pv.price, pv.amount
        FROM p
        CROSS JOIN LATERAL (
          SELECT trade_time, price, amount
          FROM ""Indiquote""
          WHERE instrument_instrument_id = p.inst
            AND trade_time < p.t0
          ORDER BY trade_time DESC
          LIMIT 1
        ) pv

        ORDER BY trade_time DESC".ToString();
    }

    public async Task<IEnumerable<CurrentPriceOfMarket>> GetLatest30MinCpmAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Database
            .SqlQueryRaw<CurrentPriceOfMarket>(BuildQuery())
            .ToArrayAsync(cancellationToken);
}