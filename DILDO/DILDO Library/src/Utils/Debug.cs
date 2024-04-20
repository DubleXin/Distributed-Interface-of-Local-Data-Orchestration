using System.Text;

namespace DILDO
{
    public static class Debug
    {
        private const ConsoleColor DEFAULT = ConsoleColor.White;

        public static void Log<From>(string message)
        {
#if UNITY_5_OR_NEWER
            UnityEngine.Debug.Log(message);
#else
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[LOG]     ");

            NonAuthLog<From>(message);
#endif
        }
        public static void Log(string message)
        {
#if UNITY_5_OR_NEWER
            UnityEngine.Debug.Log(message);
#else
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[LOG]     ");

            NonAuthLog(message);
#endif
        }

        public static void NonAuthLog<From>(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            string from = typeof(From).Name;
            string time = DateTime.Now.ToString();

            Console.Write($"{from}\t[{time}]:\t");
            NonAuthLog(message);
        }
        public static void NonAuthLog(string message)
        {
            Console.ForegroundColor = DEFAULT;
            bool buffer = false;

            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] != '<' ||
                    i + 4 >= message.Length ||
                    message[i + 4] != '>' ||
                    (i > 0 && message[i - 1] == '\\'))
                    continue;

                if (!buffer)
                {
                    Console.Write(message.Substring(0, i));
                    buffer = true;
                }

                Console.ForegroundColor = message.Substring(i + 1, 3).ToUpper() switch
                {
                    "RED" => ConsoleColor.Red,
                    "DRE" => ConsoleColor.DarkRed,
                    "BLU" => ConsoleColor.Blue,
                    "DBL" => ConsoleColor.DarkBlue,
                    "GRE" => ConsoleColor.Green,
                    "DGE" => ConsoleColor.DarkGreen,
                    "WHI" => ConsoleColor.White,
                    "GRA" => ConsoleColor.Gray,
                    "DGA" => ConsoleColor.DarkGray,
                    "CYA" => ConsoleColor.Cyan,
                    "DCY" => ConsoleColor.DarkCyan,
                    "YEL" => ConsoleColor.Yellow,
                    "DYE" => ConsoleColor.DarkYellow,
                    "MAG" => ConsoleColor.Magenta,
                    "DMA" => ConsoleColor.DarkMagenta,
                    _ => Console.ForegroundColor,
                };

                int j = 1 + (i += 5);
                for (; j < message.Length - 2; j++)
                {
                    if (message.Substring(j - 1, 3) == "</>" ||
                        message[j - 1] == '<' && j + 3 < message.Length && message[j + 3] == '>')
                    {
                        buffer = false;
                        break;
                    }
                }

                if (buffer && j - i + 2 < message.Length)
                    j += 3;

                if (i != 0)
                    Console.Write(message.Substring(i, j - i - 1));
                else
                    Console.Write(message.Substring(i - 1, j - i));

                if (!buffer)
                {
                    Console.ForegroundColor = DEFAULT;
                    buffer = true;
                }
            }
            Console.Write("\n");
            Console.ForegroundColor = DEFAULT;
        }

        public static void Exception(string situation, string reason)
        {
            int distance = situation.Length - reason.Length;

            if (distance < 2)
                distance = 2;

            int firstGap = distance / 2;
            int secondGap = distance - firstGap;

            StringBuilder builder = new();
            if(situation.Length >= reason.Length)
            {
                builder.Append($"<RED>[EXCEPTION]<DGA> >>>>>>>>>>>>>>>>> <DRE>\"{situation}\"<DGA> <<<<<<<<<<<<<<<<<\n");
                builder.Append($"<RED>[REASON]   <DGA> >>>>>>>>>>>>>>>>> ");

                for (int i = 0; i < firstGap; i++)
                    builder.Append(' ');

                builder.Append($"<DRE>\"{reason}\"<DGA>");

                for (int i = 0; i < secondGap; i++)
                    builder.Append(' ');

                builder.Append(" <<<<<<<<<<<<<<<<<");
            }
            else
            {
                builder.Append($"<RED>[EXCEPTION]<DGA> >>>>>>>>>>>>>>>>> ");

                for (int i = 0; i < -firstGap; i++)
                    builder.Append(' ');

                builder.Append($"<DRE>\"{situation}\"<DGA>");

                for (int i = 0; i < -secondGap; i++)
                    builder.Append(' ');

                builder.Append(" <<<<<<<<<<<<<<<<<\n");
                builder.Append($"<RED>[REASON]   <DGA> >>>>>>>>>>>>>>>>> <DRE>\"{reason}\"<DGA> <<<<<<<<<<<<<<<<<");
            }

            NonAuthLog(builder.ToString());
        } 
    }
}

