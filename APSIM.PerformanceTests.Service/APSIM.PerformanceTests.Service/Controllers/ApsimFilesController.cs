using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Web.Http;
using System.Web.Http.Description;
using APSIM.PerformanceTests.Models;
using System.Threading.Tasks;

namespace APSIM.PerformanceTests.Service.Controllers
{
    public class ApsimFilesController : ApiController
    {
        //  GET (Read): api/apsimfiles/ 
        public List<ApsimFile> GetAllApsimFiles()
        {
            List<ApsimFile> apsimFiles = new List<ApsimFile>();
            SqlDataReader reader = null;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GetConnectionString();

            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT * FROM ApsimFiles";
            command.Connection = con;
            con.Open();

            reader = command.ExecuteReader();
            while (reader.Read())
            {
                ApsimFile apsim = new ApsimFile();
                apsim.ID = reader.GetInt32(0);
                apsim.PullRequestId = reader.GetInt32(1);
                apsim.FileName = reader.GetString(2);
                apsim.FullFileName = reader.GetString(3);
                apsim.RunDate = reader.GetDateTime(4);
                apsim.IsReleased = reader.GetBoolean(5);
                apsimFiles.Add(apsim);
            }
            con.Close();
            return apsimFiles;
        }


        //  GET (Read): api/apsimfiles/5 
        [ResponseType(typeof(ApsimFile))]
        public async Task<IHttpActionResult> GetApsimFile(int id)
        {
            SqlDataReader reader = null;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = GetConnectionString();

            SqlCommand command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT * FROM ApsimFiles WHERE PullRequestId = " + id;
            command.Connection = con;
            con.Open();
            reader = command.ExecuteReader();
            ApsimFile apsimFile = new ApsimFile();
            while (reader.Read())
            {
                apsimFile.ID = reader.GetInt32(0);
                apsimFile.PullRequestId = reader.GetInt32(1);
                apsimFile.FileName = reader.GetString(2);
                apsimFile.FullFileName = reader.GetString(3);
                apsimFile.RunDate = reader.GetDateTime(4);
                apsimFile.IsReleased = reader.GetBoolean(5);
            }
            con.Close();
            return Ok(apsimFile);
        }



        // PUT (Update): api/apsimfiles/2 apsimfile
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutApsimFile(int id, ApsimFile apsim)
        {
            //do the changes here
            return StatusCode(HttpStatusCode.NoContent);
        }



        // POST (Create): api/apsimfiles/ apsimfile
        [ResponseType(typeof(ApsimFile))]
        public async Task<IHttpActionResult> PostApsimFile(ApsimFile apsimfile)
        {

            int ApsimID = 0;
            try
            {
                SqlConnection con = new SqlConnection();
                con.ConnectionString = GetConnectionString();

                WriteToLogFile("-----------------------------------");     
                WriteToLogFile(string.Format("Processing PullRequestId {0}, Apsim Filename {1}, dated {2}, imported successfully!", apsimfile.PullRequestId, apsimfile.FileName, apsimfile.RunDate));

                //--------------------------------------------------------------------------------------
                //Add the ApsimFile Record first, so that we can get back the IDENTITY (ID) value
                //--------------------------------------------------------------------------------------
                string strSQL = "INSERT INTO ApsimFiles (PullRequestId, FileName, FullFileName, RunDate, IsReleased) OUTPUT INSERTED.ID Values (@PullRequestId, @FileName, @FullFileName, @RunDate, @IsReleased)";
                SqlCommand command = new SqlCommand(strSQL, con);
                command.Parameters.AddWithValue("@PullRequestId", apsimfile.PullRequestId);
                command.Parameters.AddWithValue("@FileName", apsimfile.FileName);
                command.Parameters.AddWithValue("@FullFileName", apsimfile.FullFileName);
                command.Parameters.AddWithValue("@RunDate", apsimfile.RunDate);
                command.Parameters.AddWithValue("@IsReleased", apsimfile.IsReleased);

                //how many rows were inserted
                con.Open();
                //this should return the IDENTITY value for this record (which is required for the next update)
                //NEED TO UNDO THIS
                //ApsimID = 232323;
                ApsimID = (int)command.ExecuteScalar();
                WriteToLogFile( string.Format("    Filename {0} imported successfully!", apsimfile.FileName));

                //--------------------------------------------------------------------------------------
                //Add the Simulation Details to the database 
                //--------------------------------------------------------------------------------------

                if (apsimfile.Simulations.Rows.Count > 0)
                {
                    // Configure the command and parameter.
                    command = new SqlCommand("usp_SimulationsInsert", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ApsimID", ApsimID);

                    SqlParameter tvpParam = command.Parameters.AddWithValue("@Simulations", apsimfile.Simulations);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.SimulationDataTableType";

                    //NEED TO UNDO THIS
                    command.ExecuteNonQuery();
                    WriteToLogFile(string.Format("    Filename {0} Simulation Data imported successfully!", apsimfile.FileName));
                }


                //--------------------------------------------------------------------------------------
                //Add the Predited Observed Details (MetaData) and then the data
                //--------------------------------------------------------------------------------------

                //now look at each individual set of data
                foreach (PredictedObservedDetails poDetail in apsimfile.PredictedObserved)
                {

                    strSQL = "INSERT INTO PredictedObservedDetails (" 
                        + " ApsimFilesID, TableName, PredictedTableName, ObservedTableName, FieldNameUsedForMatch, FieldName2UsedForMatch, FieldName3UsedForMatch, HasTests "
                        + " ) OUTPUT INSERTED.ID Values (" 
                        + " @ApsimFilesID, @TableName, @PredictedTableName, @ObservedTableName, @FieldNameUsedForMatch, @FieldName2UsedForMatch, @FieldName3UsedForMatch, 0 "
                        + " )";



                    command = new SqlCommand(strSQL, con);
                    command.Parameters.AddWithValue("@ApsimFilesID", ApsimID);
                    command.Parameters.AddWithValue("@TableName", poDetail.DatabaseTableName);
                    command.Parameters.AddWithValue("@PredictedTableName", poDetail.PredictedTableName);
                    command.Parameters.AddWithValue("@ObservedTableName", poDetail.ObservedTableName);

                    command.Parameters.AddWithValue("@FieldNameUsedForMatch", poDetail.FieldNameUsedForMatch);
                    command.Parameters.AddWithValue("@FieldName2UsedForMatch", poDetail.FieldName2UsedForMatch);
                    command.Parameters.AddWithValue("@FieldName3UsedForMatch", poDetail.FieldName3UsedForMatch);

                    //this should return the IDENTITY value for this record (which is required for the next update)
                    //NEED TO UNDO THIS
                    //int PredictedObservedID = 232323;
                    int PredictedObservedID = (int)command.ExecuteScalar();
                    WriteToLogFile(string.Format("    Filename {0} PredictedObserved Table Details {1} imported successfully!", apsimfile.FileName, poDetail.DatabaseTableName));


                    //--------------------------------------------------------------------------------------
                    //And finally this is where we will insert the actual Predited Observed DATA
                    //--------------------------------------------------------------------------------------
                    DataView PredictedObservedView = new DataView(poDetail.PredictedObservedData);

                    string ObservedColumName, PredictedColumnName;
                    //need to find the first (and then each instance thereafter) of a field name being with Observed,
                    //the get the corresponding Predicted field name, and then create a new table definition based on this
                    //data,
                    for (int i = 0; i < poDetail.PredictedObservedData.Columns.Count; i++)
                    {
                        ObservedColumName = poDetail.PredictedObservedData.Columns[i].ColumnName.Trim();
                        if (ObservedColumName.StartsWith("Observed"))
                        {
                            //get the corresponding Predicted Column Name
                            int dotPosn = ObservedColumName.IndexOf('.');
                            string valueName = ObservedColumName.Substring(dotPosn+1);
                            PredictedColumnName = "Predicted." + valueName;

                            DataTable selectedData;

                            if (poDetail.FieldName3UsedForMatch.Length > 0)
                            {
                                selectedData = PredictedObservedView.ToTable(false, "SimulationID", poDetail.FieldNameUsedForMatch, poDetail.FieldName2UsedForMatch, poDetail.FieldName3UsedForMatch, PredictedColumnName, ObservedColumName);
                                command = new SqlCommand("usp_PredictedObservedDataThreeInsert", con);
                            }
                            else if (poDetail.FieldName2UsedForMatch.Length > 0)
                            {
                                selectedData = PredictedObservedView.ToTable(false, "SimulationID", poDetail.FieldNameUsedForMatch, poDetail.FieldName2UsedForMatch, PredictedColumnName, ObservedColumName);
                                command = new SqlCommand("usp_PredictedObservedDataTwoInsert", con);
                            }
                            else
                            {
                                selectedData = PredictedObservedView.ToTable(false, "SimulationID", poDetail.FieldNameUsedForMatch, PredictedColumnName, ObservedColumName);
                                command = new SqlCommand("usp_PredictedObservedDataInsert", con);
                            }

                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@PredictedObservedID", PredictedObservedID);
                            command.Parameters.AddWithValue("@ApsimFilesID", ApsimID);
                            command.Parameters.AddWithValue("@ValueName", valueName);

                            if (poDetail.FieldName3UsedForMatch.Length > 0)
                            {
                                command.Parameters.AddWithValue("@MatchName", poDetail.FieldNameUsedForMatch);
                                command.Parameters.AddWithValue("@MatchName2", poDetail.FieldName2UsedForMatch);
                                command.Parameters.AddWithValue("@MatchName3", poDetail.FieldName3UsedForMatch);
                            }
                            else if (poDetail.FieldName2UsedForMatch.Length > 0)
                            {
                                command.Parameters.AddWithValue("@MatchName", poDetail.FieldNameUsedForMatch);
                                command.Parameters.AddWithValue("@MatchName2", poDetail.FieldName2UsedForMatch);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@MatchName", poDetail.FieldNameUsedForMatch);
                            }

                            SqlParameter tvtpPara = command.Parameters.AddWithValue("@PredictedOabservedData", selectedData);
                            tvtpPara.SqlDbType = SqlDbType.Structured;

                            if (poDetail.FieldName3UsedForMatch.Length > 0)
                            {
                                tvtpPara.TypeName = "dbo.PredictedObservedDataThreeTableType";
                            }
                            else if (poDetail.FieldName2UsedForMatch.Length > 0)
                            {
                                tvtpPara.TypeName = "dbo.PredictedObservedDataTwoTableType";
                            }
                            else
                            {
                                tvtpPara.TypeName = "dbo.PredictedObservedDataTableType";
                            }
                            // Execute the command.
                            //NEED TO UNDO THIS
                            command.ExecuteNonQuery();
                            WriteToLogFile(string.Format("       PredictedObserved Data for {0} imported successfully!", valueName));

                        }   //ObservedColumName.StartsWith("Observed")
                    }   // for (int i = 0; i < poDetail.PredictedObservedData.Columns.Count; i++)


                    //Need to run the testing procecedure here, and then save the test data
                    if (poDetail.PredictedObservedData.Rows.Count > 0)
                    {
                        DataTable dtTests = Tests.DoValidationTest(poDetail.DatabaseTableName, poDetail.PredictedObservedData);

                        if (dtTests.Rows.Count > 0)
                        {
                            //Now update the database with the test results
                            // Configure the command and parameter.
                            command = new SqlCommand("usp_PredictedObservedTestsInsert", con);
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@PredictedObservedID", PredictedObservedID);

                            SqlParameter tvpParam = command.Parameters.AddWithValue("@Tests", dtTests);
                            tvpParam.SqlDbType = SqlDbType.Structured;
                            tvpParam.TypeName = "dbo.PredictedObservedTestsTableType";

                            command.ExecuteNonQuery();
                            WriteToLogFile(string.Format("    Filename {0} Tests Data for {1} imported successfully!", apsimfile.FileName, poDetail.DatabaseTableName));
                        }
                    }

                }   //foreach (PredictedObservedDetails poDetail in apsimfile.PredictedObserved)

                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                return CreatedAtRoute("DefaultApi", new {id=ApsimID}, apsimfile);
            }

            catch (Exception ex)
            {
                WriteToLogFile("    ERROR:  Unable to update SQL Server: " + ex.Message.ToString());
                throw new Exception("Unable to update SQL Server: " + ex.Message.ToString());
            }
        }


        private static string GetConnectionString()
        {
            string file = @"D:\Websites\dbConnect.txt";
            string connectionString = string.Empty;
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



        private static void WriteToLogFile(string message)
        {
            if (message.Length > 0)
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
        }

        /// <summary>
        /// creates the file/name path details for the for the specified file and the application's path.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string getDirectoryPath(string fileName)
        {
            string returnStr = string.Empty;

            //To get the location the assembly normally resides on disk or the install directory
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            returnStr = Path.GetDirectoryName(path) + "\\" + fileName;
            return returnStr;
        }

    }
}
