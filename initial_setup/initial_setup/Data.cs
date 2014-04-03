

using System;

namespace initial_setup
{
    class Data
    {
        public static void Logger(string x)
        {
            Console.WriteLine(x);
        }

        public static void Logger(string x, string y)
        {
            Console.WriteLine(x + "  " + y);
        }
    }
}
