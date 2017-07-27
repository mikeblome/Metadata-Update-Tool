using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUT;
using System.IO;

namespace mdapply
{
    class Program
    {
        static void Main(string[] args)
        {
            var opts = new Options(args);
            opts.PrintOptions();

            var files = Directory.GetFiles("");
            foreach (var file in files)
            {

            }

        }
    }
}
