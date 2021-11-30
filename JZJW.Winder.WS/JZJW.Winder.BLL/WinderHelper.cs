using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using JZJW.Winder.DAL;

namespace JZJW.Winder.BLL
{
    public class WinderHelper
    {
        private static string strConn = "Data Source=dwm474;User ID=hydroglobal;Password=hydroglobal474;";
        public static string GetLineChartData(string name, string regionid, string binstrlen, string binstrval, string reghighid)
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

        private static string GetLineChartVSection(string reghighid, string regionid, string binstrval, string binstrlen, string name)
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

        private static string GetLineChartData(string regionid, string bsvalue, string bslen, string name)
        {
            string str = "";
            string sql = "select to_char((to_date('1950-01-01 00:00:00','yyyy-mm-dd hh24:mi:ss')+houroffset/24),'yyyy-mm-dd HH24') as day ," + name + " as wh from discharge where regionindex=" + regionid + " and bsvalue=" + bsvalue + " and bslength=" + bslen; ;
            str = GetJson(DbHelper.Query(sql));
            return str;
        }

        private static string GetJson(DataSet ds)
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

        //orauser= hg02  ，userid=user usercode=用户密码

        public static string DoRiverSegCalc(string sid, string orauser, string userid, List<RiverSeg> rivers, string starttime, string endtime, string rainfalldata, string runoffmodel, string rivermodel, string usercode)
        {
            string str = "{\"msg\":\"ok\"}";
            string sql = "insert into schememgr(sid,orauser,userid,state,starttime,endtime,datashift）values('" + sid + "','" + orauser + "','" + userid + "',0,'" +
                   starttime + "','" + endtime + "',0)";
            DbHelper.ExecuteSql(sql);

            ///00
            sql = "truncate table riversegs;";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table discharge;";
            DbHelper.ExecuteSql(sql);
            sql = "truncate table regionconnection;";
            DbHelper.ExecuteSql(sql);

            int minRegionidnew = Int32.MaxValue, minBinstrlen = Int32.MaxValue;
            //01
            if (rivers != null && rivers.Count > 0)
            {
                foreach (RiverSeg rs in rivers)
                {
                    if (rs.regionidnew < minRegionidnew) minRegionidnew = rs.regionidnew;
                    if (rs.binstrlen < minBinstrlen) minBinstrlen = rs.binstrlen;

                    string rgin = "" + rs.regionidnew;
                    if (rgin.Length > 3) rgin = rgin.Substring(3);
                    sql = "insert into definednodes(regionindex,bsvalue,bslength,nodetype,description) values(" +
                        rgin + "," + rs.binstrval + "," + rs.binstrlen + ",'GS','" + rs.regionidnew + "')";
                    DbHelper.ExecuteSql(sql);
                }


            }

            int len = minRegionidnew.ToString().Length;

            //02
            sql = "insert into riversegs (select /*+ index( gdn_asia_river1) */ channelid,binstrlen,binstrval,999,regionidnew,strahler,src_area,lft_area,rgt_area,0,0,0,src_slope,lft_slope,rgt_slope,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regionid from gdn_asia_river1 " +
"where regionidnew=" + minRegionidnew + " and binstrlen>=" + minBinstrlen + " and 0=get_out_bsv(binstrval,binstrlen," + minBinstrlen + "))";
            DbHelper.ExecuteSql(sql);

            sql = "insert into riversegs (select /*+ index( gdn_asia_river1) */ channelid,binstrlen,binstrval,999,regionidnew,strahler,src_area,lft_area,rgt_area,0,0,0,src_slope,lft_slope,rgt_slope,src_len,lft_len,rgt_len," +
"chnl_len,chnl_slope,middlex,middley,upeleva,downeleva,upsubarea,0,0,0,0.6,0.07,0.08,30,0,0.3,2.7,regionid from gdn_asia_river1 where substr(regionidnew,1," + len + ")=" + minRegionidnew + " and  length(regionidnew)>" + len + ")";
            DbHelper.ExecuteSql(sql);
            sql = "update riversegs set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update riversegs set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);

            //03
            sql = "select distinct regionindex2 from riversegs where length(regionindex2)>" + len;
            DataSet ds = DbHelper.Query(sql);
            DataTable dt = ds.Tables[0];
            if (dt != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    int regionindex2 = Convert.ToInt32(dt.Rows[i]["regionindex2"]);
                    string tmp = regionindex2.ToString();
                    string newTmp = tmp.Substring(0, tmp.Length - 3);

                    sql = "select min(bslength) from riversegs where regionindex2=" + regionindex2;
                    int minbslength = Convert.ToInt32(DbHelper.GetSingle(sql));
                    sql = "insert into regionconnection (regiongrade,regionindex2,toregiondex2,tovalue,tolength,regionname,regionindex,toregionindex" +
 " values(1," + regionindex2 + "," + newTmp + "，0，" + (minbslength - 1) + "，'liuyu',0,0)";
                    DbHelper.ExecuteSql(sql);
                }
            }

            sql = "update regionconnection set regionindex=substr(regionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update regionconnection set toregionindex=substr(toregionindex2,4)";
            DbHelper.ExecuteSql(sql);
            sql = "update regionconnection set regiongrade=(length(regionindex2)-3)/3";
            DbHelper.ExecuteSql(sql);

            return str;
        }



    }
}
