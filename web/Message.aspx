<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="Message.aspx.cs" Inherits="acm_diy.Message" ValidateRequest="false" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <style type="text/css">
        a { color: #015BAB; text-decoration: none; }
        td { font-size: 12px; line-height: 22px; }
        table { table-layout:fixed; word-break : break-all; word-wrap : break-word;}
        .a { color: #006EFE; padding-left: 10px; text-align:left; }
        .b { float: left; margin-right: 6px; }
        .c { padding-left: 20px; text-align:left; }
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
        <div id="nav">
            <div class="hlist" id="navday">
                <ul>
                    <% Response.Write(navDay); %>
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
                    <% PrintPage(); %>
                    <div style="clear: both;">
                    </div>
                </div>
            </div>
            <div id="ie_clearing">
                &nbsp;</div>
        </div>
    </div>
</asp:Content>
