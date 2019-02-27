using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace FindOverUnderIssue.Biz
{
    class Biz
    {
        /// <summary>
        /// provide datatable and job id, return relevant process id's as string for db constraint
        /// </summary>
        /// <param name="mDataTable"></param>
        /// <param name="mJobId"></param>
        /// <returns></returns>
        public object GetProcessIDs(DataTable mDataTable, string mJobId)
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
