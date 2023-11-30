<%@ Page Async="true" Title="Pdf Editor" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FrontEndEditor._Default" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:DropDownList ID="ddlLayoutChoice" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlLayoutChoice_SelectedIndexChanged">
        <asp:ListItem Text="Select Layout" Value=""></asp:ListItem>
        <asp:ListItem Text="Choose between existing layout's" Value="ExistingLayout"></asp:ListItem>
        <asp:ListItem Text="Create New" Value="CreateNew"></asp:ListItem>
    </asp:DropDownList>
    <br />
    <asp:Panel ID="pnlCreateNewLayout" runat="server" Visible="false">
        <style>
            .checkbox-with-gap {
                display: flex;
                align-items: center;
            }

                .checkbox-with-gap input {
                    margin-right: 10px; /* You can adjust the margin value to increase or decrease the gap */
                }
        </style>
        <script type="text/javascript">
            function toggleTextBoxes(checkboxId, textBoxXId, textBoxYId) {
                var checkbox = document.getElementById(checkboxId);
                var textBoxX = document.getElementById(textBoxXId);
                var textBoxY = document.getElementById(textBoxYId);

                if (checkbox.checked) {
                    textBoxX.style.display = 'block';
                    textBoxY.style.display = 'block';
                } else {
                    textBoxX.style.display = 'none';
                    textBoxY.style.display = 'none';
                }
            }

            // Function to apply toggleTextBoxes to a set of checkboxes and textboxes
            function applyToggle(containerId, checkboxId, textBoxXId, textBoxYId) {
                var container = document.getElementById(containerId);
                var checkbox = container.querySelector('#' + checkboxId);
                var textBoxX = container.querySelector('#' + textBoxXId);
                var textBoxY = container.querySelector('#' + textBoxYId);

                // Add event listener to each checkbox
                checkbox.addEventListener('change', function () {
                    toggleTextBoxes(checkboxId, textBoxXId, textBoxYId);
                });

                // Initial call to set visibility based on checkbox state
                toggleTextBoxes(checkboxId, textBoxXId, textBoxYId);
            }
        </script>
        <br />
        <!-- Include Options Section -->
        <div id="includeOptionsSection" style="margin-bottom: 5px;">
            <!-- Textboxes for StrukturArtikelPosition -->
            <div id="strukturArtikelContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeStrukturArtikel" runat="server" Text="Struktur Artikel" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblStrukturArtikelPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtStrukturArtikelPositionX" runat="server" />
                <asp:TextBox ID="txtStrukturArtikelPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtStrukturArtikelPositionX.ClientID %>').setAttribute('placeholder', 'StrukturArtikelPositionX');
                    document.getElementById('<%= txtStrukturArtikelPositionY.ClientID %>').setAttribute('placeholder', 'StrukturArtikelPositionY');
                    // Initial call to set visibility based on checkbox state
                    applyToggle('strukturArtikelContainer', '<%= chkIncludeStrukturArtikel.ClientID %>', '<%= txtStrukturArtikelPositionX.ClientID %>', '<%= txtStrukturArtikelPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for DescriptionPosition -->
            <div id="DescriptionContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeDescription" runat="server" Text="Description" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblDescriptionPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtDescriptionPositionX" runat="server" />
                <asp:TextBox ID="txtDescriptionPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtDescriptionPositionX.ClientID %>').setAttribute('placeholder', 'DescriptionPositionX');
                    document.getElementById('<%= txtDescriptionPositionY.ClientID %>').setAttribute('placeholder', 'DescriptionPositionY');
                    applyToggle('DescriptionContainer', '<%= chkIncludeDescription.ClientID %>', '<%= txtDescriptionPositionX.ClientID %>', '<%= txtDescriptionPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for ArtNotTextPosition -->
            <div id="ArtNotTextContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeArtNotText" runat="server" Text=" Art Not Text" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblArtNotTextPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtArtNotTextPositionX" runat="server" />
                <asp:TextBox ID="txtArtNotTextPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtArtNotTextPositionX.ClientID %>').setAttribute('placeholder', 'ArtNotTextPositionX');
                    document.getElementById('<%= txtArtNotTextPositionY.ClientID %>').setAttribute('placeholder', 'ArtNotTextPositionY');
                    applyToggle('ArtNotTextContainer', '<%= chkIncludeArtNotText.ClientID %>', '<%= txtArtNotTextPositionX.ClientID %>', '<%= txtArtNotTextPositionY.ClientID %>');
                </script>
            </div>

            <!-- Textboxes for RutBokstavPosition -->
            <div id="RutBokstavContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeRutBokstav" runat="server" Text=" Rut Bokstav" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblRutBokstavPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtRutBokstavPositionX" runat="server" />
                <asp:TextBox ID="txtRutBokstavPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtRutBokstavPositionX.ClientID %>').setAttribute('placeholder', 'RutBokstavPositionX');
                    document.getElementById('<%= txtRutBokstavPositionY.ClientID %>').setAttribute('placeholder', 'RutBokstavPositionY');
                    applyToggle('RutBokstavContainer', '<%= chkIncludeRutBokstav.ClientID %>', '<%= txtRutBokstavPositionX.ClientID %>', '<%= txtRutBokstavPositionY.ClientID %>');
                </script>
            </div>

            <!-- Textboxes for ArtNrPosition -->
            <div id="ArtNrContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeArtNr" runat="server" Text=" Art Nr" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblArtNrPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtArtNrPositionX" runat="server" />
                <asp:TextBox ID="txtArtNrPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtArtNrPositionX.ClientID %>').setAttribute('placeholder', 'ArtNrPositionX');
                    document.getElementById('<%= txtArtNrPositionY.ClientID %>').setAttribute('placeholder', 'ArtNrPositionY');
                    applyToggle('ArtNrContainer', '<%= chkIncludeArtNr.ClientID %>', '<%= txtArtNrPositionX.ClientID %>', '<%= txtArtNrPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for PricePosition -->
            <div id="PriceContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludePrice" runat="server" Text=" Price" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblPricePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtPricePositionX" runat="server" />
                <asp:TextBox ID="txtPricePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtPricePositionX.ClientID %>').setAttribute('placeholder', 'PricePositionX');
                    document.getElementById('<%= txtPricePositionY.ClientID %>').setAttribute('placeholder', 'PricePositionY');
                    applyToggle('PriceContainer', '<%= chkIncludePrice.ClientID %>', '<%= txtPricePositionX.ClientID %>', '<%= txtPricePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for ServiceFeePosition -->
            <div id="ServiceFeeContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeServiceFee" runat="server" Text=" Service Fee" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblServiceFeePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtServiceFeePositionX" runat="server" />
                <asp:TextBox ID="txtServiceFeePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtServiceFeePositionX.ClientID %>').setAttribute('placeholder', 'ServiceFeePositionX');
                    document.getElementById('<%= txtServiceFeePositionY.ClientID %>').setAttribute('placeholder', 'ServiceFeePositionY');
                    applyToggle('ServiceFeeContainer', '<%= chkIncludeServiceFee.ClientID %>', '<%= txtServiceFeePositionX.ClientID %>', '<%= txtServiceFeePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for ArtNamePosition -->
            <div id="ArtNameContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeArtName" runat="server" Text=" Art Name" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblArtNamePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtArtNamePositionX" runat="server" />
                <asp:TextBox ID="txtArtNamePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtArtNamePositionX.ClientID %>').setAttribute('placeholder', 'ArtNamePositionX');
                    document.getElementById('<%= txtArtNamePositionY.ClientID %>').setAttribute('placeholder', 'ArtNamePositionY');
                    applyToggle('ArtNameContainer', '<%= chkIncludeArtName.ClientID %>', '<%= txtArtNamePositionX.ClientID %>', '<%= txtArtNamePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for ChairRowPosition -->
            <div id="ChairRowContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeChairRow" runat="server" Text=" Chair Row" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblChairRowPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtChairRowPositionX" runat="server" />
                <asp:TextBox ID="txtChairRowPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtChairRowPositionX.ClientID %>').setAttribute('placeholder', 'ChairRowPositionX');
                    document.getElementById('<%= txtChairRowPositionY.ClientID %>').setAttribute('placeholder', 'ChairRowPositionY');
                    applyToggle('ChairRowContainer', '<%= chkIncludeChairRow.ClientID %>', '<%= txtChairRowPositionX.ClientID %>', '<%= txtChairRowPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for ChairNrPosition -->
            <div id="ChairNrContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeChairNr" runat="server" Text=" Chair Nr" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblChairNrPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtChairNrPositionX" runat="server" />
                <asp:TextBox ID="txtChairNrPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtChairNrPositionX.ClientID %>').setAttribute('placeholder', 'ChairNrPositionX');
                    document.getElementById('<%= txtChairNrPositionY.ClientID %>').setAttribute('placeholder', 'ChairNrPositionY');
                    applyToggle('ChairNrContainer', '<%= chkIncludeChairNr.ClientID %>', '<%= txtChairNrPositionX.ClientID %>', '<%= txtChairNrPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for EventDatePosition -->
            <div id="EventDateContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeEventDate" runat="server" Text=" Event Date" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblEventDatePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtEventDatePositionX" runat="server" />
                <asp:TextBox ID="txtEventDatePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtEventDatePositionX.ClientID %>').setAttribute('placeholder', 'EventDatePositionX');
                    document.getElementById('<%= txtEventDatePositionY.ClientID %>').setAttribute('placeholder', 'EventDatePositionY');
                    applyToggle('EventDateContainer', '<%= chkIncludeEventDate.ClientID %>', '<%= txtEventDatePositionX.ClientID %>', '<%= txtEventDatePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for EventNamePosition -->
            <div id="EventNameContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeEventName" runat="server" Text=" Event Name" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblEventNamePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtEventNamePositionX" runat="server" />
                <asp:TextBox ID="txtEventNamePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtEventNamePositionX.ClientID %>').setAttribute('placeholder', 'EventNamePositionX');
                    document.getElementById('<%= txtEventNamePositionY.ClientID %>').setAttribute('placeholder', 'EventNamePositionY');
                    applyToggle('EventNameContainer', '<%= chkIncludeEventName.ClientID %>', '<%= txtEventNamePositionX.ClientID %>', '<%= txtEventNamePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for SubEventNamePosition -->
            <div id="SubEventNameContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeSubEventName" runat="server" Text=" Sub Event Name" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblSubEventNamePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtSubEventNamePositionX" runat="server" />
                <asp:TextBox ID="txtSubEventNamePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtSubEventNamePositionX.ClientID %>').setAttribute('placeholder', 'SubEventNamePositionX');
                    document.getElementById('<%= txtSubEventNamePositionY.ClientID %>').setAttribute('placeholder', 'SubEventNamePositionY');
                    applyToggle('SubEventNameContainer', '<%= chkIncludeSubEventName.ClientID %>', '<%= txtSubEventNamePositionX.ClientID %>', '<%= txtSubEventNamePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for Logorad1Position -->
            <div id="Logorad1Container" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeLogorad1" runat="server" Text=" Logorad 1" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblLogorad1Position" runat="server"></asp:Label>
                <asp:TextBox ID="txtLogorad1PositionX" runat="server" />
                <asp:TextBox ID="txtLogorad1PositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtLogorad1PositionX.ClientID %>').setAttribute('placeholder', 'Logorad1PositionX');
                    document.getElementById('<%= txtLogorad1PositionY.ClientID %>').setAttribute('placeholder', 'Logorad1PositionY');
                    applyToggle('Logorad1Container', '<%= chkIncludeLogorad1.ClientID %>', '<%= txtLogorad1PositionX.ClientID %>', '<%= txtLogorad1PositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for Logorad2Position -->
            <div id="Logorad2Container" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeLogorad2" runat="server" Text=" Logorad 2" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblLogorad2Position" runat="server"></asp:Label>
                <asp:TextBox ID="txtLogorad2PositionX" runat="server" />
                <asp:TextBox ID="txtLogorad2PositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtLogorad2PositionX.ClientID %>').setAttribute('placeholder', 'Logorad2PositionX');
                    document.getElementById('<%= txtLogorad2PositionY.ClientID %>').setAttribute('placeholder', 'Logorad2PositionY');
                    applyToggle('Logorad2Container', '<%= chkIncludeLogorad2.ClientID %>', '<%= txtLogorad2PositionX.ClientID %>', '<%= txtLogorad2PositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for SectionPosition -->
            <div id="SectionContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeSection" runat="server" Text=" Section" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblSectionPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtSectionPositionX" runat="server" />
                <asp:TextBox ID="txtSectionPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtSectionPositionX.ClientID %>').setAttribute('placeholder', 'SectionPositionX');
                    document.getElementById('<%= txtSectionPositionY.ClientID %>').setAttribute('placeholder', 'SectionPositionY');
                    applyToggle('SectionContainer', '<%= chkIncludeSection.ClientID %>', '<%= txtSectionPositionX.ClientID %>', '<%= txtSectionPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for BookingNrPosition -->
            <div id="BookingNrContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeBookingNr" runat="server" Text=" Booking Nr" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblBookingNrPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtBookingNrPositionX" runat="server" />
                <asp:TextBox ID="txtBookingNrPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtBookingNrPositionX.ClientID %>').setAttribute('placeholder', 'BookingNrPositionX');
                    document.getElementById('<%= txtBookingNrPositionY.ClientID %>').setAttribute('placeholder', 'BookingNrPositionY');
                    applyToggle('BookingNrContainer', '<%= chkIncludeBookingNr.ClientID %>', '<%= txtBookingNrPositionX.ClientID %>', '<%= txtBookingNrPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for WebBookingNumberPosition -->
            <div id="WebBookingNumberContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeWebBookingNr" runat="server" Text=" Web Booking Nr" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblWebBookingNumberPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtWebBookingNumberPositionX" runat="server" />
                <asp:TextBox ID="txtWebBookingNumberPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtWebBookingNumberPositionX.ClientID %>').setAttribute('placeholder', 'WebBookingNumberPositionX');
                    document.getElementById('<%= txtWebBookingNumberPositionY.ClientID %>').setAttribute('placeholder', 'WebBookingNumberPositionY');
                    applyToggle('WebBookingNumberContainer', '<%= chkIncludeWebBookingNr.ClientID %>', '<%= txtWebBookingNumberPositionX.ClientID %>', '<%= txtWebBookingNumberPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for FacilityNamePosition -->
            <div id="FacilityNameContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeFacilityName" runat="server" Text=" Facility Name" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblFacilityNamePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtFacilityNamePositionX" runat="server" />
                <asp:TextBox ID="txtFacilityNamePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtFacilityNamePositionX.ClientID %>').setAttribute('placeholder', 'FacilityNamePositionX');
                    document.getElementById('<%= txtFacilityNamePositionY.ClientID %>').setAttribute('placeholder', 'FacilityNamePositionY');
                    applyToggle('FacilityNameContainer', '<%= chkIncludeFacilityName.ClientID %>', '<%= txtFacilityNamePositionX.ClientID %>', '<%= txtFacilityNamePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for AdPosition -->
            <div id="AdContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeAd" runat="server" Text=" Ad" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblAdPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtAdPositionX" runat="server" />
                <asp:TextBox ID="txtAdPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtAdPositionX.ClientID %>').setAttribute('placeholder', 'AdPositionX');
                    document.getElementById('<%= txtAdPositionY.ClientID %>').setAttribute('placeholder', 'AdPositionY');
                    applyToggle('AdContainer', '<%= chkIncludeAd.ClientID %>', '<%= txtAdPositionX.ClientID %>', '<%= txtAdPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for NamePosition -->
            <div id="NameContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeContactPerson" runat="server" Text=" Contact Person" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblNamePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtNamePositionX" runat="server" />
                <asp:TextBox ID="txtNamePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtNamePositionX.ClientID %>').setAttribute('placeholder', 'NamePositionX');
                    document.getElementById('<%= txtNamePositionY.ClientID %>').setAttribute('placeholder', 'NamePositionY');
                    applyToggle('NameContainer', '<%= chkIncludeContactPerson.ClientID %>', '<%= txtNamePositionX.ClientID %>', '<%= txtNamePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for EmailPosition -->
            <div id="EmailContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeEmail" runat="server" Text=" Email" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblEmailPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtEmailPositionX" runat="server" />
                <asp:TextBox ID="txtEmailPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtEmailPositionX.ClientID %>').setAttribute('placeholder', 'EmailPositionX');
                    document.getElementById('<%= txtEmailPositionY.ClientID %>').setAttribute('placeholder', 'EmailPositionY');
                    applyToggle('EmailContainer', '<%= chkIncludeEmail.ClientID %>', '<%= txtEmailPositionX.ClientID %>', '<%= txtEmailPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for DatumPosition -->
            <div id="DatumContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeDatum" runat="server" Text=" Datum" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblDatumPosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtDatumPositionX" runat="server" />
                <asp:TextBox ID="txtDatumPositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtDatumPositionX.ClientID %>').setAttribute('placeholder', 'DatumPositionX');
                    document.getElementById('<%= txtDatumPositionY.ClientID %>').setAttribute('placeholder', 'DatumPositionY');
                    applyToggle('DatumContainer', '<%= chkIncludeDatum.ClientID %>', '<%= txtDatumPositionX.ClientID %>', '<%= txtDatumPositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for EntrancePosition -->
            <div id="EntranceContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeEntrance" runat="server" Text=" Entrance" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblEntrancePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtEntrancePositionX" runat="server" />
                <asp:TextBox ID="txtEntrancePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtEntrancePositionX.ClientID %>').setAttribute('placeholder', 'EntrancePositionX');
                    document.getElementById('<%= txtEntrancePositionY.ClientID %>').setAttribute('placeholder', 'EntrancePositionY');
                    applyToggle('EntranceContainer', '<%= chkIncludeEntrance.ClientID %>', '<%= txtEntrancePositionX.ClientID %>', '<%= txtEntrancePositionY.ClientID %>');
                </script>
            </div>
            <!-- Textboxes for WebbcodePosition -->
            <div id="WebbcodeContainer" style="margin-bottom: 5px;">
                <asp:CheckBox ID="chkIncludeWebbcode" runat="server" Text=" Webbcode" Checked="false" CssClass="checkbox-with-gap" />
                <asp:Label ID="lblWebbcodePosition" runat="server"></asp:Label>
                <asp:TextBox ID="txtWebbcodePositionX" runat="server" />
                <asp:TextBox ID="txtWebbcodePositionY" runat="server" />
                <script type="text/javascript">
                    document.getElementById('<%= txtWebbcodePositionX.ClientID %>').setAttribute('placeholder', 'WebbcodePositionX');
                    document.getElementById('<%= txtWebbcodePositionY.ClientID %>').setAttribute('placeholder', 'WebbcodePositionY');
                    applyToggle('WebbcodeContainer', '<%= chkIncludeWebbcode.ClientID %>', '<%= txtWebbcodePositionX.ClientID %>', '<%= txtWebbcodePositionY.ClientID %>');
                </script>
            </div>
            <div>
                <asp:CheckBox ID="chkIncludeScissorsLine" runat="server" Text=" Scissors Line" Checked="false" CssClass="checkbox-with-gap" />
            </div>
        </div>
        <!-- Textboxes for BarcodePosition -->
        <asp:Label ID="lblBarcodePosition" runat="server" Text="Barcode Positions:"></asp:Label>
        <asp:TextBox ID="txtBarcodePositionX" runat="server" />
        <asp:TextBox ID="txtBarcodePositionY" runat="server" />
        <script type="text/javascript">
            document.getElementById('<%= txtBarcodePositionX.ClientID %>').setAttribute('placeholder', 'PositionX');
            document.getElementById('<%= txtBarcodePositionY.ClientID %>').setAttribute('placeholder', 'PositionY');
        </script>

        <br />
        <!-- Other Options -->
        <asp:CheckBox ID="chkUseQRCode" runat="server" Text="Use QR Code" />
        <asp:CheckBox ID="chkFlipBarcode" runat="server" Text="Flip Barcode" />
        <br />
        <div>
            <!-- Custom Text Elements logic will be implemented in code-behind -->
            <asp:TextBox ID="txtCustomText" runat="server" />
            <asp:TextBox ID="txtPositionX" runat="server" />
            <asp:TextBox ID="txtPositionY" runat="server" />
            <asp:TextBox ID="txtFontSize" runat="server" />
            <asp:TextBox ID="txtColor" runat="server" />
            <asp:Button ID="btnAddCustomText" runat="server" Text="Add Custom Text" OnClick="btnAddCustomText_Click" />
            <br />

            <asp:TextBox ID="txtNewTicketId" runat="server" />

            <asp:FileUpload ID="bgUploadFile" runat="server" accept=".png,.jpg" />
            <script type="text/javascript">
                document.getElementById('<%= txtCustomText.ClientID %>').setAttribute('placeholder', 'CustomText');
                document.getElementById('<%= txtPositionX.ClientID %>').setAttribute('placeholder', 'CustomtxtPositionX');
                document.getElementById('<%= txtPositionY.ClientID %>').setAttribute('placeholder', 'CustomtxtPositionY');
                document.getElementById('<%= txtFontSize.ClientID %>').setAttribute('placeholder', 'txtFontSize');
                document.getElementById('<%= txtColor.ClientID %>').setAttribute('placeholder', 'txtColor');
                document.getElementById('<%= txtNewTicketId.ClientID %>').setAttribute('placeholder', 'Id');
            </script>
            <br />
            <asp:Button ID="btnPreview" runat="server" Text="Preview" OnClick="btnPreview_Click" />
            <br />
            <br />

            <asp:Button ID="btnSaveToDb" runat="server" Text="Save Your Template" OnClick="btnSaveToDb_Click" />
        </div>
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
        <br />
        <br />

        <asp:Button ID="btnGetPredefinedTemplate" runat="server" Text="Get Predefined Template" OnClick="btnGetPredefinedTemplate_Click" />
        <br />
        <br />

        <asp:Button ID="btnGetExistingTemplate" runat="server" Text="Show Existing ShowEventInfo" OnClick="btnGetExistingTemplate_Click" />
    </asp:Panel>
    <asp:Literal ID="litScript" runat="server" />
    <script src="Scripts/main.js" type="text/javascript"></script>
</asp:Content>
