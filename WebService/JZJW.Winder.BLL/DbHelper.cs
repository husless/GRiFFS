/*
 *  数据层
 *  文件名：OracleHelper.cs
 *
 *    版权所有 
 *
 *        描述：Oracle数据库操作助手类
 *
 *        作者：
 *
 *    创建日期：2013-4-28
 *
 *        版本：
 *
 *    参考文档：动软.net代码生成器
 *
 *    修订记录：
 *        修订日期    修订人    修订内容
 *        
 */

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using Oracle.DataAccess.Client;
using System.Collections;
using System.Data;
using System.Data.Services.Client;

namespace JZJW.Winder.BLL
{
    public class DbHelper
    {
        #region 从配置文件中获取数据库的连接字符串
        public static readonly String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["constr"].ToString();
        #endregion

        #region 私有方法
        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="cmdText"></param>
        /// <param name="cmdParms"></param>
        private static void PrepareCommand(OracleCommand cmd, OracleConnection conn, OracleTransaction trans, string cmdText, OracleParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (OracleParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }
        #endregion

        #region  执行不带参数的SQL语句

        /// <summary>
        /// 判断数据库中是否存在符合要求的数据（不带参数）
        /// </summary>
        /// <param name="strSql">Sql语句</param>
        /// <returns></returns>
        public static bool Exists(string strSql)
        {
            object obj = GetSingle(strSql);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (System.Data.OracleClient.OracleException E)
                    {
                        connection.Close();
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.OracleClient.OracleException e)
                    {
                        connection.Close();
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader ExecuteReader(string strSQL)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand(strSQL, connection);
            try
            {
                connection.Open();
                OracleDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (System.Data.OracleClient.OracleException e)
            {
                throw new Exception(e.Message);
            }


        }
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    OracleDataAdapter command = new OracleDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (System.Data.OracleClient.OracleException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                return ds;
            }
        }


        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql(string SQLString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (System.Data.OracleClient.OracleException E)
                    {
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 判断数据库中是否存在符合要求的数据（带参数）
        /// </summary>
        /// <param name="strSql">Sql语句</param>
        /// <param name="cmdParms">需插入sql语句中的参数</param>
        /// <returns></returns>
        public static bool Exists(string strSql, params OracleParameter[] cmdParms)
        {
            object obj = GetSingle(strSql, cmdParms);
            int cmdresult;
            if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
            {
                cmdresult = 0;
            }
            else
            {
                cmdresult = int.Parse(obj.ToString());
            }
            if (cmdresult == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                using (OracleCommand cmd = new OracleCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (System.Data.OracleClient.OracleException e)
                    {
                        throw new Exception(e.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回OracleDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>OracleDataReader</returns>
        public static OracleDataReader ExecuteReader(string SQLString, params OracleParameter[] cmdParms)
        {
            OracleConnection connection = new OracleConnection(connectionString);
            OracleCommand cmd = new OracleCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                OracleDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (System.Data.OracleClient.OracleException e)
            {
                throw new Exception(e.Message);
            }

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString, params OracleParameter[] cmdParms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand cmd = new OracleCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (System.Data.OracleClient.OracleException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                    return ds;
                }
            }
        }
        #endregion

        #region 执行数据库事物ByRen20140604
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>
        public static void ExecuteSqlTran(ArrayList SQLStringList)
        {
            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                conn.Open();
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                using (OracleTransaction tx = conn.BeginTransaction())
                {
                    cmd.Transaction = tx;
                    try
                    {
                        for (int n = 0; n < SQLStringList.Count; n++)
                        {
                            string strsql = SQLStringList[n].ToString();
                            if (strsql.Trim().Length > 1)
                            {
                                cmd.CommandText = strsql;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                    catch (System.Data.OracleClient.OracleException E)
                    {
                        tx.Rollback();
                        //conn.Close();
                        throw new Exception(E.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        conn.Close();
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 批量插入数据库 ByRen20150111
        /// </summary>
        /// <param name="dt"></param>
        public static void BatchInsert(DataTable dt, string SQLString, params OracleParameter[] cmdParms)
        {
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr.RowState != DataRowState.Added)
                    {
                        dr.SetAdded();
                    }
                }
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (OracleTransaction tx = conn.BeginTransaction())
                        {
                            OracleCommand cmd = new OracleCommand(SQLString, conn);
                            //cmd.Transaction = tx;
                            foreach (OracleParameter op in cmdParms)
                            {
                                op.SourceVersion = DataRowVersion.Current;
                                cmd.Parameters.Add(op);
                            }
                            OracleDataAdapter da = new OracleDataAdapter();
                            da.InsertCommand = cmd;
                            da.Update(dt);
                            tx.Commit();
                            cmd.Dispose();
                            conn.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        conn.Close();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }               
            }
            catch (Exception ex)
            {
            }
        }
     
        /// <summary>
        /// C#批量插入数据库 ByRen20150112
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="columnRowData">键-值存储的批量数据：键是列名称，值是对应的数据集合</param>
        /// <param name="len">每次批处理数据的大小</param>
        /// <returns></returns>
        public static int BatchInsertNew(string tableName, Dictionary<string, object> columnRowData, int len, OracleParameter[] parameters)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("必须指定批量插入的表名称", "tableName");
            }
            if (columnRowData == null || columnRowData.Count < 1)
            {
                throw new ArgumentException("必须指定批量插入的字段名称", "columnRowData");
            }
            int iResult = 0;
            //string[] dbColumns=new string[columnRowData.Keys.Count];
            //List<string> listArr = new List<string>();

            List<string> dbColumns1 =new List<string>();
           
            foreach(string pkey in columnRowData.Keys)
            {
                dbColumns1.Add(pkey);
            }
            string[] dbColumns = dbColumns1.ToArray();

            StringBuilder sbCmdText = new StringBuilder();
            if (columnRowData.Count > 0)
            {
                //准备插入的SQL
                sbCmdText.AppendFormat("INSERT INTO {0}(", tableName);
                sbCmdText.Append(string.Join(",", dbColumns));
                sbCmdText.Append(") VALUES (");
                sbCmdText.Append(":" + string.Join(",:", dbColumns));
                sbCmdText.Append(")");
                using (OracleConnection conn = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = conn.CreateCommand())
                    {
                        //绑定批处理的行数
                        cmd.ArrayBindCount = len;
                        cmd.BindByName = true;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = sbCmdText.ToString();
                        cmd.CommandTimeout = 600;//10分钟

                        //创建参数
                       // OracleParameter oraParam;
                        List<IDbDataParameter> cacher = new List<IDbDataParameter>();
                        //OracleDbType dbType = OracleDbType.Object;
                        /*
                        foreach (string colName in dbColumns)
                        {
                            dbType = GetOracleDbType(columnRowData[colName]);
                            oraParam = new OracleParameter(colName, dbType);

                            oraParam.Direction = ParameterDirection.Input;
                            oraParam.OracleDbTypeEx = dbType;

                            oraParam.Value = columnRowData[colName];
                            cmd.Parameters.Add(oraParam);
                        }
                         * */
                        int i=0;
                        foreach (string colName in dbColumns)
                        {
                            parameters[i].Direction = ParameterDirection.Input;
                            parameters[i].Value = columnRowData[colName];
                            i++;
                         }
                        cmd.Parameters.AddRange(parameters);
                        //打开连接
                        conn.Open();
                        /*执行批处理*/
                        var trans = conn.BeginTransaction();
                        try
                        {
                            cmd.Transaction = trans;
                            iResult = cmd.ExecuteNonQuery();
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                        finally
                        {
                            if (conn != null) conn.Close();
                        }

                    }
                }
            }
            return iResult;
        }

        /// <summary>
        /// 根据数据类型获取OracleDbType数据类型ByRen20150111   
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static OracleDbType GetOracleDbType(object value)
        {
            OracleDbType dataType = OracleDbType.Object;
            if (value is string[])
            {
                dataType = OracleDbType.Varchar2;
            }
            else if (value is DateTime[])
            {
                dataType = OracleDbType.TimeStamp;
            }
            else if (value is int[] || value is short[])
            {
                dataType = OracleDbType.Int32;
            }
            else if (value is long[])
            {
                dataType = OracleDbType.Int64;
            }
            else if (value is decimal[] || value is double[] || value is float[])
            {
                dataType = OracleDbType.Decimal;
            }
            else if (value is Guid[])
            {
                dataType = OracleDbType.Varchar2;
            }
            else if (value is bool[] || value is Boolean[])
            {
                dataType = OracleDbType.Byte;
            }
            else if (value is byte[])
            {
                dataType = OracleDbType.Blob;
            }
            else if (value is char[])
            {
                dataType = OracleDbType.Char;
            }
            return dataType;
        }
   }
}
