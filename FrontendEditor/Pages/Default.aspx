<%@ Page Async="true" Title="Pdf Editor" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FrontEndEditor._Default" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:DropDownList ID="ddlLayoutChoice" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLayoutChoice_SelectedIndexChanged">
        <asp:ListItem Text="Select Layout" Value=""></asp:ListItem>
        <asp:ListItem Text="Choose between existing layout's" Value="ExistingLayout"></asp:ListItem>
        <asp:ListItem Text="Create New" Value="CreateNew"></asp:ListItem>
    </asp:DropDownList>

    <asp:Panel ID="pnlCreateNewLayout" runat="server" Visible="false">
        <!-- Include Options Section -->
        <div id="includeOptionsSection">
            <asp:CheckBox ID="chkIncludeStrukturArtikel" runat="server" Text="Include Struktur Artikel" Checked="true" />
            <asp:CheckBox ID="chkIncludeDescription" runat="server" Text="Include Description" Checked="true" />
            <asp:CheckBox ID="chkIncludeArtNotText" runat="server" Text="Include Art Not Text" Checked="true" />
            <asp:CheckBox ID="chkIncludeRutBokstav" runat="server" Text="Include Rut Bokstav" Checked="true" />
            <asp:CheckBox ID="chkIncludeArtNr" runat="server" Text="Include Art Nr" Checked="true" />
            <asp:CheckBox ID="chkIncludePrice" runat="server" Text="Include Price" Checked="true" />
            <asp:CheckBox ID="chkIncludeServiceFee" runat="server" Text="Include Service Fee" Checked="true" />
            <asp:CheckBox ID="chkIncludeArtName" runat="server" Text="Include Art Name" Checked="true" />
            <asp:CheckBox ID="chkIncludeChairRow" runat="server" Text="Include Chair Row" Checked="true" />
            <asp:CheckBox ID="chkIncludeChairNr" runat="server" Text="Include Chair Nr" Checked="true" />
            <asp:CheckBox ID="chkIncludeEventDate" runat="server" Text="Include Event Date" Checked="true" />
            <asp:CheckBox ID="chkIncludeEventName" runat="server" Text="Include Event Name" Checked="true" />
            <asp:CheckBox ID="chkIncludeSubEventName" runat="server" Text="Include Sub Event Name" Checked="true" />
            <asp:CheckBox ID="chkIncludeLogorad1" runat="server" Text="Include Logorad 1" Checked="true" />
            <asp:CheckBox ID="chkIncludeLogorad2" runat="server" Text="Include Logorad 2" Checked="true" />
            <asp:CheckBox ID="chkIncludeSection" runat="server" Text="Include Section" Checked="true" />
            <asp:CheckBox ID="chkIncludeBookingNr" runat="server" Text="Include Booking Nr" Checked="true" />
            <asp:CheckBox ID="chkIncludeWebBookingNr" runat="server" Text="Include Web Booking Nr" Checked="true" />
            <asp:CheckBox ID="chkIncludeFacilityName" runat="server" Text="Include Facility Name" Checked="true" />
            <asp:CheckBox ID="chkIncludeAd" runat="server" Text="Include Ad" Checked="true" />
            <asp:CheckBox ID="chkIncludeContactPerson" runat="server" Text="Include Contact Person" Checked="true" />
            <asp:CheckBox ID="chkIncludeEmail" runat="server" Text="Include Email" Checked="true" />
            <asp:CheckBox ID="chkIncludeDatum" runat="server" Text="Include Datum" Checked="true" />
            <asp:CheckBox ID="chkIncludeEntrance" runat="server" Text="Include Entrance" Checked="true" />
            <asp:CheckBox ID="chkIncludeWebbcode" runat="server" Text="Include Webbcode" Checked="true" />
            <asp:CheckBox ID="chkIncludeScissorsLine" runat="server" Text="Include Scissors Line" Checked="true" />
        </div>


        <!-- Position Options Section -->
        <div id="positionOptionsSection">
            <asp:Label ID="lblArtNrPosition" runat="server" Text="Art Nr Position:"></asp:Label>
            <asp:TextBox ID="txtArtNrPositionX" runat="server" />
            <asp:TextBox ID="txtArtNrPositionY" runat="server" />
            <!-- Textboxes for PricePosition -->
            <asp:Label ID="lblPricePosition" runat="server" Text="Price Position:"></asp:Label>
            <asp:TextBox ID="txtPricePositionX" runat="server" />
            <asp:TextBox ID="txtPricePositionY" runat="server" />

            <!-- Textboxes for ServiceFeePosition -->
            <asp:Label ID="lblServiceFeePosition" runat="server" Text="Service Fee Position:"></asp:Label>
            <asp:TextBox ID="txtServiceFeePositionX" runat="server" />
            <asp:TextBox ID="txtServiceFeePositionY" runat="server" />

            <!-- Textboxes for ArtNamePosition -->
            <asp:Label ID="lblArtNamePosition" runat="server" Text="Art Name Position:"></asp:Label>
            <asp:TextBox ID="txtArtNamePositionX" runat="server" />
            <asp:TextBox ID="txtArtNamePositionY" runat="server" />

            <!-- Textboxes for ChairRowPosition -->
            <asp:Label ID="lblChairRowPosition" runat="server" Text="Chair Row Position:"></asp:Label>
            <asp:TextBox ID="txtChairRowPositionX" runat="server" />
            <asp:TextBox ID="txtChairRowPositionY" runat="server" />

            <!-- Textboxes for ChairNrPosition -->
            <asp:Label ID="lblChairNrPosition" runat="server" Text="Chair Nr Position:"></asp:Label>
            <asp:TextBox ID="txtChairNrPositionX" runat="server" />
            <asp:TextBox ID="txtChairNrPositionY" runat="server" />

            <!-- Textboxes for EventDatePosition -->
            <asp:Label ID="lblEventDatePosition" runat="server" Text="Event Date Position:"></asp:Label>
            <asp:TextBox ID="txtEventDatePositionX" runat="server" />
            <asp:TextBox ID="txtEventDatePositionY" runat="server" />

            <!-- Textboxes for EventNamePosition -->
            <asp:Label ID="lblEventNamePosition" runat="server" Text="Event Name Position:"></asp:Label>
            <asp:TextBox ID="txtEventNamePositionX" runat="server" />
            <asp:TextBox ID="txtEventNamePositionY" runat="server" />

            <!-- Textboxes for SubEventNamePosition -->
            <asp:Label ID="lblSubEventNamePosition" runat="server" Text="Sub Event Name Position:"></asp:Label>
            <asp:TextBox ID="txtSubEventNamePositionX" runat="server" />
            <asp:TextBox ID="txtSubEventNamePositionY" runat="server" />

            <!-- Textboxes for Logorad1Position -->
            <asp:Label ID="lblLogorad1Position" runat="server" Text="Logorad 1 Position:"></asp:Label>
            <asp:TextBox ID="txtLogorad1PositionX" runat="server" />
            <asp:TextBox ID="txtLogorad1PositionY" runat="server" />

            <!-- Textboxes for Logorad2Position -->
            <asp:Label ID="lblLogorad2Position" runat="server" Text="Logorad 2 Position:"></asp:Label>
            <asp:TextBox ID="txtLogorad2PositionX" runat="server" />
            <asp:TextBox ID="txtLogorad2PositionY" runat="server" />

            <!-- Textboxes for LocationPosition -->
            <asp:Label ID="lblLocationPosition" runat="server" Text="Location Position:"></asp:Label>
            <asp:TextBox ID="txtLocationPositionX" runat="server" />
            <asp:TextBox ID="txtLocationPositionY" runat="server" />

            <!-- Textboxes for SectionPosition -->
            <asp:Label ID="lblSectionPosition" runat="server" Text="Section Position:"></asp:Label>
            <asp:TextBox ID="txtSectionPositionX" runat="server" />
            <asp:TextBox ID="txtSectionPositionY" runat="server" />

            <!-- Textboxes for BookingNrPosition -->
            <asp:Label ID="lblBookingNrPosition" runat="server" Text="Booking Nr Position:"></asp:Label>
            <asp:TextBox ID="txtBookingNrPositionX" runat="server" />
            <asp:TextBox ID="txtBookingNrPositionY" runat="server" />

            <!-- Textboxes for WebBookingNumberPosition -->
            <asp:Label ID="lblWebBookingNumberPosition" runat="server" Text="Web Booking Number Position:"></asp:Label>
            <asp:TextBox ID="txtWebBookingNumberPositionX" runat="server" />
            <asp:TextBox ID="txtWebBookingNumberPositionY" runat="server" />

            <!-- Textboxes for FacilityNamePosition -->
            <asp:Label ID="lblFacilityNamePosition" runat="server" Text="Facility Name Position:"></asp:Label>
            <asp:TextBox ID="txtFacilityNamePositionX" runat="server" />
            <asp:TextBox ID="txtFacilityNamePositionY" runat="server" />

            <!-- Textboxes for AdPosition -->
            <asp:Label ID="lblAdPosition" runat="server" Text="Ad Position:"></asp:Label>
            <asp:TextBox ID="txtAdPositionX" runat="server" />
            <asp:TextBox ID="txtAdPositionY" runat="server" />

            <!-- Textboxes for StrukturArtikelPosition -->
            <asp:Label ID="lblStrukturArtikelPosition" runat="server" Text="Struktur Artikel Position:"></asp:Label>
            <asp:TextBox ID="txtStrukturArtikelPositionX" runat="server" />
            <asp:TextBox ID="txtStrukturArtikelPositionY" runat="server" />

            <!-- Textboxes for DescriptionPosition -->
            <asp:Label ID="lblDescriptionPosition" runat="server" Text="Description Position:"></asp:Label>
            <asp:TextBox ID="txtDescriptionPositionX" runat="server" />
            <asp:TextBox ID="txtDescriptionPositionY" runat="server" />

            <!-- Textboxes for ArtNotTextPosition -->
            <asp:Label ID="lblArtNotTextPosition" runat="server" Text="Art Not Text Position:"></asp:Label>
            <asp:TextBox ID="txtArtNotTextPositionX" runat="server" />
            <asp:TextBox ID="txtArtNotTextPositionY" runat="server" />

            <!-- Textboxes for NamePosition -->
            <asp:Label ID="lblNamePosition" runat="server" Text="Name Position:"></asp:Label>
            <asp:TextBox ID="txtNamePositionX" runat="server" />
            <asp:TextBox ID="txtNamePositionY" runat="server" />

            <!-- Textboxes for EmailPosition -->
            <asp:Label ID="lblEmailPosition" runat="server" Text="Email Position:"></asp:Label>
            <asp:TextBox ID="txtEmailPositionX" runat="server" />
            <asp:TextBox ID="txtEmailPositionY" runat="server" />

            <!-- Textboxes for DatumPosition -->
            <asp:Label ID="lblDatumPosition" runat="server" Text="Datum Position:"></asp:Label>
            <asp:TextBox ID="txtDatumPositionX" runat="server" />
            <asp:TextBox ID="txtDatumPositionY" runat="server" />

            <!-- Textboxes for EntrancePosition -->
            <asp:Label ID="lblEntrancePosition" runat="server" Text="Entrance Position:"></asp:Label>
            <asp:TextBox ID="txtEntrancePositionX" runat="server" />
            <asp:TextBox ID="txtEntrancePositionY" runat="server" />

            <!-- Textboxes for RutBokstavPosition -->
            <asp:Label ID="lblRutBokstavPosition" runat="server" Text="Rut Bokstav Position:"></asp:Label>
            <asp:TextBox ID="txtRutBokstavPositionX" runat="server" />
            <asp:TextBox ID="txtRutBokstavPositionY" runat="server" />

            <!-- Textboxes for WebbcodePosition -->
            <asp:Label ID="lblWebbcodePosition" runat="server" Text="Webbcode Position:"></asp:Label>
            <asp:TextBox ID="txtWebbcodePositionX" runat="server" />
            <asp:TextBox ID="txtWebbcodePositionY" runat="server" />

            <!-- Textboxes for BarcodePosition -->
            <asp:Label ID="lblBarcodePosition" runat="server" Text="Barcode Position:"></asp:Label>
            <asp:TextBox ID="txtBarcodePositionX" runat="server" />
            <asp:TextBox ID="txtBarcodePositionY" runat="server" />
        </div>

        <!-- Other Options -->
        <asp:CheckBox ID="chkUseQRCode" runat="server" Text="Use QR Code" />
        <asp:CheckBox ID="chkFlipBarcode" runat="server" Text="Flip Barcode" />
        <!-- Custom Text Elements logic will be implemented in code-behind -->
        <asp:TextBox ID="txtCustomText" runat="server" />
        <asp:TextBox ID="txtPositionX" runat="server" />
        <asp:TextBox ID="txtPositionY" runat="server" />
        <asp:TextBox ID="txtFontSize" runat="server" />
        <asp:TextBox ID="txtColor" runat="server" />
        <asp:Button ID="btnAddCustomText" runat="server" Text="Add Custom Text" OnClick="btnAddCustomText_Click" />
        <asp:FileUpload ID="bguploadFile" runat="server" accept=".png,.jpg" />
        <asp:TextBox ID="TextBox1" runat="server" />

        <asp:Button ID="btnPreview" runat="server" Text="Preview" OnClick="btnPreview_Click" />
        <asp:CheckBox ID="SaveToDb" runat="server" Text="Do you want to save your layout to the database" />
        <asp:Button ID="btnSaveToDb" runat="server" Text="Save To Db" OnClick="btnSaveToDb_Click" />
    </asp:Panel>

    <asp:Panel ID="pnlPredefinedLayout" runat="server" Visible="false">
        <asp:DropDownList ID="ddlPredefinedLayouts" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPredefinedLayouts_SelectedIndexChanged">
            <asp:ListItem Text="Select between existing Layout's" Value=""></asp:ListItem>
            <asp:ListItem Text="Layout 1" Value="1"></asp:ListItem>
            <asp:ListItem Text="Layout 2" Value="2"></asp:ListItem>
            <asp:ListItem Text="Layout 3" Value="3"></asp:ListItem>
            <asp:ListItem Text="Choose self created layout" Value="SelfCreated"></asp:ListItem>
        </asp:DropDownList>
        <asp:TextBox ID="txtTicketId" runat="server" />
        <asp:TextBox ID="txtShowEventInfo" runat="server" />
        <script type="text/javascript">
            document.getElementById('<%= txtTicketId.ClientID %>').setAttribute('placeholder', 'Enter Ticket ID');
            document.getElementById('<%= txtShowEventInfo.ClientID %>').setAttribute('placeholder', 'Enter ShowEventInfo ID');
        </script>
        <asp:FileUpload ID="bgFileUpload" runat="server" accept=".png,.jpg" />
        <asp:Button ID="btnGetPredefinedTemplate" runat="server" Text="Get Predefined Template" OnClick="btnGetPredefinedTemplate_Click" />
        <asp:Button ID="btnGetExistingTemplate" runat="server" Text="Get a list of existing Templates" OnClick="btnGetExistingTemplate_Click" />

    </asp:Panel>
    <asp:Literal ID="litScript" runat="server" />
    <script src="Scripts/main.js" type="text/javascript"></script>
</asp:Content>
