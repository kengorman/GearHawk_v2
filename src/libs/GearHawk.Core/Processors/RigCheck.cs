using Newtonsoft.Json;
using System;
using System.Data;
using GearHawk.Core.Data;

namespace GearHawk.Core.Processors
{
    public sealed class RigCheck
    {
        TimeZoneInfo _easternZone;

        public RigCheck()
        {
            _easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }

        public string RigCheckHistory(string sUserId, int iInventoryId)
        {
            DataTable t = DataAccess_RigCheck.LoadAuditHistory(sUserId, iInventoryId);
            foreach (DataRow row in t.Rows)
            {
                var timeUtc = (DateTime)row["created"];
                row["created"] = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, _easternZone);
            }
            t.AcceptChanges();
            return JsonConvert.SerializeObject(t);
        }

        public string PercentComplete(string userId, int nodeId)
        {
            DataTable t = DataAccess_RigCheck.PercentComplete(userId, nodeId);
            string pctComplete = ((Decimal)t.Rows[0][0]).ToString();
            pctComplete = pctComplete == "1.00" ? "100%" : pctComplete.Replace("0.", "") + "%";
            return JsonConvert.SerializeObject(pctComplete);
        }
    }
}
