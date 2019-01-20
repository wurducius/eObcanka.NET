using System;
using System.Collections.Generic;
using System.Text;

namespace eObcanka.NET
{
    class Reader
    {
        internal static string ChooseReader(IList<string> readerNames)
        {
            // Show available readers.
            Console.WriteLine("Available readers: ");
            for (var i = 0; i < readerNames.Count; i++)
            {
                Console.WriteLine("[" + i + "] " + readerNames[i]);
            }

            // Ask the user which one to choose.
            Console.Write("Which reader has an inserted card that supports the GET CHALLENGE command? ");
            var line = Console.ReadLine();

            if (int.TryParse(line, out var choice) && (choice >= 0) && (choice <= readerNames.Count))
            {
                return readerNames[choice];
            }

            Console.WriteLine("An invalid number has been entered.");
            Console.ReadKey();
            return null;
        }



        internal static bool NoReaderFound(ICollection<string> readerNames)
        {
            return readerNames == null || readerNames.Count < 1;
        }
    }
}
