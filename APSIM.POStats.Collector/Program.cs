using APSIM.POStats.Shared;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace APSIM.POStats.Collector
{
    class Program
    {
        /// <summary>
        /// Main entry points.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>0 on success. 1 on error.</returns>
        static async Task<int> Main(string[] args)
        {
            try
            {
                //Test that something has been passed
                if (args.Length != 4)
                {
                    Console.WriteLine("Arguments required are: ");
                    Console.WriteLine("  1. (int) Pull Request Id");
                    Console.WriteLine("  2. (datetime) Date");
                    Console.WriteLine("  3. (string) UserID");
                    Console.WriteLine("  4. (string) Directories(csv)");
                    Console.WriteLine(@"  Example: APSIM.POStats.Collector 1111 2016.12.01-06:33 hol353 c:\Apsimx\Tests");
                    return 1;
                }

                // Convert command line arguments to variables.
                int pullId = Convert.ToInt32(args[0]);
                DateTime runDate = DateTime.ParseExact(args[1], "yyyy.MM.dd-HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
                string author = args[2];
                string searchDir = args[3];

                var pullReqest = Shared.Collector.RetrieveData(pullId, runDate, author, searchDir);

                var t = await WebUtilities.PostAsync(Vault.Read("CollectorURL"), pullReqest);
                if (t != string.Empty)
                    throw new Exception(t);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
                return 1;
            }
            return 0;
        }
    }
}