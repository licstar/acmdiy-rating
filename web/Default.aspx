<%@ Page Title="ACM_DIY 群 Rating 第二期 (beta)" Language="C#" MasterPageFile="~/Master.Master"
    AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="acm_diy.Default" ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .calendar { border: 1px solid #C8D2F0 !important; overflow: hidden; padding: 0 1px; width: 200px; height: 200px; margin: 10px; float: left; }
        table.title { margin-bottom: auto; width: 100%; line-height: normal; border: 0px; }
        table.title td { padding: 5px; }
        table.week { margin-bottom: auto; line-height: normal; border: 0px; width: 100%; }
        table.month { border: 0px; margin-top: 0px !important; border-collapse: separate !important; }
        th, tr.bg { background: none repeat scroll 0 0 #E3EEF9 !important; padding: 5px 7px 5px 8px; }
        .fcgreen { color: #009944; }
        td.have { background: none repeat scroll 0 0 #C4E0F9; color: #015BAB; padding: 0px; width: 30px; border-bottom: 0px; border-bottom: 0px; }
        td.nothave { padding: 0px !important; width: 30px; border-bottom: 0px; }
        table { color: #313131; font-family: tahoma,simsun; font-size: 12px; line-height: 180%; }
        tr:hover td.have { background: #C4E0F9; }
        tr:hover td.nothave { background: #FFFFFF; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page">
        <div id="nav">
            <div class="hlist">
                <ul>
                    <% Response.Write(navMonth); %>
                </ul>
            </div>
        </div>
        <div id="main">
            <div id="col1">
                <div id="col1_content" class="clearfix">
                    <% Response.Write(navList); %>
                </div>
            </div>
            <div id="col3">
                <div id="col3_content" class="clearfix">
                    <h2>每日概况</h2>
                    <%PrintPage(); %>
                    <div style="clear: both;">
                    </div>
                </div>
            </div>
            <div id="ie_clearing">
                &nbsp;</div>
        </div>
    </div>
</asp:Content>
