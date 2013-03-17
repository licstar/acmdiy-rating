<%@ Page Title="FAQ" Language="C#" MasterPageFile="~/Master.Master" AutoEventWireup="true" CodeBehind="FAQ.aspx.cs" Inherits="acm_diy.FAQ" ValidateRequest="false" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
                    <h2>计分规则</h2>
                    <ul>
                    <li>
                    <ol>
                        <li>每条信息计1分</li>
                        <li>如果含图片+1</li>
                        <li>字数>10 +1</li>
                        <li>字数>100 +1</li>
                    </ol>
                    </li>
                    <li>每条信息最多计4分</li>
                    <li>早起王（5:30之后第一个发消息的），当日成绩 +20</li>
                    <li>破安静光环的得分以破1小时光环得50分为基准，得分正比于光环长度的1.5次方。每天100分封顶。其中凌晨1点到早上7点之间发的消息不计入评分（对于0点及7点后的第一条消息，假设前一个光环开始于0点及7点）。</li>

                    <li>成绩每日一评，按排名修正rating</li>
                    <li>如果当天没有参与讨论，则rating -= 1</li>
                    </ul>
                </div>
            </div>
            <div id="ie_clearing">
                &nbsp;</div>
        </div>
    </div>
</asp:Content>
