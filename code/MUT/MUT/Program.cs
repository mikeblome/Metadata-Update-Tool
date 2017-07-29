using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MUT
{

    class Program
    {
        static void Main(string[] args)
        {
            var opts = new Options(args);

            // Get a list of filenames from options
            var fileArray = opts.GetFiles();

            // Extract YML header values for output
            StringBuilder sb = new StringBuilder();
            var cb = new CommandBuilder(opts.output);

            // Parse each file in the list
            foreach (var filename in fileArray)
            {
                cb.ParseFile(filename);
            }

            // Write results to the output file or stdout
            cb.WriteFile(opts.use_output);

            // debug trap so you can see it at work; remove from production
            Console.WriteLine("Press any key to continue... ... ...");
            Console.ReadLine();
        }

        // ParseXML
    }

}
