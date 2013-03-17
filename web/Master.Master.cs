using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace acm_diy
{
    public partial class Master : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            string key = "";
            if (Request["q"] != null)
                key = Request["q"];

            if (!Page.IsPostBack)
                txtSearch.Text = key;

            txtSearch.Attributes.Add("onkeypress", "SubmitKeyClick(ctl00_btnSearch);");
            txtSearch.Attributes.Add("autocomplete", "off");

        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            Response.Redirect("Search.aspx?q=" + Server.UrlEncode(txtSearch.Text));
        }

        protected void btnConform_Click(object sender, EventArgs e)
        {
            if (txtConform.Text == "ACM_DIY")
                Session["chk"] = 1;
        }
    }
}
