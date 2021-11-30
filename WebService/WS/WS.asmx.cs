using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Services;
using JZJW.Winder.BLL;


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
        [WebMethod(Description = "tmp")]
        public string tmp()
        {
            return new WinderHelper("", "", "", "", 1, false).Tmp();
        }
        [WebMethod(Description = "ren测试服务-出流过程")]
        public string HelloWorld()
        {
            //测试实测数据
            //new  WinderHelper("","","").GetRainDataFromReal("HPC20150410", new DateTime(2015, 4, 18), new DateTime(2015, 4, 20), "hg07");
            //查询机构的最新数据日期
            return new WinderHelper("", "", "", "", 1, false).GetLatestForcastDate();
            //获取所有的实测出流数据
            /*
            DateTime dt = new DateTime(2015, 4, 24);
            do
            {
                new WinderHelper("", "", "").GetQoutputFromReal("20150410HP001", dt, "hg103");
                dt=dt.AddDays(-15);
            } while (dt > new DateTime(2015, 1, 1));
            
            return "OK";
             */
        }

        //[WebMethod]
        //public string GetLineChartData(string name, string regionid, string binstrlen, string binstrval, string reghighid)
        //{
        //    return new WinderHelper().GetLineChartData(name, regionid, binstrlen, binstrval, reghighid);
        //}
        //orauser= hg02  ，userid=user usercode=用户密码
        //orauserTable state=3  orauser 空闲状态
        //                   1          数据入库
        //                   2          计算  

        //[WebMethod(Description = "单方案计算")]
        public string DoRiverSegCalc(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode)
        {
            //orauser = "hg02";
            WinderHelper wh = new WinderHelper(orauser, sid, starttime, endtime, 1, false);
            //wh.sid = sid;
            //wh.orauser = orauser;
            //usercode = "hg02";
            string rst = wh.DoRiverSegCalc(sid, orauser, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, usercode);
            return rst;
            //多用户同时计算ByRen
            //return WinderHelper.DoRiverSegCalcNew(sid, orauser, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, usercode);
        }

        [WebMethod(Description = "Ren测试降雨数据服务2入口")]
        public void RenTest()
        {
            //new WinderHelper("","","").GetRainData(117, 117.5, 35, 35.5, "ecmwf", DateTime.Parse("2014-12-01 00:00:00"), DateTime.Parse("2014-12-05 00:00:00"));
            //没问题
            //new WinderHelper("", "", "").GetRainData2(103, 105, 32, 34, "ecmwf", DateTime.Parse("2015-03-04 00:00:00"), DateTime.Parse("2015-03-06 00:00:00"));
            new WinderHelper("", "", "", "", 1, false).GetRainData2(103, 105, 32, 34, "ncep", DateTime.Parse("2015-05-19 00:00:00"), DateTime.Parse("2015-05-24 00:00:00"));
            //new WinderHelper("", "", "").GetRainData2(103, 104, 33, 34, "ecmwf", DateTime.Parse("2010-07-01 00:00:00"), DateTime.Parse("2010-07-05 00:00:00"));
            /*
            string json = DynamicWebServiceProxy.InvokeWS();
            string[] args = new string[7] ;
            args[0] = "102";
            args[1] = "102.5";
            args[2] = "44";
            args[3] = "44.5";
            args[4] = "ecmwf";
            args[5] = "20140601";
            args[6] = "20140602";
            object rst = DynamicWebServiceProxy.InvokeWebService(DynamicWebServiceProxy.wsurl, "QueryForcastRainDataByRange", args);
           */
            /*
           string[] args = new string[2];
           args[0] = "beijing";
           args[1] = "China";
           object result = DynamicWebServiceProxy.InvokeWebService(DynamicWebServiceProxy.wsurl, "GetWeather", args);
           return result.ToString();
            */
            /*
            string[] args = new string[2];
            object rst = DynamicWebServiceProxy.InvokeWebService(DynamicWebServiceProxy.wsurl, "RenTestMore", args);
             * */
            //return rst.ToString();
        }

        /// <summary>
        /// 查询获取空闲状态下的用户名,
        /// 若有空闲用户则执行计算过程
        /// 若不存在空间用户则返回空值，等待（前台控制）
        /// </summary>
        /// <returns></returns>
        //[WebMethod(Description = "多方案并行计算")]
        public string SelectAvailableUser(string sid, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string cores, string levelsidid, string preheat)
        {
            /**********************************************************************************************/
            string Result;
            string str = "";
            //用户表 状态值3表示空闲状态
            string sql = "select * from orausertable where state=3 and isok=1 order by orauser";
            DataSet ds = DbHelper.Query(sql);
            DataTable dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                //可用用户名和密码一样
                Result = dt.Rows[0]["orauser"].ToString();
                //可用用户名密码
                //ResutlPwd = dt.Rows[0]["pwd"].ToString();
                rivers = createRivers(sid);
                //预热期设置
                //endtime = Convert.ToDateTime(starttime).AddDays(30).ToString("yyyy-MM-dd hh:mm:ss");              

                //多方案并行计算
                new WinderHelper(Result, sid, starttime, endtime, Convert.ToDouble(preheat), false).DoRiverSegCalcNew(sid, Result, userid, rivers, starttime, endtime, rainfalldata, runoffmodel, rivermodel, Result, cores, levelsidid, "ecmwf", Convert.ToDouble(preheat), 1);
                str = "{\"msg\":\"ok\"}";

            }
            else
            {
                //Result = "";
            }
            return str;
        }
        /// <summary>
        /// 获取出口河段
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public List<RiverSeg> createRivers(string sid)
        {
            string sql = "select * from hgmgr.jzjw_definednodes where sid='" + sid + "'";
            DataTable dt = DbHelper.Query(sql).Tables[0];
            List<RiverSeg> river = new List<RiverSeg>();
            if (dt != null && dt.Rows.Count > 0)
            {
                RiverSeg test;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //test = new RiverSeg(sid, Convert.ToInt64(dt.Rows[i]["REGIONID"]), Convert.ToInt64(dt.Rows[i]["BINSTRLEN"]), Convert.ToInt64(dt.Rows[i]["BINSTRVAL"]),Convert.ToInt64(dt.Rows[i]["REGIONIDNEW"]));
                    test = new RiverSeg(sid, Convert.ToInt64(dt.Rows[i]["REGIONID"]), Convert.ToInt64(dt.Rows[i]["BINSTRLEN"]),
                        Convert.ToInt64(dt.Rows[i]["BINSTRVAL"]), Convert.ToInt64(dt.Rows[i]["REGIONIDNEW"]), dt.Rows[i]["NODETYPE"].ToString());
                    river.Add(test);
                }
            }
            return river;
        }
        /// <summary>
        /// </summary>
        /// <returns></returns>
        //[WebMethod(Description = "03连续多方案并行计算")]
        public string ContSim()
        {
            //暂时没考虑多个机构的计算方式 /分隔符
            string Result;
            string str = "";
            //系统当前时间
            System.DateTime dtNow = System.DateTime.Now;
            string dtNowShort = dtNow.ToString("yyyy-MM-dd") + " 00:00:00";
            string sql = "select * from hgmgr.schemecontsim order by sid desc";//asc
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
                    string dtNext = dtNow.AddDays(System.Convert.ToDouble(dt.Rows[i]["DAYS"])).ToString("yyyy-MM-dd") + " 00:00:00";
                    sql = "update hgmgr.schemecontsim set STARTTIME=to_date('" + dtNowShort + "','yyyy-mm-dd hh24:mi:ss')" + ",ENDTIME=to_date('" +
                        dtNext + "','yyyy-mm-dd hh24:mi:ss') where sid='" + sid + "'";
                    DbHelper.ExecuteSql(sql);
                    //查询 definednodes
                    //sql = "select regionindex,bsvalue,bslength,'GS',regionindex2,channelindex from "+dt.Rows[i]["SIMUSER"].ToString()+".riversegs where regiongrade>="+dt.Rows[i]["CHANNELLEVEL"];
                    /*20150110修改
                    sql = "select regionidnew, min(binstrlen) binstrlen, 0 binstrval,nodetype from hgmgr.jzjw_definednodes where binstrval=0 and regionidnew in ("+
                        "select min(regionidnew) from hgmgr.jzjw_definednodes where sid='"+sid+"') group by regionidnew";
                     */
                    sql = "select regionidnew, binstrlen, binstrval,nodetype from hgmgr.jzjw_definednodes where sid='" + sid + "'";
                    dttmp = DbHelper.Query(sql).Tables[0];
                    if (dttmp != null && dttmp.Rows.Count > 0)
                    {
                        for (int j = 0; j < dttmp.Rows.Count; j++)
                        {
                            //1222
                            //riverTmp = new RiverSeg(sid, System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]), System.Convert.ToInt64(dttmp.Rows[j]["binstrlen"]), System.Convert.ToInt64(dttmp.Rows[j]["binstrval"]), System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]));
                            riverTmp = new RiverSeg(sid, System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]), System.Convert.ToInt64(dttmp.Rows[j]["binstrlen"]),
                               System.Convert.ToInt64(dttmp.Rows[j]["binstrval"]), System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]), dttmp.Rows[j]["nodetype"].ToString());

                            //riverTmp = new RiverSeg(sid, System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]), System.Convert.ToInt64(dttmp.Rows[j]["binstrlen"]), System.Convert.ToInt64(dttmp.Rows[j]["binstrval"]), dttmp.Rows[j]["regionidnew"].ToString());
                            rivers.Add(riverTmp);
                        }
                        //用户表 状态值3表示空闲状态
                        sql = "select * from orausertable where state=3 order by orauser";
                        DataTable dt1 = DbHelper.Query(sql).Tables[0];
                        if (dt1 != null && dt1.Rows.Count > 0)
                        {
                            Result = dt1.Rows[0]["orauser"].ToString();
                            sql = "select RAINFALLDATA,RUNOFFMODEL,RIVERMODEL from hgmgr.jzjw_scheme where sid='" + sid + "'";
                            DataTable dtmgr = DbHelper.Query(sql).Tables[0];
                            //多方案并行计算
                            new WinderHelper(Result, sid, dtNowShort, dtNext, Convert.ToDouble(dt.Rows[i]["SPREHEAT"].ToString()), true).ConSims(sid, Result, "hydroglobal", rivers, dtNowShort.ToString(), dtNext, dtmgr.Rows[0]["RAINFALLDATA"].ToString(), dtmgr.Rows[0]["RUNOFFMODEL"].ToString(), dtmgr.Rows[0]["RIVERMODEL"].ToString(), Result, level, "4", "1");
                            str = "{\"msg\":\"ok\"}";
                        }
                    }
                }
            }

            return str;

        }

        [WebMethod(Description = "04连续多方案并行计算_不同机构")]
        public string ContSimAgency()
        {
            //暂时没考虑多个机构的计算方式 /分隔符
            string Result;
            string str = "";
            //系统当前时间 往前推3天
            //System.DateTime dtNow = System.DateTime.Now.AddDays(-(kk));

            //System.DateTime dtNow = System.DateTime.Now.AddDays(-11);
            System.DateTime dtNow = System.DateTime.Now;
            string dtNowShort = dtNow.ToString("yyyy-MM-dd") + " 00:00:00";
            string sql = "select * from hgmgr.schemecontsim where isenable ='1' order by sid asc";//asc   desc
            DataTable dt = DbHelper.Query(sql).Tables[0];
            DataTable dttmp;
            RiverSeg riverTmp;

            if (dt != null && dt.Rows.Count > 0)
            {
                string sid;
                int level;
                string origin;
                double preheat;
                //从其他地方获取降雨数据  int raindataws = 1;
                //从本地获取
                int raindataws = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    List<RiverSeg> rivers = new List<RiverSeg>();
                    sid = dt.Rows[i]["sid"].ToString();
                    level = Convert.ToInt32(dt.Rows[i]["CHANNELLEVEL"]);
                    origin = dt.Rows[i]["INSTITUTION"].ToString();
                    preheat = Convert.ToInt32(dt.Rows[i]["SPREHEAT"]);
                    //修改  根据设定的起始时间
                    string dtNext = dtNow.AddDays(System.Convert.ToDouble(dt.Rows[i]["DAYS"])).ToString("yyyy-MM-dd") + " 00:00:00";
                    /*
                    dtNowShort = Convert.ToDateTime(dt.Rows[i]["STARTTIMEORIG"]).ToString();
                    string dtNext = dtNow.AddDays(System.Convert.ToDouble(dt.Rows[i]["DAYS"])).ToString("yyyy-MM-dd") + " 00:00:00";
                    */

                    sql = "select regionidnew, binstrlen, binstrval,nodetype from hgmgr.jzjw_definednodes where sid='" + sid + "'";
                    dttmp = DbHelper.Query(sql).Tables[0];
                    if (dttmp != null && dttmp.Rows.Count > 0)
                    {
                        for (int j = 0; j < dttmp.Rows.Count; j++)
                        {
                            riverTmp = new RiverSeg(sid, System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]), System.Convert.ToInt64(dttmp.Rows[j]["binstrlen"]),
                               System.Convert.ToInt64(dttmp.Rows[j]["binstrval"]), System.Convert.ToInt64(dttmp.Rows[j]["regionidnew"]), dttmp.Rows[j]["nodetype"].ToString());
                            rivers.Add(riverTmp);
                        }
                        //用户表 状态值3表示空闲状态
                        //sql = "select * from orausertable where state=3 order by orauser";
                        //DataTable dt1 = DbHelper.Query(sql).Tables[0];
                        DataTable dt1 = new DataTable();
                        for (int a = 0; a < 20; a++)
                        {
                            //判断是否有可用用户，若无，则等待60秒，共20次
                            dt1 = getorauser("0");
                            if (dt1 != null && dt1.Rows.Count > 0)
                            {
                                break;
                            }
                            else
                            {
                                //等待60秒
                                System.Threading.Thread.Sleep(60000);
                            }
                        }
                        if (dt1 != null && dt1.Rows.Count > 0)
                        {
                            Result = dt1.Rows[0]["orauser"].ToString();
                            sql = "select RAINFALLDATA,RUNOFFMODEL,RIVERMODEL from hgmgr.jzjw_scheme where sid='" + sid + "'";
                            DataTable dtmgr = DbHelper.Query(sql).Tables[0];

                            sql = "update hgmgr.schemecontsim set STARTTIME=to_date('" + dtNowShort + "','yyyy-mm-dd hh24:mi:ss')" + ",ENDTIME=to_date('" +
                       dtNext + "','yyyy-mm-dd hh24:mi:ss') where sid='" + sid + "'";
                            DbHelper.ExecuteSql(sql);
                            //连续多方案并行计算
                            new WinderHelper(Result, sid, dtNowShort, dtNext, Convert.ToDouble(preheat), true).ConSimsAgency(sid, Result, "hydroglobal", rivers, dtNowShort.ToString(),
                                dtNext, dtmgr.Rows[0]["RAINFALLDATA"].ToString(), dtmgr.Rows[0]["RUNOFFMODEL"].ToString(),
                                dtmgr.Rows[0]["RIVERMODEL"].ToString(), Result, level, "4", "1", origin, preheat, raindataws, 1);
                            str = "{\"msg\":\"ok\"}";
                        }
                    }
                }
            }

            return str;

        }
        private DataTable getorauser(string isok)
        {
            string sql = "select * from orausertable where state=3 and isok='" + isok + "' order by orauser";
            return DbHelper.Query(sql).Tables[0];
        }
        [WebMethod(Description = "04Ren测试入口_多方案_不同机构_最新测试接口_美国_sxh")]
        public string RenTestMoreAgency()
        {
            List<RiverSeg> river = new List<RiverSeg>();
            //RiverSeg test = new RiverSeg("20150617HP001", 117001348035, 2893, 0, 117001348035, "GS");
            //river.Add(test);
            //test = new RiverSeg("20150617HP001", 117001348035, 2901, 0, 117001348035, "GS");
            //river.Add(test);

            /***************************参数**************************************/
            //return SelectAvailableUserAgency("20150410HP010", "hydroglobal", river, "2015/4/17 00:00:00", "2015/4/22 10:23:46", "新安江模型", "新安江模型", "新安江模型", "4", "1", "2");
            //return SelectAvailableUserAgency("20150905HP010", "hydroglobal", river, "2015/9/5 11:26:01", "2015/9/8 11:26:01", "雨量站", "新安江模型", "扩散波模型", "4", "1", "1", "ecmwf");
            //美国第一个方案
            //return SelectAvailableUserAgency("20150628HP001", "hydroglobal", river, "2015/6/15 00:00:00", "2015/6/28 00:00:00", "雨量站", "新安江模型", "扩散波模型", "4", "1", "1", "ecmwf");
            //史四个机构
            //return SelectAvailableUserAgency("20150628HP002", "hydroglobal", river, "2015/6/15 00:00:00", "2015/6/28 00:00:00", "雨量站", "新安江模型", "扩散波模型", "4", "1", "1", "ecmwf");
            //return SelectAvailableUserAgency("20150628HP003", "hydroglobal", river, "2015/6/15 00:00:00", "2015/6/28 00:00:00", "雨量站", "新安江模型", "扩散波模型", "4", "1", "10", "cmc");
            //return SelectAvailableUserAgency("20150628HP004", "hydroglobal", river, "2015/6/15 00:00:00", "2015/6/28 00:00:00", "雨量站", "新安江模型", "扩散波模型", "4", "1", "10", "ncep");
            //return SelectAvailableUserAgency("20150628HP005", "hydroglobal", river, "2015/6/15 00:00:00", "2015/6/28 00:00:00", "雨量站", "新安江模型", "扩散波模型", "4", "1", "10", "ukmo");
            //return SelectAvailableUserAgency("20151128HP001", "hydroglobal", river, "2015/11/25 00:00:00", "2015/11/28 00:00:00", "雨量站", "新安江模型", "扩散波模型", "4", "1", "10", "ukmo");

            //sxh区域测试
            return sm_commit("20160327HP002", "hydroglobal", "2016/1/18 00:00:00", "2016/1/19 00:00:00",
                "雨量站", "新安江模型", "扩散波模型", "4", "1", "1", "ecmwf");

        }


        //沙溪河提交方案计算接口
        [WebMethod(Description = "沙溪河提交方案计算")]
        public string sm_commit(string sid, string userid,
            string starttime, string endtime,
            string rainfalldata, string runoffmodel, string rivermodel,
            string cores, string levelsidid, string preheat, string agency)
        {
            string Result;
            string str = "";
            List<RiverSeg> rivers;
            DataTable dt = getorauser("5");
            if (dt != null && dt.Rows.Count > 0)
            {
                Result = dt.Rows[0]["orauser"].ToString();
                rivers = createRivers(sid);
                //预热期设置
                //endtime = Convert.ToDateTime(starttime).AddDays(30).ToString("yyyy-MM-dd hh:mm:ss"); 
                //多方案并行计算
                //new WinderHelper(Result, sid, starttime).ConSimsAgency(sid, Result, userid, rivers, starttime, endtime, 
                //    rainfalldata, runoffmodel, rivermodel, Result,6, cores, levelsidid, "ecmwf", Convert.ToDouble(preheat), 1,0);
                //从外部获取降雨数据 暂时取消
                //new WinderHelper(Result, sid, starttime, endtime, Convert.ToDouble(preheat),true).ConSimsAgency(sid, Result, userid, rivers, starttime, endtime,
                //   rainfalldata, runoffmodel, rivermodel, Result, 6, cores, levelsidid, agency, Convert.ToDouble(preheat), 1, 0);
                new WinderHelper(Result, sid, starttime, endtime, Convert.ToDouble(preheat), false).ConSimsAgency_sxh(sid, Result, userid, rivers, starttime, endtime,
                   rainfalldata, runoffmodel, rivermodel, Result, 6, cores, levelsidid, agency, Convert.ToDouble(preheat), 1, 2);
                str = "{\"msg\":\"ok\"}";

            }
            else
            {
                str = "{\"msg\":\"暂时无可用计算用户\"}";
            }
            return str;
        }

        [WebMethod(Description = "04多方案并行计算_不同机构")]
        public string SelectAvailableUserAgency(string sid, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string cores, string levelsidid, string preheat, string agency)
        {
            string Result;
            string str = "";
            //string sql = "select * from orausertable where state=3 order by orauser";
            //DataSet ds = DbHelper.Query(sql);
            //DataTable dt = ds.Tables[0];
            DataTable dt = getorauser("2");
            if (dt != null && dt.Rows.Count > 0)
            {
                //可用用户名和密码一样
                Result = dt.Rows[0]["orauser"].ToString();
                rivers = createRivers(sid);
                //预热期设置
                //endtime = Convert.ToDateTime(starttime).AddDays(30).ToString("yyyy-MM-dd hh:mm:ss"); 
                //多方案并行计算
                //new WinderHelper(Result, sid, starttime).ConSimsAgency(sid, Result, userid, rivers, starttime, endtime, 
                //    rainfalldata, runoffmodel, rivermodel, Result,6, cores, levelsidid, "ecmwf", Convert.ToDouble(preheat), 1,0);
                //从外部获取降雨数据 暂时取消
                //new WinderHelper(Result, sid, starttime, endtime, Convert.ToDouble(preheat),true).ConSimsAgency(sid, Result, userid, rivers, starttime, endtime,
                //   rainfalldata, runoffmodel, rivermodel, Result, 6, cores, levelsidid, agency, Convert.ToDouble(preheat), 1, 0);
                new WinderHelper(Result, sid, starttime, endtime, Convert.ToDouble(preheat), true).ConSimsAgency(sid, Result, userid, rivers, starttime, endtime,
                   rainfalldata, runoffmodel, rivermodel, Result, 6, cores, levelsidid, agency, Convert.ToDouble(preheat), 0, 0);
                str = "{\"msg\":\"ok\"}";

            }
            else
            {
                str = "{\"msg\":\"error\"}";
            }
            return str;
        }
        //[WebMethod(Description = "Ren测试入口_多方案")]
        public string RenTestMore()
        {
            /***************** 最新测试******************************/
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20150331HP001", 117001389003, 777, 0, 117001389003, "GS");
            river.Add(test);
            test = new RiverSeg("20150331HP001", 117001389003, 811, 1024, 117001389003, "GS");
            river.Add(test);
            test = new RiverSeg("20150331HP001", 117001389003, 836, 34359738368, 117001389003, "GS");
            river.Add(test);
            test = new RiverSeg("20150331HP001", 117001389003, 821, 0, 117001389003, "GS");
            river.Add(test);


            /*****************20150117黑荷塘方案3号方案  hg03用户  最新测试*****************************
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20141228HP002", 117001275027, 6949, 0, 117001275027, "RS");
            river.Add(test);
            test = new RiverSeg("20141228HP002", 117001275027, 6740, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP002", 117001275027, 6762, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP002", 117001275027, 6784, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP002", 117001275027006, 6785, 0, 117001275027006, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP002", 117001275027, 6793, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP002", 117001275027, 6826, 0, 117001275027, "GS");
            river.Add(test);
            */
            /***************南坪 hg02用户********************************************************************************/
            /*
              List<RiverSeg> river = new List<RiverSeg>();
             RiverSeg test = new RiverSeg("20141228HP003", 117001275027, 6624, 0, 117001275027, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027, 6634, 0, 117001275027, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027011, 6671, 0, 117001275027011, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027011, 6687, 0, 117001275027011, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027, 6603, 0, 117001275027, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027011, 6607, 0, 117001275027011, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027011, 6635, 4, 117001275027011, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027011, 6636, 9, 117001275027011, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027011, 6636, 8, 117001275027011, "GS");
           river.Add(test);
           test = new RiverSeg("20141228HP003", 117001275027011, 6740, 0, 117001275027011, "RS");
           river.Add(test);
            * */
            /***********************青龙 hg01用户**********************************************************/
            /*
            List<RiverSeg> river = new List<RiverSeg>();
            RiverSeg test = new RiverSeg("20141228HP004", 117001275027, 6589, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP004", 117001275027, 6614, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP004", 117001275027, 6561, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP004", 117001275027, 6572, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP004", 117001275027, 6636, 0, 117001275027, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP004", 117001275027011, 6678, 1, 117001275027011, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP004", 117001275027011, 6678, 0, 117001275027011, "GS");
            river.Add(test);
            test = new RiverSeg("20141228HP004", 117001275027, 6603, 0, 117001275027, "RS");
            river.Add(test);             
            */
            /***************************参数**************************************/
            return SelectAvailableUser("20150331HP001", "hydroglobal", river, "2015/1/31 10:23:46", "2015/2/1 10:23:46", "新安江模型", "新安江模型", "新安江模型", "4", "1", "10");

        }

    }
}