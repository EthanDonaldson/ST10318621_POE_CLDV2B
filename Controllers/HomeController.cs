using Microsoft.AspNetCore.Mvc;
using POE_CLDV2B.Models;
using POE_CLDV2B.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace POE_CLDV2B.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlobService _blobService;
        private readonly TableService _tableService;
        private readonly QueueService _queueService;
        private readonly FileService _fileService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HomeController> _Logger;
        private readonly IConfiguration _configuration; // Inject configuration

        public HomeController(
            BlobService blobService,
            TableService tableService,
            QueueService queueService,
            FileService fileService,
            IHttpClientFactory httpClientFactory,
            ILogger<HomeController> logger,
            IConfiguration configuration) // Add IConfiguration parameter
        {
            _blobService = blobService;
            _tableService = tableService;
            _queueService = queueService;
            _fileService = fileService;
            _httpClientFactory = httpClientFactory;
            _Logger = logger;
            _configuration = configuration; // Assign injected configuration
        }

        public IActionResult Index()
        {
            var model = new CustomerProfile();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadBlob(IFormFile file)
        {
            if (file != null)
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    var baseUrl = _configuration["AzureFunctions:UploadBlob"];
                    var requestUrl = $"{baseUrl}&fileName={file.FileName}";

                    using var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.PostAsync(requestUrl, new StreamContent(stream));

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "Error uploading blob");
                }
            }
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> StoreToTable(CustomerProfile profile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient();

                    var baseUrl = _configuration["AzureFunctions:StoreToTable"]; // Correct configuration access
                    var requestUrl = $"{baseUrl}&tableName=st10318621table&partitionKey={profile.PartitionKey}&rowKey={profile.RowKey}&FirstName={profile.FirstName}&LastName={profile.LastName}&phoneNumber={profile.PhoneNumber}&Email={profile.Email}";

                    var response = await httpClient.PostAsync(requestUrl, null);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "Error adding customer profile");
                }
            }
            return View("Index", profile);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessQueueMessages(string orderId)
        {
            try
            {
                var baseUrl = _configuration["AzureFunctions:ProcessQueueMessages"];
                var requestUrl = $"{baseUrl}&orderId={orderId}";

                using var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsync(requestUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error processing order");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null)
            {
                try
                {
                    using var stream = file.OpenReadStream();
                    var baseUrl = _configuration["AzureFunctions:UploadFile"];
                    var requestUrl = $"{baseUrl}&fileName={file.FileName}";

                    using var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.PostAsync(requestUrl, new StreamContent(stream));

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "Error uploading file");
                }
            }
            return View("Index");
        }
    }
}
