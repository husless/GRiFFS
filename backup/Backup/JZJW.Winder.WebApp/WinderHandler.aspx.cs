using System;
using System.Collections.Specialized;
using JZJW.Winder.BLL;


namespace JZJW.Winder.WebApp
{
    public partial class WinderHandler : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string str = string.Empty;
            if (!IsPostBack)
            {
                NameValueCollection reqParams = this.Request.Params;
                if (reqParams != null)
                {
                    string act = reqParams.Get("act");
                    switch (act)
                    {
                        //获取新方案ID号
                        case "scmnewid": { str = WinderHelper.GetSchemeNewId(reqParams); break; }
                        //新建方案
                        case "newscm": { str = WinderHelper.NewScheme(reqParams); break; }
                        //方案列表
                        case "scmlist": { str = WinderHelper.GetSchemeList(reqParams); break; }
                        //取消方法
                        case "addregion": { str = WinderHelper.AddRegion(reqParams); break; }
                        //提交河段选择
                        case "addrgnsel": { str = WinderHelper.AddRegionSel(reqParams); break; }
                        //模型列表
                        case "modellist": { str = WinderHelper.GetModelList(reqParams); break; }
                        //获取指定方案
                        case "scmrgn": { str = WinderHelper.GetSchemeRegion(reqParams); break; }
                        //新建方案参数
                        case "scmparams": { str = WinderHelper.SetSchemeParams(reqParams); break; }
                        //获取指定方案的出口河段
                        case "scmrgnshplist": { str = WinderHelper.GetSchemeRegionShapeList(reqParams); break; }
                        //登陆
                        case "login": { str = WinderHelper.DoLogin(reqParams); break; }
                        //用户注册
                        case "userregister": { str = WinderHelper.DoUserRegister(reqParams); break; }
                        //水位过程 流量过程和纵断面
                        case "linechart": { str = WinderHelper.GetLineChartData(reqParams); break; }
                        //获取指定方案的上游河段
                        case "scmupstream": { str = WinderHelper.GetSchemeRegionUpstream(reqParams); break; }
                        //ren添加
                        //删除方案
                        case "delscm": { str = WinderHelper.DelScm(reqParams); break; }
                        //判断某方案是否存在definedload
                        case "IsExitShape": { str = WinderHelper.IsExitShape(reqParams); break; }
                        //获取指定方案的出口河段属性
                        case "scmrgnlist": { str = WinderHelper.GetSchemeRegionList(reqParams); break; }
                        
                        
                        /*以下是河道模拟的接口*/
                        //河道方案表 新ID号 建立的sequence
                        case "Nscmnewid": { str = WinderHelper.getNscmnewid(reqParams); break; }
                        //新建河道模拟方案
                        case "Nscmnew": { str = WinderHelper.getNscmnew(reqParams); break; }
                        //判断河道模拟方案是否存在河网ID号
                        case "IsExitShapeR": { str = WinderHelper.IsExitShapeR(reqParams); break; }
                        //提交河道选择
                        case "addrgnselR": { str = WinderHelper.AddRegionSelR(reqParams); break; }

                    }
                }

                this.Response.Write(str);
            }

        }
    }
}