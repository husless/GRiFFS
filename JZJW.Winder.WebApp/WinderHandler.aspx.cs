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
                        //提交河段选择  未使用用户权限控制
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
                        //获取水库信息
                        case "reservoirInfo": { str = WinderHelper.GetReservoirInfo(reqParams); break; }
                        //设置水库是否参与计算
                        case "setreservoirCal": { str = WinderHelper.SetReservoirCal(reqParams); break; }
                        //添加水库流量过程
                        case "addreservoirflow": { str = WinderHelper.AddReservoirFlow(reqParams); break; }
                        //获取水库流量过程
                        case "getreservoirflow": { str = WinderHelper.GetReservoirFlow(reqParams); break; }

                        //获取计算用户列表及其是否限定了流域出口
                        //获取当前已申请未审核用户列表
                        case "scmlistUser": { str = WinderHelper.GetscmlistUser(reqParams); break; }
                        //获取某一用户的出口河段
                        case "userExtent": { str = WinderHelper.GetuserExtent(reqParams); break; }
                        //获取某一用户的出口河段个数
                        case "userExtentCount": { str = WinderHelper.GetuserExtentCount(reqParams); break; }
                        //设置某一用户的某一个出口
                        case "SetUserUetent": { str = WinderHelper.SetUserUetent(reqParams); break; }
                        //删除某一用户的某一个出口
                        case "DelUserUetent": { str = WinderHelper.DelUserUetent(reqParams); break; }
                        //设置用户是否为计算用户
                        case "SetUser": { str = WinderHelper.SetUser(reqParams); break; }
                        //设置某一用户的流域出口----提交河段选择
                        case "addrgnselUser": { str = WinderHelper.AddrgnselUser(reqParams); break; }
                        //用户费用
                        case "userfee": { str = WinderHelper.Getuserfee(reqParams); break; }
                        //用户费用单价
                        case "getfeedesc": { str = WinderHelper.GetFeeDesc(reqParams); break; }
                        //管理员设置用户费用单价
                        case "setfeedesc": { str = WinderHelper.SetFeeDesc(reqParams); break; }


                        //导入雨量站点
                        case "addrains": { str = WinderHelper.AddRainS(reqParams); break; }
                        //导入雨量数据
                        case "addrain": { str = WinderHelper.AddRain(reqParams); break; }
                        //获取雨量站点
                        case "getrains": { str = WinderHelper.GetRainS(reqParams); break; }


                        /*以下是河道模拟的接口*/
                        //河道方案表 新ID号 建立的sequence
                        case "Nscmnewid": { str = WinderHelper.getNscmnewid(reqParams); break; }
                        //新建河道模拟方案
                        case "Nscmnew": { str = WinderHelper.getNscmnew(reqParams); break; }
                        //判断河道模拟方案是否存在河网ID号
                        case "IsExitShapeR": { str = WinderHelper.IsExitShapeR(reqParams); break; }
                        //提交河道选择
                        case "addrgnselR": { str = WinderHelper.AddRegionSelR(reqParams); break; }
                        case "test": { str = WinderHelper.test(reqParams); break; }

                    }
                }

                this.Response.Write(str);
            }

        }
    }
}