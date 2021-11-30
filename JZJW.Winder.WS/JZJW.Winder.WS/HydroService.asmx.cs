using JZJW.Winder.BLL;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;

namespace JZJW.Winder.WS
{
    /// <summary>
    /// HydroService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class HydroService : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public string GetLineChartData(string name, string regionid, string binstrlen, string binstrval, string reghighid)
        {
            return WinderHelper.GetLineChartData(name, regionid, binstrlen, binstrval, reghighid);
        }

        [WebMethod]
        public string DoRiverSegCalc(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode)
        {
            return WinderHelper.DoRiverSegCalc(sid, orauser, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, usercode);
        }

    }
}
