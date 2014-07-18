

using System;
using System.IO;
using System.Threading;

namespace initial_setup
{
    class Program
    {
        static void Main()
        {
            DoPatcher initsetup = new DoPatcher();

            initsetup.DoDownload();
            initsetup.DoExtract();
            initsetup.MoveFiles();
            Console.Beep();
            Console.Beep();
            Console.WriteLine("Finished with Initial setup.");
            Thread.Sleep(1500);
            Console.ReadKey();
        }
    }
}
