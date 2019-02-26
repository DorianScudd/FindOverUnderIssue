using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FindOverUnderIssue
{

    class Program
    {
        static void Main(string[] args)
        {
            string jobId, processIds = "";
            string connectionString = "Data Source=pelican-fs3;Database=M1_PW;Integrated Security=true";

            Console.Write("Enter Job Number: ");
            jobId = Console.ReadLine();

            string queryStringMaterialsTracking = "SELECT jmpJobId, jmoProcessID, jmmEstimatedQuantity, jmmQuantityReceived FROM UVWJOBANALYSIS_MATERIALS_TRACKING WHERE JMPJOBID = @jobId";
            string queryStringLaborTracking = "SELECT jmpJobID, lmlProcessID, lmlEmployeeID, lmlGoodQuantity FROM UVWJOBANALYSIS_LABOR_TRACKING WHERE JMPJOBID = @jobId AND lmlProcessID IN @processIds";

            double estimatedQuantity, quantityReceived, percentDeviation = 0.000;
            
            DataSet dsMaterials = new DataSet();
            DataTable dtMaterials = dsMaterials.Tables.Add("Materials");
            DataTable dtLabor = dsMaterials.Tables.Add("Labor");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(queryStringMaterialsTracking, conn))
                {
                    command.Connection = conn;
                    command.Parameters.Add("@jobId", SqlDbType.Char);
                    command.Parameters["@jobId"].Value = jobId;
                    conn.Open();
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        dtMaterials.Load(reader);
                    }
                    conn.Close();
                }

            }

            dtMaterials.Columns.Add("Problem Row", typeof(Double));

            foreach (DataRow dataRow in dtMaterials.Rows)
            {
                estimatedQuantity = Convert.ToDouble(dataRow.ItemArray[2]); //planned quantity
                quantityReceived = Convert.ToDouble(dataRow.ItemArray[3]); //recorded quantity consumed
                percentDeviation = (quantityReceived - estimatedQuantity) / (estimatedQuantity); //if amount is off greater than +- 20.5%, then let's take a look at the process and find the operator
                dtMaterials.Rows[dtMaterials.Rows.IndexOf(dataRow)]["Problem Row"] = percentDeviation;

                //visual inspection of table
                Console.Write("Row #: {0} \t {1} \t {2} \t {3} \t {4} \t", dtMaterials.Rows.IndexOf(dataRow), dataRow.ItemArray[0], dataRow.ItemArray[1], dataRow.ItemArray[2], dataRow.ItemArray[3]);
                if ((percentDeviation < -0.205) || (percentDeviation > 0.205))
                {
                    //red means +- 20.5% inaccurate material consumption
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                else
                {
                    // do nothing;
                }
                Console.WriteLine("{0}", dataRow.ItemArray[3]);
                Console.ResetColor();
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(queryStringLaborTracking, conn))
                {
                    command.Connection = conn;
                    command.Parameters.Add("@jobId", SqlDbType.Char);
                    command.Parameters.Add("@processIds", SqlDbType.VarChar);

                    command.Parameters["@jobId"].Value = jobId;
                    //run this in sql and with parameters defined and see if get same client error. "'W_DHT', 'W_DHT'" as processIds parameter
                    command.Parameters["@processIds"].Value = GetProcessIDs(dtMaterials, jobId);
                    conn.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dtLabor.Load(reader);
                    }
                    conn.Close();
                }
            }


                Console.WriteLine("\nComplete");
            Console.ReadLine();
        }
        /// <summary>
        /// provide datatable and job id, return relevant process id's as string for db constraint
        /// </summary>
        /// <param name="mDataTable"></param>
        /// <param name="mJobId"></param>
        /// <returns></returns>
        private static object GetProcessIDs(DataTable mDataTable, string mJobId)
        {
            List<string> mprocess = new List<string>(new string[] { });

            foreach (DataRow dataRow in mDataTable.Rows)
            {
                if ((Convert.ToDouble(dataRow.ItemArray[3]) < -0.205) || (Convert.ToDouble(dataRow.ItemArray[3]) > 0.205))
                {
                    mprocess.Add("'" + dataRow.ItemArray[1].ToString() + "'");
                }
            }

            var result = String.Join(", ", mprocess.ToArray());
            result = result.Replace("\"", "'");

            return result;
        }
    }
}
