using System;
using eObcanka;

namespace SampleApplication
{
    class Program
    {
        static void Main(string[] args)
        {          
                Reader reader = new Reader();
                Reader.DoWork();
                Console.ReadKey();         
        }   
    }
}
