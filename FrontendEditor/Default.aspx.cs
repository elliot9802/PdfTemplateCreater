using System;
using System.IO;
using System.Web.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Pipes;
using Newtonsoft.Json;

namespace FrontEndEditor
{
    public partial class _Default : Page
    {
        HttpClient client = new HttpClient();

        protected async void btnPreview_Click(object sender, EventArgs e)
        {
            // Construct the TicketHandling object with the selected options
            var ticketHandling = new
            {
                IncludePrice = chkIncludePrice.Checked,
                // Repeat for other options
                // ...
                PBookIdPositionX = double.Parse(txtBarcodePositionX.Text), // Make sure to validate and convert appropriately
                                                                           // Repeat for other position fields
                                                                           // ...
                UseQRCode = true // or however you determine this value
            };

            // Create the main object
            var templateRequest = new
            {
                TicketTemplateId = Guid.NewGuid(), // or however you obtain this value
                TicketsHandling = ticketHandling,
                TicketHandlingJson = JsonConvert.SerializeObject(ticketHandling), // If you need to send the JSON as a separate field
                ShowEventInfo = int.Parse(txtShowEventInfo.Text) // Make sure to validate and convert appropriately
            };

            // Serialize the main object to JSON
            var templateJson = JsonConvert.SerializeObject(templateRequest);

            // Prepare the HTTP request content
            var content = new StringContent(templateJson, System.Text.Encoding.UTF8, "application/json");


            try
            {
                // Send the POST request to the API
                var response = await client.PostAsync("https://localhost:7104/api/PdfTemplate/CreateTicketTemplate", content);

                // Handle the response
                if (response.IsSuccessStatusCode)
                {
                    // Read the response as a blob
                    var pdfBlob = await response.Content.ReadAsByteArrayAsync();

                    // Create a local URL for the blob object
                    var pdfBlobUrl = "data:application/pdf;base64," + Convert.ToBase64String(pdfBlob);

                    // Set the iframe source to the local URL
                    litScript.Text = $"<iframe src='{pdfBlobUrl}' style='width: 100%; height: 600px'></iframe>";
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    litScript.Text = $"Error in PDF generation: {response.StatusCode} - {errorResponse}";
                }
            }
            catch (Exception ex)
            {
                litScript.Text = $"Error: {ex.Message}";
            }
        }
    }
}

