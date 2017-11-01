using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Service
{
    public class Utilities
    {

        public static string GetModifiedFileName(string fileName)
        {
            string returnStr;
            int posn = fileName.IndexOf(@"ApsimX\Tests");
            if (posn < 0)
            {
                posn = fileName.IndexOf(@"ApsimX\Prototypes");
            }
            if (posn > 0) { posn += 7; }
            if (posn < 0) { posn = 0; }
            returnStr = fileName.Substring(posn);

            return returnStr;
        }

        public static string GetConnectionString()
        {
            string connectionString = string.Empty;
            string file = @"D:\Websites\dbConnect.txt";
#if DEBUG
            file = @"C:\Dev\PerformanceTests\dbConnect.txt";
#endif
            try
            {
                connectionString = File.ReadAllText(file) + ";Database=\"APSIM.PerformanceTests\"";
                return connectionString;

            }
            catch (Exception ex)
            {
                WriteToLogFile("ERROR: Unable to retrieve Database connection details: " + ex.Message.ToString());
                return connectionString;
            }
        }

        public static string GetGitHubToken()
        {
            string tokenString = string.Empty;
            string file = @"D:\Websites\GitHubToken.txt";
#if DEBUG
            file =@"C:\Dev\PerformanceTests\GitHubToken.txt";
#endif
            try
            {
                tokenString = File.ReadAllText(file);
            }
            catch (Exception ex)
            {
                WriteToLogFile("ERROR: Unable to retrieve GitHub Token: " + ex.Message.ToString());
            }
            return tokenString;
        }

        public static string GetStatsAcceptedToken()
        {
            string tokenString = string.Empty;
            string file = @"D:\Websites\PerformanceTestsStatsAcceptedToken.txt";
#if DEBUG
            file = @"C:\Dev\PerformanceTests\PerformanceTestsStatsAcceptedToken.txt";
#endif
            try
            {
                tokenString = File.ReadAllText(file);
            }
            catch (Exception ex)
            {
                WriteToLogFile("ERROR: Unable to retrieve AcceptedStats Token: " + ex.Message.ToString());
            }
            return tokenString;
        }


        public static void WriteToLogFile(string message)
        {
            if (message.Length > 0)
            {
                try
                {
                    //this is just a temporary measure so that I can see what is happening
                    //Console.WriteLine(message);

                    //Need to make sure we are in the same directory as this application 
                    //string fileName = getDirectoryPath("PerformanceTestsLog.txt");
                    string fileName = @"D:\Websites\APSIM.PerformanceTests.Service\PerformanceTestsLog.txt";
#if DEBUG
                    fileName = @"C:\Dev\PerformanceTests\PerformanceTestsLog.txt";
#endif
                    StreamWriter sw;

                    if (!File.Exists(fileName))
                    {
                        sw = new StreamWriter(fileName);
                    }
                    else
                    {
                        sw = File.AppendText(fileName);
                    }
                    string logLine = String.Format("{0}: {1}", System.DateTime.Now.ToString("yyyy-MMM-dd HH:mm"), message);
                    sw.WriteLine(logLine);
                    sw.Close();

                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// creates the file/name path details for the for the specified file and the application's path.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string getDirectoryPath(string fileName)
        {
            string returnStr = string.Empty;

            //To get the location the assembly normally resides on disk or the install directory
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            returnStr = Path.GetDirectoryName(path) + "\\" + fileName;
            return returnStr;
        }

    }

}