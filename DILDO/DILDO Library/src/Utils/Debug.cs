using System;

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
            Console.ForegroundColor = ConsoleColor.Green;

            string from = typeof(From).Name;
            string time = DateTime.Now.ToString();

            Console.Write($"{from}\t[{time}]:\t");
            Log(message);
#endif
        }

        public static void Log(string message)
        {
#if UNITY_5_OR_NEWER
            UnityEngine.Debug.Log(message);
#else
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

                if (buffer)
                    j += 3;

                Console.Write(message.Substring(i, j - i - 1));
                if (!buffer)
                {
                    Console.ForegroundColor = DEFAULT;
                    buffer = true;
                }
            }
            Console.Write("\n");
            Console.ForegroundColor = DEFAULT;
#endif
        }
    }
}

