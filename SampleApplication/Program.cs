using System;
using eObcanka;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {          
                Card card = new Card();
                Card.ReadData();
                Console.ReadKey();         
        }   
    }
}
