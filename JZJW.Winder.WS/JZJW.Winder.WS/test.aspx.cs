using System;


namespace JZJW.Winder.WS
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string ss = Winder.BLL.WinderHelper.GetLineChartData("V", "339001083", "2892", "0", "0");
            Response.Write(ss);
        }
    }
}