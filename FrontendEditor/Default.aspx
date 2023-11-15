<%@ Page Async="true" Title="Pdf Editor" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FrontEndEditor._Default" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <!-- Add other form elements here -->
        <asp:FileUpload ID="bgFileUpload" runat="server" accept=".png,.jpg" />
        <asp:TextBox ID="txtBarcodeContent" runat="server" />
        <asp:TextBox ID="txtId" runat="server" />
        <asp:Button ID="btnPreview" runat="server" Text="Preview" OnClick="btnPreview_Click" />
        <asp:Literal ID="litScript" runat="server" />
</asp:Content>
