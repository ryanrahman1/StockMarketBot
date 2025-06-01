using Discord;
using Discord.WebSocket;
using System.Net.Http.Json;
using dotenv.net;
using dotenv;


namespace StockMarketBot;


class Program
{
    
    
    
    private readonly DiscordSocketClient _client = new(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
    });

    private readonly string Token;
    private readonly string FinnhubToken;
    

    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
        var program = new Program();
        await program.RunBotAsync();
    }

    public Program()
    {
        Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN")?? throw new Exception("DISCORD_TOKEN not found in .env file");
        FinnhubToken = Environment.GetEnvironmentVariable("FINNHUB_TOKEN")?? throw new Exception("FINNHUB_TOKEN not found in .env file");
    }
    
    private async Task RunBotAsync()
    {
        _client.Log += LogAsync;
        _client.Ready += OnBotReadyAsync;
        _client.MessageReceived += HandleMessageAsync;

        await _client.LoginAsync(TokenType.Bot, Token);
        await _client.StartAsync();
        
        await Task.Delay(-1);
    }
    
    private async Task OnBotReadyAsync()
    {
        foreach (var guild in _client.Guilds)
        {
            var channel = guild.TextChannels.FirstOrDefault(c => c.Name == "general");
            if (channel != null)
            {
                await channel.SendMessageAsync("Stock Bot is online and ready to go!");
            }
        }
    }

    private Task LogAsync(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(SocketMessage msg)
    {
        if (msg.Author.IsBot) return;
        

        string content = msg.Content.Trim();
        if (content.StartsWith("!stockprice "))
        {
            string symbol = content.Replace("!stockprice ", "").ToUpper();
            await HandleStockPriceCommand(msg, symbol);
        }
        else if (content.StartsWith("!news "))
        {
            string symbol = content.Replace("!news ", "").ToUpper();
            await HandleNewsCommand(msg, symbol);
        }
        else if (content.StartsWith("!help"))
        {
            await HandleHelpCommand(msg);
        }
        
    }

    private async Task HandleStockPriceCommand(SocketMessage msg, string symbol)
    {
        using var http = new HttpClient();
        var url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={FinnhubToken}";

        try
        {
            var response = await http.GetFromJsonAsync<FinnhubQuote>(url);
            if (response is not null && response.c != 0)
            {
                await msg.Channel.SendMessageAsync(
                    $"**{symbol}** Current Price: **{response.c:F2}** (Prev Close: ${response.pc:F2})");
            }
            else
            {
                await msg.Channel.SendMessageAsync($"Couldn't find stock with symbol {symbol}");
            }
        }
        catch
        {
            await msg.Channel.SendMessageAsync($"Error fetching data for `{symbol}`");
        }
    }

    private async Task HandleNewsCommand(SocketMessage msg, String symbol)
    {
        using var http = new HttpClient();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var weekAgo = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var url = $"https://finnhub.io/api/v1/company-news?symbol={symbol}&from={weekAgo}&to={today}&token={FinnhubToken}";

        try
        {
            var articles = await http.GetFromJsonAsync<List<FinnhubNews>>(url);
            Console.WriteLine(url);

            if (articles != null && articles.Count > 0)
            {
                var topArticle = articles.First();
                await msg.Channel.SendMessageAsync(
                    $"**{symbol} News**\n**{topArticle.headline}**\n{topArticle.url}");
            }
            else
            {
                await msg.Channel.SendMessageAsync($"Couldn't find news for {symbol}");
            }
        }
        catch
        {
            await msg.Channel.SendMessageAsync($"Error fetching data for `{symbol}`");
        }
    }

    private async Task HandleHelpCommand(SocketMessage msg)
    {
        string helpText = "**Stock Bot Commands:**\n" +
                          "`!stockprice SYMBOL` - Get current stock price\n" +
                          "`!news SYMBOL` - Get latest news headline\n" +
                          "`!help` - Show this help menu";
        
        await msg.Channel.SendMessageAsync(helpText);
    }
    
    
    
}

class FinnhubQuote
{
    public decimal c { get; set; } // current price
    public decimal pc { get; set; } // previous close
}

class FinnhubNews
{
    public string headline { get; set; }
    public string url { get; set; }
}