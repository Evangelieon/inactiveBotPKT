using inactiveBotPKT;
namespace TelegramInactivityBot
{
    class Program
    {

        private static Timer _timer;
        private static Telebot telebot = new Telebot();
        static async Task Main(string[] args)
        {
            _timer = new Timer(CheckInactivityCallback, null, TimeSpan.Zero, TimeSpan.FromHours(1));

            Console.WriteLine("Бот запущен. Нажмите Enter для завершения...");
           
            telebot.BotStartResiving();
            Console.ReadLine();
        }

        private static void CheckInactivityCallback(object state)
        {
            CheckInactivity();
        }

        private static void CheckInactivity()
        {
            telebot.CheckInactivity();
            Console.WriteLine("Проверка неактивности выполнена в: " + DateTime.Now);
        }
           
    }
}
