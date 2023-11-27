using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
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
            }
            else if (!string.IsNullOrEmpty(ddlLayoutChoice.SelectedValue))
            {
                pnlCreateNewLayout.Visible = false;
                pnlPredefinedLayout.Visible = true;
            }
            else
            {
                // Hide controls for creating a new layout
                pnlCreateNewLayout.Visible = false;
                pnlPredefinedLayout.Visible = false;
            }
        }

        protected async void btnGetPredefinedTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                //int showEventInfo = int.Parse(txtShowEventInfo.Text); // Validate and convert appropriately
                int showEventInfo = int.Parse(ddlLayoutChoice.SelectedValue);
                int ticketId = int.Parse(txtTicketId.Text); // Obtain the ticketId

                if (fileUploadBg.HasFile)
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        formData.Add(new StringContent(showEventInfo.ToString()), "showEventInfo");
                        formData.Add(new StringContent(ticketId.ToString()), "ticketId");
                        formData.Add(new StreamContent(fileUploadBg.PostedFile.InputStream), "bgFile", fileUploadBg.FileName);

                        var response = await client.PostAsync("https://localhost:7104/api/PdfTemplate/GetPredefinedTemplate", formData);

                        if (response.IsSuccessStatusCode)
                        {
                            var templateJson = await response.Content.ReadAsStringAsync();
                            // Process the template JSON as needed
                        }
                        else
                        {
                            litScript.Text = $"Error fetching predefined template: {response.StatusCode}";
                        }
                    }
                }
                else
                {
                    litScript.Text = "Please upload a background file.";
                }
            }
            catch (Exception ex)
            {
                litScript.Text = $"Error: {ex.Message}";
            }
        }

        protected async void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                string templateJson = await GetTemplateJson();
                var content = new StringContent(templateJson, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7104/api/PdfTemplate/CreateTemplate?saveToDb=false", content);

                if (response.IsSuccessStatusCode)
                {
                    var pdfBlob = await response.Content.ReadAsByteArrayAsync();
                    var pdfBlobUrl = "data:application/pdf;base64," + Convert.ToBase64String(pdfBlob);
                    litScript.Text = $"<iframe src='{pdfBlobUrl}' style='width: 100%; height: 600px'></iframe>";
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    litScript.Text = $"Error in PDF layout generation: {response.StatusCode} - {errorResponse}";
                }
            }
            catch (Exception ex)
            {
                litScript.Text = $"Error: {ex.Message}";
            }
        }

        protected async void btnSaveToDb_Click(object sender, EventArgs e)
        {
            try
            {
                string templateJson = await GetTemplateJson();
                var content = new StringContent(templateJson, System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7104/api/PdfTemplate/CreateTemplate?saveToDb=true", content);

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

        private async Task<string> GetTemplateJson()
        {
            //var customTextElements = GetCustomTextElements();
            //var jsonCustomTextElements = JsonConvert.SerializeObject(customTextElements);

            var ticketHandling = new
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
                //ArtNrPositionX = float.Parse(txtArtNrPositionX.Text),
                //ArtNrPositionY = float.Parse(txtArtNrPositionY.Text),
                //PricePositionX = float.Parse(txtPricePositionX.Text),
                //PricePositionY = float.Parse(txtPricePositionY.Text),
                //ServiceFeePositionX = float.Parse(txtServiceFeePositionX.Text),
                //ServiceFeePositionY = float.Parse(txtServiceFeePositionY.Text),
                //ArtNamePositionX = float.Parse(txtArtNamePositionX.Text),
                //ArtNamePositionY = float.Parse(txtArtNamePositionY.Text),
                //ChairRowPositionX = float.Parse(txtChairRowPositionX.Text),
                //ChairRowPositionY = float.Parse(txtChairRowPositionY.Text),
                //ChairNrPositionX = float.Parse(txtChairNrPositionX.Text),
                //ChairNrPositionY = float.Parse(txtChairNrPositionY.Text),
                //EventDatePositionX = float.Parse(txtEventDatePositionX.Text),
                //EventDatePositionY = float.Parse(txtEventDatePositionY.Text),
                //EventNamePositionX = float.Parse(txtEventNamePositionX.Text),
                //EventNamePositionY = float.Parse(txtEventNamePositionY.Text),
                //SubEventNamePositionX = float.Parse(txtSubEventNamePositionX.Text),
                //SubEventNamePositionY = float.Parse(txtSubEventNamePositionY.Text),
                //Logorad1PositionX = float.Parse(txtLogorad1PositionX.Text),
                //Logorad1PositionY = float.Parse(txtLogorad1PositionY.Text),
                //Logorad2PositionX = float.Parse(txtLogorad2PositionX.Text),
                //Logorad2PositionY = float.Parse(txtLogorad2PositionY.Text),
                //LocationPositionX = float.Parse(txtLocationPositionX.Text),
                //LocationPositionY = float.Parse(txtLocationPositionY.Text),
                //SectionPositionX = float.Parse(txtSectionPositionX.Text),
                //SectionPositionY = float.Parse(txtSectionPositionY.Text),
                //BookingNrPositionX = float.Parse(txtBookingNrPositionX.Text),
                //BookingNrPositionY = float.Parse(txtBookingNrPositionY.Text),
                //WebBookingNumberPositionX = float.Parse(txtWebBookingNumberPositionX.Text),
                //WebBookingNumberPositionY = float.Parse(txtWebBookingNumberPositionY.Text),
                //FacilityNamePositionX = float.Parse(txtFacilityNamePositionX.Text),
                //FacilityNamePositionY = float.Parse(txtFacilityNamePositionY.Text),
                //AdPositionX = float.Parse(txtAdPositionX.Text),
                //AdPositionY = float.Parse(txtAdPositionY.Text),
                //StrukturArtikelPositionX = float.Parse(txtStrukturArtikelPositionX.Text),
                //StrukturArtikelPositionY = float.Parse(txtStrukturArtikelPositionY.Text),
                //DescriptionPositionX = float.Parse(txtDescriptionPositionX.Text),
                //DescriptionPositionY = float.Parse(txtDescriptionPositionY.Text),
                //ArtNotTextPositionX = float.Parse(txtArtNotTextPositionX.Text),
                //ArtNotTextPositionY = float.Parse(txtArtNotTextPositionY.Text),
                //NamePositionX = float.Parse(txtNamePositionX.Text),
                //NamePositionY = float.Parse(txtNamePositionY.Text),
                //EmailPositionX = float.Parse(txtEmailPositionX.Text),
                //EmailPositionY = float.Parse(txtEmailPositionY.Text),
                //DatumPositionX = float.Parse(txtDatumPositionX.Text),
                //DatumPositionY = float.Parse(txtDatumPositionY.Text),
                //EntrancePositionX = float.Parse(txtEntrancePositionX.Text),
                //EntrancePositionY = float.Parse(txtEntrancePositionY.Text),
                //RutBokstavPositionX = float.Parse(txtRutBokstavPositionX.Text),
                //RutBokstavPositionY = float.Parse(txtRutBokstavPositionY.Text),
                //WebbcodePositionX = float.Parse(txtWebbcodePositionX.Text),
                //WebbcodePositionY = float.Parse(txtWebbcodePositionY.Text),
                //BarcodePositionX = float.Parse(txtBarcodePositionX.Text),
                //BarcodePositionY = float.Parse(txtBarcodePositionY.Text),

                // Other properties
                UseQRCode = chkUseQRCode.Checked,
                FlipBarcode = chkFlipBarcode.Checked,
                //CustomTextElementsJson = jsonCustomTextElements // Implement this method based on your UI and requirements
            };

            var templateRequest = new
            {
                //TicketTemplateId = Guid.NewGuid(), // or however you obtain this value
                TicketsHandling = ticketHandling,
                TicketHandlingJson = JsonConvert.SerializeObject(ticketHandling),
                ShowEventInfo = int.Parse(txtShowEventInfo.Text) // Validate and convert appropriately
            };

            return JsonConvert.SerializeObject(templateRequest);
        }

        //public class CustomTextElementDTO
        //{
        //    public string Text { get; set; }
        //    public float PositionX { get; set; }
        //    public float PositionY { get; set; }
        //    public float FontSize { get; set; }
        //    public string Color { get; set; }

        //    public CustomTextElementDTO(string text, float x, float y, float fontSize, string color)
        //    {
        //        Text = text;
        //        PositionX = x;
        //        PositionY = y;
        //        FontSize = fontSize;
        //        Color = color;
        //    }
        //}

        private float? ParseFloat(string input)
        {
            if (float.TryParse(input, out float result))
            {
                return result;
            }
            return null; // or handle the error as needed
        }

        //protected void btnAddCustomText_Click(object sender, EventArgs e)
        //{
        //    var text = txtCustomText.Text;
        //    var positionX = float.Parse(txtPositionX.Text);
        //    var positionY = float.Parse(txtPositionY.Text);
        //    var fontSize = float.Parse(txtFontSize.Text);
        //    var color = txtColor.Text;

        //    var customElement = new CustomTextElementDTO(text, positionX, positionY, fontSize, color);
        //    AddCustomElementToList(customElement);
        //}

        //private void AddCustomElementToList(CustomTextElementDTO customElement)
        //{
        //    var customElements = GetCustomTextElements();
        //    customElements.Add(customElement);
        //    // Store the updated list in session or view state
        //    Session["CustomTextElements"] = customElements;
        //}

        //private List<CustomTextElementDTO> GetCustomTextElements()
        //{
        //    // Retrieve the list from session or view state
        //    var customElements = Session["CustomTextElements"] as List<CustomTextElementDTO>;
        //    return customElements ?? new List<CustomTextElementDTO>();
        //}

    }
}

