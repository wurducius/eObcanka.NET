using System;
using eObcanka;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.ForegroundColor = ConsoleColor.Green;
            Card card = new Card();
            Console.WriteLine("Reading data..");
            Card.ReadData();
            Console.ReadKey();
        }
    }
}
