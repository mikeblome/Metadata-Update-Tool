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
            // Load options
            var opts = new Options(args);

            // For debugging, remove from production
            opts.PrintOptions();

            // open commands file or report failure and exit
            // create objects for applies-to file, metadata collection
            // for each line in commands file
            //   load fields from line or report failure and continue
            //   if first line or filename field is not the current applies-to file
            //     if filename field is not the current applies-to file
            //       if there's an updated metadata collection
            //         write the metadata collection to the applies-to file or report failure
            //       close current applies-to file
            //     open new applies-to file or report failure and continue
            //     parse metadata collection from applies-to file or report failure and continue
            //   apply action to metadata collection or report failure and continue
            // if there's an open applies-to file
            //   if there's an updated metadata collection
            //     write the metadata collection to the applies-to file or report failure
            //   close current applies-to file
            // report complete and exit

        }
    }
}
