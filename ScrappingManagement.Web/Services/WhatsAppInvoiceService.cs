using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScrappingManagement.Web.Services
{
     public class WhatsAppInvoiceService
     {
          private readonly HttpClient _httpClient;
		private readonly IConfiguration _config;

		private string _phoneNumberId = "YOUR_PHONE_NUMBER_ID"; // From Meta Developer Console
          private string _accessToken = "YOUR_PERMANENT_ACCESS_TOKEN"; // Generate in Meta Business Manager

          public WhatsAppInvoiceService(IConfiguration config)
          {
			_config = config;
			_httpClient = new HttpClient();
			_phoneNumberId = _config["WhatsApp:PhoneNumberId"];
			_accessToken = _config["WhatsApp:AccessToken"];
			_httpClient.DefaultRequestHeaders.Authorization =
                   new AuthenticationHeaderValue("Bearer", _accessToken);
          }

          /// <summary>
          /// Send invoice summary as a text message.
          /// </summary>
          public async Task SendInvoiceTextAsync(string toPhone, string invoiceNumber, decimal amount, string dueDate)
          {
			
			string invoiceText = $"📄 Invoice #{invoiceNumber}\nAmount: ${amount}\nDue Date: {dueDate}";

               var payload = new
               {
                    messaging_product = "whatsapp",
                    to = toPhone,
                    type = "text",
                    text = new { body = invoiceText }
               };

               await PostToWhatsApp(payload);
          }

          /// <summary>
          /// Send invoice as a PDF document.
          /// </summary>
          public async Task SendInvoicePdfAsync(string toPhone, string pdfUrl, string fileName)
          {
               var payload = new
               {
                    messaging_product = "whatsapp",
                    to = toPhone,
                    type = "document",
                    document = new
                    {
                         link = pdfUrl,
                         filename = fileName
                    }
               };

               await PostToWhatsApp(payload);
          }

          /// <summary>
          /// Internal helper to send API request.
          /// </summary>
          private async Task PostToWhatsApp(object payload)
          {
               var url = $"https://graph.facebook.com/v20.0/{_phoneNumberId}/messages";

               var content = new StringContent(
                   JsonSerializer.Serialize(payload),
                   Encoding.UTF8,
                   "application/json"
               );

               var response = await _httpClient.PostAsync(url, content);
               var responseContent = await response.Content.ReadAsStringAsync();

               if (!response.IsSuccessStatusCode)
               {
                    throw new HttpRequestException($"WhatsApp API error: {response.StatusCode} - {responseContent}");
               }

               System.Console.WriteLine($"✅ WhatsApp message sent: {responseContent}");
          }
     }
}