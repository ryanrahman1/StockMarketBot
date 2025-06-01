# StockMarketBot

A Discord bot for accessing real-time stock market data using the [Finnhub API](https://finnhub.io/). Built with C# and Discord.Net.

## Features

* `!stockprice SYMBOL` – Get the current price and previous close of a stock
* `!news SYMBOL` – Fetch the latest news headline related to a stock
* `!help` – Show available commands
* Sends a message in the general channel when the bot starts up

## Tech Stack

* .NET 8
* [Discord.Net](https://github.com/discord-net/Discord.Net)
* [Finnhub API](https://finnhub.io/)
* dotenv.net for environment variable management

## Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/ryanrahman1/StockMarketBot.git
   cd StockMarketBot
   ```

2. Create a `.env` file in the root directory:

   ```
   DISCORD_TOKEN=your_discord_bot_token
   FINNHUB_TOKEN=your_finnhub_api_key
   ```

3. Install dependencies:

   ```bash
   dotnet restore
   ```

4. Run the bot:

   ```bash
   dotnet run
   ```

## Example Commands

```
!stockprice AAPL
!news TSLA
!help
```

## Notes

* Your bot must have the Message Content Intent enabled in the Discord Developer Portal.
* The `.env` file must be placed in the root directory, at the same level as the `.csproj` file.
* Do not hardcode your tokens; keep them in the `.env` file.

