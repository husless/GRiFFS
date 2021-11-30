using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using JZJW.Winder.BLL;
using System.Data;

namespace WS
{
    /// <summary>
    /// Service1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class WS : System.Web.Services.WebService
    {

        //[WebMethod(Description="ren测试服务")]
        //public string HelloWorld()
        //{
        //    return new WinderHelper().TestRen();
           
        //}
        //[WebMethod]
        //public string GetLineChartData(string name, string regionid, string binstrlen, string binstrval, string reghighid)
        //{
        //    return new WinderHelper().GetLineChartData(name, regionid, binstrlen, binstrval, reghighid);
        //}
        //orauser= hg02  ，userid=user usercode=用户密码
        [WebMethod(Description = "ren根据提交的方案进行计算")]
        public string DoRiverSegCalc(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode)
        {
            WinderHelper wh = new WinderHelper(orauser, sid);
            wh.sid = sid;
            wh.orauser = orauser;
            string rst = wh.DoRiverSegCalc(sid, orauser, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, usercode);
            return rst;
            //多用户同时计算ByRen



            //return WinderHelper.DoRiverSegCalcNew(sid, orauser, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, usercode);
        }
        [WebMethod(Description = "Ren测试入口")]

        public string RenTest()
        {
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140515HP002", 339001122012, 848, 0, 339001122012);
            river.Add(test);

            test = new RiverSeg("20140515HP002", 339001122012, 877, 0, 339001122012);
            river.Add(test);
            test = new RiverSeg("20140515HP002", 339001122012, 801, 4194304, 339001122012);
            river.Add(test);
            test = new RiverSeg("20140515HP002", 339001122012, 836, 4194304, 339001122012);
            river.Add(test);


            //return DoRiverSegCalc("20140311HP009", "hg02", "hydroglobal", river, "2013/12/20 20:28:54", "2014/3/9 20:28:54", "雨量站", "新安江模型", "扩散波模型", "hg474");
            return DoRiverSegCalc("20140515HP002", "hg02", "hydroglobal", river, "2014/5/10 20:36:48", "2014/5/16 20:36:48", "雨量站", "新安江模型", "扩散波模型", "hg474");
        
        }
        /// <summary>
        /// 查询获取空闲状态下的用户名,
        /// 若有空闲用户则执行计算过程
        /// 若不存在空间用户则返回空值，等待（前台控制）
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "ren2014-04-27")]
        public string SelectAvailableUser(string sid, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode)
        {
            //2014-04-27ByRen添加代码
            /**********************************************************************************************/
            string Result;
            //用户表 状态值3表示空闲状态
            string sql = "select * from orauserTable where state=3";
            DataSet ds = DbHelper.Query(sql);
            DataTable dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                Result = dt.Rows[0]["orauser"].ToString();
                //多用户同时计算ByRen
                new WinderHelper(userid, sid).DoRiverSegCalcNew(sid, Result, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, usercode);
            }
            else
            {
                Result = "";
            }
            return Result;
        }

    }
}