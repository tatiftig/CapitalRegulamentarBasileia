using System;
using System.Collections.Generic;
using System.Globalization;


namespace PrimeiroProjeto
{
    class Program
{
    static void Main(string[] args)
    {
            double A, B, C, D, dif;

        A = double.Parse(Console.ReadLine());
        B = double.Parse(Console.ReadLine());
        C = double.Parse(Console.ReadLine());
        D = double.Parse(Console.ReadLine());

        dif = A * B - C * D;
     
        Console.WriteLine("DIFERENÇA =  " + dif);
    }
}
}

