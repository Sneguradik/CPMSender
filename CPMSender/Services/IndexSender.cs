using System.Text.Json;
using CPMSender.Models;
using Microsoft.Extensions.Options;

namespace CPMSender.Services;

public interface IIndexSender
{
    Task SendCpmAsync(CurrentPriceOfMarket cpm, CancellationToken cancellationToken = default);
}

public class IndexSender(HttpClient httpClient, IOptions<BotConfig> conf, ILogger<IndexSender> logger) : IIndexSender
{
    public const string BaseAddress = "https://indexapi.spbexchange.ru";
    public async Task SendCpmAsync(CurrentPriceOfMarket cpm, CancellationToken cancellationToken = default)
    {
        var dt = DateTime.UtcNow;
        dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
        var msg = new HttpRequestMessage()
        {
            RequestUri = new Uri(conf.Value.Endpoint,  UriKind.Relative),
            Method = HttpMethod.Post,
            Content = new StringContent(JsonSerializer.Serialize(new []{new IndexModel(){Timestamp = dt, Value = Math.Round(cpm.Price, 2)}})),
        };
        msg.Content.Headers.ContentType = new("application/json");
        var response = await httpClient.SendAsync(msg, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError(content);
            throw new Exception(response.ReasonPhrase);
        }
        
        logger.LogInformation($"Sent current price of market {dt} {cpm.Price}");
    }
}