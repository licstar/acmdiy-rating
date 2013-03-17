using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace acm_diy {
    public partial class FAQ : System.Web.UI.Page {
        protected DateTime end;
        protected DateTime begin;
        protected string navList, navMonth;

        protected void Page_Load(object sender, EventArgs e) {
            begin = Functions.begin;
            end = Functions.end;
            navList = Functions.GetNavList("FAQ.aspx", "", "");
            navMonth = Functions.GetNavMonth(new DateTime(1970, 1, 1));
        }
    }
}
