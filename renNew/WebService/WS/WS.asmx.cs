using System.Collections.Generic;
using System.Data;
using System.Web.Services;
using JZJW.Winder.BLL;
using System;

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
        //orauserTable state=3  orauser 空闲状态
        //                   1          数据入库
        //                   2          计算  

        [WebMethod(Description = "单方案计算")]
        public string DoRiverSegCalc(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode)
        {
            //orauser = "hg02";
            WinderHelper wh = new WinderHelper(orauser, sid);
            //wh.sid = sid;
            //wh.orauser = orauser;
            //usercode = "hg02";
            string rst = wh.DoRiverSegCalc(sid, orauser, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, usercode);
            return rst;
            //多用户同时计算ByRen
            //return WinderHelper.DoRiverSegCalcNew(sid, orauser, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, usercode);
        }

        [WebMethod(Description = "Ren测试入口")]
        public string RenTest()
        {
            /*
            //湘江之流  V2.0版本参数未修改*/
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140515HP002", 339001122012, 848, 4194304, 339001122012);
            river.Add(test);
            test = new RiverSeg("20140515HP002", 339001122012, 877, 0, 339001122012);
            river.Add(test);
            test = new RiverSeg("20140515HP002", 339001122012, 801, 0, 339001122012);
            river.Add(test);
            test = new RiverSeg("20140515HP002", 339001122012, 836, 0, 339001122012);
            river.Add(test);

            /***************************上罗镇参数**************************************/
            //上罗镇参数
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140521HP002", 339001084, 2075,65536, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP002", 339001084, 2040, 0, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP002", 339001084, 2065, 0, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP002", 339001084, 2109, 0, 339001084);
            river.Add(test);
            */
            /***************************上罗镇参数**************************************/
            /***************************孝儿镇参数**************************************/
            //孝儿镇参数
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140521HP004", 339001084, 2001, 65536, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP004", 339001084002, 2028, 0, 339001084002);
            river.Add(test);
            test = new RiverSeg("20140521HP004", 339001084, 2040, 0, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP004", 339001084, 2059, 0, 339001084);
            river.Add(test);
             * */
            /***************************孝儿镇参数**************************************/
            /***************************怀化方案**************************************/
            //怀化方案
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140521HP005", 339001122022, 835, 0, 339001122022);
            river.Add(test);
            test = new RiverSeg("20140521HP005", 339001122022, 1069, 0, 339001122022);
            river.Add(test);
            test = new RiverSeg("20140521HP005", 1122022010, 1002, 0, 339001122022010);
            river.Add(test);
            test = new RiverSeg("20140521HP005", 1122022005, 1147, 0, 339001122022005);
            river.Add(test);
            test = new RiverSeg("20140521HP005", 1122022008, 1102, 0, 339001122022008);
            river.Add(test);
             */
            /***************************怀化方案**************************************/
            //return DoRiverSegCalc("20140311HP009", "hg02", "hydroglobal", river, "2013/12/20 20:28:54", "2014/3/9 20:28:54", "雨量站", "新安江模型", "扩散波模型", "hg474");
            //湘江之流
            return DoRiverSegCalc("20140515HP002", "hg02", "hydroglobal", river, "2014/5/10 20:36:48", "2014/5/16 20:36:48", "雨量站", "新安江模型", "扩散波模型", "hg02");
            //上罗镇参数
            //return DoRiverSegCalc("20140521HP002", "hg02", "hydroglobal", river, "2014/5/10 17:18:49", "2014/5/16 17:18:49", "雨量站", "新安江模型", "扩散波模型", "hg02");
            //孝儿镇参数
            //return DoRiverSegCalc("20140521HP004", "hg02", "hydroglobal", river, "2014/5/10 17:18:49", "2014/5/16 17:18:49", "雨量站", "新安江模型", "扩散波模型", "hg02");
            //怀化方案
            //return DoRiverSegCalc("20140521HP005", "hg02", "hydroglobal", river, "2014/5/10 17:18:49", "2014/5/16 17:18:49", "雨量站", "新安江模型", "扩散波模型", "hg02");


            //直接调用hpc 测试hpc部分
            /*
            string orauser = "hg02";
            string sid = "20140521HP005";
            WinderHelper wh = new WinderHelper(orauser, sid);
            wh.sid = "20140521HP005";
            wh.orauser = "hg02";
            string usercode = "hg02";
            return wh.CreateJob(sid, orauser, usercode);
           */
        }

        /// <summary>
        /// 查询获取空闲状态下的用户名,
        /// 若有空闲用户则执行计算过程
        /// 若不存在空间用户则返回空值，等待（前台控制）
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "多方案并行计算")]
        public string SelectAvailableUser(string sid, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel)
        {
            /**********************************************************************************************/
            string Result;
            string str = "";
            //string ResutlPwd;
            //测试用 把状态全部改成3
            //string sql = "update orausertable set state=3";
            //DbHelper.ExecuteSql(sql);
            //用户表 状态值3表示空闲状态
            string sql = "select * from orausertable where state=3";
            DataSet ds = DbHelper.Query(sql);
            DataTable dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                //可用用户名和密码一样
                Result = dt.Rows[0]["orauser"].ToString();
                //可用用户名密码
                //ResutlPwd = dt.Rows[0]["pwd"].ToString();
                //多方案并行计算
                new WinderHelper(Result, sid).DoRiverSegCalcNew(sid, Result, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, Result);
                str = "{\"msg\":\"ok\"}";
            }
            else
            {
                //Result = "";
            }
            return str;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "03连续多方案并行计算")]
        public string ContSim()
        {
            //暂时没考虑多个机构的计算方式 /分隔符
            string Result;
            string str = "";
            //系统当前时间
            System.DateTime dtNow = System.DateTime.Now;
            string dtNowShort = dtNow.ToString("yyyy-MM-dd")+" 00:00:00";
            string sql = "select * from hgmgr.schemecontsim order by sid desc";
            DataTable dt = DbHelper.Query(sql).Tables[0];
            DataTable dttmp;
            RiverSeg riverTmp;
            if (dt != null && dt.Rows.Count > 0)
            {
                string sid;
                int level;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    List<RiverSeg> rivers = new List<RiverSeg>();
                    sid = dt.Rows[i]["sid"].ToString();
                    level = Convert.ToInt32(dt.Rows[i]["CHANNELLEVEL"]);
                    string dtNext = dtNow.AddDays(System.Convert.ToDouble(dt.Rows[i]["DAYS"])).ToString("yyyy-MM-dd")+" 00:00:00";
                    sql = "update hgmgr.schemecontsim set STARTTIME=to_date('" + dtNowShort + "','yyyy-mm-dd hh24:mi:ss')" + ",ENDTIME=to_date('" +
                        dtNext + "','yyyy-mm-dd hh24:mi:ss') where sid='"+sid+"'";
                    DbHelper.ExecuteSql(sql);
                    //查询 definednodes
                    //sql = "select regionindex,bsvalue,bslength,'GS',regionindex2,channelindex from "+dt.Rows[i]["SIMUSER"].ToString()+".riversegs where regiongrade>="+dt.Rows[i]["CHANNELLEVEL"];
                    sql = "select regionidnew, min(binstrlen) binstrlen, 0 binstrval from hgmgr.jzjw_definednodes where binstrval=0 and regionidnew in ("+
                        "select min(regionidnew) from hgmgr.jzjw_definednodes where sid='"+sid+"') group by regionidnew";
                    dttmp = DbHelper.Query(sql).Tables[0];
                    if (dttmp != null && dttmp.Rows.Count > 0)
                    {
                        for (int j = 0; j < dttmp.Rows.Count; j++)
                        {
                            //
                            riverTmp = new RiverSeg(sid, System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]), System.Convert.ToInt64(dttmp.Rows[j]["binstrlen"]), System.Convert.ToInt64(dttmp.Rows[j]["binstrval"]), System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]));
                            rivers.Add(riverTmp);
                        }
                        //用户表 状态值3表示空闲状态
                        sql = "select * from orausertable where state=3";
                        DataTable dt1 = DbHelper.Query(sql).Tables[0];
                        if (dt1 != null && dt1.Rows.Count > 0)
                        {
                            Result = dt1.Rows[0]["orauser"].ToString();
                            sql = "select RAINFALLDATA,RUNOFFMODEL,RIVERMODEL from hgmgr.jzjw_scheme where sid='"+sid+"'";
                            DataTable dtmgr = DbHelper.Query(sql).Tables[0];
                            //多方案并行计算
                            new WinderHelper(Result, sid).ConSims(sid, Result, "hydroglobal", rivers, dtNowShort.ToString(), dtNext, dtmgr.Rows[0]["RAINFALLDATA"].ToString(), dtmgr.Rows[0]["RUNOFFMODEL"].ToString(), dtmgr.Rows[0]["RIVERMODEL"].ToString(), Result, level);
                            str = "{\"msg\":\"ok\"}";
                        }
                    }
                }
            }

            return str;
            
        }


        [WebMethod(Description = "Ren测试入口")]
        public string RenTestMore()
        {
            /**/
            /*****************欧洲云端测试数据******************************/
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140913HP002", 117001372022, 1731, 512, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1715, 0, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1743, 2097152, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1749, 134217728, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1724, 0, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1771, 131072, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1801, 3, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1814, 4096, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1796, 129, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022, 1793, 16, 117001372022);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022001, 1840, 0, 117001372022001);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022001, 1847, 512, 117001372022001);
            river.Add(test);
            test = new RiverSeg("20140913HP002", 117001372022001, 1861, 8388609, 117001372022001);
            river.Add(test);
            */
            /*****************法国代尔湖******************************/
            /*List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140922HP001", 234001011003, 901, 524288, 234001011003);
            river.Add(test);
            test = new RiverSeg("20140922HP001", 234001011003, 917, 1, 234001011003);
            river.Add(test);
            test = new RiverSeg("20140922HP001", 234001011003001, 939, 0, 234001011003001);
            river.Add(test);
            test = new RiverSeg("20140922HP001", 234001011, 879, 0, 234001011);
            river.Add(test);
            test = new RiverSeg("20140922HP001", 234001011, 911, 0, 234001011);
            river.Add(test);
            test = new RiverSeg("20140922HP001", 234001011003, 876, 0, 234001011003);
            river.Add(test);
            test = new RiverSeg("20140922HP001", 234001011, 863, 0, 234001011);
            river.Add(test);
            test = new RiverSeg("20140922HP001", 234001011003, 899, 536870944, 234001011003);
            river.Add(test);
            test = new RiverSeg("20140922HP001", 234001011003, 880, 1024, 234001011003);
            river.Add(test);
            */
            

            /*****************白水江******************************/
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140923HP001", 117001275027014, 6530, 0, 117001275027014);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027014, 6523, 0, 117001275027014);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027014, 6678, 0, 117001275027014);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027010, 6739, 0, 117001275027010);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027010, 6747, 1, 117001275027010);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027010, 6747, 0, 117001275027010);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027, 6757, 0, 117001275027);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027, 6418, 0, 117001275027);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027014, 6455, 0, 117001275027014);
            river.Add(test);

            test = new RiverSeg("20140923HP001", 117001275027, 6470, 0, 117001275027);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027, 6470, 1, 117001275027);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027, 6468, 0, 117001275027);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027, 6522, 0, 117001275027);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027, 6589, 0, 117001275027);
            river.Add(test);

            test = new RiverSeg("20140923HP001", 117001275027014, 6530, 1, 117001275027014);
            river.Add(test);
            test = new RiverSeg("20140923HP001", 117001275027, 6547, 65536, 117001275027);
            river.Add(test);
            */
            /*****************白水江0929新******************************/
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20141013HP001", 117001275027, 6949, 0, 117001275027);
            river.Add(test);
            test = new RiverSeg("20141013HP001", 117001275027003, 7021, 8192, 117001275027003);
            river.Add(test);
            test = new RiverSeg("20141013HP001", 117001275027003, 6998, 0, 117001275027003);
            river.Add(test);
            test = new RiverSeg("20141013HP001", 117001275027, 7023, 0, 117001275027);
            river.Add(test);
            test = new RiverSeg("20141013HP001", 117001275027, 7139, 0, 117001275027);
            river.Add(test);
            test = new RiverSeg("20141013HP001", 117001275027, 6956, 1, 117001275027);
            river.Add(test);
            test = new RiverSeg("20141013HP001", 117001275027, 6956, 0, 117001275027);
            river.Add(test);
           

            //珠江测试
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140620HP002", 341001114040014011, 1995, 0, 341001114040014011);
            river.Add(test);
            test = new RiverSeg("20140620HP002", 341001114040014011, 2085, 0, 341001114040014011);
            river.Add(test);
             */
            //test = new RiverSeg("20140620HP002", 341001114040014011002, 2063, 0, 341001114040014011002);
            //river.Add(test);
            //test = new RiverSeg("20140620HP002", 341001114040014011001, 2056, 0, 341001114040014011001);
            //river.Add(test);

            /***************************上罗镇参数**************************************/
            //上罗镇参数
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140521HP002", 339001084, 2075,65536, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP002", 339001084, 2040, 0, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP002", 339001084, 2065, 0, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP002", 339001084, 2109, 0, 339001084);
            river.Add(test);
            */
            /***************************上罗镇参数**************************************/
            /***************************孝儿镇参数**************************************/
            //孝儿镇参数
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140521HP004", 339001084, 2001, 65536, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP004", 339001084002, 2028, 0, 339001084002);
            river.Add(test);
            test = new RiverSeg("20140521HP004", 339001084, 2040, 0, 339001084);
            river.Add(test);
            test = new RiverSeg("20140521HP004", 339001084, 2059, 0, 339001084);
            river.Add(test);
             * */
            /***************************孝儿镇参数**************************************/
            /***************************怀化方案**************************************/
            //怀化方案
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20140521HP005", 339001122022, 835, 0, 339001122022);
            river.Add(test);
            test = new RiverSeg("20140521HP005", 339001122022, 1069, 0, 339001122022);
            river.Add(test);
            test = new RiverSeg("20140521HP005", 1122022010, 1002, 0, 339001122022010);
            river.Add(test);
            test = new RiverSeg("20140521HP005", 1122022005, 1147, 0, 339001122022005);
            river.Add(test);
            test = new RiverSeg("20140521HP005", 1122022008, 1102, 0, 339001122022008);
            river.Add(test);
             */
            /***************************怀化方案*****************************************************************/
            return SelectAvailableUser("20141013HP001", "hydroglobal", river, "2014/10/13 19:41:33", "2014/10/16 19:41:34", "雨量站", "新安江模型", "扩散波模型");
            //return SelectAvailableUser("20140311HP009","hydroglobal", river, "2013/12/20 20:28:54", "2014/3/9 20:28:54", "雨量站", "新安江模型", "扩散波模型", "hg474");
            //珠江测试
            //return SelectAvailableUser("20140615HP001", "hydroglobal", river, "2014/6/20 9:31:58", "2014/6/23 9:31:59", "雨量站", "新安江模型", "扩散波模型");
            //上罗镇参数
            //return SelectAvailableUser("20140521HP002", "hydroglobal", river, "2014/5/10 17:18:49", "2014/5/16 17:18:49", "雨量站", "新安江模型", "扩散波模型", "hg02");
            //孝儿镇参数
            //return SelectAvailableUser("20140521HP004", "hydroglobal", river, "2014/5/10 17:18:49", "2014/5/16 17:18:49", "雨量站", "新安江模型", "扩散波模型", "hg02");
            //怀化方案
            //return SelectAvailableUser("20140521HP005", "hydroglobal", river, "2014/5/10 17:18:49", "2014/5/16 17:18:49", "雨量站", "新安江模型", "扩散波模型", "hg02");


            //直接调用hpc 测试hpc部分
            /*
            string orauser = "hg02";
            string sid = "20140521HP005";
            WinderHelper wh = new WinderHelper(orauser, sid);
            wh.sid = "20140521HP005";
            wh.orauser = "hg02";
            string usercode = "hg02";
            return wh.CreateJob(sid, orauser, usercode);
           */
        }


    }
}