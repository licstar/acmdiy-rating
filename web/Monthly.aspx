<%@ Page Title="" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true"
    CodeBehind="Monthly.aspx.cs" Inherits="acm_diy.Monthly" ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page">
        <!-- begin: main navigation #nav -->
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
                    <%PrintPage(); %>
                    <div style="clear: both;">
                    </div>
                    <hr />
                </div>
            </div>
            <div id="ie_clearing">
                &nbsp;</div>
        </div>
    </div>
</asp:Content>
