<%@ Page Async="true" Title="Pdf Editor" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FrontEndEditor._Default" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:CheckBox ID="chkIncludePrice" runat="server" Text="Include Price" />
<!-- Repeat for other options -->
<asp:TextBox ID="txtBarcodePositionX" runat="server" />
<!-- Repeat for other position fields -->
<asp:TextBox ID="txtShowEventInfo" runat="server" />
<asp:Button ID="btnPreview" runat="server" Text="Preview" OnClick="btnPreview_Click" />

    <asp:Literal ID="litScript" runat="server" />
</asp:Content>
