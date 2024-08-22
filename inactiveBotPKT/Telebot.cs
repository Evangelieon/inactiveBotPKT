using System.Data.SqlClient;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace inactiveBotPKT
{

    internal class Telebot
    {
        private CancellationTokenSource cts = new();
        private static readonly string BotToken = "7265553435:AAHep0LWC61bVo_Oswfp3uaKYGC5m7k5pqE";
        private static readonly TelegramBotClient Bot = new TelegramBotClient(BotToken);
        private static readonly string connectionString = "Server=WIN-4PIS4180I9O\\MSSQLSERVER01;Database=master;Trusted_Connection=True;";
        public async Task BotStartResiving()
        {
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            Bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
            );
            Console.WriteLine("Бот запущен...");
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            if (update.Message.From is not { } From)
                return;

            if (update.Message.From.Username is not { } MessageFromUsername)
                return;
            Console.WriteLine(update.Message.Chat.Id);
            if (update.Message.Text != null)
            {
                await UpdateUserActivity(update.Message.From.Id, update.Message.From.Username);

            }

        }

        private async Task UpdateUserActivity(long userId, string username)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    IF EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId)
                    BEGIN
                        UPDATE Users SET LastActive = @LastActive WHERE UserId = @UserId;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Users (UserId, Username, LastActive) VALUES (@UserId, @Username, @LastActive);
                    END";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@Username", username ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastActive", DateTime.UtcNow);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task CheckInactivity()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT Username 
                    FROM Users 
                    WHERE LastActive < DATEADD(day, -30, GETUTCDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string username = reader.GetString(0);
                            await NotifyInactiveUser(username);
                        }
                    }
                }
            }
        }

        private async Task NotifyInactiveUser(string username)
        {
            string message = $"{username} получил статус неактивный, потому что за последние 30 дней от него не было ни одного сообщения в чате.";
         //   await Bot.SendTextMessageAsync(chatId: -4286137952, text: message);
        }
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
