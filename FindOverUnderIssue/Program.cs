using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;

namespace FindOverUnderIssue
{

    class Program
    {
        static void Main(string[] args)
        {
            PelData.PelData pelData = new PelData.PelData();

            string jobId = "";
            
            Console.Write("Enter Job Number: ");
            jobId = Console.ReadLine();

            double estimatedQuantity, quantityReceived, percentDeviation = 0.000;

            DataSet dsMaterials = new DataSet();
            DataTable dtMaterials = dsMaterials.Tables.Add("Materials");
            DataTable dtLabor = dsMaterials.Tables.Add("Labor");

            dtMaterials = pelData.GetMaterialsDataTable(jobId);
            

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

            dtLabor = pelData.GetLaborDataTable(dtMaterials, jobId);


                Console.WriteLine("\nComplete");
            Console.ReadLine();
        }
        
    }
}
