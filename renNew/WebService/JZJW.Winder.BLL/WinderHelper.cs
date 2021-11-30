using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

//using JZJW.Winder.DAL;


namespace JZJW.Winder.BLL
{
    public class WinderHelper
    {
        /// <summary>
        /// 定义属性
        /// </summary>
        /// <param name="orauser">用户名</param>
        /// <param name="sid">方案编码</param>
        public WinderHelper(string orauser, string sid)
        {
            this.orauser = orauser;
            this.sid = sid;
        }
        public string orauser { get; set; }
        public string sid { get; set; }

        string sql = "";
        // 测试ren
        public string TestRen()
        {
            //string sql = "select * from bureau t";
            /*1测试
            string sql = "select * from jzjw_schememgr";
            DataSet i=DbHelper.Query(sql);
            return i.Tables[0].Rows.Count.ToString();
            */
            /*
            string starttime = "2014/3/10 20:28:54";
            string endtime = "2014/3/9 20:28:54";
            string orauser = "dddd";
            string sql = "";
            //模拟开始时间
            DateTime dtStart = Convert.ToDateTime(starttime);
            //模拟结束时间
            DateTime dtEnd = Convert.ToDateTime(endtime);
            string TimeStart = "1950-01-01 00:00:00";
            //起始时间的跨度小时
            double starthouroffset = dtStart.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //终止时间的跨度小时
            double endhouroffset = dtEnd.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //STARTYEAR,STARTMONTH,STARTDAY,STARTHOUR,ENDYEAR,ENDMONTH,ENDDAY,ENDHOUR及STARTHOUROFFSET,ENDHOUROFFSET
            sql = "update " + orauser + ".hydrousepara set STARTYEAR=" + dtStart.Year + ",STARTMONTH=" + dtStart.Month +
                ",STARTDAY=" + dtStart.Day + ",STARTHOUR=" + dtStart.Hour + ",ENDYEAR=" + dtEnd.Year + ",ENDMONTH=" +
                dtEnd.Month + ",ENDDAY=" + dtEnd.Day + ",STARTHOUROFFSET=" + starthouroffset + ",ENDHOUROFFSET=" + endhouroffset;
            DbHelper.ExecuteSql(sql);
            */

            return "";
        }

        //private string strConn = "Data Source=dwm474;User ID=hydroglobal;Password=hydroglobal474;";
        #region 隐藏
        /// <summary>
        /// 流量过程和水位过程
        /// </summary>
        /// <param name="name"></param>
        /// <param name="regionid"></param>
        /// <param name="binstrlen"></param>
        /// <param name="binstrval"></param>
        /// <param name="reghighid"></param>
        /// <returns></returns>
        public string GetLineChartData(string name, string regionid, string binstrlen, string binstrval, string reghighid)
        {
            string strResult = "";
            if (name == "V")
            {
                strResult = GetLineChartVSection(reghighid, regionid, binstrval, binstrlen, name);
            }
            else
            {
                strResult = GetLineChartData(regionid, binstrval, binstrlen, name);
            }
            return strResult;
        }

        /// <summary>
        /// 高程
        /// </summary>
        /// <param name="reghighid"></param>
        /// <param name="regionid"></param>
        /// <param name="binstrval"></param>
        /// <param name="binstrlen"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetLineChartVSection(string reghighid, string regionid, string binstrval, string binstrlen, string name)
        {
            string str = "";
            string sql = "select regionidnew,upeleva,downeleva,chnl_len from hg02.gdn_asia_river1 where regionid=" + regionid + " and binstrlen>=0 and binstrlen<=" + binstrlen + " and binstrval=gdem.get_route_bsv(" + binstrval + ",binstrlen," + binstrlen + ") order by binstrlen desc";
            StringBuilder sb = new StringBuilder();
            try
            {
                //IDataBase mydb = DBFactory.GetDBInstance(strConn);
                //DataTable dt = mydb.GetDataTable(sql);
                DataTable dt = DbHelper.Query(sql).Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    double x = 0.0;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        double upeleva = Convert.ToInt32(dt.Rows[i]["upeleva"]);
                        double downeleva = Convert.ToInt32(dt.Rows[i]["downeleva"]);
                        double chnllen = Convert.ToDouble(dt.Rows[i]["chnl_len"]);
                        if (i == (dt.Rows.Count - 1))
                        {
                            sb.Append("{\"DAY\":" + x + ",\"WH\":" + downeleva + "}");
                        }
                        else
                        {
                            sb.Append("{\"DAY\":" + x + ",\"WH\":" + upeleva + "},");
                        }

                        x = x + chnllen;

                    }
                }
                str = "{\"rows\":[" + sb.ToString() + "]}";
            }
            catch (Exception e)
            {
                str = e.Message;
            }

            return str;
        }

        private string GetLineChartData(string regionid, string bsvalue, string bslen, string name)
        {
            string str = "";
            string sql = "select to_char((to_date('1950-01-01 00:00:00','yyyy-mm-dd hh24:mi:ss')+houroffset/24),'yyyy-mm-dd HH24') as day ," + name + " as wh from discharge where regionindex=" + regionid + " and bsvalue=" + bsvalue + " and bslength=" + bslen; ;
            str = GetJson(DbHelper.Query(sql));
            return str;
        }
        #endregion

        /// <summary>
        /// DataSet转成Json格式
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        private string GetJson(DataSet ds)
        {
            string str = "", colName = "";
            int rowCount = 0;
            StringBuilder sb = new StringBuilder();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows.Count > 0)
                {
                    rowCount = dt.Rows.Count;
                    for (int i = 0; i < rowCount; i++)
                    {
                        int cols = dt.Columns.Count;
                        sb.Append("{");
                        for (int j = 0; j < cols; j++)
                        {
                            colName = dt.Columns[j].ColumnName;
                            sb.Append("\"" + colName + "\":\"" + dt.Rows[i][colName] + "\"");
                            if (j != (cols - 1)) sb.Append(",");
                        }
                        if (i == (dt.Rows.Count - 1)) sb.Append("}"); else sb.Append("},");
                    }
                    str = sb.ToString();
                }
            }

            return "{\"total\":" + rowCount + ",\"rows\":[" + str + "]}";
        }


        /// <summary>
        /// 单方案计算
        /// </summary>
        /// <param name="sid">方案编号</param>
        /// <param name="orauser">用户名</param>
        /// <param name="userid">hydrologal用户</param>
        /// <param name="rivers">上游河段</param>
        /// <param name="starttime">起始时间</param>
        /// <param name="endtime">终止时间</param>
        /// <param name="rainfalldata">降雨</param>
        /// <param name="runoffmodel">降雨模型</param>
        /// <param name="rivermodel">河流模型</param>
        /// <param name="usercode">用户密码</param>
        /// <returns></returns>
        public string DoRiverSegCalc(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode)
        {
            string str = "{\"msg\":\"ok\"}";
            //删除已有Sid
            string sql = "delete from jzjw_schememgr where sid='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            //插入大洲编码
            RiverSeg topOne = rivers[0];
            string topTwo = topOne.REGIONIDNEW.ToString();
            if (topTwo.Length > 3) topTwo = topTwo.Substring(0, 3);

            sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,datashift,concode) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                    starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "')";
            DbHelper.ExecuteSql(sql);

            ///00  truncate
            sql = "truncate table " + orauser + ".riversegs";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".discharge";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".regionconnection";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".definednodes";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".status";
            DbHelper.ExecuteSql(sql);
            //20140320添加代码ByRen
            /////////////////////////////////////////////////
            sql = "truncate table " + orauser + ".tvarparameter";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".raincmorph3H25KM";
            DbHelper.ExecuteSql(sql);
            /////////////////////////////////////////////////
            long minRegionidnew = long.MaxValue, minBinstrlen = long.MaxValue;
            //01
            if (rivers != null && rivers.Count > 0)
            {
                foreach (RiverSeg rs in rivers)
                {
                    if (rs.REGIONIDNEW < minRegionidnew) minRegionidnew = rs.REGIONIDNEW;
                    if (rs.BINSTRLEN < minBinstrlen) minBinstrlen = rs.BINSTRLEN;

                    string rgin = "" + rs.REGIONIDNEW;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                        rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "')";
                    DbHelper.ExecuteSql(sql);
                }


            }

            int len = minRegionidnew.ToString().Length;

            //02
            //此句不用修改
            sql = "insert into " + orauser + ".riversegs (select channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "hg02" + ".gdn2_asia_river1 " +
"where regioncode='" + minRegionidnew + "' and binstrlen>=" + minBinstrlen + " and 0=" + "get_out_bsv(binstrval,binstrlen," + minBinstrlen + "))";

            DbHelper.ExecuteSql(sql);
            /*之前
            sql="insert into "+orauser+".riversegs (select  channelid,binstrlen,binstrval,999,regionidnew,strahler,src_area,lft_area,rgt_area,0,0,0,src_slope,lft_slope,rgt_slope,src_len,lft_len,rgt_len,"+
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regionid from "+orauser+".gdn_asia_river1 where substr(regionidnew,1,"+len+")='"+minRegionidnew+"' and  length(regionidnew)>"+len+")"; 
            */
            //ren修改 上游河段拷贝1
            /*
            sql = "insert into " + orauser + ".riversegs (select channelid,binstrlen,binstrval,999,regionidnew,strahler,src_area,lft_area,rgt_area,0,0,0,src_slope,lft_slope,rgt_slope,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regionid from " + orauser + ".gdn_asia_river1 "+
"where regionidnew in (select regionidnew from (select regionidnew,min(binstrlen) from "+orauser+".gdn_asia_river1 "+
"where substr(regionidnew,1,"+len+")='"+minRegionidnew+"' and length(regionidnew)>"+len+" group by regionidnew "+
"having min(binstrlen)>"+minBinstrlen+")))";
            */
            //ren修改 上游河段拷贝2

            int lenNew = len + 3;
            sql = "insert into " + orauser + ".riversegs (select  channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "hg02" + ".gdn2_asia_river1 " +
"where regioncode in (select regioncode from (select regioncode,min(binstrlen) from " + "hg02" + ".gdn2_asia_river1 " +
"where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" + lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")))";
            DbHelper.ExecuteSql(sql);
            sql = "insert into " + orauser + ".riversegs (select  channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
 "chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "hg02" + ".gdn2_asia_river1 " +
 "where substr(regioncode,1," + lenNew + ") in (select regioncode from (select regioncode,min(binstrlen) from " + "hg02" + ".gdn2_asia_river1 " +
"where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" + lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")) and length(regioncode)>" + lenNew + ")";
            DbHelper.ExecuteSql(sql);

            sql = "update " + orauser + ".riversegs set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);

            //03
            sql = "select distinct regionindex2 from " + orauser + ".riversegs where length(regionindex2)>" + len;
            DataSet ds = DbHelper.Query(sql);
            DataTable dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //long regionindex2=Convert.ToInt64(dt.Rows[i]["regionindex2"]);
                    string regionindex2 = dt.Rows[i]["regionindex2"].ToString();
                    string tmp = regionindex2.ToString();
                    string newTmp = tmp.Substring(0, tmp.Length - 3);

                    sql = "select min(bslength) from " + orauser + ".riversegs where regionindex2=" + regionindex2;
                    long minbslength = Convert.ToInt64(DbHelper.GetSingle(sql));
                    sql = "insert into " + orauser + ".regionconnection (regiongrade,regionindex2,toregionindex2,tovalue,tolength,regionname,regionindex,toregionindex)" +
 " values(1," + regionindex2 + "," + newTmp + ",0," + (minbslength - 1) + ",'liuyu',0,0)";
                    DbHelper.ExecuteSql(sql);
                }
            }

            sql = "update " + orauser + ".regionconnection set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set toregionindex=substr(toregionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);

            //20140320添加代码ByRen
            /************************************************************************************************/
            //04 设置参数

            //模拟开始时间
            DateTime dtStart = Convert.ToDateTime(starttime);
            //模拟结束时间
            DateTime dtEnd = Convert.ToDateTime(endtime);

            DateTime dtTmp = dtStart;
            //字典集合要求Key必须不同 Dictionary<string, string> dtYearMonth = new Dictionary<string, string>();
            List<List<string>> dtYearMonth = new List<List<string>>();
            List<string> dtYearMonthTmp;

            while ((dtTmp.Year < dtEnd.Year) || (dtTmp.Year == dtEnd.Year && dtTmp.Month <= dtEnd.Month))
            {
                dtYearMonthTmp = new List<string>();
                dtYearMonthTmp.Add(dtTmp.Year.ToString());
                dtYearMonthTmp.Add(dtTmp.Month.ToString());
                dtYearMonth.Add(dtYearMonthTmp);
                dtTmp = dtTmp.AddMonths(1);
            }
            foreach (List<string> TM in dtYearMonth)
            {
                sql = "insert into " + orauser + ".tvarparameter (select regionindex,bsvalue,bslength,'" + TM.ToArray()[0] + "','" + TM.ToArray()[1] + "',0.2,240 from " + orauser + ".riversegs)";
                DbHelper.ExecuteSql(sql);
            }
            //05 拷贝雨量数据
            //1)查询整个上游流域的范围
            sql = "select min(xmin) xmin,min(ymin) ymin,max(xmax) xmax,max(ymax) ymax from " + "hg02" + ".gdn2_asia_river1 a inner join " + orauser + ".riversegs b " +
                "on  a.regioncode=b.regionindex2 and a.binstrlen=b.bslength and a.binstrval=b.bsvalue";
            dt = DbHelper.Query(sql).Tables[0];
            if (dt != null && dt.Rows.Count == 1)
            {
                //分别取天花板和地板值
                double XMIN = Math.Floor(Convert.ToDouble(dt.Rows[0]["XMIN"]));
                double YMIN = Math.Floor(Convert.ToDouble(dt.Rows[0]["YMIN"]));
                double XMAX = Math.Ceiling(Convert.ToDouble(dt.Rows[0]["XMAX"]));
                double YMAX = Math.Ceiling(Convert.ToDouble(dt.Rows[0]["YMAX"]));
                //2)创建ID集合
                //ArrayList idss=new ArrayList();

                List<int> IDs = new List<int>();
                for (double x = XMIN; x <= XMAX; x = x + 0.25)
                {
                    for (double y = YMIN; y <= YMAX; y = y + 0.25)
                    {
                        int id = Convert.ToInt32(((90 - y) * 4 - 125 + 1) * 10000 + x * 4);
                        IDs.Add(id);
                    }
                }
                //3)先查找最新下载的预报数据
                sql = "select max(downid) from " + "hg02" + ".rain_downinfo";
                string downID = DbHelper.GetSingle(sql).ToString();
                if (!string.IsNullOrEmpty(downID))
                {
                    //每100个ID为一组来执行  改成20个
                    string tmp = "";
                    for (int i = 0; i < IDs.Count; i++)
                    {
                        if (i % 100 == 0 && i != 0)
                        {
                            tmp += IDs[i].ToString();
                            //4)拷贝最新的降雨预报数据到Raincomorph3h25km
                            sql = "insert into " + orauser + ".raincmorph3h25km (select ID,year,month,day,hours,houre,p from " + "hg02" +
                                    ".rain_remote_fc25km where downid='" + downID + "' and id in (" + tmp + "))";
                            DbHelper.ExecuteSql(sql);
                            tmp = "";
                        }
                        else
                        {
                            tmp += IDs[i].ToString() + ",";
                            if (i == IDs.Count - 1)
                            {
                                tmp = tmp.Substring(0, tmp.Length - 1);
                                //4)拷贝最新的降雨预报数据到Raincomorph3h25km
                                sql = "insert into " + orauser + ".raincmorph3h25km (select ID,year,month,day,hours,houre,p from " + "hg02" +
                                        ".rain_remote_fc25km where downid='" + downID + "' and id in (" + tmp + "))";
                                DbHelper.ExecuteSql(sql);
                            }
                        }
                    }
                }
            }

            //06 修改Scheme起止时间
            string TimeStart = "1950-01-01 00:00:00";
            //起始时间的跨度小时
            double starthouroffset = dtStart.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //终止时间的跨度小时
            double endhouroffset = dtEnd.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //STARTYEAR,STARTMONTH,STARTDAY,STARTHOUR,ENDYEAR,ENDMONTH,ENDDAY,ENDHOUR及STARTHOUROFFSET,ENDHOUROFFSET
            sql = "update " + orauser + ".hydrousepara set STARTYEAR=" + dtStart.Year + ",STARTMONTH=" + dtStart.Month +
                ",STARTDAY=" + dtStart.Day + ",STARTHOUR=" + dtStart.Hour + ",ENDYEAR=" + dtEnd.Year + ",ENDMONTH=" +
                dtEnd.Month + ",ENDDAY=" + dtEnd.Day + ",STARTHOUROFFSET=" + starthouroffset + ",ENDHOUROFFSET=" + endhouroffset;
            DbHelper.ExecuteSql(sql);

            //新添加代码 2014年5月15日
            //更改hydrousepara表中的CalcRegion，CalcOutValue及CalcOutLength字段
            sql = "select min(regionindex) from " + orauser + ".riversegs";
            string minregionindex = DbHelper.GetSingle(sql).ToString();

            sql = "select min(bslength) from " + orauser + ".riversegs where regionindex=" + minregionindex + " and bsvalue=0 ";
            string minbslengthNew = DbHelper.GetSingle(sql).ToString();

            sql = "update " + orauser + ".hydrousepara set calcregion=" + minregionindex + ",calcoutvalue=0,calcoutlength=" + minbslengthNew;
            DbHelper.ExecuteSql(sql);

            //新添加代码  2014年5月21日 更新regionindex
            sql = "update " + orauser + ".parameter set regionindex=" + minregionindex;
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".basinmodel set regionindex=" + minregionindex;
            DbHelper.ExecuteSql(sql);

            //新添加代码 2014年5月17日 更新错误数据
            sql = "update " + orauser + ".riversegs set slopesource=-1 where slopesource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopesource=20 where slopesource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopeleft=20 where slopeleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set sloperight=20 where sloperight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=-1 where areasource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=0.01 where areasource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arealeft=0.01 where arealeft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arearight=0.01 where arearight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthsource=-1 where lengthsource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthsource=0.01 where lengthsource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthleft=0.01 where lengthleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthright=0.01 where lengthright=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set seglength=1 where seglength=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slope=10 where slope=0";
            DbHelper.ExecuteSql(sql);


            //调用计算hpc
            CreateJob(sid, orauser, usercode);
            /************************************************************************************************/
            return str;
        }
        /// <summary>
        /// 根据regionid获取前三位返回大洲所在表
        /// </summary>
        /// <param name="regionid"></param>
        /// <returns></returns>
        private static string ConName(string regionid)
        {
            string continentName = "";
            switch (regionid.Substring(0, 1))
            {
                case "1":
                    continentName = "gdn2_asia_river1 ";
                    break;
                case "2":
                    continentName = "gdn2_europe_river1 ";
                    break;
                case "4":
                    continentName = "gdn2_northamerica_river1 ";
                    break;
            }
            return continentName;
        }


        /// <summary>
        /// 03连续多方案并行计算
        /// </summary>
        /// <param name="sid">方案编号</param>
        /// <param name="orauser">用户名</param>
        /// <param name="userid">hydrologal用户</param>
        /// <param name="rivers">上游河段</param>
        /// <param name="starttime">起始时间</param>
        /// <param name="endtime">终止时间</param>
        /// <param name="rainfalldata">降雨</param>
        /// <param name="runoffmodel">降雨模型</param>
        /// <param name="rivermodel">河流模型</param>
        /// <param name="usercode">用户密码</param>
        /// <returns></returns>
        public string ConSims(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode,int level)
        {
            string continentName = "";
            /*******************/
            //设置状态 正在入库 2014-04-27ByRen
            string sql = "update orauserTable set state=1 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);


            string str = "{\"msg\":\"ok\"}";
            //删除已有Sid
            sql = "delete from jzjw_schememgr where sid='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            //插入大洲编码
            RiverSeg topOne = rivers[0];
            string topTwo = topOne.REGIONIDNEW.ToString();
            if (topTwo.Length > 3) topTwo = topTwo.Substring(0, 3);

            //获取大洲
            continentName = ConName(topTwo);

            sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,datashift,concode) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                    starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "')";
            DbHelper.ExecuteSql(sql);

            ///00  truncate
            sql = "truncate table " + orauser + ".riversegs";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".discharge";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".regionconnection";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".definednodes";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".status";
            DbHelper.ExecuteSql(sql);
            //20140320添加代码ByRen
            /////////////////////////////////////////////////
            sql = "truncate table " + orauser + ".tvarparameter";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".raincmorph3H25KM";
            DbHelper.ExecuteSql(sql);
            /////////////////////////////////////////////////
            long minRegionidnew = long.MaxValue, minBinstrlen = long.MaxValue;
            //01
            if (rivers != null && rivers.Count > 0)
            {
                foreach (RiverSeg rs in rivers)
                {
                    if (rs.REGIONIDNEW < minRegionidnew) minRegionidnew = rs.REGIONIDNEW;
                    if (rs.BINSTRLEN < minBinstrlen) minBinstrlen = rs.BINSTRLEN;

                    string rgin = "" + rs.REGIONIDNEW;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    //sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                    //     rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "')";
                    //0831日插入null值不允许
                    sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description,channelid) values(" +
                             rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "',0)";
                    DbHelper.ExecuteSql(sql);
                }


            }

            int len = minRegionidnew.ToString().Length;

            //02
            //此句不用修改
            sql = "insert into " + orauser + ".riversegs (select channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName +
"where regioncode='" + minRegionidnew + "' and binstrlen>=" + minBinstrlen + " and 0=" + "get_out_bsv(binstrval,binstrlen," + minBinstrlen + "))";

            DbHelper.ExecuteSql(sql);
            /*之前
            sql="insert into "+orauser+".riversegs (select  channelid,binstrlen,binstrval,999,regionidnew,strahler,src_area,lft_area,rgt_area,0,0,0,src_slope,lft_slope,rgt_slope,src_len,lft_len,rgt_len,"+
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regionid from "+orauser+".gdn_asia_river1 where substr(regionidnew,1,"+len+")='"+minRegionidnew+"' and  length(regionidnew)>"+len+")"; 
            */
            //ren修改 上游河段拷贝1
            /*
            sql = "insert into " + orauser + ".riversegs (select channelid,binstrlen,binstrval,999,regionidnew,strahler,src_area,lft_area,rgt_area,0,0,0,src_slope,lft_slope,rgt_slope,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regionid from " + orauser + ".gdn_asia_river1 "+
"where regionidnew in (select regionidnew from (select regionidnew,min(binstrlen) from "+orauser+".gdn_asia_river1 "+
"where substr(regionidnew,1,"+len+")='"+minRegionidnew+"' and length(regionidnew)>"+len+" group by regionidnew "+
"having min(binstrlen)>"+minBinstrlen+")))";
            */
            //ren修改 上游河段拷贝2

            int lenNew = len + 3;
            sql = "insert into " + orauser + ".riversegs (select  channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName +
"where regioncode in (select regioncode from (select regioncode,min(binstrlen) from " + "GDN2." + continentName +
"where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" + lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")))";
            DbHelper.ExecuteSql(sql);
            sql = "insert into " + orauser + ".riversegs (select  channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
 "chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName +
 "where substr(regioncode,1," + lenNew + ") in (select regioncode from (select regioncode,min(binstrlen) from " + "GDN2." + continentName +
"where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" + lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")) and length(regioncode)>" + lenNew + ")";
            DbHelper.ExecuteSql(sql);

            sql = "update " + orauser + ".riversegs set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);

            //清空表
            sql = "truncate table " + orauser + ".definednodes";
            DbHelper.ExecuteSql(sql);

            sql = "insert into " + orauser + ".definednodes select regionindex,bsvalue,bslength,'GS',regionindex2,channelindex from " + orauser + ".riversegs where stralherorder>=" + level;
            DbHelper.ExecuteSql(sql);
            //03
            sql = "select distinct regionindex2 from " + orauser + ".riversegs where length(regionindex2)>" + len;
            DataSet ds = DbHelper.Query(sql);
            DataTable dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //long regionindex2=Convert.ToInt64(dt.Rows[i]["regionindex2"]);
                    string regionindex2 = dt.Rows[i]["regionindex2"].ToString();
                    string tmp = regionindex2.ToString();
                    string newTmp = tmp.Substring(0, tmp.Length - 3);

                    sql = "select min(bslength) from " + orauser + ".riversegs where regionindex2=" + regionindex2;
                    long minbslength = Convert.ToInt64(DbHelper.GetSingle(sql));
                    sql = "insert into " + orauser + ".regionconnection (regiongrade,regionindex2,toregionindex2,tovalue,tolength,regionname,regionindex,toregionindex)" +
 " values(1," + regionindex2 + "," + newTmp + ",0," + (minbslength - 1) + ",'liuyu',0,0)";
                    DbHelper.ExecuteSql(sql);
                }
            }

            sql = "update " + orauser + ".regionconnection set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set toregionindex=substr(toregionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);

            //20140320添加代码ByRen
            /************************************************************************************************/
            //04 设置参数

            //模拟开始时间
            DateTime dtStart = Convert.ToDateTime(starttime);
            //模拟结束时间
            DateTime dtEnd = Convert.ToDateTime(endtime);

            DateTime dtTmp = dtStart;
            //字典集合要求Key必须不同 Dictionary<string, string> dtYearMonth = new Dictionary<string, string>();
            List<List<string>> dtYearMonth = new List<List<string>>();
            List<string> dtYearMonthTmp;

            while ((dtTmp.Year < dtEnd.Year) || (dtTmp.Year == dtEnd.Year && dtTmp.Month <= dtEnd.Month))
            {
                dtYearMonthTmp = new List<string>();
                dtYearMonthTmp.Add(dtTmp.Year.ToString());
                dtYearMonthTmp.Add(dtTmp.Month.ToString());
                dtYearMonth.Add(dtYearMonthTmp);
                dtTmp = dtTmp.AddMonths(1);
            }
            foreach (List<string> TM in dtYearMonth)
            {
                sql = "insert into " + orauser + ".tvarparameter (select regionindex,bsvalue,bslength,'" + TM.ToArray()[0] + "','" + TM.ToArray()[1] + "',0.2,240 from " + orauser + ".riversegs)";
                DbHelper.ExecuteSql(sql);
            }
            //05 拷贝雨量数据
            //1)查询整个上游流域的范围
            sql = "select min(xmin) xmin,min(ymin) ymin,max(xmax) xmax,max(ymax) ymax from " + "GDN2." + continentName + "a inner join " + orauser + ".riversegs b " +
                "on  a.regioncode=b.regionindex2 and a.binstrlen=b.bslength and a.binstrval=b.bsvalue";
            dt = DbHelper.Query(sql).Tables[0];
            if (dt != null && dt.Rows.Count == 1)
            {
                //分别取天花板和地板值
                double XMIN = Math.Floor(Convert.ToDouble(dt.Rows[0]["XMIN"]));
                double YMIN = Math.Floor(Convert.ToDouble(dt.Rows[0]["YMIN"]));
                double XMAX = Math.Ceiling(Convert.ToDouble(dt.Rows[0]["XMAX"]));
                double YMAX = Math.Ceiling(Convert.ToDouble(dt.Rows[0]["YMAX"]));
                //2)创建ID集合
                //ArrayList idss=new ArrayList();

                List<int> IDs = new List<int>();
                for (double x = XMIN; x <= XMAX; x = x + 0.25)
                {
                    for (double y = YMIN; y <= YMAX; y = y + 0.25)
                    {
                        int id = Convert.ToInt32(((90 - y) * 4 - 125 + 1) * 10000 + x * 4);
                        IDs.Add(id);
                    }
                }
                //3)先查找最新下载的预报数据
                sql = "select max(downid) from " + "raindata" + ".rain_downinfo";
                string downID = DbHelper.GetSingle(sql).ToString();
                if (!string.IsNullOrEmpty(downID))
                {
                    //每100个ID为一组来执行  改成20个
                    string tmp = "";
                    for (int i = 0; i < IDs.Count; i++)
                    {
                        if (i % 100 == 0 && i != 0)
                        {
                            tmp += IDs[i].ToString();
                            //4)拷贝最新的降雨预报数据到Raincomorph3h25km
                            sql = "insert into " + orauser + ".raincmorph3h25km (select ID,year,month,day,hours,houre,p from " + "raindata" +
                                    ".rain_remote_fc25km where downid='" + downID + "' and id in (" + tmp + "))";
                            DbHelper.ExecuteSql(sql);
                            tmp = "";
                        }
                        else
                        {
                            tmp += IDs[i].ToString() + ",";
                            if (i == IDs.Count - 1)
                            {
                                tmp = tmp.Substring(0, tmp.Length - 1);
                                //4)拷贝最新的降雨预报数据到Raincomorph3h25km
                                sql = "insert into " + orauser + ".raincmorph3h25km (select ID,year,month,day,hours,houre,p from " + "raindata" +
                                        ".rain_remote_fc25km where downid='" + downID + "' and id in (" + tmp + "))";
                                DbHelper.ExecuteSql(sql);
                            }
                        }
                    }
                }
            }

            //06 修改Scheme起止时间
            string TimeStart = "1950-01-01 00:00:00";
            //起始时间的跨度小时
            double starthouroffset = dtStart.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //终止时间的跨度小时
            double endhouroffset = dtEnd.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //STARTYEAR,STARTMONTH,STARTDAY,STARTHOUR,ENDYEAR,ENDMONTH,ENDDAY,ENDHOUR及STARTHOUROFFSET,ENDHOUROFFSET
            sql = "update " + orauser + ".hydrousepara set STARTYEAR=" + dtStart.Year + ",STARTMONTH=" + dtStart.Month +
                ",STARTDAY=" + dtStart.Day + ",STARTHOUR=" + dtStart.Hour + ",ENDYEAR=" + dtEnd.Year + ",ENDMONTH=" +
                dtEnd.Month + ",ENDDAY=" + dtEnd.Day + ",STARTHOUROFFSET=" + starthouroffset + ",ENDHOUROFFSET=" + endhouroffset;
            DbHelper.ExecuteSql(sql);

            //新添加代码 2014年5月15日
            //更改hydrousepara表中的CalcRegion，CalcOutValue及CalcOutLength字段
            sql = "select min(regionindex) from " + orauser + ".riversegs";
            string minregionindex = DbHelper.GetSingle(sql).ToString();

            sql = "select min(bslength) from " + orauser + ".riversegs where regionindex=" + minregionindex + " and bsvalue=0 ";
            string minbslengthNew = DbHelper.GetSingle(sql).ToString();

            sql = "update " + orauser + ".hydrousepara set calcregion=" + minregionindex + ",calcoutvalue=0,calcoutlength=" + minbslengthNew;
            DbHelper.ExecuteSql(sql);

            //新添加代码  2014年5月21日 更新regionindex
            sql = "update " + orauser + ".parameter set regionindex=" + minregionindex;
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".basinmodel set regionindex=" + minregionindex;
            DbHelper.ExecuteSql(sql);

            //新添加代码 2014年5月17日 更新错误数据
            sql = "update " + orauser + ".riversegs set slopesource=-1 where slopesource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopesource=20 where slopesource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopeleft=20 where slopeleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set sloperight=20 where sloperight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=-1 where areasource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=0.01 where areasource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arealeft=0.01 where arealeft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arearight=0.01 where arearight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthsource=-1 where lengthsource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthsource=0.01 where lengthsource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthleft=0.01 where lengthleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthright=0.01 where lengthright=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set seglength=1 where seglength=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slope=10 where slope=0";
            DbHelper.ExecuteSql(sql);
            /************************************************************************************************/

            //后面就是计算代码调用hpc部分

            //设置状态 正在计算 调用hpcJob代码 2014-04-27ByRen
            sql = "update orauserTable set state=2 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);

            //调用计算hpc
            CreateJob(sid, orauser, usercode);

            //在hpc计算完成后，入库和设置状态
            /************************************************************************************************/
            return str;
        }



        /// <summary>
        /// 多方案并行计算
        /// </summary>
        /// <param name="sid">方案编号</param>
        /// <param name="orauser">用户名</param>
        /// <param name="userid">hydrologal用户</param>
        /// <param name="rivers">上游河段</param>
        /// <param name="starttime">起始时间</param>
        /// <param name="endtime">终止时间</param>
        /// <param name="rainfalldata">降雨</param>
        /// <param name="runoffmodel">降雨模型</param>
        /// <param name="rivermodel">河流模型</param>
        /// <param name="usercode">用户密码</param>
        /// <returns></returns>
        public string DoRiverSegCalcNew(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode)
        {
            string continentName = "";
            /*******************/
            //设置状态 正在入库 2014-04-27ByRen
            string sql = "update orauserTable set state=1 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);


            string str = "{\"msg\":\"ok\"}";
            //删除已有Sid
            sql = "delete from jzjw_schememgr where sid='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            //插入大洲编码
            RiverSeg topOne = rivers[0];
            string topTwo = topOne.REGIONIDNEW.ToString();
            if (topTwo.Length > 3) topTwo = topTwo.Substring(0, 3);

            //获取大洲
            continentName = ConName(topTwo);

            sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,datashift,concode) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                    starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "')";
            DbHelper.ExecuteSql(sql);

            ///00  truncate
            sql = "truncate table " + orauser + ".riversegs";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".discharge";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".regionconnection";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".definednodes";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".status";
            DbHelper.ExecuteSql(sql);
            //20140320添加代码ByRen
            /////////////////////////////////////////////////
            sql = "truncate table " + orauser + ".tvarparameter";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".raincmorph3H25KM";
            DbHelper.ExecuteSql(sql);
            /////////////////////////////////////////////////
            long minRegionidnew = long.MaxValue, minBinstrlen = long.MaxValue;
            //01
            if (rivers != null && rivers.Count > 0)
            {
                foreach (RiverSeg rs in rivers)
                {
                    if (rs.REGIONIDNEW < minRegionidnew) minRegionidnew = rs.REGIONIDNEW;
                    if (rs.BINSTRLEN < minBinstrlen) minBinstrlen = rs.BINSTRLEN;

                    string rgin = "" + rs.REGIONIDNEW;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    //sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                   //     rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "')";
                    //0831日插入null值不允许
                    sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description,channelid) values(" +
                             rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "',0)";
                   
                    DbHelper.ExecuteSql(sql);
                }


            }

            int len = minRegionidnew.ToString().Length;

            //02
            //此句不用修改
            sql = "insert into " + orauser + ".riversegs (select channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName +
"where regioncode='" + minRegionidnew + "' and binstrlen>=" + minBinstrlen + " and 0=" + "get_out_bsv(binstrval,binstrlen," + minBinstrlen + "))";

            DbHelper.ExecuteSql(sql);
            /*之前
            sql="insert into "+orauser+".riversegs (select  channelid,binstrlen,binstrval,999,regionidnew,strahler,src_area,lft_area,rgt_area,0,0,0,src_slope,lft_slope,rgt_slope,src_len,lft_len,rgt_len,"+
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regionid from "+orauser+".gdn_asia_river1 where substr(regionidnew,1,"+len+")='"+minRegionidnew+"' and  length(regionidnew)>"+len+")"; 
            */
            //ren修改 上游河段拷贝1
            /*
            sql = "insert into " + orauser + ".riversegs (select channelid,binstrlen,binstrval,999,regionidnew,strahler,src_area,lft_area,rgt_area,0,0,0,src_slope,lft_slope,rgt_slope,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regionid from " + orauser + ".gdn_asia_river1 "+
"where regionidnew in (select regionidnew from (select regionidnew,min(binstrlen) from "+orauser+".gdn_asia_river1 "+
"where substr(regionidnew,1,"+len+")='"+minRegionidnew+"' and length(regionidnew)>"+len+" group by regionidnew "+
"having min(binstrlen)>"+minBinstrlen+")))";
            */
            //ren修改 上游河段拷贝2

            int lenNew = len + 3;
            sql = "insert into " + orauser + ".riversegs (select  channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName +
"where regioncode in (select regioncode from (select regioncode,min(binstrlen) from " + "GDN2." + continentName +
"where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" + lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")))";
            DbHelper.ExecuteSql(sql);
            sql = "insert into " + orauser + ".riversegs (select  channelid,binstrlen,binstrval,999,regioncode,strahler,src_area,lft_area,rgt_area,src_slope,lft_slope,rgt_slope,0,0,0,src_len,lft_len,rgt_len," +
 "chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName +
 "where substr(regioncode,1," + lenNew + ") in (select regioncode from (select regioncode,min(binstrlen) from " + "GDN2." + continentName +
"where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" + lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")) and length(regioncode)>" + lenNew + ")";
            DbHelper.ExecuteSql(sql);

            sql = "update " + orauser + ".riversegs set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);

            //03
            sql = "select distinct regionindex2 from " + orauser + ".riversegs where length(regionindex2)>" + len;
            DataSet ds = DbHelper.Query(sql);
            DataTable dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    //long regionindex2=Convert.ToInt64(dt.Rows[i]["regionindex2"]);
                    string regionindex2 = dt.Rows[i]["regionindex2"].ToString();
                    string tmp = regionindex2.ToString();
                    string newTmp = tmp.Substring(0, tmp.Length - 3);

                    sql = "select min(bslength) from " + orauser + ".riversegs where regionindex2=" + regionindex2;
                    long minbslength = Convert.ToInt64(DbHelper.GetSingle(sql));
                    sql = "insert into " + orauser + ".regionconnection (regiongrade,regionindex2,toregionindex2,tovalue,tolength,regionname,regionindex,toregionindex)" +
 " values(1," + regionindex2 + "," + newTmp + ",0," + (minbslength - 1) + ",'liuyu',0,0)";
                    DbHelper.ExecuteSql(sql);
                }
            }

            sql = "update " + orauser + ".regionconnection set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set toregionindex=substr(toregionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);

            //20140320添加代码ByRen
            /************************************************************************************************/
            //04 设置参数

            //模拟开始时间
            DateTime dtStart = Convert.ToDateTime(starttime);
            //模拟结束时间
            DateTime dtEnd = Convert.ToDateTime(endtime);

            DateTime dtTmp = dtStart;
            //字典集合要求Key必须不同 Dictionary<string, string> dtYearMonth = new Dictionary<string, string>();
            List<List<string>> dtYearMonth = new List<List<string>>();
            List<string> dtYearMonthTmp;

            while ((dtTmp.Year < dtEnd.Year) || (dtTmp.Year == dtEnd.Year && dtTmp.Month <= dtEnd.Month))
            {
                dtYearMonthTmp = new List<string>();
                dtYearMonthTmp.Add(dtTmp.Year.ToString());
                dtYearMonthTmp.Add(dtTmp.Month.ToString());
                dtYearMonth.Add(dtYearMonthTmp);
                dtTmp = dtTmp.AddMonths(1);
            }
            foreach (List<string> TM in dtYearMonth)
            {
                sql = "insert into " + orauser + ".tvarparameter (select regionindex,bsvalue,bslength,'" + TM.ToArray()[0] + "','" + TM.ToArray()[1] + "',0.2,240 from " + orauser + ".riversegs)";
                DbHelper.ExecuteSql(sql);
            }
            //05 拷贝雨量数据
            //1)查询整个上游流域的范围
            sql = "select min(xmin) xmin,min(ymin) ymin,max(xmax) xmax,max(ymax) ymax from " + "GDN2." + continentName + "a inner join " + orauser + ".riversegs b " +
                "on  a.regioncode=b.regionindex2 and a.binstrlen=b.bslength and a.binstrval=b.bsvalue";
            dt = DbHelper.Query(sql).Tables[0];
            if (dt != null && dt.Rows.Count == 1)
            {
                //分别取天花板和地板值
                double XMIN = Math.Floor(Convert.ToDouble(dt.Rows[0]["XMIN"]));
                double YMIN = Math.Floor(Convert.ToDouble(dt.Rows[0]["YMIN"]));
                double XMAX = Math.Ceiling(Convert.ToDouble(dt.Rows[0]["XMAX"]));
                double YMAX = Math.Ceiling(Convert.ToDouble(dt.Rows[0]["YMAX"]));
                //2)创建ID集合
                //ArrayList idss=new ArrayList();

                List<int> IDs = new List<int>();
                for (double x = XMIN; x <= XMAX; x = x + 0.25)
                {
                    for (double y = YMIN; y <= YMAX; y = y + 0.25)
                    {
                        int id = Convert.ToInt32(((90 - y) * 4 - 125 + 1) * 10000 + x * 4);
                        IDs.Add(id);
                    }
                }
                //3)先查找最新下载的预报数据
                sql = "select max(downid) from " + "raindata" + ".rain_downinfo";
                string downID = DbHelper.GetSingle(sql).ToString();
                if (!string.IsNullOrEmpty(downID))
                {
                    //每100个ID为一组来执行  改成20个
                    string tmp = "";
                    for (int i = 0; i < IDs.Count; i++)
                    {
                        if (i % 100 == 0 && i != 0)
                        {
                            tmp += IDs[i].ToString();
                            //4)拷贝最新的降雨预报数据到Raincomorph3h25km
                            sql = "insert into " + orauser + ".raincmorph3h25km (select ID,year,month,day,hours,houre,p from " + "raindata" +
                                    ".rain_remote_fc25km where downid='" + downID + "' and id in (" + tmp + "))";
                            DbHelper.ExecuteSql(sql);
                            tmp = "";
                        }
                        else
                        {
                            tmp += IDs[i].ToString() + ",";
                            if (i == IDs.Count - 1)
                            {
                                tmp = tmp.Substring(0, tmp.Length - 1);
                                //4)拷贝最新的降雨预报数据到Raincomorph3h25km
                                sql = "insert into " + orauser + ".raincmorph3h25km (select ID,year,month,day,hours,houre,p from " + "raindata" +
                                        ".rain_remote_fc25km where downid='" + downID + "' and id in (" + tmp + "))";
                                DbHelper.ExecuteSql(sql);
                            }
                        }
                    }
                }
            }

            //06 修改Scheme起止时间
            string TimeStart = "1950-01-01 00:00:00";
            //起始时间的跨度小时
            double starthouroffset = dtStart.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //终止时间的跨度小时
            double endhouroffset = dtEnd.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //STARTYEAR,STARTMONTH,STARTDAY,STARTHOUR,ENDYEAR,ENDMONTH,ENDDAY,ENDHOUR及STARTHOUROFFSET,ENDHOUROFFSET
            sql = "update " + orauser + ".hydrousepara set STARTYEAR=" + dtStart.Year + ",STARTMONTH=" + dtStart.Month +
                ",STARTDAY=" + dtStart.Day + ",STARTHOUR=" + dtStart.Hour + ",ENDYEAR=" + dtEnd.Year + ",ENDMONTH=" +
                dtEnd.Month + ",ENDDAY=" + dtEnd.Day + ",STARTHOUROFFSET=" + starthouroffset + ",ENDHOUROFFSET=" + endhouroffset;
            DbHelper.ExecuteSql(sql);

            //新添加代码 2014年5月15日
            //更改hydrousepara表中的CalcRegion，CalcOutValue及CalcOutLength字段
            sql = "select min(regionindex) from " + orauser + ".riversegs";
            string minregionindex = DbHelper.GetSingle(sql).ToString();

            sql = "select min(bslength) from " + orauser + ".riversegs where regionindex=" + minregionindex + " and bsvalue=0 ";
            string minbslengthNew = DbHelper.GetSingle(sql).ToString();

            sql = "update " + orauser + ".hydrousepara set calcregion=" + minregionindex + ",calcoutvalue=0,calcoutlength=" + minbslengthNew;
            DbHelper.ExecuteSql(sql);

            //新添加代码  2014年5月21日 更新regionindex
            sql = "update " + orauser + ".parameter set regionindex=" + minregionindex;
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".basinmodel set regionindex=" + minregionindex;
            DbHelper.ExecuteSql(sql);

            //新添加代码 2014年5月17日 更新错误数据
            sql = "update " + orauser + ".riversegs set slopesource=-1 where slopesource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopesource=20 where slopesource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopeleft=20 where slopeleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set sloperight=20 where sloperight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=-1 where areasource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=0.01 where areasource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arealeft=0.01 where arealeft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arearight=0.01 where arearight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthsource=-1 where lengthsource=0 and stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthsource=0.01 where lengthsource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthleft=0.01 where lengthleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthright=0.01 where lengthright=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set seglength=1 where seglength=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slope=10 where slope=0";
            DbHelper.ExecuteSql(sql);
            /************************************************************************************************/

            //后面就是计算代码调用hpc部分

            //设置状态 正在计算 调用hpcJob代码 2014-04-27ByRen
            sql = "update orauserTable set state=2 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);

            //调用计算hpc
            CreateJob(sid, orauser, usercode);

            //在hpc计算完成后，入库和设置状态
            /************************************************************************************************/
            return str;
        }



        /// <summary>
        /// 创建计算Job
        /// </summary>
        /// <returns></returns>
        public string CreateJob(string SCCD, string username, string userpwd)
        {
            string str = "OK";
            //创建Job            
            IScheduler scheduler = new Scheduler();
            scheduler.Connect("192.168.0.4");
            //方法一
            //scheduler.SetCertificateCredentials("SediHPC\\tangshuang", "tangshuangpwd");
            //scheduler.SetCachedCredentials("tangshuang", "tangshuangpwd");
            //scheduler.SetCachedCredentials("SediHPC\\tangshuang", "tangshuangpwd");
            //scheduler.Connect("USERMIC-1VDLLCD");
            //scheduler.Connect("101.6.54.4");
            //方法二
            //scheduler.ConnectServiceAsClient("101.6.54.4", Serverdel, "SediHPC\\tangshuang", "tangshuangpwd");
            //scheduler.ConnectServiceAsClient("101.6.54.4", Serverdel, "sedihpc\\mpi", "ilovempi");

            ISchedulerJob job = scheduler.CreateJob();
            ISchedulerTask task = job.CreateTask();

            // 指定计算方案运行的节点
            //job.RequestedNodes.Add("USERMIC-1VDLLCD");
            //job.RequestedNodes.Add("WINDOWS-N19");
            //job.RequestedNodes.Add("WINDOWS-N17");
            job.RequestedNodes.Add("HPC-CENTER");

            // 设置计算任务的属性
            job.Name = SCCD;  //
            task.Name = SCCD;
            task.Type = TaskType.Basic;
            task.MinimumNumberOfCores = 4;//参数 24
            task.MaximumNumberOfCores = 4;//参数
            task.WorkDirectory = @"\\HPC-CENTER\GlobalF3S\HydroGlobal";
            // 管道编号通过命令行的参数传递给mpi程序  ***参数  数据库别名、用户名、密码，EXE名称            
            task.CommandLine = "mpiexec.exe newrouting.exe " + username + " " + userpwd + " orcl5 ";
            task.StdOutFilePath = task.WorkDirectory + @"\out.txt";
            task.StdErrFilePath = task.WorkDirectory + @"\err.txt";
            job.AddTask(task);

            // 任务状态转变回调
            job.OnJobState += new EventHandler<JobStateEventArg>(job_OnJobState);
            // 向HPC提交任务
            scheduler.SubmitJob(job, null, null);
            return str;
        }

        /// <summary>
        /// 委托，任务状态转变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void job_OnJobState(object sender, JobStateEventArg e)
        {
            // 任务完成
            if (e.NewState.Equals(JobState.Finished))
            {
                //SimulationComplete(orauser, sid);
                SimulationCompleteNon(orauser, sid);
            }
            // 任务错误
            if (e.NewState.Equals(JobState.Failed))
            {
                //Service1.SetException(oSSimulation.ExeFileName, "任务失败,可能计算节点不正常");
                SimulationFault();
            }
        }

        /// <summary>
        /// 方案计算完成,拷贝数据和更新状态（未使用数据库事务）
        /// </summary>
        /// <param name="orauser"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public static Boolean SimulationCompleteNon(string orauser, string sid)
        {
            Boolean rst = false;
            //03查找definednodes description 的第一条记录 ，然后获取器前三位数字
            string sql = "select description from " + orauser + ".definednodes where rownum<=1";
            string Code = DbHelper.GetSingle(sql).ToString();
            if (Code.Length > 0)
                Code = Code.Substring(0, 3);
            //04更新大洲编码

            //删除 SQL事务 要修改成SQL事务删除和插入同时进行
            sql = "delete from discharge where SCCD='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            //将discharge表中数据拷贝到hydroglobal.discharge表中
            //REGIONIDNEW,ADDZERO两个字段为空值
            //01拷贝数据
            sql = "insert into discharge(SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                  "B,H,V) (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                  "B,H,V from " + orauser + ".discharge)";
            DbHelper.ExecuteSql(sql);
            //02更新字段
            sql = "update discharge set addzero='0' where mod(length(REGIONINDEX),3)=2 and SCCD='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            sql = "update discharge set addzero='00' where mod(length(REGIONINDEX),3)=1 and SCCD='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            //这句有问题
            //sql = "update discharge set regionidnew=addzero||regionid where reghighid=0 and SCCD='" + sid + "'";
            sql = "update discharge set regionidnew=addzero||REGIONINDEX where SCCD='" + sid + "'";
            DbHelper.ExecuteSql(sql);          

            sql = "update discharge set regionidnew='" + Code + "'||addzero||regionindex where SCCD='" + sid + "'";
            DbHelper.ExecuteSql(sql);

            //设置状态 空闲状态2014-04-27ByRen
            sql = "update orauserTable set state=3 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            //更新方案表 完成状态
            sql = "update jzjw_schememgr set state=3 where sid='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            //更新连续计算表
            //sql = "insert into hgmgr.schemecontsim(sid) values ('" + sid +"')";
            //sqlList.Add(sql);
            sql = "update hgmgr.schemecontsim set simuser='" + orauser + "'" + " where sid='" + sid + "'";
            DbHelper.ExecuteSql(sql);

            rst = true;
            return rst;
        }

        /// <summary>
        /// 方案计算完成,拷贝数据和更新状态（事务机制）
        /// </summary>
        /// <param name="orauser"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public static Boolean SimulationComplete(string orauser, string sid)
        {
            //这句话要先执行 得到Code
            //03查找definednodes description 的第一条记录 ，然后获取器前三位数字
            string sql = "select description from " + orauser + ".definednodes where rownum<=1";
            string Code = DbHelper.GetSingle(sql).ToString();
            if (Code.Length > 0)
                Code = Code.Substring(0, 3);

            Boolean rst = false;
            ArrayList sqlList = new ArrayList();
            //删除 SQL事务 要修改成SQL事务删除和插入同时进行
            sql = "delete from discharge where SCCD='" + sid + "'";
            sqlList.Add(sql);
            //DbHelper.ExecuteSql(sql);
            //将discharge表中数据拷贝到hydroglobal.discharge表中
            //REGIONIDNEW,ADDZERO两个字段为空值
            //01拷贝数据
            sql = "insert into discharge(SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                  "B,H,V) (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                  "B,H,V from " + orauser + ".discharge)";
            //DbHelper.ExecuteSql(sql);
            sqlList.Add(sql);
            //02更新字段
            sql = "update discharge set addzero='0' where mod(length(REGIONINDEX),3)=2 and SCCD='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            sqlList.Add(sql);
            sql = "update discharge set addzero='00' where mod(length(REGIONINDEX),3)=1 and SCCD='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            sqlList.Add(sql);
            //这句有问题
            //sql = "update discharge set regionidnew=addzero||regionid where reghighid=0 and SCCD='" + sid + "'";
            sql = "update discharge set regionidnew=addzero||REGIONINDEX where SCCD='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            sqlList.Add(sql);

            //04更新大洲编码

            sql = "update discharge set regionidnew='" + Code + "'||addzero||regionindex where SCCD='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            sqlList.Add(sql);
            //设置状态 空闲状态2014-04-27ByRen
            sql = "update orauserTable set state=3 where orauser='" + orauser + "'";
            //DbHelper.ExecuteSql(sql);
            sqlList.Add(sql);
            //更新方案表 完成状态
            sql = "update jzjw_schememgr set state=3 where sid='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            sqlList.Add(sql);

            //更新连续计算表
            //sql = "insert into hgmgr.schemecontsim(sid) values ('" + sid +"')";
            //sqlList.Add(sql);
            sql = "update hgmgr.schemecontsim set simuser='" + orauser+"'" +" where sid='"+sid+"'";
            sqlList.Add(sql);
            //执行数据库事务
            DbHelper.ExecuteSqlTran(sqlList);

            //设置状态 空闲状态 2014-09-27ByRen
            sql = "update orauserTable set state=3 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            rst = true;
            return rst;
        }

        /// <summary>
        /// 方案计算失败，拷贝数据和更新状态
        /// </summary>
        /// <returns></returns>
        public string SimulationFault()
        {
            string rst;

            //设置状态 空闲状态 2014-04-27ByRen
            sql = "update orauserTable set state=3 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            //更新方案表 错误状态
            sql = "update jzjw_schememgr set state=4 where sid='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            rst = "true";

            return rst;
        }
    }
}
