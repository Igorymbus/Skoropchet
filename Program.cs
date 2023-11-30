using System.Diagnostics;

namespace Скоропечатание2
{
    public class Program
    {
        static void Main(string[] args)
        {
            KeyboardTrainer trainer = new KeyboardTrainer();
            trainer.RunTest();
        }
    }

    public class KeyboardTrainer
    {
        private string CurrentText { get; set; }
        private int[] States { get; set; }
        private int Position { get; set; }
        private bool TimerExpired { get; set; }

        public void RunTest()
        {
            do
            {
                Console.Write("Введите свое имя: ");
                string name = Console.ReadLine();

                StartTraining(name);

                Console.WriteLine("\nХотите пройти тест еще раз? (да/нет)");
            } while (Console.ReadLine().ToLower() == "да");
        }

        private void StartTraining(string name)
        {
            CurrentText = "Курчатов Игорь Васильевич (1903–1960) — советский физик, учёный и изобретатель. Вошёл в историю как создатель атомной бомбы в СССР, который в дальнейшем приложил усилия для использования ядерной энергии в мирных целях. Курчатов Игорь Васильевич, биография которого будет весьма интересна для детей, является автором многих важных научных достижений, без которых сложно представить современный мир.";

            Console.Clear();
            Console.WriteLine(CurrentText);

            int downPositionX = Console.CursorLeft;
            int downPositionY = Console.CursorTop;
            Console.Clear();

            States = new int[CurrentText.Length];
            Position = 0;
            TimerExpired = false;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            
            Thread timerThread = new Thread(() =>
            {
                for (int i = 60; i >= 0; i--)
                {
                    Console.SetCursorPosition(0, 10);
                    Console.Write($"Оставшееся время: {i} сек");
                    Thread.Sleep(1000);
                }
                TimerExpired = true;
            });
            timerThread.Start();

            while (true)
            {
                DisplayText();
                var key = Console.ReadKey();

                if (TimerExpired || Position == CurrentText.Length)
                    break;

                CheckInput(key.KeyChar);
                Position++;
            }

            stopwatch.Stop();
            Console.SetCursorPosition(downPositionX, downPositionY + 1);
            Console.WriteLine("Тест окончен.");

            int charactersPerMinute = (int)((Position / 60.0) * stopwatch.Elapsed.TotalSeconds);
            int charactersPerSecond = (int)(Position / stopwatch.Elapsed.TotalSeconds);

            User user = new User
            {
                Name = name,
                CharactersPerMinute = charactersPerMinute,
                CharactersPerSecond = charactersPerSecond
            };

            Leaderboard.UpdateLeaderboard(user);
            DisplayLeaderboard();
        }

        private void DisplayText()
        {
            Console.SetCursorPosition(0, 0);
            int carretX = 0;
            int carretY = 0;

            for (int i = 0; i < CurrentText.Length; i++)
            {
                if (States[i] == 0)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                else if (States[i] == 1)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (States[i] == 2)
                    Console.ForegroundColor = ConsoleColor.Red;

                if (i == Position)
                {
                    carretX = Console.CursorLeft;
                    carretY = Console.CursorTop;
                }

                Console.Write(CurrentText[i]);
            }

            Console.ResetColor();
            Console.SetCursorPosition(carretX, carretY);
        }

        private void CheckInput(char input)
        {
            if (input == CurrentText[Position])
                States[Position] = 1;
            else
                States[Position] = 2;
        }

        private void DisplayLeaderboard()
        {
            Console.WriteLine("\nТаблица рекордов:");

            foreach (var user in Leaderboard.Users)
            {
                Console.WriteLine($"Имя: {user.Name}, CPM: {user.CharactersPerMinute}, CPS: {user.CharactersPerSecond}");
            }
        }
    }

    public class User
    {
        public string Name { get; set; }
        public int CharactersPerMinute { get; set; }
        public int CharactersPerSecond { get; set; }
    }

    public static class Leaderboard
    {
        private const string FilePath = "leaderboard.json";

        public static List<User> Users { get; private set; }

        static Leaderboard()
        {
            Users = File.Exists(FilePath) ? Serializer.DeserializeLeaderboard() : new List<User>();
        }

        public static void UpdateLeaderboard(User user)
        {
            Users.Add(user);
            Users = Users.OrderByDescending(u => u.CharactersPerMinute).ToList();
            Serializer.SerializeLeaderboard(Users);
        }
    }

    public static class Serializer
    {
        private const string FilePath = "leaderboard.json";

        public static void SerializeLeaderboard(List<User> users)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(users);
            File.WriteAllText(FilePath, json);
        }

        public static List<User> DeserializeLeaderboard()
        {
            string json = File.ReadAllText(FilePath);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(json);
        }
    }
}
