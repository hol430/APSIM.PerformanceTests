using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;


namespace ApsimPipe
{
    public class Utilities
    {
        public static string GetConnectionString()
        {
            string connectionString = string.Empty;
            string file = @"E:\Sites\APSIM-Sites\dbConnect.txt";
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
                    string fileName = @"E:\Sites\APSIM-Sites\Logs\APSIMPipeLog.txt";
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

    }
}