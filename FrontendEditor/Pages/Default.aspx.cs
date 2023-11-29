using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace FrontEndEditor
{
    public partial class _Default : Page
    {
        HttpClient client = new HttpClient();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlCreateNewLayout.Visible = false;
                pnlPredefinedLayout.Visible = false;
            }
        }

        protected void ddlLayoutChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlLayoutChoice.SelectedValue == "CreateNew")
            {
                // Show controls for creating a new layout
                pnlCreateNewLayout.Visible = true;
                pnlPredefinedLayout.Visible = false;
                litScript.Text = " ";
            }
            else if (ddlLayoutChoice.SelectedValue == "ExistingLayout")
            {
                pnlCreateNewLayout.Visible = false;
                pnlPredefinedLayout.Visible = true;
                txtTicketId.Visible = false;
                txtShowEventInfo.Visible = false;
                bgFileUpload.Visible = false;
                btnGetPredefinedTemplate.Visible = false;
                litScript.Text = " ";
            }
            else
            {
                // Hide controls for creating a new layout
                pnlCreateNewLayout.Visible = false;
                pnlPredefinedLayout.Visible = false;
                litScript.Text = " ";
            }
        }

        protected void ddlPredefinedLayouts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlPredefinedLayouts.SelectedValue == "SelfCreated")
            {
                //Show controls for creating a new layout
                pnlPredefinedLayout.Visible = true;
                txtShowEventInfo.Visible = true;
                txtTicketId.Visible = true;
                bgFileUpload.Visible = true;
                btnGetPredefinedTemplate.Visible = true;
                litScript.Text = " ";
            }
            else if (!string.IsNullOrEmpty(ddlPredefinedLayouts.SelectedValue))
            {
                pnlPredefinedLayout.Visible = true;
                txtShowEventInfo.Visible = false;
                txtTicketId.Visible = true;
                bgFileUpload.Visible = true;
                btnGetPredefinedTemplate.Visible = true;
                litScript.Text = " ";
            }
            else
            {
                // Hide controls for creating a new layout
                pnlCreateNewLayout.Visible = false;
                txtTicketId.Visible = false;
                txtShowEventInfo.Visible = false;
                bgFileUpload.Visible = false;
                btnGetPredefinedTemplate.Visible = false;
                litScript.Text = " ";
            }
        }

        protected async void btnGetExistingTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Update with your API URL
                    string apiUrl = "https://localhost:7104/api/PdfTemplate/GetTicketTemplate";
                    var response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var showEventInfos = JsonConvert.DeserializeObject<List<int>>(data);

                        var htmlBuilder = new StringBuilder();
                        htmlBuilder.Append("<ul>");
                        foreach (var info in showEventInfos)
                        {
                            htmlBuilder.Append($"<li>Existing ShowEventInfo: {info}</li>");
                        }
                        htmlBuilder.Append("</ul>");

                        litScript.Text = htmlBuilder.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                litScript.Text = $"Error: {ex.Message}";
            }
        }

        protected async void btnGetPredefinedTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                if (bgFileUpload.HasFile)
                {
                    var content = new MultipartFormDataContent();
                    var fileContent = new StreamContent(bgFileUpload.PostedFile.InputStream);

                    fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                    {
                        Name = "bgFile",
                        FileName = bgFileUpload.PostedFile.FileName
                    };

                    content.Add(fileContent);

                    string ticketId = txtTicketId.Text;
                    int showEventInfo;
                    string requestUrl;

                    if (ddlPredefinedLayouts.SelectedValue == "SelfCreated")
                    {
                        showEventInfo = int.Parse(txtShowEventInfo.Text);
                        requestUrl = $"https://localhost:7104/api/PdfTemplate/GetPredefinedTemplate/GetPredefinedTemplate?showEventInfo={showEventInfo}&ticketId={ticketId}";
                    }
                    else
                    {
                        showEventInfo = int.Parse(ddlPredefinedLayouts.SelectedValue);
                        requestUrl = $"https://localhost:7104/api/PdfTemplate/GetPredefinedTemplate/GetPredefinedTemplate?showEventInfo={showEventInfo}&ticketId={ticketId}";
                    }

                    try
                    {
                        var response = await client.PostAsync(requestUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            var pdfBlob = await response.Content.ReadAsByteArrayAsync();
                            var pdfBlobUrl = "data:application/pdf;base64," + Convert.ToBase64String(pdfBlob);
                            litScript.Text = $"<iframe src='{pdfBlobUrl}' style='width: 100%; height: 600px'></iframe>";
                        }
                        else
                        {
                            litScript.Text = $"Error in PDF generation: {response.StatusCode} - {response.ReasonPhrase}";
                        }
                    }
                    catch (Exception ex)
                    {
                        litScript.Text = $"Error: {ex.Message}";
                    }
                }
                else
                {
                    litScript.Text = "Please provide all required inputs.";
                }
            }
            catch (Exception ex)
            {
                litScript.Text = $"Error: {ex.Message}";
            }
        }

        protected async void btnPreview_Click(object sender, EventArgs e)
        {

            if (bguploadFile.HasFile)
            {
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(bguploadFile.PostedFile.InputStream);

                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "bgFile",
                    FileName = bguploadFile.PostedFile.FileName
                };

                content.Add(fileContent);

                // Assuming GetTicketHandling() returns a TicketHandling object
                TicketHandling ticketHandling = GetTicketHandling();

                // Use reflection to add properties dynamically
                foreach (PropertyInfo property in ticketHandling.GetType().GetProperties())
                {
                    var value = property.GetValue(ticketHandling, null)?.ToString();
                    if (value != null)
                    {
                        content.Add(new StringContent(value), property.Name);
                    }
                }
                string customJson = GetCustomJson();

                // Ensure that customJson is properly serialized and added with the correct content type
                content.Add(new StringContent(customJson, System.Text.Encoding.UTF8, "application/json"), "customTextElementsJson");

                string ticketId = TextBox1.Text;
                string requestUrl = $"https://localhost:7104/api/PdfTemplate/CreateTemplate/CreateTemplate?ticketId={ticketId}&saveToDb=false";

                try
                {
                    var response = await client.PostAsync(requestUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var pdfBlob = await response.Content.ReadAsByteArrayAsync();
                        var pdfBlobUrl = "data:application/pdf;base64," + Convert.ToBase64String(pdfBlob);
                        litScript.Text = $"<iframe src='{pdfBlobUrl}' style='width: 100%; height: 600px'></iframe>";
                    }
                    else
                    {
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        litScript.Text = $"Error in PDF generation: {response.StatusCode} - {response.ReasonPhrase} - {errorResponse}";
                    }
                }
                catch (Exception ex)
                {
                    litScript.Text = $"Error: {ex.Message}";
                }
            }
            else
            {
                litScript.Text = "Please provide all required inputs.";
            }
        }

        protected async void btnSaveToDb_Click(object sender, EventArgs e)
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(bguploadFile.PostedFile.InputStream);

            fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = "bgFile",
                FileName = bguploadFile.PostedFile.FileName
            };

            content.Add(fileContent);

            // Assuming GetTicketHandling() returns a TicketHandling object
            TicketHandling ticketHandling = GetTicketHandling();

            // Use reflection to add properties dynamically
            foreach (PropertyInfo property in ticketHandling.GetType().GetProperties())
            {
                var value = property.GetValue(ticketHandling, null)?.ToString();
                if (value != null)
                {
                    content.Add(new StringContent(value), property.Name);
                }
            }
            string customJson = GetCustomJson();

            // Ensure that customJson is properly serialized and added with the correct content type
            content.Add(new StringContent(customJson, System.Text.Encoding.UTF8, "application/json"), "customTextElementsJson");

            string ticketId = TextBox1.Text;
            string requestUrl = $"https://localhost:7104/api/PdfTemplate/CreateTemplate/CreateTemplate?ticketId={ticketId}&saveToDb=true";

            try
            {
                var response = await client.PostAsync(requestUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    litScript.Text = "Saved to Database!";
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    litScript.Text = $"Error in saving PDF template to database: {response.StatusCode} - {errorResponse}";
                }
            }
            catch (Exception ex)
            {
                litScript.Text = $"Errorrrr: {ex.Message}";
            }
        }

        private TicketHandling GetTicketHandling()
        {
            var ticketHandling = new TicketHandling()
            {
                // Include properties
                IncludeStrukturArtikel = chkIncludeStrukturArtikel.Checked,
                IncludeDescription = chkIncludeDescription.Checked,
                IncludeArtNotText = chkIncludeArtNotText.Checked,
                IncludeRutBokstav = chkIncludeRutBokstav.Checked,
                IncludeArtNr = chkIncludeArtNr.Checked,
                IncludePrice = chkIncludePrice.Checked,
                IncludeServiceFee = chkIncludeServiceFee.Checked,
                IncludeArtName = chkIncludeArtName.Checked,
                IncludeChairRow = chkIncludeChairRow.Checked,
                IncludeChairNr = chkIncludeChairNr.Checked,
                IncludeEventDate = chkIncludeEventDate.Checked,
                IncludeEventName = chkIncludeEventName.Checked,
                IncludeSubEventName = chkIncludeSubEventName.Checked,
                IncludeLogorad1 = chkIncludeLogorad1.Checked,
                IncludeLogorad2 = chkIncludeLogorad2.Checked,
                IncludeSection = chkIncludeSection.Checked,
                IncludeBookingNr = chkIncludeBookingNr.Checked,
                IncludeWebBookingNr = chkIncludeWebBookingNr.Checked,
                IncludeFacilityName = chkIncludeFacilityName.Checked,
                IncludeAd = chkIncludeAd.Checked,
                IncludeContactPerson = chkIncludeContactPerson.Checked,
                IncludeEmail = chkIncludeEmail.Checked,
                IncludeDatum = chkIncludeDatum.Checked,
                IncludeEntrance = chkIncludeEntrance.Checked,
                IncludeWebbcode = chkIncludeWebbcode.Checked,
                IncludeScissorsLine = chkIncludeScissorsLine.Checked,

                //// Position properties
                ArtNrPositionX = TryParseFloat(txtArtNrPositionX.Text),
                ArtNrPositionY = TryParseFloat(txtArtNrPositionY.Text),
                PricePositionX = TryParseFloat(txtPricePositionX.Text),
                PricePositionY = TryParseFloat(txtPricePositionY.Text),
                ServiceFeePositionX = TryParseFloat(txtServiceFeePositionX.Text),
                ServiceFeePositionY = TryParseFloat(txtServiceFeePositionY.Text),
                ArtNamePositionX = TryParseFloat(txtArtNamePositionX.Text),
                ArtNamePositionY = TryParseFloat(txtArtNamePositionY.Text),
                ChairRowPositionX = TryParseFloat(txtChairRowPositionX.Text),
                ChairRowPositionY = TryParseFloat(txtChairRowPositionY.Text),
                ChairNrPositionX = TryParseFloat(txtChairNrPositionX.Text),
                ChairNrPositionY = TryParseFloat(txtChairNrPositionY.Text),
                EventDatePositionX = TryParseFloat(txtEventDatePositionX.Text),
                EventDatePositionY = TryParseFloat(txtEventDatePositionY.Text),
                EventNamePositionX = TryParseFloat(txtEventNamePositionX.Text),
                EventNamePositionY = TryParseFloat(txtEventNamePositionY.Text),
                SubEventNamePositionX = TryParseFloat(txtSubEventNamePositionX.Text),
                SubEventNamePositionY = TryParseFloat(txtSubEventNamePositionY.Text),
                Logorad1PositionX = TryParseFloat(txtLogorad1PositionX.Text),
                Logorad1PositionY = TryParseFloat(txtLogorad1PositionY.Text),
                Logorad2PositionX = TryParseFloat(txtLogorad2PositionX.Text),
                Logorad2PositionY = TryParseFloat(txtLogorad2PositionY.Text),
                LocationPositionX = TryParseFloat(txtLocationPositionX.Text),
                LocationPositionY = TryParseFloat(txtLocationPositionY.Text),
                SectionPositionX = TryParseFloat(txtSectionPositionX.Text),
                SectionPositionY = TryParseFloat(txtSectionPositionY.Text),
                BookingNrPositionX = TryParseFloat(txtBookingNrPositionX.Text),
                BookingNrPositionY = TryParseFloat(txtBookingNrPositionY.Text),
                WebBookingNumberPositionX = TryParseFloat(txtWebBookingNumberPositionX.Text),
                WebBookingNumberPositionY = TryParseFloat(txtWebBookingNumberPositionY.Text),
                FacilityNamePositionX = TryParseFloat(txtFacilityNamePositionX.Text),
                FacilityNamePositionY = TryParseFloat(txtFacilityNamePositionY.Text),
                AdPositionX = TryParseFloat(txtAdPositionX.Text),
                AdPositionY = TryParseFloat(txtAdPositionY.Text),
                StrukturArtikelPositionX = TryParseFloat(txtStrukturArtikelPositionX.Text),
                StrukturArtikelPositionY = TryParseFloat(txtStrukturArtikelPositionY.Text),
                DescriptionPositionX = TryParseFloat(txtDescriptionPositionX.Text),
                DescriptionPositionY = TryParseFloat(txtDescriptionPositionY.Text),
                ArtNotTextPositionX = TryParseFloat(txtArtNotTextPositionX.Text),
                ArtNotTextPositionY = TryParseFloat(txtArtNotTextPositionY.Text),
                NamePositionX = TryParseFloat(txtNamePositionX.Text),
                NamePositionY = TryParseFloat(txtNamePositionY.Text),
                EmailPositionX = TryParseFloat(txtEmailPositionX.Text),
                EmailPositionY = TryParseFloat(txtEmailPositionY.Text),
                DatumPositionX = TryParseFloat(txtDatumPositionX.Text),
                DatumPositionY = TryParseFloat(txtDatumPositionY.Text),
                EntrancePositionX = TryParseFloat(txtEntrancePositionX.Text),
                EntrancePositionY = TryParseFloat(txtEntrancePositionY.Text),
                RutBokstavPositionX = TryParseFloat(txtRutBokstavPositionX.Text),
                RutBokstavPositionY = TryParseFloat(txtRutBokstavPositionY.Text),
                WebbcodePositionX = TryParseFloat(txtWebbcodePositionX.Text),
                WebbcodePositionY = TryParseFloat(txtWebbcodePositionY.Text),
                BarcodePositionX = TryParseFloat(txtBarcodePositionX.Text),
                BarcodePositionY = TryParseFloat(txtBarcodePositionY.Text),

                // Other properties
                UseQRCode = chkUseQRCode.Checked,
                FlipBarcode = chkFlipBarcode.Checked,
            };
            return ticketHandling;
        }

        private float? TryParseFloat(string input)
        {
            if (float.TryParse(input, out float result))
            {
                return result;
            }
            return null;
        }

        private string GetCustomJson()
        {
            var customTextElements = GetCustomTextElements() ?? new List<CustomTextElement>();
            return JsonConvert.SerializeObject(customTextElements);
        }

        public class TicketHandling
        {
            // Properties for customization options
            public bool IncludeStrukturArtikel { get; set; }
            public bool IncludeDescription { get; set; }
            public bool IncludeArtNotText { get; set; }
            #region helt tom, inte null
            public bool IncludeRutBokstav { get; set; }
            #endregion
            public bool IncludeArtNr { get; set; }
            public bool IncludePrice { get; set; }
            public bool IncludeServiceFee { get; set; }
            public bool IncludeArtName { get; set; }
            public bool IncludeChairRow { get; set; }
            public bool IncludeChairNr { get; set; }
            public bool IncludeEventDate { get; set; }
            public bool IncludeEventName { get; set; }
            public bool IncludeSubEventName { get; set; }
            public bool IncludeLogorad1 { get; set; }
            public bool IncludeLogorad2 { get; set; }
            public bool IncludeSection { get; set; }
            public bool IncludeBookingNr { get; set; }
            public bool IncludeWebBookingNr { get; set; }
            public bool IncludeFacilityName { get; set; }
            public bool IncludeAd { get; set; }

            public bool IncludeContactPerson { get; set; }
            public bool IncludeEmail { get; set; }
            public bool IncludeDatum { get; set; }
            public bool IncludeEntrance { get; set; }
            public bool IncludeWebbcode { get; set; }
            public bool IncludeScissorsLine { get; set; }
            #region Position 
            // Properties for positioning elements on the ticket
            public float? ArtNrPositionX { get; set; }
            public float? ArtNrPositionY { get; set; }

            public float? PricePositionX { get; set; }
            public float? PricePositionY { get; set; }

            public float? ServiceFeePositionX { get; set; }
            public float? ServiceFeePositionY { get; set; }

            public float? ArtNamePositionX { get; set; }
            public float? ArtNamePositionY { get; set; }

            public float? ChairRowPositionX { get; set; }
            public float? ChairRowPositionY { get; set; }

            public float? ChairNrPositionX { get; set; }
            public float? ChairNrPositionY { get; set; }

            public float? EventDatePositionX { get; set; }
            public float? EventDatePositionY { get; set; }

            public float? EventNamePositionX { get; set; }
            public float? EventNamePositionY { get; set; }

            public float? SubEventNamePositionX { get; set; }
            public float? SubEventNamePositionY { get; set; }

            public float? Logorad1PositionX { get; set; }
            public float? Logorad1PositionY { get; set; }

            public float? Logorad2PositionX { get; set; }
            public float? Logorad2PositionY { get; set; }

            public float? LocationPositionX { get; set; }
            public float? LocationPositionY { get; set; }

            public float? SectionPositionX { get; set; }
            public float? SectionPositionY { get; set; }

            public float? BookingNrPositionX { get; set; }
            public float? BookingNrPositionY { get; set; }

            public float? WebBookingNumberPositionX { get; set; }
            public float? WebBookingNumberPositionY { get; set; }

            public float? FacilityNamePositionX { get; set; }
            public float? FacilityNamePositionY { get; set; }

            public float? AdPositionX { get; set; }
            public float? AdPositionY { get; set; }

            public float? StrukturArtikelPositionX { get; set; }
            public float? StrukturArtikelPositionY { get; set; }

            public float? DescriptionPositionX { get; set; }
            public float? DescriptionPositionY { get; set; }

            public float? ArtNotTextPositionX { get; set; }
            public float? ArtNotTextPositionY { get; set; }

            public float? NamePositionX { get; set; }
            public float? NamePositionY { get; set; }

            public float? EmailPositionX { get; set; }
            public float? EmailPositionY { get; set; }

            public float? DatumPositionX { get; set; }
            public float? DatumPositionY { get; set; }

            public float? EntrancePositionX { get; set; }
            public float? EntrancePositionY { get; set; }

            public float? RutBokstavPositionX { get; set; }
            public float? RutBokstavPositionY { get; set; }

            public float? WebbcodePositionX { get; set; }
            public float? WebbcodePositionY { get; set; }

            public float? BarcodePositionX { get; set; }
            public float? BarcodePositionY { get; set; }
            #endregion

            // Property to choose between QR code and Barcode
            public bool UseQRCode { get; set; }

            public bool FlipBarcode { get; set; }

            //public List<CustomTextElement> CustomTextElements { get; set; } = new List<CustomTextElement>();

            public TicketHandling()
            {
            }
        }
        public class CustomTextElement
        {
            public string Text { get; set; }
            public float PositionX { get; set; } 
            public float PositionY { get; set; }
            public float FontSize { get; set; } = 10;
            public string Color { get; set; }

            public CustomTextElement(string text, float x, float y, float fontSize, string color)
            {
                Text = text;
                PositionX = x;
                PositionY = y;
                FontSize = fontSize;
                Color = color;
            }
        }

        protected void btnAddCustomText_Click(object sender, EventArgs e)
        {
            var text = txtCustomText.Text;
            var positionX = float.Parse(txtPositionX.Text);
            var positionY = float.Parse(txtPositionY.Text);
            var fontSize = float.Parse(txtFontSize.Text);
            var color = txtColor.Text;

            var customElement = new CustomTextElement(text, positionX, positionY, fontSize, color);
            AddCustomElementToList(customElement);
        }

        private void AddCustomElementToList(CustomTextElement customElement)
        {
            var customElements = GetCustomTextElements();
            customElements.Add(customElement);
            // Store the updated list in session or view state
            Session["CustomTextElements"] = customElements;
        }

        private List<CustomTextElement> GetCustomTextElements()
        {
            // Retrieve the list from session or view state
            var customElements = Session["CustomTextElements"] as List<CustomTextElement>;
            return customElements ?? new List<CustomTextElement>();
        }
    }
}
