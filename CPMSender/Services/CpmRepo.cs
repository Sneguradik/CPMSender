using System.Globalization;
using CPMSender.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CPMSender.Services;

public interface ICpmRepo
{
    Task<IEnumerable<CurrentPriceOfMarket>> GetLatest30MinCpmAsync(DateTime from, CancellationToken cancellationToken = default);
}

public class CpmRepo(PosttradeDbContext dbContext) : ICpmRepo
{
    private string BuildQuery(DateTime from)
    {
        var instId = 2679262;
        var fromUtc = from.ToUniversalTime();
        var ts = fromUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        return $@"
        WITH p AS (
          SELECT {instId} AS inst,
                 ('{ts}'::timestamp) - INTERVAL '30 minutes' AS t0
        )
        -- последние 30 минут
        SELECT i.trade_time, i.price, i.amount
        FROM ""Indiquote"" i
        JOIN p ON i.instrument_instrument_id = p.inst
        WHERE i.trade_time >= t0 and i.trade_time < '{ts}'::timestamp

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

    public async Task<IEnumerable<CurrentPriceOfMarket>> GetLatest30MinCpmAsync(DateTime from, CancellationToken cancellationToken = default) =>
        await dbContext.Database
            .SqlQueryRaw<CurrentPriceOfMarket>(BuildQuery(from))
            .ToArrayAsync(cancellationToken);
}