using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using System.Threading.Tasks;

namespace FindOverUnderIssue.Biz
{
    class SendOutput
    {
        
        /// <summary>
        /// Sanity check of filtered dataset for problem operations.
        /// </summary>
        /// <param name="datatable"></param>
        public void PrintOut(DataTable datatable)
        {
            double estimatedQuantity, quantityReceived, percentDeviation = 0.000;

            foreach (DataRow dataRow in datatable.Rows)
            {
                estimatedQuantity = Convert.ToDouble(dataRow.ItemArray[2]); //planned quantity
                quantityReceived = Convert.ToDouble(dataRow.ItemArray[3]); //recorded quantity consumed
                percentDeviation = ((quantityReceived - estimatedQuantity) / estimatedQuantity); //if amount is off greater than +- 20.5%, then let's take a look at the process and find the operator
                datatable.Rows[datatable.Rows.IndexOf(dataRow)]["Problem Row"] = percentDeviation;

                //visual inspection of table
                Console.Write("Row #: {0} \t {1} \t {2} \t {3} \t {4} \t", datatable.Rows.IndexOf(dataRow), dataRow.ItemArray[0], dataRow.ItemArray[1], dataRow.ItemArray[2], dataRow.ItemArray[3]);
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
        }
    }
}
