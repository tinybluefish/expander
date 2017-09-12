using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Expander
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting parse");
            int pointer = 0;
            Node root = Node.ParseExpression(args[0], ref pointer, 0);
            Console.WriteLine("Results:\n {0}", root);
            Console.WriteLine("\n");
            Console.WriteLine(Node.GetStats());
            Console.ReadLine();
        }
    }

}
