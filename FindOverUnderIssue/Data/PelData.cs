using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading.Tasks;

namespace FindOverUnderIssue.PelData
{
    /// <summary>
    /// Contains Query strings, SQL commands
    /// </summary>
    class PelData
    {
        public double percentDeviation = 0.025;

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["Database"]?.ConnectionString;

        public string QueryStringMaterialsTracking { get; set; } = "SELECT jmpJobId, jmoProcessID, jmmEstimatedQuantity, jmmQuantityReceived FROM UVWJOBANALYSIS_MATERIALS_TRACKING WHERE JMPJOBID = @jobId";

        public string QueryStringLaborTracking { get; set; } = "SELECT jmpJobID, lmlProcessID, lmlEmployeeID, lmlGoodQuantity FROM UVWJOBANALYSIS_LABOR_TRACKING WHERE JMPJOBID = @jobId AND lmlProcessID IN (@p1, @p2, @p3, @p4, @p5, @p6)";

        /// <summary>
        /// Returns materials data table for a specific job
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public DataTable GetMaterialsDataTable(string jobId)
        {
            DataTable dataTable = new DataTable
            {
                TableName = "Materials"
            };
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(QueryStringMaterialsTracking, conn))
                {
                    command.Connection = conn;
                    command.Parameters.Add("@jobId", SqlDbType.Char);
                    command.Parameters["@jobId"].Value = jobId;

                    try
                    {
                        conn.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception caught: {0}", ex.Message);
                    }

                }

            }
            //column to identify if material issue is out of tolerance range
            dataTable.Columns.Add("Problem Row", typeof(Double));

            return dataTable;
        }

        public DataTable GetLaborDataTable(DataTable dtMaterials, string jobId)
        {
            DataTable dataTable = new DataTable
            {
                TableName = "Labor"
            };
            Biz.Biz processIds = new Biz.Biz();

            string x = processIds.GetProcessIDs(dtMaterials, jobId).ToString();
            string[] stringSplit = x.Split(',');
            int stringSplitElementCount = stringSplit.Count();


            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(QueryStringLaborTracking, conn))
                {
                    command.Connection = conn;
                    command.Parameters.AddWithValue("@jobId", SqlDbType.Char);
                    command.Parameters.AddWithValue("@p1", SqlDbType.VarChar);

                    command.Parameters["@jobId"].Value = jobId;
                    command.Parameters["@p1"].Value = stringSplit[0];

                    for (int i = 1; i < stringSplitElementCount - 1; i++)
                    {
                        command.Parameters.AddWithValue("@p" + (i + 1), SqlDbType.VarChar);
                        command.Parameters["@p" + (i + 1)].Value = stringSplit[i];
                    }

                    try
                    {
                        conn.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dataTable.Load(reader);
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception caught: {0}", ex.Message);
                    }

                }
            }
            return dataTable;
        }
    }
}
