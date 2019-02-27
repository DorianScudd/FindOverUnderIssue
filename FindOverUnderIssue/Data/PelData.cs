﻿using System;
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

        private string connectionString = ConfigurationManager.ConnectionStrings["Database"]?.ConnectionString;

        public string QueryStringMaterialsTracking { get; set; } = "SELECT jmpJobId, jmoProcessID, jmmEstimatedQuantity, jmmQuantityReceived FROM UVWJOBANALYSIS_MATERIALS_TRACKING WHERE JMPJOBID = @jobId";

        public string QueryStringLaborTracking { get; set; } = "SELECT jmpJobID, lmlProcessID, lmlEmployeeID, lmlGoodQuantity FROM UVWJOBANALYSIS_LABOR_TRACKING WHERE JMPJOBID = @jobId AND lmlProcessID IN @processIds";

        /// <summary>
        /// Returns materials data table for a specific job
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public DataTable GetMaterialsDataTable(string jobId)
        {
            DataTable dataTable = new DataTable();
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
            DataTable dataTable = new DataTable();
            Biz.Biz processIds = new Biz.Biz();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(QueryStringLaborTracking, conn))
                {
                    command.Connection = conn;
                    command.Parameters.AddWithValue("@jobId", SqlDbType.Char);
                    command.Parameters.AddWithValue("@processIds", SqlDbType.VarChar);

                    command.Parameters["@jobId"].Value = jobId;

                    //run this in sql and with parameters defined and see if get same client error. "'W_DHT', 'W_DHT'" as processIds parameter
                    command.Parameters["@processIds"].Value = processIds.GetProcessIDs(dtMaterials, jobId).ToString();
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
