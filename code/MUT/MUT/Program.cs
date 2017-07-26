using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUT
{
    class Program
    {
        static void Main(string[] args)
        {
            var opts = new Options(args);

            // Debug output of recognized options
            opts.PrintOptions();
            var files = opts.GetFiles();
            Console.WriteLine("Specified files:");
            foreach (var filepath in files)
            {
                Console.WriteLine(filepath);
            }
        }
    }
}
