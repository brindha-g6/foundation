
using Microsoft.AspNetCore.Mvc;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.InventoryService;
using OfficeOpenXml;
using System.IO;

namespace Foundation.Features.Api
{
    [Route("InventorySync")]
    public class InventorySyncController : Controller

    {
        private readonly IInventoryService _inventoryService;

        public InventorySyncController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("/Views/Shared/InventorySync.cshtml");
        }

        [HttpPost("ImportExcel")]
        public async Task<IActionResult> ImportExcel(IFormFile excelFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Status"] = "Please upload a valid Excel file.";
                return RedirectToAction("Index");
            }

            try
            {


                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    TempData["Status"] = "No worksheet found in the Excel file.";
                    return RedirectToAction("Index");
                }

                int rowCount = worksheet.Dimension.Rows;
                int updatedCount = 0;
                string warehouseCode = "default"; // Replace with your actual warehouse code

                for (int row = 2; row <= rowCount; row++)
                {
                    string sku = worksheet.Cells[row, 1].Text?.Trim();
                    string quantityText = worksheet.Cells[row, 2].Text?.Trim();

                    if (string.IsNullOrEmpty(sku) || !int.TryParse(quantityText, out int quantity))
                        continue;

                    var inventoryRecord = _inventoryService.Get(sku, warehouseCode);

                    if (inventoryRecord != null)
                    {
                        inventoryRecord.PurchaseAvailableQuantity = quantity;
                        _inventoryService.Save(new[] { inventoryRecord });
                        updatedCount++;
                    }
                }

                TempData["Status"] = $"{updatedCount} inventory records updated from Excel.";
            }
            catch (Exception ex)
            {
                TempData["Status"] = $"Error processing Excel file: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }

    public class InventoryUpdateDto
    {
        public string Sku { get; set; }
        public int Quantity { get; set; }
    }
}
