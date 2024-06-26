﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Models;
using Newtonsoft.Json;
using Services;
using System.Net.Http.Headers;
using System.Text;

namespace AppBlazor.Components
{
    public partial class PdfCreatorComponent
    {
        // Dependency Injection Properties
        [Inject]
        public ConfigService ConfigService { get; set; } = default!;

        [Inject]
        public HttpClient HttpClient { get; set; } = default!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        // Component State Properties
        [Parameter] public EventCallback<bool> OnSave { get; set; }

        [Parameter] public bool IsEditMode { get; set; }
        [Parameter] public TicketHandling TicketHandling { get; set; } = new();
        [Parameter] public List<CustomTextElement> CustomTexts { get; set; } = new();
        [Parameter] public Guid TemplateId { get; set; }
        [Parameter] public string? TemplateName { get; set; }
        [Parameter] public int? ShowEventInfo { get; set; }

        private string pdfBase64 = string.Empty;
        private bool isPreviewLoading;
        private bool isSaveLoading;
        private string? ErrorMessage = string.Empty;
        public string? SuccessMessage { get; set; }
        private readonly Code Barcode = Code.Barcode;

        private readonly Dictionary<string, string> errorElementMap = new()
        {
            { "Vänligen välj en bakgrundsbild innan du fortsätter", "backgroundImageUpload" },
            { "Vänligen välj ett namn för din mall innan du fortsätter", "templateName" }
        };

        private string currentFocusElementId = string.Empty;
        private bool shouldFocusOnError;

        private string templateName = string.Empty;

        private ByteArrayContent? bgFileContent;

        private enum Tab
        {
            Info,
            CustomText,
            PropertyGroup,
            Others
        }

        private Tab selectedTab = Tab.Info;

        private void SelectTab(Tab tab)
        {
            selectedTab = tab;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            templateName = TemplateName ?? string.Empty;
        }

        private async Task HandleFileUpload(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file != null)
            {
                await using var stream = file.OpenReadStream();
                byte[] buffer = new byte[file.Size];
                await stream.ReadAsync(buffer);
                bgFileContent = new ByteArrayContent(buffer);
                bgFileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                bgFileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"bgFile\"",
                    FileName = $"\"{file.Name}\""
                };
                ErrorMessage = string.Empty;
            }
        }

        private static MultipartFormDataContent CreateMultipartFormDataContent(ByteArrayContent bgFileContent, string fileName, Dictionary<string, string> additionalFields)
        {
            var content = new MultipartFormDataContent
            {
                { bgFileContent, "bgFile", fileName }
            };

            foreach (var field in additionalFields)
            {
                content.Add(new StringContent(field.Value, Encoding.UTF8, "application/json"), field.Key);
            }

            return content;
        }

        private async Task HandleHttpResponse(HttpResponseMessage response, bool saveToDb)
        {
            if (response.IsSuccessStatusCode)
            {
                if (saveToDb)
                {
                    SuccessMessage = "Mall sparad!";
                    await OnSave.InvokeAsync(true);
                }
                else
                {
                    var pdfData = await response.Content.ReadAsByteArrayAsync();
                    pdfBase64 = Convert.ToBase64String(pdfData);
                }
            }
            else
            {
                var statusCode = response.StatusCode;
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Operation failed: {statusCode} - {(string.IsNullOrEmpty(errorContent) ? "No additional error information provided." : errorContent)}";
            }
        }

        private void SetErrorMessageAndResetLoading(string message)
        {
            ErrorMessage = message;
            isPreviewLoading = false;
            isSaveLoading = false;
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                SelectTab(Tab.Info);
                currentFocusElementId = errorElementMap.FirstOrDefault(x => ErrorMessage.Contains(x.Key)).Value ?? string.Empty;
                shouldFocusOnError = true;
                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (shouldFocusOnError && JSRuntime != null && !string.IsNullOrEmpty(currentFocusElementId))
            {
                shouldFocusOnError = false;
                await JSRuntime.InvokeVoidAsync("focusOnElementById", currentFocusElementId);
            }
        }

        private async Task CreatePdf(bool saveToDb)
        {
            isSaveLoading = saveToDb;
            isPreviewLoading = !saveToDb;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (bgFileContent == null)
            {
                SetErrorMessageAndResetLoading("Vänligen välj en bakgrundsbild innan du fortsätter");
                return;
            }

            if (saveToDb && string.IsNullOrWhiteSpace(templateName))
            {
                SetErrorMessageAndResetLoading("Vänligen välj ett namn för din mall innan du fortsätter");
                return;
            }

            var fileName = bgFileContent.Headers?.ContentDisposition?.FileName?.Trim('"') ?? "defaultFileName";

            var additionalFields = new Dictionary<string, string>
            {
                { "TicketHandlingJson", System.Text.Json.JsonSerializer.Serialize(TicketHandling) },
                { "CustomTextElementsJson", System.Text.Json.JsonSerializer.Serialize(CustomTexts) },
                { "Name", templateName },
                { "SaveToDb", saveToDb.ToString().ToLower() }
            };
            var content = CreateMultipartFormDataContent(bgFileContent, fileName, additionalFields);

            try
            {
                var response = await HttpClient.PostAsync(ConfigService.GetApiUrl($"/api/PdfTemplate/CreateTemplate?saveToDb={saveToDb}"), content);
                await HandleHttpResponse(response, saveToDb);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            }
            finally
            {
                isSaveLoading = false;
                isPreviewLoading = false;
            }
        }

        private async Task UpdateTemplate()
        {
            isSaveLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (bgFileContent == null)
            {
                SetErrorMessageAndResetLoading("Vänligen välj en bakgrundsbild innan du fortsätter");
                return;
            }
            if (string.IsNullOrWhiteSpace(templateName))
            {
                SetErrorMessageAndResetLoading("Vänligen välj ett namn för din mall innan du fortsätter");
                return;
            }

            var fileName = bgFileContent.Headers?.ContentDisposition?.FileName?.Trim('"') ?? "defaultFileName";

            var additionalFields = new Dictionary<string, string>
            {
                { "TicketTemplateId", TemplateId.ToString() },
                { "Name", templateName },
                { "TicketHandlingJson", JsonConvert.SerializeObject(TicketHandling) }
            };

            var content = CreateMultipartFormDataContent(bgFileContent, fileName, additionalFields);

            try
            {
                var response = await HttpClient.PutAsync(ConfigService.GetApiUrl("/api/PdfTemplate/UpdateTemplate"), content);
                await HandleHttpResponse(response, true);
            }
            catch (HttpRequestException httpException)
            {
                ErrorMessage = "A network error occurred. Please check your connection and try again.";
                Console.WriteLine($"HttpRequestException: {httpException.Message}");
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred. Please try again.";
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                isSaveLoading = false;
            }
        }
    }
}