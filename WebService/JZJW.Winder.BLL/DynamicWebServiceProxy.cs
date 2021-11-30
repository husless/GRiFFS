using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services.Protocols;
using Microsoft.CSharp;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Net;
using System.Security.Cryptography;

namespace JZJW.Winder.BLL
{
    public class DynamicWebServiceProxy
    {
        //webservice服务的url
        public static readonly string wsurl = System.Configuration.ConfigurationManager.ConnectionStrings["wsurl"].ToString();
        public static string InvokeWS()
        {
            JZJW.Winder.BLL.wsf.GlobalForcastRainDataInfoClient ws = new JZJW.Winder.BLL.wsf.GlobalForcastRainDataInfoClient();
            //string json = ws.QueryForcastRainDataByRange(102.0, 102.5, 44.0, 44.5, "20140601", "20140602", "ecmwf");
            string json = ws.QueryLocation(0,0);
            return json;
        }
        /// <summary>
        /// 动态调用web服务
        /// </summary>
        /// <param name="url">WSDL服务地址，不带？wsdl</param>
        /// <param name="methodname">方法名</param>
        /// <param name="args">参数< /param>
        /// <returns>< /returns>
        public static object InvokeWebService(string url,string methodname, object[] args)
        {
           return DynamicWebServiceProxy.InvokeWebService(url, null, methodname, args);
        }
        /// < summary>
        /// 动态调用web服务
        /// < /summary>
        /// < param name="url">WSDL服务地址< /param>
        /// < param name="classname">类名< /param>
        /// < param name="methodname">方法名< /param>
        /// < param name="args">参数< /param>
        /// < returns>< /returns>
        public static object InvokeWebService(string url, string classname, string methodname, object[] args)
        {
            string @namespace = "EnterpriseServerBase.WebService.DynamicWebCalling";
            //string @namespace = "http://Iservice.cxf.tsinghua.com/";
            if ((classname == null) || (classname == ""))
            {
                classname = DynamicWebServiceProxy.GetWsClassName(url);
            }
            try
            {
                //获取服务描述语言(WSDL)  
                WebClient wc = new WebClient();
                Stream stream = wc.OpenRead(url + "?WSDL");
                //Stream stream = wc.OpenRead(@"C:\Users\lvdou\Desktop\renNew_pricenodetypeV1.3\WebService\test.xml");
                //Stream stream = wc.OpenRead(url);
                ServiceDescription sd = ServiceDescription.Read(stream);
                
                //sd.Imports[0].Namespace = "http://101.6.54.28:8081/RainDataService/";
                ServiceDescriptionImporter sdi = new ServiceDescriptionImporter();
                sdi.ProtocolName = "soap";
                sdi.Style = ServiceDescriptionImportStyle.Client; 
                sdi.AddServiceDescription(sd, "","");
                
                CodeNamespace cn = new CodeNamespace(@namespace);
                //生成客户端代理类代码
                CodeCompileUnit ccu = new CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);
                CSharpCodeProvider icc = new CSharpCodeProvider();
                //设定编译参数
                CompilerParameters cplist = new CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");
                //编译代理类
                CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }
                //生成代理实例，并调用方法
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                return mi.Invoke(obj, args);
                // PropertyInfo propertyInfo = type.GetProperty(propertyname);
                //return propertyInfo.GetValue(obj, null);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message, new Exception(ex.InnerException.StackTrace));
            }
        }
        private static string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }       
    }
}
