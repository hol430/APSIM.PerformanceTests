using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace APSIM.PerformanceTests.Service
{
    public class Utilities
    {
        //this is for apsim.info
        //private static string filePath = @"D:\Websites\";
        //private static string filePathLog = @"D:\Websites\APSIM.PerformanceTests.Service\";

        //this is for csiro.apsim.au
        // private static string filePath = @"E:\Sites\APSIM-Sites\";            
        //private static string filePathLog = @"E:\Sites\APSIM-Sites\Logs";

        //#if DEBUG
        //  filePath = @"C:\Dev\PerformanceTests\";
        //  filePathLog = @"C:\Dev\PerformanceTests\";
        //#endif


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
            string filePath = ConfigurationManager.AppSettings["filePath"].ToString();
            string connectionString = string.Empty;
            //string connectStr = @"D:\Websites\dbConnect.txt";                //this is for apsim.info
            //string connectStr = @"E:\Sites\APSIM-Sites\dbConnect.txt";            //this is for csiro.apsim.au
            try
            {
                string file = filePath + "dbConnect.txt";
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
            string filePath = ConfigurationManager.AppSettings["filePath"].ToString();
            string tokenString = string.Empty;
            //string tokenFile = @"D:\Websites\GitHubToken.txt";  //this is for apsim.info
            //string tokenFile = @"E:\Sites\APSIM-Sites\GitHubToken.txt";            //this is for csiro.apsim.au
            try
            {
                string file = filePath + "GitHubToken.txt";
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
            string filePath = ConfigurationManager.AppSettings["filePath"].ToString();
            string tokenString = string.Empty;
            //string acceptStatsFile = @"D:\Websites\PerformanceTestsStatsAcceptedToken.txt";  //this is for apsim.info
            //string acceptStatsFile = @"E:\Sites\APSIM-Sites\PerformanceTestsStatsAcceptedToken.txt";            //this is for csiro.apsim.au
            try
            {
                string file = filePath + "PerformanceTestsStatsAcceptedToken.txt";
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
                    string filePathLog = ConfigurationManager.AppSettings["LogFilePath"].ToString();
                    //this is just a temporary measure so that I can see what is happening
                    //Console.WriteLine(message);

                    //Need to make sure we are in the same directory as this application 
                    //string fileName = getDirectoryPath("PerformanceTestsLog.txt");
                    //string fileName = @"D:\Websites\APSIM.PerformanceTests.Service\PerformanceServiceLog.txt";   //this is for apsim.info
                    //string fileName = @"E:\Sites\APSIM-Sites\Logs\PerformanceServiceLog.txt";            //this is for csiro.apsim.au

                    StreamWriter sw;

                    string file = filePathLog + "PerformanceServiceLog.txt";
                    if (!File.Exists(file))
                    {
                        sw = new StreamWriter(file);
                    }
                    else
                    {
                        sw = File.AppendText(file);
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