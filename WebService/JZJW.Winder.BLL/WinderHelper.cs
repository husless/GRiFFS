using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using JZJW.Winder.Json;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;
using Oracle.DataAccess.Client;

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
        public WinderHelper(string orauser, string sid, string starttime, string endtime, double ppreheat, Boolean iscontiune)
        {
            this.orauser = orauser;
            this.sid = sid;
            this.starttime = starttime;
            this.endtime = endtime;
            this.ppreheat = ppreheat;
            this.iscontiune = iscontiune;
        }
        public string orauser { get; set; }
        public string sid { get; set; }
        public string starttime { get; set; }
        public string endtime { get; set; }
        public double ppreheat { get; set; }
        public Boolean iscontiune { get; set; }


        string sql = "";
        DateTime submitDate;
        DateTime simetime;
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
                    //1222
                    if (rs.REGIONIDNEW < minRegionidnew) minRegionidnew = rs.REGIONIDNEW;
                    //if (Convert.ToInt64(rs.REGIONIDNEW) < minRegionidnew) minRegionidnew = Convert.ToInt64(rs.REGIONIDNEW);
                    if (rs.BINSTRLEN < minBinstrlen) minBinstrlen = rs.BINSTRLEN;

                    string rgin = "" + rs.REGIONIDNEW;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                        rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'" + rs.NODETYPE + "','" + rs.REGIONIDNEW + "')";
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
            /*
            sql = "select min(xmin) xmin,min(ymin) ymin,max(xmax) xmax,max(ymax) ymax from " + "hg02" + ".gdn2_asia_river1 a inner join " + orauser + ".riversegs b " +
                "on  a.regioncode=b.regionindex2 and a.binstrlen=b.bslength and a.binstrval=b.bsvalue";
             */
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

            //sql = "update " + orauser + ".hydrousepara set calcregion=" + minregionindex + ",calcoutvalue=0,calcoutlength=" + minbslengthNew;
            //任修改20150108 hydrousepara.calcrsvup=0
            sql = "update " + orauser + ".hydrousepara set calcregion=" + minregionindex + ",calcoutvalue=0,calcrsvup=0,calcoutlength=" + minbslengthNew;
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
            CreateJob(sid, orauser, usercode, "4");
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
                case "5":
                    continentName = "gdn2_sxh_river1 ";
                    break;
                case "1":
                    continentName = "gdn2_asia_river1 ";
                    break;
                case "2":
                    continentName = "gdn2_europe_river1 ";
                    break;
                case "4":
                    if (regionid.Substring(0, 3) == "419")
                        continentName = "gdn2_missi_river1 ";
                    else
                        continentName = "gdn2_northamerica_river1 ";
                    break;
            }
            return continentName;
        }

        /// <summary>
        /// 批量插入语句ByRen20150111
        /// </summary>
        /// <returns></returns>
        public void ConSimsBatchInesert(string orauser, int level)
        {
            //查询数据
            string sql = "select regionindex,bsvalue,bslength,'GS' nodetype,regionindex2 DESCRIPTION,channelindex channelid from " + orauser + ".riversegs where stralherorder>=" + level;
            DataTable dt = DbHelper.Query(sql).Tables[0];
            //查询definednodes表数据
            sql = "select regionindex,bsvalue,bslength,nodetype,DESCRIPTION,channelid from " + orauser + ".definednodes";
            DataTable dtdefinednodes = DbHelper.Query(sql).Tables[0];
            DataTable dttmp = dt.Copy();
            foreach (DataRow dttmp_row in dttmp.Rows)
            {
                foreach (DataRow dtdefinednodes_row in dtdefinednodes.Rows)
                {
                    if (dttmp_row["regionindex"].ToString() == dtdefinednodes_row["regionindex"].ToString() && dttmp_row["bsvalue"].ToString() == dtdefinednodes_row["bsvalue"].ToString()
                        && dttmp_row["bslength"].ToString() == dtdefinednodes_row["bslength"].ToString() && dttmp_row["nodetype"].ToString() == dtdefinednodes_row["nodetype"].ToString())
                    {
                        dttmp_row.Delete();
                        break;
                    }

                }
            }
            dttmp.AcceptChanges();
            //dttmp 结果
            /*
            sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,DESCRIPTION,channelid) values (:regionindex,:bsvalue,:bslength,:nodetype,:DESCRIPTION,:channelid)";
            OracleParameter[] parameters = {
					new OracleParameter(":REGIONINDEX",OracleDbType.Double),	
					new OracleParameter(":BSVALUE",OracleDbType.Double),
					new OracleParameter(":BSLENGTH",OracleDbType.Double),
					new OracleParameter(":NODETYPE",OracleDbType.Char,2),
					new OracleParameter(":DESCRIPTION",OracleDbType.Varchar2,50),					
					new OracleParameter(":CHANNELID",OracleDbType.Double)};
             * */
            //批量插入数据库
            //DbHelper.BatchInsert(dttmp, sql, parameters);
            OracleParameter[] parameters = {
					new OracleParameter(":REGIONINDEX",OracleDbType.Double),	
					new OracleParameter(":BSVALUE",OracleDbType.Double),
					new OracleParameter(":BSLENGTH",OracleDbType.Double),
					new OracleParameter(":NODETYPE",OracleDbType.Char,2),
					new OracleParameter(":DESCRIPTION",OracleDbType.Varchar2,50),					
					new OracleParameter(":CHANNELID",OracleDbType.Double)};
            submitInsertdefinednodes(dttmp, 10000, parameters);

            //批量插入数据库，先以每10万条插入一次

        }
        /// <summary>
        /// 将DataTable数据插入到数据库 ByRen20140113
        /// </summary>
        /// <param name="dttmp">DataTable</param>
        /// <param name="rowcounts">每多条为一批次插入</param>
        private void submitInsertdefinednodes(DataTable dttmp, int rowcounts, OracleParameter[] parameters)
        {
            Dictionary<string, object> datas = new Dictionary<string, object>();
            //定义存放数据的数组 注意效率问题
            ArrayList regionindex = new ArrayList(dttmp.Rows.Count);
            ArrayList bsvalue = new ArrayList(dttmp.Rows.Count);
            ArrayList bslength = new ArrayList(dttmp.Rows.Count);
            ArrayList nodetype = new ArrayList(dttmp.Rows.Count);
            ArrayList DESCRIPTION = new ArrayList(dttmp.Rows.Count);
            ArrayList channelid = new ArrayList(dttmp.Rows.Count);
            int segmentSize = 0;
            foreach (DataRow dc in dttmp.Rows)
            {
                regionindex.Add(dc["regionindex"]);
                bsvalue.Add(dc["bsvalue"]);
                bslength.Add(dc["bslength"]);
                nodetype.Add(dc["nodetype"]);
                DESCRIPTION.Add(dc["DESCRIPTION"]);
                channelid.Add(dc["channelid"]);
                ++segmentSize;
                if (segmentSize == rowcounts)
                {
                    //执行插入
                    datas.Add("regionindex", regionindex.ToArray());
                    datas.Add("bsvalue", bsvalue.ToArray());
                    datas.Add("bslength", bslength.ToArray());
                    datas.Add("nodetype", nodetype.ToArray());
                    datas.Add("DESCRIPTION", DESCRIPTION.ToArray());
                    datas.Add("channelid", channelid.ToArray());
                    DbHelper.BatchInsertNew(orauser + ".definednodes", datas, segmentSize, parameters);
                    //清空
                    regionindex.Clear();
                    bsvalue.Clear();
                    bslength.Clear();
                    nodetype.Clear();
                    DESCRIPTION.Clear();
                    channelid.Clear();
                }
            }
            if (segmentSize > 0)
            {
                //执行插入
                datas.Add("regionindex", regionindex.ToArray());
                datas.Add("bsvalue", bsvalue.ToArray());
                datas.Add("bslength", bslength.ToArray());
                datas.Add("nodetype", nodetype.ToArray());
                datas.Add("DESCRIPTION", DESCRIPTION.ToArray());
                datas.Add("channelid", channelid.ToArray());
                DbHelper.BatchInsertNew(orauser + ".definednodes", datas, segmentSize, parameters);
            }
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
        public string ConSims(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime,
            string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode, int level, string cores, string levelsidid)
        {
            #region
            string continentName = "";
            /*******************/
            //设置状态 正在入库 2014-04-27ByRen
            string sql = "update orauserTable set state=1 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            string str = "{\"msg\":\"ok\"}";
            //删除已有Sid
            //sql = "delete from jzjw_schememgr where sid='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            //插入大洲编码
            RiverSeg topOne = rivers[0];
            string topTwo = topOne.REGIONIDNEW.ToString();
            if (topTwo.Length > 3) topTwo = topTwo.Substring(0, 3);

            //获取大洲
            continentName = ConName(topTwo);
            submitDate = DateTime.Now;
            /*
            sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,simstime,datashift,concode) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                    starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),to_date('"+DateTime.Now+"','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "')";
            */
            sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,SUBMITTIME,datashift,concode,CORES,LEVELSIDID) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                 starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "'," + cores + "," + levelsidid + ")";

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
            //20150110修改
            sql = "truncate table " + orauser + ".reservoir";
            DbHelper.ExecuteSql(sql);

            /////////////////////////////////////////////////
            long minRegionidnew = long.MaxValue, minBinstrlen = long.MaxValue;
            //01
            if (rivers != null && rivers.Count > 0)
            {
                foreach (RiverSeg rs in rivers)
                {
                    //1222
                    if (rs.REGIONIDNEW < minRegionidnew) minRegionidnew = rs.REGIONIDNEW;
                    //if ( Convert.ToInt64(rs.REGIONIDNEW) < minRegionidnew) minRegionidnew = Convert.ToInt64(rs.REGIONIDNEW);
                    if (rs.BINSTRLEN < minBinstrlen) minBinstrlen = rs.BINSTRLEN;

                    string rgin = "" + rs.REGIONIDNEW;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    //sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                    //     rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "')";
                    //0831日插入null值不允许
                    sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description,channelid) values(" +
                             rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'" + rs.NODETYPE + "','" + rs.REGIONIDNEW + "',0)";
                    DbHelper.ExecuteSql(sql);
                    if (rs.NODETYPE == "RS")
                    {
                        //添加水库节点 20150109添加插入语句
                        sql = "insert into " + orauser + ".reservoir(regionindex ,bsvalue,bslength,houroffset,minuteoffset,qoutput) values (" +
                            rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",289272,0,0)";
                        DbHelper.ExecuteSql(sql);
                    }
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
            #endregion
            //清空表  20150107
            /*
            sql = "truncate table " + orauser + ".definednodes";
            DbHelper.ExecuteSql(sql);
             **/
            //sql = "insert into " + orauser + ".definednodes select regionindex,bsvalue,bslength,'GS',regionindex2,channelindex from " + orauser + ".riversegs where stralherorder>=" + level;
            //DbHelper.ExecuteSql(sql);
            //20150112修改代码语句
            ConSimsBatchInesert(orauser, level);

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

            //No source slope
            sql = "update " + orauser + ".riversegs set lengthsource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopesource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);

            //pseudo river reaches
            sql = "update " + orauser + ".riversegs set areasource=-1, arealeft=-1, arearight=-1, catchmentarea=-1, seglength=-1 where seglength=0";
            DbHelper.ExecuteSql(sql);

            //jiejue guoxiao zhi
            sql = "update " + orauser + ".riversegs set slopesource=0.1 where slopesource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopeleft=0.1 where slopeleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set sloperight=0.1 where sloperight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=0.01 where areasource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arealeft=0.01 where arealeft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arearight=0.01 where arearight=0";
            DbHelper.ExecuteSql(sql);


            //km-->m
            sql = "update " + orauser + ".riversegs set lengthsource=lengthsource*1000 where lengthsource>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthleft=lengthleft*1000 where lengthleft>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthright=lengthright*1000 where lengthright>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set seglength=seglength*1000 where seglength>0";
            DbHelper.ExecuteSql(sql);
            /************************************************************************************************/

            //后面就是计算代码调用hpc部分

            //设置状态 正在计算 调用hpcJob代码 2014-04-27ByRen
            sql = "update orauserTable set state=2 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            //模拟计算开始时间
            sql = "update jzjw_schememgr set simstime= to_date('" + DateTime.Now + "','yyyy-mm-dd hh24:mi:ss') " + "where sid='" + sid + "' and SUBMITTIME=to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            //调用计算hpc
            CreateJob(sid, orauser, usercode, cores);

            //在hpc计算完成后，入库和设置状态
            /************************************************************************************************/
            return str;
        }

        /// <summary>
        /// 04连续多方案并行计算_不同机构
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

        public string ConSimsAgency(string sid, string orauser, string userid, List<RiverSeg> rivers,
            string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode,
            int level, string cores, string levelsidid, string origin, double preheat, int raindataws = 1, int raindatatype = 1)
        {
            ppreheat = preheat;

            #region 01更新orauserTable状态为1
            string continentName = "";
            /*******************/
            //设置状态 正在入库 2014-04-27ByRen
            string sql = "update orauserTable set state=1 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);


            string str = "{\"msg\":\"ok\"}";
            //删除已有Sid
            //sql = "delete from jzjw_schememgr where sid='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            #endregion
            #region 02获取大洲编号和相应的数据库表名
            //插入大洲编码
            RiverSeg topOne = rivers[0];
            string topTwo = topOne.REGIONIDNEW.ToString();
            if (topTwo.Length > 3) topTwo = topTwo.Substring(0, 3);

            //获取大洲
            continentName = ConName(topTwo);
            #endregion
            #region 03插入jzjw_schememgr记录
            submitDate = DateTime.Now;
            if (iscontiune == true)
            {
                /*
                sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,SUBMITTIME,datashift,concode,CORES,LEVELSIDID) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                      starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "'," + cores + "," + levelsidid + ")";
                 */
                sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,SUBMITTIME,datashift,concode,CORES,LEVELSIDID,preheat) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                      starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "'," + cores + "," + levelsidid + "," + preheat + ")";

                DbHelper.ExecuteSql(sql);
            }
            #endregion
            #region 04 truncate计算用户相关表-修改
            sql = "truncate table " + orauser + ".riversegs";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".discharge";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".regionconnection";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".definednodes";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".rainhour";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".nametabhour";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".status";
            DbHelper.ExecuteSql(sql);
            //是否获取数据 从hgcom.Status表中
            //模拟开始时间 根据预热期重新设置开始和结束时间
            DateTime dtStart0 = Convert.ToDateTime(starttime).AddDays(preheat * (-1));
            DateTime dtStart1 = Convert.ToDateTime(starttime);
            if (iscontiune == true)
            {
                //double starthouroffset = dt.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours;
                //是否存 存几天-20150411  自预热器开始的几天内的数据  houroffset
                sql = "select statusisget,statusisgetdays from hgmgr.schemecontsim where sid='" + sid + "'";
                DataTable dtgetdays = DbHelper.Query(sql).Tables[0];
                if (dtgetdays != null && dtgetdays.Rows.Count == 1)
                {
                    if (dtgetdays.Rows[0]["statusisget"].ToString() == "1")
                    {
                        Double statusisgetdays = Convert.ToDouble(dtgetdays.Rows[0]["statusisgetdays"]);
                        DateTime dtEnd0 = dtStart0.AddDays(statusisgetdays);
                        sql = "insert into " + orauser + ".status (select SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,WLL,WLU,WLM,WLD," +
                            "WRL,WRU,WRM,WRD,WSL,WSU,WSM,WSD,E,P,SLOPEERO,CHANNELERO,GRAVITYERO from hgcom.status where sccd='" + sid + "' and (" +
                            "houroffset >= " + dtStart0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + " and houroffset<" +
                            dtEnd0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + "))";
                        DbHelper.ExecuteSql(sql);
                    }
                }
            }
            //20140320添加代码ByRen
            /////////////////////////////////////////////////
            sql = "truncate table " + orauser + ".tvarparameter";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".raincmorph3H25KM";
            DbHelper.ExecuteSql(sql);
            //20150110修改
            sql = "truncate table " + orauser + ".reservoir";

            DbHelper.ExecuteSql(sql);
            #endregion
            #region 05向计算用户的definednodes，riversegs表中插入数据
            long minRegionidnew = long.MaxValue, minBinstrlen = long.MaxValue;
            //01
            if (rivers != null && rivers.Count > 0)
            {
                foreach (RiverSeg rs in rivers)
                {
                    //1222
                    if (rs.REGIONIDNEW < minRegionidnew) minRegionidnew = rs.REGIONIDNEW;
                    //if (Convert.ToInt64(rs.REGIONIDNEW) < minRegionidnew) minRegionidnew = Convert.ToInt64(rs.REGIONIDNEW);
                    if (rs.BINSTRLEN < minBinstrlen) minBinstrlen = rs.BINSTRLEN;

                    string rgin = "" + rs.REGIONIDNEW;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    //sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                    //     rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "')";
                    //0831日插入null值不允许
                    sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description,channelid) values(" +
                             rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'" + rs.NODETYPE + "','" + rs.REGIONIDNEW + "',0)";
                    DbHelper.ExecuteSql(sql);
                    if (rs.NODETYPE == "RS")
                    {
                        //添加水库节点 20150109添加插入语句
                        sql = "insert into " + orauser + ".reservoir(regionindex ,bsvalue,bslength,houroffset,minuteoffset,qoutput) values (" +
                            rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",289272,0,0)";
                        DbHelper.ExecuteSql(sql);
                    }
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
                "chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName + "where substr(regioncode,1," + lenNew +
                ") in (select regioncode from (select regioncode,min(binstrlen) from " + "GDN2." + continentName + "where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" +
                lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")) and length(regioncode)>" + lenNew + ")";
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
            #endregion

            #region 06update regionconnection
            sql = "update " + orauser + ".regionconnection set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set toregionindex=substr(toregionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 07拷贝降雨数据
            #region 隐藏
            //20140320添加代码ByRen
            /************************************************************************************************/
            //04 设置参数

            //模拟开始时间 根据预热期重新设置开始和结束时间
            //DateTime dtStart = Convert.ToDateTime(starttime);
            //后两行代码放置到前面
            //DateTime dtStart0 = Convert.ToDateTime(starttime).AddDays(preheat * (-1));
            //DateTime dtStart1 = Convert.ToDateTime(starttime);
            //模拟结束时间
            DateTime dtEnd = Convert.ToDateTime(endtime);

            //DateTime dtTmp = dtStart;
            DateTime dtTmp = dtStart0;
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
                DataTable rst = new DataTable();
                //测试计算时间 20150124
                DateTime dts = DateTime.Now;
            #endregion

                if (raindataws == 1)
                {
                    //20150116修改此部分代码，降雨数据从外部的webservice接口获取数据--新接口（从外网服务获取数据，并插入到raincmorph3h25km表中）
                    //测试用
                    //XMIN = 102;
                    // XMAX = 102.5;
                    //YMIN = 44;
                    // YMAX = 44.5;                    
                    /*
                    rst = GetRainData(XMIN, XMAX, YMIN, YMAX, origin, dtStart, dtEnd);
                    if (rst.Rows.Count > 0 && rst != null)
                        //指定单独一个机构名
                        insertRainData(rst, orauser, origin);
                     */
                    //分块读取数据后插入数据库
                    //GetRainDataByBlock(XMIN, XMAX, YMIN, YMAX, origin, dtStart, dtEnd,orauser);
                    //测试
                    /*
                    sql = "truncate table AREN_GETRAINbug";
                    DbHelper.ExecuteSql(sql);
                    sql = "truncate table aren_getrainnull";
                    DbHelper.ExecuteSql(sql);
                     */

                    //改参数
                    //sql = "truncate table "+orauser+".hydrousepara";
                    //DbHelper.ExecuteSql(sql);
                    //sql = "insert into "+orauser+".hydrousepara (select * from hg07.hydrousepara)";
                    //DbHelper.ExecuteSql(sql);

                    if (raindatatype == 1)
                    {

                        //A_B段 分批获取
                        //GetRainDataByBlock(XMIN, XMAX, YMIN, YMAX, origin, dtStart1, dtEnd, orauser);
                        //A_B段 整个获取
                        DataTable rst1 = GetRainData2(XMIN, XMAX, YMIN, YMAX, origin, dtStart1, dtEnd);
                        //入库
                        insertRainData(rst1, orauser, origin);
                        //C-A段
                        //GetRainDataFromReal(sid, dtStart0, dtEnd,orauser);

                        GetRainDataFromReal(sid, dtStart0, dtStart1, orauser);
                        sql = "update " + orauser + ".hydrousepara set raintype='TimePoint'";
                        DbHelper.ExecuteSql(sql);

                    }
                    else
                    {
                        //获取整个时间段的降雨量
                        //分批获取
                        //GetRainDataByBlock(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtEnd, orauser);
                        /*
                        //AB段
                        DataTable rst1 = GetRainData2(XMIN, XMAX, YMIN, YMAX, origin, dtStart1, dtEnd);
                        //入库
                        insertRainData(rst1, orauser, origin);
                        //CA段
                        DataTable rst2 = GetRainData3(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtStart1);
                        //入库                        
                        insertRainData(rst2, orauser, origin);
                         /***************************方式1************************************/
                        /*
                        dtStart1 = new DateTime(2015, 6, 24, 0, 0, 0);
                        DataTable rst1 = GetRainData2(XMIN, XMAX, YMIN, YMAX, origin, dtStart1, dtEnd);                        
                        insertRainData(rst1, orauser, origin);
                        DataTable rst2 = GetRainData3(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtStart1);
                        insertRainData(rst2, orauser, origin);
                        */
                        /***************************方式2************************************/
                        /**/
                        DataTable rst2 = GetRainData3(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtEnd);
                        insertRainData(rst2, orauser, origin);

                        sql = "update " + orauser + ".hydrousepara set raintype='CMORPH25'";
                        DbHelper.ExecuteSql(sql);


                        /*
                        //只适合美国方案的第一个方案  将美国方案的预报降雨数据倒入到raincmorph3h25km表中  select * from qh.qhraincmorph3h25km
                        sql = "insert into " + orauser + ".raincmorph3h25km(id,year,month,day,hours,houre,p) (select id,year,month,day,hours,houre,p from qh.qhraincmorph3h25km where month=6 and day<28)";
                        DbHelper.ExecuteSql(sql);
                         */

                    }

                }
                else if (rst.Rows.Count == 0 || rst == null || raindataws == 0)
                {

                    //从webservice获取不到数据，则从本地数据库中获取数据
                    #region 获取本地数据库中的降雨数据 ByRenEdit20150116
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
                    sql = "update " + orauser + ".hydrousepara set raintype='CMORPH25'";
                    DbHelper.ExecuteSql(sql);
                    #endregion
                }
                DateTime dtn = DateTime.Now;
                TimeSpan span = (TimeSpan)(dtn - dts);
                Double second = span.TotalSeconds;

                /*
                //测试插入获取降雨数据的时间 to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')
                sql = "insert into AREN_GetRainTime(SID,N_stime,N_etime,N_tspan,XMIN,XMAX,YMIN,YMAX,stime,etime) values('" + sid + "',to_date('" + dts + "','yyyy-mm-dd hh24:mi:ss')," +
                    "to_date('" + dtn + "','yyyy-mm-dd hh24:mi:ss')," + second + "," + XMIN + "," + XMAX + "," + YMIN + "," + YMAX + ",to_date('" + dtStart0 + "','yyyy-mm-dd hh24:mi:ss')," +
                    "to_date('" + dtEnd + "','yyyy-mm-dd hh24:mi:ss'))";
                DbHelper.ExecuteSql(sql);
                 */
            }

            //06 修改Scheme起止时间
            string TimeStart = "1950-01-01 00:00:00";
            //起始时间的跨度小时
            double starthouroffset = dtStart0.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //终止时间的跨度小时
            double endhouroffset = dtEnd.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //STARTYEAR,STARTMONTH,STARTDAY,STARTHOUR,ENDYEAR,ENDMONTH,ENDDAY,ENDHOUR及STARTHOUROFFSET,ENDHOUROFFSET
            sql = "update " + orauser + ".hydrousepara set STARTYEAR=" + dtStart0.Year + ",STARTMONTH=" + dtStart0.Month +
                ",STARTDAY=" + dtStart0.Day + ",STARTHOUR=" + dtStart0.Hour + ",ENDYEAR=" + dtEnd.Year + ",ENDMONTH=" +
                dtEnd.Month + ",ENDDAY=" + dtEnd.Day + ",STARTHOUROFFSET=" + starthouroffset + ",ENDHOUROFFSET=" + endhouroffset;
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 08更新计算用户相关表数据
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


            //No source slope
            sql = "update " + orauser + ".riversegs set lengthsource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopesource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);

            //pseudo river reaches
            sql = "update " + orauser + ".riversegs set areasource=-1, arealeft=-1, arearight=-1, catchmentarea=-1, seglength=-1 where seglength=0";
            DbHelper.ExecuteSql(sql);

            //jiejue guoxiao zhi
            sql = "update " + orauser + ".riversegs set slopesource=0.1 where slopesource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopeleft=0.1 where slopeleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set sloperight=0.1 where sloperight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=0.01 where areasource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arealeft=0.01 where arealeft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arearight=0.01 where arearight=0";
            DbHelper.ExecuteSql(sql);


            //km-->m
            sql = "update " + orauser + ".riversegs set lengthsource=lengthsource*1000 where lengthsource>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthleft=lengthleft*1000 where lengthleft>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthright=lengthright*1000 where lengthright>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set seglength=seglength*1000 where seglength>0";
            DbHelper.ExecuteSql(sql);

            //sql = "update " + orauser + ".riversegs set slope=10 where slope=0";
            //DbHelper.ExecuteSql(sql);
            /************************************************************************************************/
            #endregion
            //后面就是计算代码调用hpc部分

            #region 09更新orausertable的状态为2，更新模拟计算时间
            //设置状态 正在计算 调用hpcJob代码 2014-04-27ByRen
            sql = "update orauserTable set state=2 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            //模拟计算开始时间
            sql = "update jzjw_schememgr set simstime= to_date('" + DateTime.Now + "','yyyy-mm-dd hh24:mi:ss') " + "where sid='" + sid + "' and SUBMITTIME=to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 10调用HPC
            //调用计算hpc
            CreateJob(sid, orauser, usercode, cores);
            #endregion
            //在hpc计算完成后，入库和设置状态
            /************************************************************************************************/
            return str;
        }
        /// <summary>
        /// 04连续多方案并行计算_不同机构_沙溪河区域
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
        public string ConSimsAgency_sxh(string sid, string orauser, string userid, List<RiverSeg> rivers,
            string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode,
            int level, string cores, string levelsidid, string origin, double preheat, int raindataws = 1, int raindatatype = 1)
        {
            ppreheat = preheat;

            #region 01更新orauserTable状态为1
            string continentName = "";
            /*******************/
            //设置状态 正在入库 2014-04-27ByRen
            string sql = "update orauserTable set state=1 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);


            string str = "{\"msg\":\"ok\"}";
            //删除已有Sid
            //sql = "delete from jzjw_schememgr where sid='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            #endregion
            #region 02获取大洲编号和相应的数据库表名
            //插入大洲编码
            RiverSeg topOne = rivers[0];
            string topTwo = topOne.REGIONIDNEW.ToString();
            if (topTwo.Length > 3) topTwo = topTwo.Substring(0, 3);

            //获取大洲
            continentName = ConName(topTwo);
            #endregion
            #region 03插入jzjw_schememgr记录
            submitDate = DateTime.Now;

            //方案情景表功能接口
            sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,SUBMITTIME,datashift,concode,CORES,LEVELSIDID,preheat) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                     starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "'," + cores + "," + levelsidid + "," + preheat + ")";
            DbHelper.ExecuteSql(sql);
            /*if (iscontiune == true)
            {
                sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,SUBMITTIME,datashift,concode,CORES,LEVELSIDID,preheat) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                      starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "'," + cores + "," + levelsidid + "," + preheat + ")";
                DbHelper.ExecuteSql(sql);
            }*/

            #endregion
            #region 04 truncate计算用户相关表-修改
            sql = "truncate table " + orauser + ".riversegs";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".discharge";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".regionconnection";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".definednodes";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".rainhour";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".nametabhour";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".status";
            DbHelper.ExecuteSql(sql);
            //是否获取数据 从hgcom.Status表中
            //模拟开始时间 根据预热期重新设置开始和结束时间
            DateTime dtStart0 = Convert.ToDateTime(starttime).AddDays(preheat * (-1));
            DateTime dtStart1 = Convert.ToDateTime(starttime);
            if (iscontiune == true)
            {
                //double starthouroffset = dt.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours;
                //是否存 存几天-20150411  自预热器开始的几天内的数据  houroffset
                sql = "select statusisget,statusisgetdays from hgmgr.schemecontsim where sid='" + sid + "'";
                DataTable dtgetdays = DbHelper.Query(sql).Tables[0];
                if (dtgetdays != null && dtgetdays.Rows.Count == 1)
                {
                    if (dtgetdays.Rows[0]["statusisget"].ToString() == "1")
                    {
                        Double statusisgetdays = Convert.ToDouble(dtgetdays.Rows[0]["statusisgetdays"]);
                        DateTime dtEnd0 = dtStart0.AddDays(statusisgetdays);
                        sql = "insert into " + orauser + ".status (select SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,WLL,WLU,WLM,WLD," +
                            "WRL,WRU,WRM,WRD,WSL,WSU,WSM,WSD,E,P,SLOPEERO,CHANNELERO,GRAVITYERO from hgcom.status where sccd='" + sid + "' and (" +
                            "houroffset >= " + dtStart0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + " and houroffset<" +
                            dtEnd0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + "))";
                        DbHelper.ExecuteSql(sql);
                    }
                }
            }
            //20140320添加代码ByRen
            /////////////////////////////////////////////////
            sql = "truncate table " + orauser + ".tvarparameter";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table " + orauser + ".raincmorph3H25KM";
            DbHelper.ExecuteSql(sql);
            //20150110修改
            sql = "truncate table " + orauser + ".reservoir";

            DbHelper.ExecuteSql(sql);
            #endregion
            #region 05向计算用户的definednodes，riversegs表中插入数据
            long minRegionidnew = long.MaxValue, minBinstrlen = long.MaxValue;
            //01
            if (rivers != null && rivers.Count > 0)
            {
                foreach (RiverSeg rs in rivers)
                {
                    //1222
                    if (rs.REGIONIDNEW < minRegionidnew) minRegionidnew = rs.REGIONIDNEW;
                    //if (Convert.ToInt64(rs.REGIONIDNEW) < minRegionidnew) minRegionidnew = Convert.ToInt64(rs.REGIONIDNEW);
                    if (rs.BINSTRLEN < minBinstrlen) minBinstrlen = rs.BINSTRLEN;

                    string rgin = "" + rs.REGIONIDNEW;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    //sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                    //     rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "')";
                    //0831日插入null值不允许
                    sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description,channelid) values(" +
                             rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'" + rs.NODETYPE + "','" + rs.REGIONIDNEW + "',0)";
                    DbHelper.ExecuteSql(sql);
                    if (rs.NODETYPE == "RS")
                    {
                        //添加水库节点 20150109添加插入语句
                        sql = "insert into " + orauser + ".reservoir(regionindex ,bsvalue,bslength,houroffset,minuteoffset,qoutput) values (" +
                            rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",289272,0,0)";
                        DbHelper.ExecuteSql(sql);
                    }
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
                "chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName + "where substr(regioncode,1," + lenNew +
                ") in (select regioncode from (select regioncode,min(binstrlen) from " + "GDN2." + continentName + "where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" +
                lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")) and length(regioncode)>" + lenNew + ")";
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
            #endregion

            #region 06update regionconnection
            sql = "update " + orauser + ".regionconnection set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set toregionindex=substr(toregionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 07拷贝降雨数据
            #region 隐藏
            //20140320添加代码ByRen
            /************************************************************************************************/
            //04 设置参数

            //模拟开始时间 根据预热期重新设置开始和结束时间
            //DateTime dtStart = Convert.ToDateTime(starttime);
            //后两行代码放置到前面
            //DateTime dtStart0 = Convert.ToDateTime(starttime).AddDays(preheat * (-1));
            //DateTime dtStart1 = Convert.ToDateTime(starttime);
            //模拟结束时间
            DateTime dtEnd = Convert.ToDateTime(endtime);

            //DateTime dtTmp = dtStart;
            DateTime dtTmp = dtStart0;
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
                DataTable rst = new DataTable();
                //测试计算时间 20150124
                DateTime dts = DateTime.Now;
            #endregion

                if (raindataws == 1)
                {
                    //20150116修改此部分代码，降雨数据从外部的webservice接口获取数据--新接口（从外网服务获取数据，并插入到raincmorph3h25km表中）
                    //测试用
                    //XMIN = 102;
                    // XMAX = 102.5;
                    //YMIN = 44;
                    // YMAX = 44.5;                    
                    /*
                    rst = GetRainData(XMIN, XMAX, YMIN, YMAX, origin, dtStart, dtEnd);
                    if (rst.Rows.Count > 0 && rst != null)
                        //指定单独一个机构名
                        insertRainData(rst, orauser, origin);
                     */
                    //分块读取数据后插入数据库
                    //GetRainDataByBlock(XMIN, XMAX, YMIN, YMAX, origin, dtStart, dtEnd,orauser);
                    //测试
                    /*
                    sql = "truncate table AREN_GETRAINbug";
                    DbHelper.ExecuteSql(sql);
                    sql = "truncate table aren_getrainnull";
                    DbHelper.ExecuteSql(sql);
                     */

                    //改参数
                    //sql = "truncate table "+orauser+".hydrousepara";
                    //DbHelper.ExecuteSql(sql);
                    //sql = "insert into "+orauser+".hydrousepara (select * from hg07.hydrousepara)";
                    //DbHelper.ExecuteSql(sql);

                    if (raindatatype == 1)
                    {
                        //A_B段 分批获取
                        //GetRainDataByBlock(XMIN, XMAX, YMIN, YMAX, origin, dtStart1, dtEnd, orauser);
                        //A_B段 整个获取
                        DataTable rst1 = GetRainData2(XMIN, XMAX, YMIN, YMAX, origin, dtStart1, dtEnd);
                        //入库
                        insertRainData(rst1, orauser, origin);
                        //C-A段
                        //GetRainDataFromReal(sid, dtStart0, dtEnd,orauser);

                        GetRainDataFromReal(sid, dtStart0, dtStart1, orauser);
                        sql = "update " + orauser + ".hydrousepara set raintype='TimePoint'";
                        DbHelper.ExecuteSql(sql);
                    }
                    else if (raindatatype == 2)
                    {
                        //沙溪河区域的数据
                        GetRainDataFromsxh(sid, dtStart1, dtEnd, orauser);
                        sql = "update " + orauser + ".hydrousepara set raintype='TimePoint'";
                        DbHelper.ExecuteSql(sql);
                    }
                    else
                    {
                        //获取整个时间段的降雨量
                        //分批获取
                        //GetRainDataByBlock(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtEnd, orauser);
                        /*
                        //AB段
                        DataTable rst1 = GetRainData2(XMIN, XMAX, YMIN, YMAX, origin, dtStart1, dtEnd);
                        //入库
                        insertRainData(rst1, orauser, origin);
                        //CA段
                        DataTable rst2 = GetRainData3(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtStart1);
                        //入库                        
                        insertRainData(rst2, orauser, origin);
                         /***************************方式1************************************/
                        /*
                        dtStart1 = new DateTime(2015, 6, 24, 0, 0, 0);
                        DataTable rst1 = GetRainData2(XMIN, XMAX, YMIN, YMAX, origin, dtStart1, dtEnd);                        
                        insertRainData(rst1, orauser, origin);
                        DataTable rst2 = GetRainData3(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtStart1);
                        insertRainData(rst2, orauser, origin);
                        */
                        /***************************方式2************************************/
                        /**/
                        DataTable rst2 = GetRainData3(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtEnd);
                        insertRainData(rst2, orauser, origin);

                        sql = "update " + orauser + ".hydrousepara set raintype='CMORPH25'";
                        DbHelper.ExecuteSql(sql);


                        /*
                        //只适合美国方案的第一个方案  将美国方案的预报降雨数据倒入到raincmorph3h25km表中  select * from qh.qhraincmorph3h25km
                        sql = "insert into " + orauser + ".raincmorph3h25km(id,year,month,day,hours,houre,p) (select id,year,month,day,hours,houre,p from qh.qhraincmorph3h25km where month=6 and day<28)";
                        DbHelper.ExecuteSql(sql);
                         */

                    }

                }
                else if (rst.Rows.Count == 0 || rst == null || raindataws == 0)
                {

                    //从webservice获取不到数据，则从本地数据库中获取数据
                    #region 获取本地数据库中的降雨数据 ByRenEdit20150116
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
                    sql = "update " + orauser + ".hydrousepara set raintype='CMORPH25'";
                    DbHelper.ExecuteSql(sql);
                    #endregion
                }
                DateTime dtn = DateTime.Now;
                TimeSpan span = (TimeSpan)(dtn - dts);
                Double second = span.TotalSeconds;

                /*
                //测试插入获取降雨数据的时间 to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')
                sql = "insert into AREN_GetRainTime(SID,N_stime,N_etime,N_tspan,XMIN,XMAX,YMIN,YMAX,stime,etime) values('" + sid + "',to_date('" + dts + "','yyyy-mm-dd hh24:mi:ss')," +
                    "to_date('" + dtn + "','yyyy-mm-dd hh24:mi:ss')," + second + "," + XMIN + "," + XMAX + "," + YMIN + "," + YMAX + ",to_date('" + dtStart0 + "','yyyy-mm-dd hh24:mi:ss')," +
                    "to_date('" + dtEnd + "','yyyy-mm-dd hh24:mi:ss'))";
                DbHelper.ExecuteSql(sql);
                 */
            }

            //06 修改Scheme起止时间
            string TimeStart = "1950-01-01 00:00:00";
            //起始时间的跨度小时
            double starthouroffset = dtStart0.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //终止时间的跨度小时
            double endhouroffset = dtEnd.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //STARTYEAR,STARTMONTH,STARTDAY,STARTHOUR,ENDYEAR,ENDMONTH,ENDDAY,ENDHOUR及STARTHOUROFFSET,ENDHOUROFFSET
            sql = "update " + orauser + ".hydrousepara set STARTYEAR=" + dtStart0.Year + ",STARTMONTH=" + dtStart0.Month +
                ",STARTDAY=" + dtStart0.Day + ",STARTHOUR=" + dtStart0.Hour + ",ENDYEAR=" + dtEnd.Year + ",ENDMONTH=" +
                dtEnd.Month + ",ENDDAY=" + dtEnd.Day + ",STARTHOUROFFSET=" + starthouroffset + ",ENDHOUROFFSET=" + endhouroffset;
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 08更新计算用户相关表数据
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


            //No source slope
            sql = "update " + orauser + ".riversegs set lengthsource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopesource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);

            //pseudo river reaches
            sql = "update " + orauser + ".riversegs set areasource=-1, arealeft=-1, arearight=-1, catchmentarea=-1, seglength=-1 where seglength=0";
            DbHelper.ExecuteSql(sql);

            //jiejue guoxiao zhi
            sql = "update " + orauser + ".riversegs set slopesource=0.1 where slopesource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopeleft=0.1 where slopeleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set sloperight=0.1 where sloperight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=0.01 where areasource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arealeft=0.01 where arealeft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arearight=0.01 where arearight=0";
            DbHelper.ExecuteSql(sql);


            //km-->m
            sql = "update " + orauser + ".riversegs set lengthsource=lengthsource*1000 where lengthsource>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthleft=lengthleft*1000 where lengthleft>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthright=lengthright*1000 where lengthright>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set seglength=seglength*1000 where seglength>0";
            DbHelper.ExecuteSql(sql);

            //sql = "update " + orauser + ".riversegs set slope=10 where slope=0";
            //DbHelper.ExecuteSql(sql);
            /************************************************************************************************/
            #endregion
            //后面就是计算代码调用hpc部分

            #region 09更新orausertable的状态为2，更新模拟计算时间
            //设置状态 正在计算 调用hpcJob代码 2014-04-27ByRen
            sql = "update orauserTable set state=2 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            //模拟计算开始时间
            sql = "update jzjw_schememgr set simstime= to_date('" + DateTime.Now + "','yyyy-mm-dd hh24:mi:ss') " + "where sid='" + sid + "' and SUBMITTIME=to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 10调用HPC
            //调用计算hpc
            CreateJob(sid, orauser, usercode, cores);
            #endregion
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
        /// <param name="cores">核心数</param>
        /// <param name="levelsidid">用户方案级别</param>
        /// <param name="preheat">预热期</param>
        /// <returns></returns>
        public string DoRiverSegCalcNew(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode, string cores, string levelsidid, string origin, double preheat, int raindataws = 0)
        {
            #region 01更新orauserTable状态为1
            string continentName = "";
            /*******************/
            //设置状态 正在入库 2014-04-27ByRen
            string sql = "update orauserTable set state=1 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);


            string str = "{\"msg\":\"ok\"}";
            //删除已有Sid
            //sql = "delete from jzjw_schememgr where sid='" + sid + "'";
            //DbHelper.ExecuteSql(sql);
            #endregion
            #region 02获取大洲编号和相应的数据库表名
            //插入大洲编码
            RiverSeg topOne = rivers[0];
            string topTwo = topOne.REGIONIDNEW.ToString();
            if (topTwo.Length > 3) topTwo = topTwo.Substring(0, 3);

            //获取大洲
            continentName = ConName(topTwo);
            #endregion
            #region 03插入jzjw_schememgr记录
            submitDate = DateTime.Now;
            /*
            sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,SUBMITTIME,datashift,concode,CORES,LEVELSIDID) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                  starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "'," + cores + "," + levelsidid + ")";
             */
            sql = "insert into jzjw_schememgr(sid,orauser,userid,state,starttime,endtime,SUBMITTIME,datashift,concode,CORES,LEVELSIDID,preheat) values('" + sid + "','" + orauser + "','" + userid + "',0,to_date('" +
                  starttime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss'),to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss'),0,'" + topTwo + "'," + cores + "," + levelsidid + "," + preheat + ")";

            DbHelper.ExecuteSql(sql);
            #endregion
            #region 04 truncate计算用户相关表
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
            //20150110修改
            sql = "truncate table " + orauser + ".reservoir";

            DbHelper.ExecuteSql(sql);
            #endregion
            #region 05向计算用户的definednodes，riversegs表中插入数据
            long minRegionidnew = long.MaxValue, minBinstrlen = long.MaxValue;
            //01
            if (rivers != null && rivers.Count > 0)
            {
                foreach (RiverSeg rs in rivers)
                {
                    //1222
                    if (rs.REGIONIDNEW < minRegionidnew) minRegionidnew = rs.REGIONIDNEW;
                    //if (Convert.ToInt64(rs.REGIONIDNEW) < minRegionidnew) minRegionidnew = Convert.ToInt64(rs.REGIONIDNEW);
                    if (rs.BINSTRLEN < minBinstrlen) minBinstrlen = rs.BINSTRLEN;

                    string rgin = "" + rs.REGIONIDNEW;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    //sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                    //     rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'GS','" + rs.REGIONIDNEW + "')";
                    //0831日插入null值不允许
                    sql = "insert into " + orauser + ".definednodes(regionindex,bsvalue,bslength,nodetype,description,channelid) values(" +
                             rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",'" + rs.NODETYPE + "','" + rs.REGIONIDNEW + "',0)";
                    DbHelper.ExecuteSql(sql);
                    if (rs.NODETYPE == "RS")
                    {
                        //添加水库节点 20150109添加插入语句
                        sql = "insert into " + orauser + ".reservoir(regionindex ,bsvalue,bslength,houroffset,minuteoffset,qoutput) values (" +
                            rgin + "," + rs.BINSTRVAL + "," + rs.BINSTRLEN + ",289272,0,0)";
                        DbHelper.ExecuteSql(sql);
                    }
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
                "chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regioncode from " + "GDN2." + continentName + "where substr(regioncode,1," + lenNew +
                ") in (select regioncode from (select regioncode,min(binstrlen) from " + "GDN2." + continentName + "where substr(regioncode,1," + len + ")='" + minRegionidnew + "' and length(regioncode)=" +
                lenNew + " group by regioncode having min(binstrlen)>" + minBinstrlen + ")) and length(regioncode)>" + lenNew + ")";
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
            #endregion

            #region 06update regionconnection
            sql = "update " + orauser + ".regionconnection set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set toregionindex=substr(toregionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".regionconnection set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 07拷贝降雨数据
            //20140320添加代码ByRen
            /************************************************************************************************/
            //04 设置参数

            //模拟开始时间 根据预热期重新设置开始和结束时间
            //DateTime dtStart = Convert.ToDateTime(starttime);
            DateTime dtStart0 = Convert.ToDateTime(starttime).AddDays(preheat * (-1));
            DateTime dtStart1 = Convert.ToDateTime(starttime);
            //模拟结束时间
            DateTime dtEnd = Convert.ToDateTime(endtime);

            //DateTime dtTmp = dtStart;
            DateTime dtTmp = dtStart0;
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
                DataTable rst = new DataTable();
                //测试计算时间 20150124
                DateTime dts = DateTime.Now;
                if (raindataws == 1)
                {
                    //20150116修改此部分代码，降雨数据从外部的webservice接口获取数据--新接口（从外网服务获取数据，并插入到raincmorph3h25km表中）
                    //测试用
                    //XMIN = 102;
                    // XMAX = 102.5;
                    //YMIN = 44;
                    // YMAX = 44.5;                    
                    /*
                    rst = GetRainData(XMIN, XMAX, YMIN, YMAX, origin, dtStart, dtEnd);
                    if (rst.Rows.Count > 0 && rst != null)
                        //指定单独一个机构名
                        insertRainData(rst, orauser, origin);
                     */
                    //分块读取数据后插入数据库
                    //GetRainDataByBlock(XMIN, XMAX, YMIN, YMAX, origin, dtStart, dtEnd,orauser);
                    //测试
                    /*
                    sql = "truncate table AREN_GETRAINbug";
                    DbHelper.ExecuteSql(sql);
                    sql = "truncate table aren_getrainnull";
                    DbHelper.ExecuteSql(sql);
                     */
                    GetRainDataByBlock(XMIN, XMAX, YMIN, YMAX, origin, dtStart0, dtEnd, orauser);

                }
                else if (rst.Rows.Count == 0 || rst == null || raindataws == 0)
                {

                    //从webservice获取不到数据，则从本地数据库中获取数据
                    #region 获取本地数据库中的降雨数据 ByRenEdit20150116
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
                    #endregion
                }
                DateTime dtn = DateTime.Now;
                TimeSpan span = (TimeSpan)(dtn - dts);
                Double second = span.TotalSeconds;

                //测试插入获取降雨数据的时间 to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')
                sql = "insert into AREN_GetRainTime(SID,N_stime,N_etime,N_tspan,XMIN,XMAX,YMIN,YMAX,stime,etime) values('" + sid + "',to_date('" + dts + "','yyyy-mm-dd hh24:mi:ss')," +
                    "to_date('" + dtn + "','yyyy-mm-dd hh24:mi:ss')," + second + "," + XMIN + "," + XMAX + "," + YMIN + "," + YMAX + ",to_date('" + dtStart0 + "','yyyy-mm-dd hh24:mi:ss')," +
                    "to_date('" + dtEnd + "','yyyy-mm-dd hh24:mi:ss'))";
                DbHelper.ExecuteSql(sql);
            }

            //06 修改Scheme起止时间
            string TimeStart = "1950-01-01 00:00:00";
            //起始时间的跨度小时
            double starthouroffset = dtStart0.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //终止时间的跨度小时
            double endhouroffset = dtEnd.Subtract(Convert.ToDateTime(TimeStart)).TotalHours;
            //STARTYEAR,STARTMONTH,STARTDAY,STARTHOUR,ENDYEAR,ENDMONTH,ENDDAY,ENDHOUR及STARTHOUROFFSET,ENDHOUROFFSET
            sql = "update " + orauser + ".hydrousepara set STARTYEAR=" + dtStart0.Year + ",STARTMONTH=" + dtStart0.Month +
                ",STARTDAY=" + dtStart0.Day + ",STARTHOUR=" + dtStart0.Hour + ",ENDYEAR=" + dtEnd.Year + ",ENDMONTH=" +
                dtEnd.Month + ",ENDDAY=" + dtEnd.Day + ",STARTHOUROFFSET=" + starthouroffset + ",ENDHOUROFFSET=" + endhouroffset;
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 08更新计算用户相关表数据
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


            //No source slope
            sql = "update " + orauser + ".riversegs set lengthsource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopesource=-1 where stralherorder>1";
            DbHelper.ExecuteSql(sql);

            //pseudo river reaches
            sql = "update " + orauser + ".riversegs set areasource=-1, arealeft=-1, arearight=-1, catchmentarea=-1, seglength=-1 where seglength=0";
            DbHelper.ExecuteSql(sql);

            //jiejue guoxiao zhi
            sql = "update " + orauser + ".riversegs set slopesource=0.1 where slopesource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set slopeleft=0.1 where slopeleft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set sloperight=0.1 where sloperight=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set areasource=0.01 where areasource=0 and stralherorder=1";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arealeft=0.01 where arealeft=0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set arearight=0.01 where arearight=0";
            DbHelper.ExecuteSql(sql);


            //km-->m
            sql = "update " + orauser + ".riversegs set lengthsource=lengthsource*1000 where lengthsource>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthleft=lengthleft*1000 where lengthleft>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set lengthright=lengthright*1000 where lengthright>0";
            DbHelper.ExecuteSql(sql);
            sql = "update " + orauser + ".riversegs set seglength=seglength*1000 where seglength>0";
            DbHelper.ExecuteSql(sql);

            //sql = "update " + orauser + ".riversegs set slope=10 where slope=0";
            //DbHelper.ExecuteSql(sql);
            /************************************************************************************************/
            #endregion
            //后面就是计算代码调用hpc部分

            #region 09更新orausertable的状态为2，更新模拟计算时间
            //设置状态 正在计算 调用hpcJob代码 2014-04-27ByRen
            sql = "update orauserTable set state=2 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            //模拟计算开始时间
            sql = "update jzjw_schememgr set simstime= to_date('" + DateTime.Now + "','yyyy-mm-dd hh24:mi:ss') " + "where sid='" + sid + "' and SUBMITTIME=to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            #endregion

            #region 10调用HPC
            //调用计算hpc
            CreateJob(sid, orauser, usercode, cores);
            #endregion
            //在hpc计算完成后，入库和设置状态
            /************************************************************************************************/
            return str;
        }
        /// <summary>
        /// 分块  从其他WebService获取降雨数据
        /// </summary>
        /// <param name="minLongitude"></param>
        /// <param name="maxLongitude"></param>
        /// <param name="minLatitude"></param>
        /// <param name="maxLatitude"></param>
        /// <param name="origin"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        public void GetRainDataByBlock(double minLongitude, double maxLongitude, double minLatitude, double maxLatitude, String origin, DateTime sDate, DateTime eDate, string orauser)
        {
            DataTable rst;
            TimeSpan tspan = eDate.Subtract(sDate);
            double dtDays = tspan.TotalDays;
            double x = 0;
            double y = 0;
            DateTime dts = new DateTime();
            int datespan = 5;
            double xyspan = 0.5;
            //经纬度跨度为 0.5 时间跨度为1个月  分辨率为0.25度
            for (x = minLongitude; x < maxLongitude; x = x + xyspan)
            {
                for (y = minLatitude; y < maxLatitude; y = y + xyspan)
                {
                    if (dtDays < datespan)
                    {
                        //不足一个月
                        if ((x - minLongitude) % 0.5 == 0 && (y - minLatitude) % 0.5 == 0)
                        {
                            //rst= GetRainData(x, x + 0.5, y, y + 0.5, "ecmwf", sDate, eDate);
                            rst = GetRainData(x, x + 0.5, y, y + 0.5, origin, sDate, eDate);
                            if (rst != null && rst.Rows.Count > 0)
                                //指定单独一个机构名
                                insertRainData(rst, orauser, origin);
                        }
                    }
                    else
                    {
                        //超过一个月
                        for (dts = sDate; dts.Ticks < eDate.Ticks; dts = dts.AddDays(datespan))
                        {
                            if ((x - minLongitude) % 0.5 == 0 && (y - minLatitude) % 0.5 == 0)
                            {
                                if (dts.AddDays(datespan - 1).Ticks < eDate.Ticks)
                                    rst = GetRainData(x, x + 0.5, y, y + 0.5, origin, dts, dts.AddDays(datespan - 1));
                                else
                                    rst = GetRainData(x, x + 0.5, y, y + 0.5, origin, dts, eDate);

                                if (rst != null && rst.Rows.Count > 0)
                                    //指定单独一个机构名
                                    insertRainData(rst, orauser, origin);
                            }
                        }
                    }
                }
            }
            //剩余部分的也要插入
            /*
            rst= GetRainData(x, maxLongitude, y, maxLatitude, "ecmwf", dts, eDate);
            if (rst.Rows.Count > 0 && rst != null)
                //指定单独一个机构名
                insertRainData(rst, orauser, origin);
             */
        }
        /// <summary>
        /// 一次获取降雨数据服务
        /// </summary>
        /// <param name="minLongitude"></param>
        /// <param name="maxLongitude"></param>
        /// <param name="minLatitude"></param>
        /// <param name="maxLatitude"></param>
        /// <param name="origin"></param>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="orauser"></param>
        public void GetRainDataByAll(double minLongitude, double maxLongitude, double minLatitude, double maxLatitude, String origin, DateTime sDate, DateTime eDate, string orauser)
        {
            DataTable rst;
            rst = GetRainData(minLongitude, maxLongitude, minLatitude, maxLatitude, origin, sDate, eDate);
            if (rst != null && rst.Rows.Count > 0)
                insertRainData(rst, orauser, origin);
        }
        public void GetQoutputFromReal(string sid, DateTime eDate, string orauser)
        {
            //设置参数
            string url = "http://221.10.62.212:2013/RainfallService/getRunoff.aspx";
            string data = "date=" + eDate.ToString("yyyy-MM-dd");
            string result = HttpGet(url, data);
            //XML文档
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(result);

            DataTable datas = new DataTable();
            datas.Columns.Add("SID", Type.GetType("System.String"));
            datas.Columns.Add("STCD", Type.GetType("System.String"));
            datas.Columns.Add("STIME", Type.GetType("System.DateTime"));
            datas.Columns.Add("Q", Type.GetType("System.Double"));
            DataRow dr;
            string pid = "";
            string name = "";
            DateTime dt;
            double q = 0;
            //挨个查找其下站点pid 和 name 
            foreach (XmlElement childElement in doc.DocumentElement)
            {
                if (childElement.Name == "data")
                {
                    pid = childElement.GetAttribute("pid");
                    name = childElement.GetAttribute("name");
                    foreach (XmlNode grandsonNode in childElement.ChildNodes)
                    {
                        dt = Convert.ToDateTime(((XmlElement)grandsonNode).GetAttribute("time"));
                        q = Convert.ToDouble(grandsonNode.InnerXml);
                        dr = datas.NewRow();
                        dr["SID"] = sid;
                        dr["STCD"] = pid;
                        dr["STIME"] = dt; ;
                        dr["Q"] = q;
                        datas.Rows.Add(dr);
                    }
                }
            }
            //批量插入数据库
            insertRQoutput(datas, orauser);
        }

        //沙溪河降雨数据
        public void GetRainDataFromsxh(string sid, DateTime sDate, DateTime eDate, string orauser)
        {
            //测试用
            //sid = "20160327HP002";
            //00获取服务，然后插入到 rainhour表中
            //设置参数
            string url = "http://210.13.204.19:8083/smfxbServlets/servlet/STPPTNRServletMany";
            //string data = "date=2015-4-20";
            //当前服务中返回的结果是当前给定日期的前15天，若超过15天则循环获取数据
            string data, result;
            DataTable dtID = DbHelper.Query("select distinct(id) from hgmgr.nametabhour where sid='" + sid + "'").Tables[0];
            List<string> IDlist = new List<string>();
            for (int r = 0; r < dtID.Rows.Count; r++)
            {
                IDlist.Add("'" + dtID.Rows[r][0].ToString() + "'");
            }
            string IDS = string.Join(",", IDlist.ToArray());
            data = "stcds=" + IDS + "&stm=" + sDate.ToString() + "&etm=" + eDate.ToString();
            result = HttpGet(url, data);
            result = result.Substring(5, result.Length - 6);
            DataTable dtXML = ParseRainData(result, eDate, sDate, dtID);
            insertRRainData_sxh(dtXML, orauser);
            //do
            //{

            //    data = "date=" + eDate.ToString("yyyy-MM-dd");
            //    result = HttpGet(url, data);
            //    //XML文档
            //    XmlDocument doc = new XmlDocument();
            //    doc.LoadXml(result);
            //    //解析doc 获取数据的当前时间，预热期
            //    DataTable dtXML = ParseXML(doc, eDate, sDate, dtID);
            //    //插入数据库
            //    insertRRainData(dtXML, orauser);
            //    eDate = eDate.AddDays(-15);
            //} while (eDate > sDate);



            //将rainhour25km表中的部分数据拷贝到 rianhour表中
            //01更新hgmgr.nametabhour中的idmy
            sql = "select * from hgmgr.nametabhour where sid='" + sid + "'";
            DataTable dt = DbHelper.Query(sql).Tables[0];
            double y;
            double x;
            double xnew;
            double ynew;
            int id;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                y = Convert.ToDouble(dt.Rows[i]["Y"]);
                x = Convert.ToDouble(dt.Rows[i]["X"]);
                //这种方法是错误的
                //id = Convert.ToInt32(((90 -y) * 4 - 125 + 1) * 10000 + x * 4);
                //根据 xy生成最近的 0.125或0.375或0.625或0.875这种格式的数
                xnew = Math.Ceiling(x * 4) / 4 - 0.125;
                ynew = Math.Ceiling(y * 4) / 4 - 0.125;
                id = Convert.ToInt32(Math.Round(((90 - ynew) * 4 - 125 + 1) * 10000 + xnew * 4, MidpointRounding.AwayFromZero));
                sql = "update hgmgr.nametabhour set idmy=" + id + " where x=" + x + " and y=" + y;
                DbHelper.ExecuteSql(sql);
            }
            //02
            sql = "insert into " + orauser + ".nametabhour (select id,x,y from hgmgr.nametabhour where sid='" + sid + "')";
            DbHelper.ExecuteSql(sql);
            //03从raincmorph3h25km表中获取数据插入到rainhour表中 其中id用站点的gid
            //sql = "insert into " + orauser + ".rainhour (select a.id,b.year,b.month,b.day,b.hours,b.houre,b.p from hgmgr.nametabhour a," + orauser + ".raincmorph3h25km b where sid='" + sid + "' and a.idmy=b.id )";
            //DbHelper.ExecuteSql(sql);
        }

        public void GetRainDataFromReal(string sid, DateTime sDate, DateTime eDate, string orauser)
        {
            //测试用
            sid = "20150410HP001";
            //00获取服务，然后插入到 rainhour表中
            //设置参数
            string url = "http://221.10.62.212:2013/RainfallService/getRainData.aspx";
            //string data = "date=2015-4-20";
            //当前服务中返回的结果是当前给定日期的前15天，若超过15天则循环获取数据
            string data, result;
            DataTable dtID = DbHelper.Query("select distinct(id) from hgmgr.nametabhour where sid='" + sid + "'").Tables[0];
            do
            {
                data = "date=" + eDate.ToString("yyyy-MM-dd");
                result = HttpGet(url, data);
                //XML文档
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(result);
                //解析doc 获取数据的当前时间，预热期
                DataTable dtXML = ParseXML(doc, eDate, sDate, dtID);
                //插入数据库
                insertRRainData(dtXML, orauser);
                eDate = eDate.AddDays(-15);
            } while (eDate > sDate);



            //将rainhour25km表中的部分数据拷贝到 rianhour表中
            //01更新hgmgr.nametabhour中的idmy
            sql = "select * from hgmgr.nametabhour where sid='" + sid + "'";
            DataTable dt = DbHelper.Query(sql).Tables[0];
            double y;
            double x;
            double xnew;
            double ynew;
            int id;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                y = Convert.ToDouble(dt.Rows[i]["Y"]);
                x = Convert.ToDouble(dt.Rows[i]["X"]);
                //这种方法是错误的
                //id = Convert.ToInt32(((90 -y) * 4 - 125 + 1) * 10000 + x * 4);
                //根据 xy生成最近的 0.125或0.375或0.625或0.875这种格式的数
                xnew = Math.Ceiling(x * 4) / 4 - 0.125;
                ynew = Math.Ceiling(y * 4) / 4 - 0.125;
                id = Convert.ToInt32(Math.Round(((90 - ynew) * 4 - 125 + 1) * 10000 + xnew * 4, MidpointRounding.AwayFromZero));
                sql = "update hgmgr.nametabhour set idmy=" + id + " where x=" + x + " and y=" + y;
                DbHelper.ExecuteSql(sql);
            }
            //02
            sql = "insert into " + orauser + ".nametabhour (select id,x,y from hgmgr.nametabhour where sid='" + sid + "')";
            DbHelper.ExecuteSql(sql);
            //03从raincmorph3h25km表中获取数据插入到rainhour表中 其中id用站点的gid
            sql = "insert into " + orauser + ".rainhour (select a.id,b.year,b.month,b.day,b.hours,b.houre,b.p from hgmgr.nametabhour a," + orauser + ".raincmorph3h25km b where sid='" + sid + "' and a.idmy=b.id )";
            DbHelper.ExecuteSql(sql);
        }
        /// <summary>
        /// 从其他WebService获取降雨数据
        /// </summary>
        /// <param name="minLongitude">xmin</param>
        /// <param name="maxLongitude">xmax</param>
        /// <param name="minLatitude">ymin</param>
        /// <param name="maxLatitude">ymax</param>
        /// <param name="origin">机构名称</param>
        /// <param name="sDate">模拟开始时间</param>
        /// <param name="eDate">模拟结束时间</param>
        /// <returns></returns>
        public DataTable GetRainData(double minLongitude, double maxLongitude, double minLatitude, double maxLatitude, String origin, DateTime sDate, DateTime eDate)
        {
            DateTime n = DateTime.Now;
            string strsDate = sDate.ToString("yyyyMMdd");
            string streDate = eDate.ToString("yyyyMMdd");
            try
            {
                //调用java的WebService服务
                JZJW.Winder.BLL.wsf.GlobalForcastRainDataInfoClient ws = new wsf.GlobalForcastRainDataInfoClient();
                string jsonStr = ws.QueryForcastRainDataByRange(minLongitude, maxLongitude, minLatitude, maxLatitude, origin, sDate.ToString("yyyyMMdd"), eDate.ToString("yyyyMMdd"));
                return parseRainDataJson(jsonStr);

            }
            catch
            {
                DateTime dtn = DateTime.Now;
                TimeSpan span = (TimeSpan)(dtn - n);
                Double second = span.TotalSeconds;
                string sql = "insert into AREN_GETRAINbug(SID,N_stime,N_etime,N_tspan,XMIN,XMAX,YMIN,YMAX,stime,etime) values('" + this.sid + "',to_date('" + n + "','yyyy-mm-dd hh24:mi:ss')," +
                   "to_date('" + dtn + "','yyyy-mm-dd hh24:mi:ss')," + second + "," + minLongitude + "," + maxLongitude + "," + minLatitude + "," + maxLatitude +
                   ",to_date('" + sDate + "','yyyy-mm-dd hh24:mi:ss')," + "to_date('" + eDate + "','yyyy-mm-dd hh24:mi:ss'))";
                DbHelper.ExecuteSql(sql);
                return null;
            }
        }
        //获取各个机构最新预报数据的日期
        public string GetLatestForcastDate()
        {
            JZJW.Winder.BLL.wsf2.GlobalForcastRainDataServerClient ws2 = new wsf2.GlobalForcastRainDataServerClient();
            string jsonStr = ws2.ObtainLatestForcastDate();
            return jsonStr;
        }
        /// <summary>
        /// tmp函数计算 区域平均
        /// </summary>
        /// <returns></returns>
        public string Tmp()
        {
            string sql = "select distinct filename,rownuma from qh.qhrain_nws_precip t order by filename";
            DataTable dt = DbHelper.Query(sql).Tables[0];
            sql = " select * from qh.point";
            DataTable dtPoint = DbHelper.Query(sql).Tables[0];
            string where = "";
            double avgvalue;
            foreach (DataRow dr in dt.Rows)
            {
                foreach (DataRow drpoint in dtPoint.Rows)
                {
                    /*
                    if (Convert.ToInt64(dr["ROWNUMA"]) ==446876 || Convert.ToInt64(dr["ROWNUMA"]) ==1053389 || Convert.ToInt64(dr["ROWNUMA"]) == 1165200)
                        continue;
                     */
                    where = " where filename='" + dr["FILENAME"] + "' and lon between " + drpoint["minx"] + " and " + drpoint["maxx"] +
                        " and lat between " + drpoint["miny"] + " and " + drpoint["maxy"];
                    sql = "select to_char(avg(globvalue)) from qh.qhrain_nws_precip" + where;
                    avgvalue = Convert.ToDouble(DbHelper.GetSingle(sql));
                    if (avgvalue != 0)
                    {
                        //sql = "update qh.qhrain_nws_precip set globvalueage=" + avgvalue + ",ID=" + drpoint["id"] +",LATOUTER="+drpoint["LAT"]+ ",lonouter="+drpoint["LON"]+ where;
                        sql = "insert into qh.qhrain_nws_precip_new(filename,lat,lon,globvalue,rownuma,globvalueage,latouter,lonouter,id) (select filename,lat,lon,globvalue,rownuma," +
                            avgvalue + ",latouter,lonouter,id from (select filename,lat,lon,globvalue,rownuma," +
                            avgvalue + ",latouter,lonouter,id from qh.qhrain_nws_precip " + where + ") where rownum=1)";
                        DbHelper.ExecuteSql(sql);
                    }
                }
            }
            return "ok";
        }
        //获取降雨数据接口2
        public DataTable GetRainData2(double minLongitude, double maxLongitude, double minLatitude, double maxLatitude, String origin, DateTime sDate, DateTime eDate)
        {
            DateTime n = DateTime.Now;
            string strsDate = sDate.ToString("yyyyMMdd");
            //注意时间的问题
            string streDate = eDate.AddDays(-1).ToString("yyyyMMdd");
            try
            {
                //调用java的WebService服务
                JZJW.Winder.BLL.wsf2.GlobalForcastRainDataServerClient ws2 = new wsf2.GlobalForcastRainDataServerClient();
                //string jsonStr = ws2.QueryForcastRainDataByRange(minLongitude, maxLongitude, minLatitude, maxLatitude, origin, strsDate, streDate);
                //新接口
                string jsonStr = ws2.QueryLatestForcastRainDataByRange(minLongitude, maxLongitude, minLatitude, maxLatitude, origin, strsDate, streDate);
                DateTime dtn = DateTime.Now;
                TimeSpan span = (TimeSpan)(dtn - n);
                Double second = span.TotalSeconds;
                return parseRainDataJson2(jsonStr);
            }
            catch
            {
                DateTime dtn = DateTime.Now;
                TimeSpan span = (TimeSpan)(dtn - n);
                Double second = span.TotalSeconds;
                string sql = "insert into AREN_GETRAINbug(SID,N_stime,N_etime,N_tspan,XMIN,XMAX,YMIN,YMAX,stime,etime) values('" + this.sid + "',to_date('" + n + "','yyyy-mm-dd hh24:mi:ss')," +
                   "to_date('" + dtn + "','yyyy-mm-dd hh24:mi:ss')," + second + "," + minLongitude + "," + maxLongitude + "," + minLatitude + "," + maxLatitude +
                   ",to_date('" + sDate + "','yyyy-mm-dd hh24:mi:ss')," + "to_date('" + eDate + "','yyyy-mm-dd hh24:mi:ss'))";
                DbHelper.ExecuteSql(sql);
                return null;
            }
        }
        //获取降雨数据接口3 历史数据
        public DataTable GetRainData3(double minLongitude, double maxLongitude, double minLatitude, double maxLatitude, String origin, DateTime sDate, DateTime eDate)
        {
            DateTime n = DateTime.Now;
            string strsDate = sDate.ToString("yyyyMMdd");
            //注意时间的问题
            string streDate = eDate.AddDays(-1).ToString("yyyyMMdd");
            try
            {
                //调用java的WebService服务
                JZJW.Winder.BLL.wsf2.GlobalForcastRainDataServerClient ws2 = new wsf2.GlobalForcastRainDataServerClient();
                //新接口
                string jsonStr = ws2.QueryHistoryForcastData(minLongitude, maxLongitude, minLatitude, maxLatitude, origin, strsDate, streDate);
                DateTime dtn = DateTime.Now;
                TimeSpan span = (TimeSpan)(dtn - n);
                Double second = span.TotalSeconds;
                return parseRainDataJson2(jsonStr);
            }
            catch
            {
                DateTime dtn = DateTime.Now;
                TimeSpan span = (TimeSpan)(dtn - n);
                Double second = span.TotalSeconds;
                string sql = "insert into AREN_GETRAINbug(SID,N_stime,N_etime,N_tspan,XMIN,XMAX,YMIN,YMAX,stime,etime) values('" + this.sid + "',to_date('" + n + "','yyyy-mm-dd hh24:mi:ss')," +
                   "to_date('" + dtn + "','yyyy-mm-dd hh24:mi:ss')," + second + "," + minLongitude + "," + maxLongitude + "," + minLatitude + "," + maxLatitude +
                   ",to_date('" + sDate + "','yyyy-mm-dd hh24:mi:ss')," + "to_date('" + eDate + "','yyyy-mm-dd hh24:mi:ss'))";
                DbHelper.ExecuteSql(sql);
                return null;
            }
        }

        //ws2.QueryForcastRainDataByRange();
        /// <summary>
        /// 解析降雨数据结构
        /// </summary>
        /// <param name="jsonStr">json字符串</param>
        /// <returns></returns>
        public DataTable parseRainDataJson(string jsonStr)
        {
            //测试
            //string jsonStr = "[{\"origin\": \"ecmwf\", \"data\": [{\"latitude\": 44.125, \"data\": [[{\"rain\": 0.0, \"datetime\": \"2014060100\"}, {\"rain\": 0.0, \"datetime\": \"2014060106\"}, {\"rain\": 0.0, \"datetime\": \"2014060112\"}, {\"rain\": 0.0, \"datetime\": \"2014060118\"}], [{\"rain\": 0.83294511, \"datetime\": \"2014060200\"}, {\"rain\": 0.0, \"datetime\": \"2014060206\"}, {\"rain\": 0.0, \"datetime\": \"2014060212\"}, {\"rain\": 0.090048119, \"datetime\": \"2014060218\"}]], \"longitude\": 102.125}, {\"latitude\": 44.375, \"data\": [[{\"rain\": 0.0, \"datetime\": \"2014060100\"}, {\"rain\": 0.0, \"datetime\": \"2014060106\"}, {\"rain\": 0.0, \"datetime\": \"2014060112\"}, {\"rain\": 0.0, \"datetime\": \"2014060118\"}], [{\"rain\": 0.83294511, \"datetime\": \"2014060200\"}, {\"rain\": 0.0, \"datetime\": \"2014060206\"}, {\"rain\": 0.0, \"datetime\": \"2014060212\"}, {\"rain\": 0.090048119, \"datetime\": \"2014060218\"}]], \"longitude\": 102.125}, {\"latitude\": 44.125, \"data\": [[{\"rain\": 0.0, \"datetime\": \"2014060100\"}, {\"rain\": 0.0, \"datetime\": \"2014060106\"}, {\"rain\": 0.0, \"datetime\": \"2014060112\"}, {\"rain\": 0.0, \"datetime\": \"2014060118\"}], [{\"rain\": 0.83294511, \"datetime\": \"2014060200\"}, {\"rain\": 0.0, \"datetime\": \"2014060206\"}, {\"rain\": 0.0, \"datetime\": \"2014060212\"}, {\"rain\": 0.090048119, \"datetime\": \"2014060218\"}]], \"longitude\": 102.375}, {\"latitude\": 44.375, \"data\": [[{\"rain\": 0.0, \"datetime\": \"2014060100\"}, {\"rain\": 0.0, \"datetime\": \"2014060106\"}, {\"rain\": 0.0, \"datetime\": \"2014060112\"}, {\"rain\": 0.0, \"datetime\": \"2014060118\"}], [{\"rain\": 0.83294511, \"datetime\": \"2014060200\"}, {\"rain\": 0.0, \"datetime\": \"2014060206\"}, {\"rain\": 0.0, \"datetime\": \"2014060212\"}, {\"rain\": 0.090048119, \"datetime\": \"2014060218\"}]], \"longitude\": 102.375}], \"precision\": 0.25, \"timeSpan\": \"6\"}]";
            if (string.IsNullOrEmpty(jsonStr))
            {
                return null;
            }
            DataTable datas = new DataTable();
            //解析返回的Json结构
            //机构名称
            string rstorigin = "";
            //时间间隔 小时
            string rsttimeSpan = "";
            //精度
            string rstprecision = "";
            //经度
            string rstlongitude = "";
            //纬度
            string rstlatitude = "";
            //降雨量
            string rstrain = "";
            //降雨时间
            string rstdatetime = "";
            //构造存放数据的DataTable
            //int id, string rain, string datetime, string timespan, string origin

            datas.Columns.Add("ID", Type.GetType("System.Int32"));
            datas.Columns.Add("YEAR", Type.GetType("System.Int32"));
            datas.Columns.Add("MONTH", Type.GetType("System.Int32"));
            datas.Columns.Add("DAY", Type.GetType("System.Int32"));
            datas.Columns.Add("HOURS", Type.GetType("System.Int32"));
            datas.Columns.Add("HOURE", Type.GetType("System.Int32"));
            datas.Columns.Add("P", Type.GetType("System.Double"));
            datas.Columns.Add("ORIGIN", Type.GetType("System.String"));

            jsonStr = "{\"rst\":" + jsonStr + "}";
            JsonProperty tmpJsonproperty;
            JsonProperty tmpjpItem;
            JsonObject resultsJsonObj = new JsonObject(jsonStr);
            JsonProperty jsonProperty = resultsJsonObj["rst"];
            for (int i = 0; i < jsonProperty.Items.Count; i++)
            {
                //每个机构数据
                JsonProperty jp = jsonProperty.Items[i];
                if (jp != null)
                {
                    //机构名称，时间跨度，精度，data
                    JsonProperty jpItem = jp["origin"];
                    if (jpItem != null) { rstorigin = jpItem.Value; }
                    jpItem = jp["timeSpan"];
                    if (jpItem != null) { rsttimeSpan = jpItem.Value; }
                    jpItem = jp["precision"];
                    if (jpItem != null) { rstprecision = jpItem.Value; }
                    jpItem = jp["data"];
                    if (jpItem != null)
                    {
                        for (int j = 0; j < jpItem.Items.Count; j++)
                        {
                            //每个机构内的经度，纬度，data
                            JsonProperty jpdata = jpItem.Items[j];
                            if (jpdata != null)
                            {
                                JsonProperty jpdataItem = jpdata["longitude"];
                                if (jpdataItem != null) { rstlongitude = jpdataItem.Value; }
                                jpdataItem = jpdata["latitude"];
                                if (jpdataItem != null) { rstlatitude = jpdataItem.Value; }
                                jpdataItem = jpdata["data"];
                                //根据经纬度构造ID
                                //int id = Convert.ToInt32(((90 -Convert.ToDouble(rstlatitude)) * 4 - 125 + 1) * 10000 +Convert.ToDouble(rstlongitude) * 4);
                                //Math.Round(0.5,MidpointRounding.AwayFromZero)  四舍六入五凑偶
                                int id = Convert.ToInt32(Math.Round(((90 - Convert.ToDouble(rstlatitude)) * 4 - 125 + 1) * 10000 + Convert.ToDouble(rstlongitude) * 4, MidpointRounding.AwayFromZero));
                                if (id == 1075414)
                                {
                                    string jingdu = rstlongitude;
                                    string weidu = rstlatitude;
                                }
                                if (jpdataItem != null)
                                {
                                    //降雨量，时间
                                    for (int k = 0; k < jpdataItem.Items.Count; k++)
                                    {
                                        JsonProperty jprain = jpdataItem.Items[k];
                                        for (int l = 0; l < jprain.Items.Count; l++)
                                        {
                                            tmpJsonproperty = jprain.Items[l];
                                            tmpjpItem = tmpJsonproperty["rain"];
                                            if (tmpjpItem != null) { rstrain = tmpjpItem.Value; }
                                            tmpjpItem = tmpJsonproperty["datetime"];
                                            if (tmpjpItem != null) { rstdatetime = tmpjpItem.Value; }
                                            //测试rainData is null
                                            if (rstrain == null)
                                            {
                                                string sql = "insert into AREN_GETRAINNULL(SID,X,Y,XYID,stime,jsonstr) values('" + this.sid + "'," + rstlongitude + "," + rstlatitude + "," + id + "," +
                                                    "to_date('" + rstdatetime + "','yyyy-mm-dd hh24:mi:ss'),'" + jsonStr + "')";
                                                DbHelper.ExecuteSql(sql);
                                            }
                                            //获取webservice服务后构造降雨数据
                                            createRainData(id, rstrain, rstdatetime, rsttimeSpan, rstorigin, datas);

                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return datas;
        }

        //解析降雨数据结构2
        public DataTable parseRainDataJson2(string jsonStr)
        {
            //string jsonStr = "[{\"origin\": \"ecmwf\", \"data\": [{\"latitude\": 44.125, \"data\": [[{\"rain\": 0.0, \"datetime\": \"2014060100\"}, {\"rain\": 0.0, \"datetime\": \"2014060106\"}, {\"rain\": 0.0, \"datetime\": \"2014060112\"}, {\"rain\": 0.0, \"datetime\": \"2014060118\"}], [{\"rain\": 0.83294511, \"datetime\": \"2014060200\"}, {\"rain\": 0.0, \"datetime\": \"2014060206\"}, {\"rain\": 0.0, \"datetime\": \"2014060212\"}, {\"rain\": 0.090048119, \"datetime\": \"2014060218\"}]], \"longitude\": 102.125}, {\"latitude\": 44.375, \"data\": [[{\"rain\": 0.0, \"datetime\": \"2014060100\"}, {\"rain\": 0.0, \"datetime\": \"2014060106\"}, {\"rain\": 0.0, \"datetime\": \"2014060112\"}, {\"rain\": 0.0, \"datetime\": \"2014060118\"}], [{\"rain\": 0.83294511, \"datetime\": \"2014060200\"}, {\"rain\": 0.0, \"datetime\": \"2014060206\"}, {\"rain\": 0.0, \"datetime\": \"2014060212\"}, {\"rain\": 0.090048119, \"datetime\": \"2014060218\"}]], \"longitude\": 102.125}, {\"latitude\": 44.125, \"data\": [[{\"rain\": 0.0, \"datetime\": \"2014060100\"}, {\"rain\": 0.0, \"datetime\": \"2014060106\"}, {\"rain\": 0.0, \"datetime\": \"2014060112\"}, {\"rain\": 0.0, \"datetime\": \"2014060118\"}], [{\"rain\": 0.83294511, \"datetime\": \"2014060200\"}, {\"rain\": 0.0, \"datetime\": \"2014060206\"}, {\"rain\": 0.0, \"datetime\": \"2014060212\"}, {\"rain\": 0.090048119, \"datetime\": \"2014060218\"}]], \"longitude\": 102.375}, {\"latitude\": 44.375, \"data\": [[{\"rain\": 0.0, \"datetime\": \"2014060100\"}, {\"rain\": 0.0, \"datetime\": \"2014060106\"}, {\"rain\": 0.0, \"datetime\": \"2014060112\"}, {\"rain\": 0.0, \"datetime\": \"2014060118\"}], [{\"rain\": 0.83294511, \"datetime\": \"2014060200\"}, {\"rain\": 0.0, \"datetime\": \"2014060206\"}, {\"rain\": 0.0, \"datetime\": \"2014060212\"}, {\"rain\": 0.090048119, \"datetime\": \"2014060218\"}]], \"longitude\": 102.375}], \"precision\": 0.25, \"timeSpan\": \"6\"}]";
            if (string.IsNullOrEmpty(jsonStr))
            {
                return null;
            }
            DataTable datas = new DataTable();
            //解析返回的Json结构
            //机构名称
            string rstorigin = "";
            //时间间隔 小时
            string rsttimeSpan = "6";
            //精度
            string rstprecision = "";
            //经度
            string rstlongitude = "";
            //纬度
            string rstlatitude = "";
            //降雨年月日
            string rstdate = "";
            //降雨小时
            string rstdatehour = "";
            //降雨时间
            string rstdatetime = "";

            datas.Columns.Add("ID", Type.GetType("System.Int32"));
            datas.Columns.Add("YEAR", Type.GetType("System.Int32"));
            datas.Columns.Add("MONTH", Type.GetType("System.Int32"));
            datas.Columns.Add("DAY", Type.GetType("System.Int32"));
            datas.Columns.Add("HOURS", Type.GetType("System.Int32"));
            datas.Columns.Add("HOURE", Type.GetType("System.Int32"));
            datas.Columns.Add("P", Type.GetType("System.Double"));
            datas.Columns.Add("ORIGIN", Type.GetType("System.String"));

            JsonObject resultsJsonObj = new JsonObject(jsonStr);
            JsonProperty jsonProperty = resultsJsonObj["Origins"];
            for (int i = 0; i < jsonProperty.Items.Count; i++)
            {
                //每个机构数据
                JsonProperty jp = jsonProperty.Items[i];
                if (jp != null)
                {
                    //机构名称，时间跨度，精度，data
                    JsonProperty jpItem = jp["Origin"];
                    //01机构名称
                    if (jpItem != null) { rstorigin = jpItem.Value; }
                    jpItem = jp["DateStrs"];
                    if (jpItem != null)
                    {
                        for (int j = 0; j < jpItem.Items.Count; j++)
                        {
                            //日期
                            JsonProperty jpdata = jpItem.Items[j];
                            if (jpdata != null)
                            {
                                //02年月日
                                JsonProperty jpdataItem = jpdata["DateStr"];
                                if (jpdataItem != null) { rstdate = jpdataItem.Value; }
                                jpdataItem = jpdata["TimeSpans"];
                                if (jpdataItem != null)
                                {
                                    for (int k = 0; k < jpdataItem.Items.Count; k++)
                                    {
                                        //时间 小时
                                        JsonProperty jpdatatime = jpdataItem.Items[k];
                                        if (jpdatatime != null)
                                        {
                                            JsonProperty jpdataTimeItem = jpdatatime["TimeStr"];
                                            //03
                                            if (jpdataTimeItem != null) { rstdatehour = jpdataTimeItem.Value; }
                                            jpdataTimeItem = jpdatatime["Datas"];
                                            if (jpdataTimeItem != null)
                                            {
                                                for (int m = 0; m < jpdataTimeItem.Items.Count; m++)
                                                {
                                                    //Value
                                                    JsonProperty jpdatatimeDats = jpdataTimeItem.Items[m];
                                                    if (jpdatatimeDats != null)
                                                    {
                                                        //经度  纬度  降雨量
                                                        JsonProperty jpdataTimeItemDataValue = jpdatatimeDats["Lon"];
                                                        rstlongitude = jpdataTimeItemDataValue.Value;
                                                        jpdataTimeItemDataValue = jpdatatimeDats["Lat"];
                                                        rstlatitude = jpdataTimeItemDataValue.Value;
                                                        jpdataTimeItemDataValue = jpdatatimeDats["Value"];
                                                        rstprecision = jpdataTimeItemDataValue.Value;

                                                        //Math.Round(0.5,MidpointRounding.AwayFromZero)  四舍六入五凑偶
                                                        int id = Convert.ToInt32(Math.Round(((90 - Convert.ToDouble(rstlatitude)) * 4 - 125 + 1) * 10000 + Convert.ToDouble(rstlongitude) * 4, MidpointRounding.AwayFromZero));
                                                        rstdatetime = rstdate + rstdatehour;
                                                        //测试rainData is null
                                                        if (rstprecision == null)
                                                        {
                                                            string sql = "insert into AREN_GETRAINNULL(SID,X,Y,XYID,stime,jsonstr) values('" + this.sid + "'," + rstlongitude + "," + rstlatitude + "," + id + "," +
                                                            "to_date('" + rstdatetime + "','yyyy-mm-dd hh24:mi:ss'),'" + jsonStr + "')";
                                                            DbHelper.ExecuteSql(sql);
                                                        }
                                                        else
                                                        {
                                                            //获取webservice服务后构造降雨数据
                                                            createRainData(id, rstprecision, rstdatetime, rsttimeSpan, rstorigin, datas);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return datas;
        }
        //解析沙溪河区域数据
        public static DataTable ParseRainData(string jsonStr, DateTime dtnow, DateTime dts, DataTable dtID)
        {
            //需要修改的时 要根据 数据库中当前方案的雨量站id号获取对应的数据？？？？？？？？？？
            DataTable datas = new DataTable();
            //构造存放数据的DataTable
            datas.Columns.Add("ID", Type.GetType("System.String"));
            datas.Columns.Add("YEAR", Type.GetType("System.Int32"));
            datas.Columns.Add("MONTH", Type.GetType("System.Int32"));
            datas.Columns.Add("DAY", Type.GetType("System.Int32"));
            datas.Columns.Add("HOURS", Type.GetType("System.Double"));
            datas.Columns.Add("HOURE", Type.GetType("System.Double"));
            datas.Columns.Add("P", Type.GetType("System.Double"));

            DataRow dr;
            string id = "";
            DateTime dtEnd = DateTime.Now; ;
            DateTime dtStart;
            double DRP = 0;
            double INTV = 0;

            JsonObject resultsJsonObj = new JsonObject("{\"rst\":" + jsonStr + "}");
            JsonProperty jsonProperty = resultsJsonObj["rst"];
            int cnt = jsonProperty.Count;
            for (int j = 0; j < cnt; j++)
            {
                JsonProperty jp = jsonProperty.Items[j];
                if (jp != null)
                {
                    JsonProperty jpItem = jp["STCD"];
                    if (jpItem != null) { id = jpItem.Value.Trim(); }
                    jpItem = jp["TM"];
                    if (jpItem != null) { dtEnd = DateTime.ParseExact(jpItem.Value, "yyyy/MM/ddHH:mm:ss", new System.Globalization.CultureInfo("zh-CN")); }
                    jpItem = jp["DRP"];
                    if (jpItem != null) { DRP = Convert.ToDouble(jpItem.Value); }
                    jpItem = jp["INTV"];
                    if (jpItem != null) { INTV = Convert.ToDouble(jpItem.Value); }
                    dtStart = dtEnd.AddHours(-(INTV));

                    dr = datas.NewRow();
                    dr["ID"] = id;
                    dr["YEAR"] = dtStart.Year;
                    dr["MONTH"] = dtStart.Month; ;
                    dr["DAY"] = dtStart.Day;
                    dr["HOURS"] = dtStart.Hour;// +Convert.ToDouble(dtprior.Minute) / 60 + Convert.ToDouble(dtprior.Second) / 3600;
                    dr["HOURE"] = dtEnd.Hour;// +Convert.ToDouble(dt.Minute) / 60 + Convert.ToDouble(dt.Second) / 3600;
                    dr["P"] = DRP;
                    datas.Rows.Add(dr);
                }
            }
            return datas;
        }

        /// <summary>
        /// 解析XML，返回DataTable
        /// </summary>
        /// <param name="doc">xml数据文件</param>
        /// <param name="dtnow">获取数据的当前时间</param>
        /// <param name="preheat">预热期</param>
        /// <returns></returns>
        public static DataTable ParseXML(XmlDocument doc, DateTime dtnow, DateTime dts, DataTable dtID)
        {
            //需要修改的时 要根据 数据库中当前方案的雨量站id号获取对应的数据？？？？？？？？？？
            DataTable datas = new DataTable();
            //构造存放数据的DataTable
            datas.Columns.Add("ID", Type.GetType("System.Int32"));
            //datas.Columns.Add("NAME", Type.GetType("System.String"));
            datas.Columns.Add("YEAR", Type.GetType("System.Int32"));
            datas.Columns.Add("MONTH", Type.GetType("System.Int32"));
            datas.Columns.Add("DAY", Type.GetType("System.Int32"));
            datas.Columns.Add("HOURS", Type.GetType("System.Double"));
            datas.Columns.Add("HOURE", Type.GetType("System.Double"));
            datas.Columns.Add("P", Type.GetType("System.Double"));

            DataRow dr;
            string pid = "";
            string name = "";
            DateTime dt;
            double p = 0;
            DateTime dtprior = DateTime.Now;
            double pprior = 0;
            //DateTime dts =dtnow.AddDays(preheat * (-1));
            int i = 0;
            //挨个查找其下站点pid 和 name 
            foreach (XmlElement childElement in doc.DocumentElement)
            {
                if (childElement.Name == "data")
                {
                    pid = childElement.GetAttribute("pid");
                    name = childElement.GetAttribute("name");
                    i = 0;
                    //判断ID是否存在此方案中
                    if (dtID.Select("ID=" + pid).Length != 1)
                        continue;
                    foreach (XmlNode grandsonNode in childElement.ChildNodes)
                    {
                        //站点的 时间 雨量
                        dt = Convert.ToDateTime(((XmlElement)grandsonNode).GetAttribute("time"));
                        p = Convert.ToDouble(grandsonNode.InnerXml);
                        if (dt >= dts)
                        {
                            if (i == 0)
                            {
                                dtprior = dt;
                                pprior = Convert.ToDouble(grandsonNode.InnerXml);
                            }
                            else
                            {
                                if (p > pprior)
                                {
                                    dr = datas.NewRow();
                                    dr["ID"] = Convert.ToInt32(pid);
                                    dr["YEAR"] = dtprior.Year;
                                    dr["MONTH"] = dtprior.Month; ;
                                    dr["DAY"] = dtprior.Day;
                                    dr["HOURS"] = dtprior.Hour + Convert.ToDouble(dtprior.Minute) / 60 + Convert.ToDouble(dtprior.Second) / 3600;
                                    dr["HOURE"] = dt.Hour + Convert.ToDouble(dt.Minute) / 60 + Convert.ToDouble(dt.Second) / 3600;
                                    dr["P"] = p - pprior;
                                    //dr["NAME"] = name;
                                    datas.Rows.Add(dr);
                                    //入库数据
                                    /*2015-04-18  22:00:00  1600
                                      2015-04-18  23:25:00  1601
                                      2015-04-19  00:50:00  1602
                                     
                                    if (dt.Day == dtprior.Day && dt.Hour > dtprior.Hour)
                                    {
                                       
                                    }
                                    else
                                    {

                                    }
                                    */
                                    dtprior = dt;
                                    pprior = p;
                                }
                            }
                            i++;
                        }
                    }
                }
            }
            return datas;
        }
        /// <summary>
        ///  httpservice GET请求与获取服务结果
        /// </summary>
        /// <param name="Url">服务的URL地址 不带？</param>
        /// <param name="postDataStr">请求的参数</param>
        /// <returns></returns>
        public static string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();
            return retString;
        }
        /// <summary>
        /// 将真实出流数据插入到数据库 批量插入
        /// </summary>
        /// <param name="dttmp"></param>
        /// <param name="orauser"></param>
        /// <param name="origin"></param>
        /// <param name="rowcounts"></param>
        public static void insertRQoutput(DataTable dttmp, string orauser, int rowcounts = 100000)
        {
            OracleParameter[] parameters = {
					new OracleParameter(":SID",OracleDbType.NVarchar2),	
					new OracleParameter(":STCD",OracleDbType.NVarchar2),
					new OracleParameter(":STIME",OracleDbType.Date),
					new OracleParameter(":Q",OracleDbType.Double)};

            Dictionary<string, object> datas = new Dictionary<string, object>();
            //定义存放数据的数组 注意效率问题
            ArrayList SID = new ArrayList(dttmp.Rows.Count);
            ArrayList STCD = new ArrayList(dttmp.Rows.Count);
            ArrayList STIME = new ArrayList(dttmp.Rows.Count);
            ArrayList Q = new ArrayList(dttmp.Rows.Count);

            int segmentSize = 0;
            //选择指定结构的数据
            foreach (DataRow dc in dttmp.Rows)
            {
                SID.Add(dc["SID"]);
                STCD.Add(dc["STCD"]);
                STIME.Add(dc["STIME"]);
                Q.Add(dc["Q"]);

                ++segmentSize;
                if (segmentSize == rowcounts)
                {
                    //执行插入
                    datas.Add("SID", SID.ToArray());
                    datas.Add("STCD", STCD.ToArray());
                    datas.Add("STIME", STIME.ToArray());
                    datas.Add("Q", Q.ToArray());

                    DbHelper.BatchInsertNew("hgmgr.realqoutput", datas, segmentSize, parameters);
                    //清空
                    SID.Clear();
                    STCD.Clear();
                    STIME.Clear();
                    Q.Clear();

                }
            }
            if (segmentSize > 0)
            {
                //执行插入
                datas.Add("SID", SID.ToArray());
                datas.Add("STCD", STCD.ToArray());
                datas.Add("STIME", STIME.ToArray());
                datas.Add("Q", Q.ToArray());
                DbHelper.BatchInsertNew("hgmgr.realqoutput", datas, segmentSize, parameters);
            }
        }
        public static void insertRRainData_sxh(DataTable dttmp, string orauser, int rowcounts = 100000)
        {
            OracleParameter[] parameters = {
					new OracleParameter(":ID",OracleDbType.Varchar2),	
					new OracleParameter(":YEAR",OracleDbType.Int32),
					new OracleParameter(":MONTH",OracleDbType.Int32),
					new OracleParameter(":DAY",OracleDbType.Int32),
					new OracleParameter(":HOURS",OracleDbType.Double),					
					new OracleParameter(":HOURE",OracleDbType.Double),					
					new OracleParameter(":P",OracleDbType.Double)};

            Dictionary<string, object> datas = new Dictionary<string, object>();
            //定义存放数据的数组 注意效率问题
            ArrayList ID = new ArrayList(dttmp.Rows.Count);
            ArrayList YEAR = new ArrayList(dttmp.Rows.Count);
            ArrayList MONTH = new ArrayList(dttmp.Rows.Count);
            ArrayList DAY = new ArrayList(dttmp.Rows.Count);
            ArrayList HOURS = new ArrayList(dttmp.Rows.Count);
            ArrayList HOURE = new ArrayList(dttmp.Rows.Count);
            ArrayList P = new ArrayList(dttmp.Rows.Count);
            int segmentSize = 0;
            //选择指定结构的数据
            foreach (DataRow dc in dttmp.Rows)
            {
                ID.Add(dc["ID"]);
                YEAR.Add(dc["YEAR"]);
                MONTH.Add(dc["MONTH"]);
                DAY.Add(dc["DAY"]);
                HOURS.Add(dc["HOURS"]);
                HOURE.Add(dc["HOURE"]);
                P.Add(dc["P"]);
                ++segmentSize;
                if (segmentSize == rowcounts)
                {
                    //执行插入
                    datas.Add("ID", ID.ToArray());
                    datas.Add("YEAR", YEAR.ToArray());
                    datas.Add("MONTH", MONTH.ToArray());
                    datas.Add("DAY", DAY.ToArray());
                    datas.Add("HOURS", HOURS.ToArray());
                    datas.Add("HOURE", HOURE.ToArray());
                    datas.Add("P", P.ToArray());
                    DbHelper.BatchInsertNew(orauser + ".rainhour", datas, segmentSize, parameters);
                    //清空
                    ID.Clear();
                    YEAR.Clear();
                    MONTH.Clear();
                    DAY.Clear();
                    HOURS.Clear();
                    HOURE.Clear();
                    P.Clear();
                }
            }
            if (segmentSize > 0)
            {
                //执行插入
                datas.Add("ID", ID.ToArray());
                datas.Add("YEAR", YEAR.ToArray());
                datas.Add("MONTH", MONTH.ToArray());
                datas.Add("DAY", DAY.ToArray());
                datas.Add("HOURS", HOURS.ToArray());
                datas.Add("HOURE", HOURE.ToArray());
                datas.Add("P", P.ToArray());
                DbHelper.BatchInsertNew(orauser + ".rainhour", datas, segmentSize, parameters);
            }
        }
        /// <summary>
        /// 将真实数据插入到数据库 批量插入
        /// </summary>
        /// <param name="dttmp"></param>
        /// <param name="orauser"></param>
        /// <param name="origin"></param>
        /// <param name="rowcounts"></param>
        public static void insertRRainData(DataTable dttmp, string orauser, int rowcounts = 100000)
        {
            OracleParameter[] parameters = {
					new OracleParameter(":ID",OracleDbType.Int32),	
					new OracleParameter(":YEAR",OracleDbType.Int32),
					new OracleParameter(":MONTH",OracleDbType.Int32),
					new OracleParameter(":DAY",OracleDbType.Int32),
					new OracleParameter(":HOURS",OracleDbType.Double),					
					new OracleParameter(":HOURE",OracleDbType.Double),					
					new OracleParameter(":P",OracleDbType.Double)};

            Dictionary<string, object> datas = new Dictionary<string, object>();
            //定义存放数据的数组 注意效率问题
            ArrayList ID = new ArrayList(dttmp.Rows.Count);
            ArrayList YEAR = new ArrayList(dttmp.Rows.Count);
            ArrayList MONTH = new ArrayList(dttmp.Rows.Count);
            ArrayList DAY = new ArrayList(dttmp.Rows.Count);
            ArrayList HOURS = new ArrayList(dttmp.Rows.Count);
            ArrayList HOURE = new ArrayList(dttmp.Rows.Count);
            ArrayList P = new ArrayList(dttmp.Rows.Count);
            int segmentSize = 0;
            //选择指定结构的数据
            foreach (DataRow dc in dttmp.Rows)
            {
                ID.Add(dc["ID"]);
                YEAR.Add(dc["YEAR"]);
                MONTH.Add(dc["MONTH"]);
                DAY.Add(dc["DAY"]);
                HOURS.Add(dc["HOURS"]);
                HOURE.Add(dc["HOURE"]);
                P.Add(dc["P"]);
                ++segmentSize;
                if (segmentSize == rowcounts)
                {
                    //执行插入
                    datas.Add("ID", ID.ToArray());
                    datas.Add("YEAR", YEAR.ToArray());
                    datas.Add("MONTH", MONTH.ToArray());
                    datas.Add("DAY", DAY.ToArray());
                    datas.Add("HOURS", HOURS.ToArray());
                    datas.Add("HOURE", HOURE.ToArray());
                    datas.Add("P", P.ToArray());
                    DbHelper.BatchInsertNew(orauser + ".rainhour", datas, segmentSize, parameters);
                    //清空
                    ID.Clear();
                    YEAR.Clear();
                    MONTH.Clear();
                    DAY.Clear();
                    HOURS.Clear();
                    HOURE.Clear();
                    P.Clear();
                }
            }
            if (segmentSize > 0)
            {
                //执行插入
                datas.Add("ID", ID.ToArray());
                datas.Add("YEAR", YEAR.ToArray());
                datas.Add("MONTH", MONTH.ToArray());
                datas.Add("DAY", DAY.ToArray());
                datas.Add("HOURS", HOURS.ToArray());
                datas.Add("HOURE", HOURE.ToArray());
                datas.Add("P", P.ToArray());
                DbHelper.BatchInsertNew(orauser + ".rainhour", datas, segmentSize, parameters);
            }
        }

        /// <summary>
        /// 获取webservice服务后构造降雨数据
        /// </summary>
        /// <param name="id">由经纬度构造</param>
        /// <param name="rain">降雨量</param>
        /// <param name="datetime">开始时间</param>
        /// <param name="timespan">时间跨度</param>
        /// <param name="origin">机构名称</param>
        /// <returns></returns>
        public static void createRainData(int id, string rain, string datetime, string timespan, string origin, DataTable datas)
        {
            //构造ID,year,month,day,hours,houre,p
            //int id = Convert.ToInt32(((90 - y) * 4 - 125 + 1) * 10000 + x * 4);
            //2014060206
            DateTime dts = DateTime.ParseExact(datetime, "yyyyMMddHH", null, System.Globalization.DateTimeStyles.AllowWhiteSpaces); //Parse(datetime + "0000",);
            int year = dts.Year;
            int month = dts.Month;
            int day = dts.Day;
            int hours = dts.Hour;
            DateTime dte = dts.AddHours(Convert.ToDouble(timespan));
            int houre = dte.Hour;

            DataRow dr = datas.NewRow();
            dr["ID"] = id;
            dr["YEAR"] = year;
            dr["MONTH"] = month;
            dr["DAY"] = day;
            dr["HOURS"] = hours;
            dr["HOURE"] = houre;
            //rain存在null值
            if (rain != null)
                dr["P"] = rain;
            else
                dr["P"] = 0;
            dr["ORIGIN"] = origin;
            datas.Rows.Add(dr);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dttmp">存放数据的表格</param>
        /// <param name="rowcounts">每批次导入数据量</param>
        /// <param name="orauser">计算用户</param>
        /// <param name="parameters">oracle参数</param>
        public static void insertRainData(DataTable dttmp, string orauser, string origin, int rowcounts = 100000)
        {

            //raincmorph3h25km (select ID,year,month,day,hours,houre,p
            OracleParameter[] parameters = {
					new OracleParameter(":ID",OracleDbType.Int32),	
					new OracleParameter(":YEAR",OracleDbType.Int32),
					new OracleParameter(":MONTH",OracleDbType.Int32),
					new OracleParameter(":DAY",OracleDbType.Int32),
					new OracleParameter(":HOURS",OracleDbType.Int32),					
					new OracleParameter(":HOURE",OracleDbType.Int32),					
					new OracleParameter(":P",OracleDbType.Double)};

            Dictionary<string, object> datas = new Dictionary<string, object>();
            //定义存放数据的数组 注意效率问题
            ArrayList ID = new ArrayList(dttmp.Rows.Count);
            ArrayList YEAR = new ArrayList(dttmp.Rows.Count);
            ArrayList MONTH = new ArrayList(dttmp.Rows.Count);
            ArrayList DAY = new ArrayList(dttmp.Rows.Count);
            ArrayList HOURS = new ArrayList(dttmp.Rows.Count);
            ArrayList HOURE = new ArrayList(dttmp.Rows.Count);
            ArrayList P = new ArrayList(dttmp.Rows.Count);
            int segmentSize = 0;
            //选择指定结构的数据
            foreach (DataRow dc in dttmp.Select("ORIGIN='" + origin + "'"))
            {
                ID.Add(dc["ID"]);
                YEAR.Add(dc["YEAR"]);
                MONTH.Add(dc["MONTH"]);
                DAY.Add(dc["DAY"]);
                HOURS.Add(dc["HOURS"]);
                HOURE.Add(dc["HOURE"]);
                P.Add(dc["P"]);
                ++segmentSize;
                if (segmentSize == rowcounts)
                {
                    //执行插入
                    datas.Add("ID", ID.ToArray());
                    datas.Add("YEAR", YEAR.ToArray());
                    datas.Add("MONTH", MONTH.ToArray());
                    datas.Add("DAY", DAY.ToArray());
                    datas.Add("HOURS", HOURS.ToArray());
                    datas.Add("HOURE", HOURE.ToArray());
                    datas.Add("P", P.ToArray());
                    DbHelper.BatchInsertNew(orauser + ".raincmorph3h25km", datas, segmentSize, parameters);
                    //清空
                    ID.Clear();
                    YEAR.Clear();
                    MONTH.Clear();
                    DAY.Clear();
                    HOURS.Clear();
                    HOURE.Clear();
                    P.Clear();
                }
            }
            if (segmentSize > 0)
            {
                //执行插入
                datas.Add("ID", ID.ToArray());
                datas.Add("YEAR", YEAR.ToArray());
                datas.Add("MONTH", MONTH.ToArray());
                datas.Add("DAY", DAY.ToArray());
                datas.Add("HOURS", HOURS.ToArray());
                datas.Add("HOURE", HOURE.ToArray());
                datas.Add("P", P.ToArray());
                DbHelper.BatchInsertNew(orauser + ".raincmorph3h25km", datas, segmentSize, parameters);
            }
        }
        /// <summary>
        /// 创建计算Job
        /// </summary>
        /// <returns></returns>
        public string CreateJob(string SCCD, string username, string userpwd, string cores)
        {
            string str = "OK";
            //创建Job            
            IScheduler scheduler = new Scheduler();
            scheduler.Connect("192.168.0.7");
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

            //job.RequestedNodes.Add("HPC-CENTER");
            //修改
            job.RequestedNodes.Add("HPC-CLOUD");
            // 设置计算任务的属性
            job.Name = SCCD;  //
            task.Name = SCCD;
            task.Type = TaskType.Basic;
            task.MinimumNumberOfCores = Convert.ToInt32(cores);//默认的为4  参数 24
            task.MaximumNumberOfCores = Convert.ToInt32(cores);//参数
            task.WorkDirectory = @"\\HPC-CLOUD\GlobalF3S\HydroGlobal";
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
                //SimulationCompleteNon(orauser, sid, starttime);   
                SimulationCompleteNonAgency(orauser, sid, starttime, endtime);
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
        public Boolean SimulationCompleteNon(string orauser, string sid, string starttime)
        {
            Boolean rst = false;
            //03查找definednodes description 的第一条记录 ，然后获取器前三位数字
            string sql = "select description from " + orauser + ".definednodes where rownum<=1";
            string Code = DbHelper.GetSingle(sql).ToString();
            if (Code.Length > 0)
                Code = Code.Substring(0, 3);
            //04更新大洲编码

            //删除 SQL事务 要修改成SQL事务删除和插入同时进行
            //sql = "delete from discharge where SCCD='" + sid + "'";
            //DbHelper.ExecuteSql(sql);

            //将discharge表中数据拷贝到hydroglobal.discharge表中
            //REGIONIDNEW,ADDZERO两个字段为空值
            //模拟计算完成时间
            simetime = DateTime.Now;
            //01拷贝数据  还未修改
            /*
            sql = "insert into discharge(SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                  "B,H,V,SIMETIME) (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                  "B,H,V,to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss') from " + orauser + ".discharge)";
             */

            //只取开始时间到结束时间范围的数据，预热期的数据不取。
            DateTime dt = Convert.ToDateTime(starttime);
            double starthouroffset = dt.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours;
            sql = "insert into discharge(SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                 "B,H,V,SIMETIME) (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                 "B,H,V,to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss') from " + orauser + ".discharge where HOUROFFSET >=" + starthouroffset + ")";
            DbHelper.ExecuteSql(sql);
            //02更新字段
            sql = "update discharge set addzero='0' where mod(length(REGIONINDEX),3)=2 and SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            sql = "update discharge set addzero='00' where mod(length(REGIONINDEX),3)=1 and SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            sql = "update discharge set regionidnew=addzero||REGIONINDEX where SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);

            sql = "update discharge set regionidnew='" + Code + "'||addzero||regionindex where SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);

            //设置状态 空闲状态2014-04-27ByRen
            sql = "update orauserTable set state=3 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            //更新方案表 完成状态
            //sql = "update jzjw_schememgr set state=3,simetime=to_date('" +DateTime.Now+ "','yyyy-mm-dd hh24:mi:ss') where sid='" + sid + "'";
            sql = "update jzjw_schememgr set state=3,simetime=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss') where sid='" + sid + "' and SUBMITTIME=to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);

            //更新连续计算表
            //sql = "insert into hgmgr.schemecontsim(sid) values ('" + sid +"')";
            //sqlList.Add(sql);
            sql = "update hgmgr.schemecontsim set simuser='" + orauser + "'" + " where sid='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            //用户方案费用表
            sidprice(sid, simetime);
            rst = true;
            return rst;
        }

        /// <summary>
        /// 方案计算完成,拷贝数据和更新状态（未使用数据库事务）-20150411
        /// </summary>
        /// <param name="orauser"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public Boolean SimulationCompleteNonAgency(string orauser, string sid, string starttime, string endtime)
        {
            Boolean rst = false;
            //03查找definednodes description 的第一条记录 ，然后获取器前三位数字
            string sql = "select description from " + orauser + ".definednodes where rownum<=1";
            string Code = DbHelper.GetSingle(sql).ToString();
            if (Code.Length > 0)
                Code = Code.Substring(0, 3);
            //04更新大洲编码

            //删除 SQL事务 要修改成SQL事务删除和插入同时进行
            //sql = "delete from discharge where SCCD='" + sid + "'";
            //DbHelper.ExecuteSql(sql);

            //将discharge表中数据拷贝到hydroglobal.discharge表中
            //REGIONIDNEW,ADDZERO两个字段为空值
            //模拟计算完成时间
            simetime = DateTime.Now;
            //01拷贝数据  还未修改
            /*
            sql = "insert into discharge(SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                  "B,H,V,SIMETIME) (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                  "B,H,V,to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss') from " + orauser + ".discharge)";
             */


            //是否存数据 到hgcom.Status表中  -20150411
            //模拟开始时间 根据预热期重新设置开始和结束时间
            DateTime dtStart0 = Convert.ToDateTime(starttime).AddDays(ppreheat * (-1));
            DateTime dtStart1 = Convert.ToDateTime(starttime);
            if (iscontiune == true)
            {
                //double starthouroffset = dt.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours;
                sql = "select statusissave,statusissavefrom,statusissaveto from hgmgr.schemecontsim where sid='" + sid + "'";
                DataTable dtsave = DbHelper.Query(sql).Tables[0];
                if (dtsave != null && dtsave.Rows.Count == 1)
                {
                    if (dtsave.Rows[0]["statusissave"].ToString() == "1")
                    {
                        Double statusissavefrom = Convert.ToDouble(dtsave.Rows[0]["statusissavefrom"]);
                        Double statusissaveto = Convert.ToDouble(dtsave.Rows[0]["statusissaveto"]);

                        DateTime dtS0 = dtStart1.AddDays(statusissavefrom * (-1));
                        DateTime dtE0 = dtStart1.AddDays(statusissaveto * (-1));
                        //DateTime dtS0 = Convert.ToDateTime(starttime).AddDays(statusissavefrom*(-1));
                        //DateTime dtE0 = Convert.ToDateTime(starttime).AddDays(statusissaveto*(-1));
                        if (statusissavefrom == statusissaveto)
                        {
                            //先删除
                            sql = "delete from hgcom.status where sccd='" + sid + "' and " +
                                "(houroffset >= " + dtS0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + " and houroffset<" +
                                dtStart1.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + ")";
                            DbHelper.ExecuteSql(sql);
                            sql = "insert into hgcom.status (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,WLL,WLU,WLM,WLD," +
                           "WRL,WRU,WRM,WRD,WSL,WSU,WSM,WSD,E,P,SLOPEERO,CHANNELERO,GRAVITYERO from " + orauser + ".status where " +
                           "houroffset >= " + dtS0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + " and houroffset<" +
                           dtStart1.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + ")";
                        }
                        else
                        {
                            //先删除
                            sql = "delete from hgcom.status where sccd='" + sid + "' and " +
                                "(houroffset >= " + dtS0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + " and houroffset<" +
                                dtE0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + ")";
                            DbHelper.ExecuteSql(sql);
                            sql = "insert into hgcom.status (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,WLL,WLU,WLM,WLD," +
                             "WRL,WRU,WRM,WRD,WSL,WSU,WSM,WSD,E,P,SLOPEERO,CHANNELERO,GRAVITYERO from " + orauser + ".status where " +
                             "houroffset >= " + dtS0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + " and houroffset<" +
                             dtE0.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours + ")";
                        }
                        DbHelper.ExecuteSql(sql);
                    }
                }
            }
            //只取开始时间到结束时间范围的数据，预热期的数据不取。
            DateTime dt = Convert.ToDateTime(starttime);
            double starthouroffset = dt.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours;
            //修改  不删除数据，直接保存
            double endhouroffset = Convert.ToDateTime(endtime).Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours;
            //sql = "delete from hgcom.discharge where sccd='" + sid + "' and (HOUROFFSET >=" + starthouroffset + " and HOUROFFSET <=" + endhouroffset+")";
            //DbHelper.ExecuteSql(sql);
            //是否为连续模拟方案，若是则保留一天的数据 修改
            if (iscontiune == true)
                SimulationCompleteNonAgencyRst(orauser, sid, starttime, simetime, Code);
            sql = "insert into discharge(SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                 "B,H,V,SIMETIME) (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                 "B,H,V,to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss') from " + orauser + ".discharge where HOUROFFSET >=" + starthouroffset + ")";
            DbHelper.ExecuteSql(sql);
            //02更新字段
            sql = "update discharge set addzero='0' where mod(length(REGIONINDEX),3)=2 and SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            sql = "update discharge set addzero='00' where mod(length(REGIONINDEX),3)=1 and SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            sql = "update discharge set regionidnew=addzero||REGIONINDEX where SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);

            sql = "update discharge set regionidnew='" + Code + "'||addzero||regionindex where SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);

            //设置状态 空闲状态2014-04-27ByRen
            sql = "update orauserTable set state=3 where orauser='" + orauser + "'";
            DbHelper.ExecuteSql(sql);
            //更新方案表 完成状态
            //sql = "update jzjw_schememgr set state=3,simetime=to_date('" +DateTime.Now+ "','yyyy-mm-dd hh24:mi:ss') where sid='" + sid + "'";
            sql = "update jzjw_schememgr set state=3,simetime=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss') where sid='" + sid + "' and SUBMITTIME=to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);

            //更新连续计算表
            //sql = "insert into hgmgr.schemecontsim(sid) values ('" + sid +"')";
            //sqlList.Add(sql);
            sql = "update hgmgr.schemecontsim set simuser='" + orauser + "'" + " where sid='" + sid + "'";
            DbHelper.ExecuteSql(sql);
            //用户方案费用表
            sidprice(sid, simetime);
            rst = true;
            //统计计算结果
            sql = "select count(*) from " + orauser + ".raincmorph3h25km order by day,hours";

            string raincount = DbHelper.GetSingle(sql).ToString();
            sql = "insert into hgmgr.continuesubmitRST(SID,ORAUSER,STIME,ETIME,rainCount,INSERTTIME) values ('" + sid + "','" + orauser + "',to_date('" + starttime + "','yyyy-mm-dd hh24:mi:ss')," +
                "to_date('" + endtime + "','yyyy-mm-dd hh24:mi:ss')," + raincount + ",to_date('" + DateTime.Now.ToString() + "','yyyy-mm-dd hh24:mi:ss'))";
            DbHelper.ExecuteSql(sql);
            return rst;
        }
        /// <summary>
        /// 针对连续模拟计算的结果同时保留当前的数据到dischargecon表中
        /// </summary>
        /// <param name="orauser"></param>
        /// <param name="sid"></param>
        /// <param name="starttime"></param>
        /// <param name="simetime1"></param>
        /// <returns></returns>
        public Boolean SimulationCompleteNonAgencyRst(string orauser, string sid, string starttime, DateTime simetime1, string Code)
        {
            string sqltmp = "select stype from hgmgr.jzjw_scheme where sid='" + sid + "'";
            string stype = DbHelper.GetSingle(sqltmp).ToString();
            if (stype == "SIMCON")
            {
                //是连续模拟方案
                DateTime dt = Convert.ToDateTime(starttime);
                double starthouroffset = dt.Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours;
                double starthouroffset1 = (dt.AddDays(1)).Subtract(Convert.ToDateTime("1950-01-01 00:00:00")).TotalHours;
                sqltmp = "insert into dischargecon(SCCD,REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                "B,H,V,SIMETIME) (select '" + sid + "',REGIONINDEX,BSVALUE,BSLENGTH,HOUROFFSET,MINUTEOFFSET,QINPUT,QOUTPUT,SINPUT,SOUTPUT," +
                "B,H,V,to_date('" + simetime1 + "','yyyy-mm-dd hh24:mi:ss') from " + orauser + ".discharge where HOUROFFSET >=" + starthouroffset + " and HOUROFFSET <" + starthouroffset1 + ")";
                DbHelper.ExecuteSql(sqltmp);
                //02更新字段
                sql = "update dischargecon set addzero='0' where mod(length(REGIONINDEX),3)=2 and SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
                DbHelper.ExecuteSql(sql);
                sql = "update dischargecon set addzero='00' where mod(length(REGIONINDEX),3)=1 and SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
                DbHelper.ExecuteSql(sql);
                sql = "update dischargecon set regionidnew=addzero||REGIONINDEX where SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
                DbHelper.ExecuteSql(sql);

                sql = "update dischargecon set regionidnew='" + Code + "'||addzero||regionindex where SCCD='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
                DbHelper.ExecuteSql(sql);
            }
            return true;
        }
        /// <summary>
        /// 方案计算完成,拷贝数据和更新状态（事务机制）
        /// </summary>
        /// <param name="orauser"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        public Boolean SimulationComplete(string orauser, string sid)
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
            sql = "update hgmgr.schemecontsim set simuser='" + orauser + "'" + " where sid='" + sid + "'";
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
            sql = "update jzjw_schememgr set state=4 where sid='" + sid + "' and SUBMITTIME=to_date('" + submitDate + "','yyyy-mm-dd hh24:mi:ss')";
            DbHelper.ExecuteSql(sql);
            rst = "true";
            return rst;
        }
        public string sidprice(string sid, DateTime simetime)
        {

            //计算成功后向方案费用表中插入记录
            //01获取方案的用户名
            sql = "select username from hgmgr.jzjw_scheme where sid='" + sid + "'";
            object obj = DbHelper.GetSingle(sql);
            string username = Convert.ToString(obj);
            //获取用户级别 和级别名字
            sql = "select leveluserid from hgmgr.jzjw_user where username='" + username + "'";
            obj = DbHelper.GetSingle(sql);
            string leveluserid = Convert.ToString(obj);
            sql = "select levelname from hgcom.leveluser where leveluserid =(" + leveluserid + ")";
            obj = DbHelper.GetSingle(sql);
            string levelusername = Convert.ToString(obj);

            //02获取模拟计算开始时间，模拟计算完成时间，计算时长，核心数，
            string simstime = "";
            string cores = "";
            string levelsidid = "";
            sql = "select simstime,cores,levelsidid from jzjw_schememgr where sid='" + sid + "' and SIMETIME=to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            DataTable dt = DbHelper.Query(sql).Tables[0];
            if (dt != null && dt.Rows.Count == 1)
            {
                simstime = dt.Rows[0]["simstime"].ToString();
                cores = dt.Rows[0]["cores"].ToString();
                levelsidid = dt.Rows[0]["levelsidid"].ToString();
            }
            sql = "select levelname from hgcom.levelsid where levelsidid=(levelsidid)";
            obj = DbHelper.GetSingle(sql);
            string levelsidname = Convert.ToString(obj);
            //计算时常
            double timespan = simetime.Subtract(Convert.ToDateTime(simstime)).TotalMinutes;
            //03从discharge表中获取关注河段数，记录数
            sql = "select count(*) from (select distinct regionindex,bsvalue,bslength from hgcom.discharge where sccd='" + sid +
                "'and simetime =to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss'))";
            long rivercounts = Convert.ToInt64(DbHelper.GetSingle(sql));
            sql = "select count(*)/1000 from hgcom.discharge where sccd='" + sid + "' and simetime =to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')";
            double rowscounts = Convert.ToDouble(DbHelper.GetSingle(sql));
            //计算费用，折扣率，
            string upricecores = "";
            string upricediscount_c = "";

            sql = "select upricecores,upricediscount_c from upricecommit where leveluserid=" + leveluserid + " and levelsidid=" + levelsidid;
            dt = DbHelper.Query(sql).Tables[0];
            if (dt != null && dt.Rows.Count == 1)
            {
                upricecores = dt.Rows[0]["upricecores"].ToString();
                upricediscount_c = dt.Rows[0]["upricediscount_c"].ToString();
            }
            double total_c = Convert.ToInt64(cores) * Convert.ToInt64(upricecores) * timespan * Convert.ToDouble(upricediscount_c);
            //存储费用，折扣率
            string upricestorage = "";
            string upricediscount_s = "";
            sql = "select upricestorage,upricediscount_s from upricestorage where leveluserid=" + leveluserid + " and levelsidid=" + levelsidid;
            dt = DbHelper.Query(sql).Tables[0];
            if (dt != null && dt.Rows.Count == 1)
            {
                upricestorage = dt.Rows[0]["upricestorage"].ToString();
                upricediscount_s = dt.Rows[0]["upricediscount_s"].ToString();
            }
            double total_s = Convert.ToInt64(rowscounts) * Convert.ToInt64(upricestorage) * Convert.ToDouble(upricediscount_s);

            //USERNAME,SID,SIMSTIME,SIMETIME,MINUTEOFFSET,CORES,RIVERCOUNT,STORAGES,UPRICEDISCOUNT_C,UPRICEDISCOUNT_S,PRICE_C,PRICE_S ,PRICE_TOTAL,LEVELNAME_USER,LEVELNAME_SID
            sql = "insert into usersidprices values('" + username + "','" + sid + "',to_date('" + simstime + "','yyyy-mm-dd hh24:mi:ss')," +
                "to_date('" + simetime + "','yyyy-mm-dd hh24:mi:ss')," + timespan + "," + cores + "," + rivercounts + "," + rowscounts + "," +
                upricediscount_c + "," + upricediscount_s + "," + total_c + "," + total_s + "," + (total_s + total_c) + ",'" + levelusername + "','" + levelsidname + "')";
            DbHelper.ExecuteSql(sql);
            return "OK";
        }
    }
}
