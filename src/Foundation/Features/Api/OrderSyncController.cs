
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Catalog.Provider;
using EPiServer.Commerce.Catalog;
using EPiServer.Security;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.DataAccess;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;

namespace Foundation.Features.Api
{
    [Route("Ordersync")]
    public class OrderSyncController : Controller
    {
        private readonly IContentRepository _contentRepository;
        private readonly ReferenceConverter _referenceConverter;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderSearchService _orderSearchService;

        public OrderSyncController(
            IContentRepository contentRepository,
            ReferenceConverter referenceConverter,
            IOrderRepository orderRepository,
            IOrderSearchService orderSearchService)
        {
            _contentRepository = contentRepository;
            _referenceConverter = referenceConverter;
            _orderRepository = orderRepository;
            _orderSearchService = orderSearchService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            Console.WriteLine("Upload page accessed");
            return View("/Views/Shared/Ordersync.cshtml");
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UploadExcelAndUpdateOrders()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                TempData["Status"] = "No file uploaded.";
                return RedirectToAction("Index");
            }

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();
                var rowCount = worksheet.Dimension.Rows;

                int successCount = 0;
                int failedCount = 0;

                for (int row = 2; row <= rowCount; row++) // Assuming row 1 is header
                {
                    var orderId = worksheet.Cells[row, 1].Text?.Trim();
                    var statusText = worksheet.Cells[row, 2].Text?.Trim();

                    if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(statusText))
                    {
                        failedCount++;
                        continue;
                    }

                    if (!OrderStatus.TryParse(statusText, out var status))
                    {
                        failedCount++;
                        continue;
                    }

                    var criteria = new OrderSearchCriteria
                    {
                        OrderNumber = orderId,
                        RecordsToRetrieve = 1
                    };

                    var results = _orderSearchService.FindPurchaseOrders(criteria);
                    var purchaseOrder = results.Orders.FirstOrDefault() as IPurchaseOrder;

                    if (purchaseOrder != null)
                    {
                        purchaseOrder.OrderStatus = status;
                        _orderRepository.Save(purchaseOrder);
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }

                TempData["Status"] = $"Order update completed. Success: {successCount}";
            }
            catch (Exception ex)
            {
                TempData["Status"] = $"Error processing file: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
