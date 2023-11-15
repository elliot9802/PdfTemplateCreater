using System;
using System.IO;
using System.Web.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Pipes;

namespace FrontEndEditor
{
    public partial class _Default : Page
    {
        HttpClient client = new HttpClient();

        protected async void btnPreview_Click(object sender, EventArgs e)
        {
            if (bgFileUpload.HasFile && int.TryParse(txtId.Text, out int id))
            {
                // Prepare the MultipartFormDataContent
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(bgFileUpload.PostedFile.InputStream);
                fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                {
                    Name = "bgFile",
                    FileName = bgFileUpload.PostedFile.FileName
                };
                content.Add(fileContent);
                content.Add(new StringContent(txtId.Text), "Id");
                content.Add(new StringContent(txtBarcodeContent.Text), "BarcodeContent");

                try
                {
                    // Send the POST request to the API
                    var response = await client.PostAsync("https://localhost:7104/api/PdfTemplate/createPdf", content);

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
                        litScript.Text = "Error in PDF generation.";
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
    }
}
