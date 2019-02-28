using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using FindOverUnderIssue.Biz;

namespace FindOverUnderIssue
{

    class Program
    {
        static void Main(string[] args)
        {
            PelData.PelData pelData = new PelData.PelData();
            SendOutput mScreen = new SendOutput();

            string jobId = "";
            
            Console.Write("Enter Job Number: ");
            jobId = Console.ReadLine();

            DataSet dsMaterials = new DataSet();
            DataTable dtMaterials = dsMaterials.Tables.Add("Materials");
            DataTable dtLabor = dsMaterials.Tables.Add("Labor");

            dtMaterials = pelData.GetMaterialsDataTable(jobId);

            try
            {
                mScreen.PrintOut(dtMaterials);
                Console.WriteLine("\nMaterials Complete");
            }
            catch (Exception ex)
            {

                Console.WriteLine("Exception caught: {0} ", ex.Message);
            }

            try
            {
                dtLabor = pelData.GetLaborDataTable(dtMaterials, jobId);

                mScreen.PrintOut(dtLabor);
                Console.WriteLine("\nLabor Complete");
            }
            catch (SqlException ex)
            {

                Console.WriteLine("Exception caught: {0} ", ex.Message);
            }

            Console.ReadLine();
        }
    }
}
