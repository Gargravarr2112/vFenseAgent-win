

using System;
using System.Threading;

namespace initial_setup
{
    class Program
    {
        static void Main()
        {
            DoPatcher.DoDownload();
            DoPatcher.MoveFiles();
            Console.Beep();
            Console.Beep();
            Console.WriteLine("Finished with Initial setup.");
            Thread.Sleep(1500);
        }
    }
}
