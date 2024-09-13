using ImportApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace ImportApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new ImportViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile uploadedFile, int rowNumber, ImportViewModel model)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    await uploadedFile.CopyToAsync(stream);
                    HttpContext.Session.Set("UploadedFile", stream.ToArray());

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var headers = new List<string>();
                        var columnValues = new Dictionary<string, List<string>>();

                        rowNumber++;

                        foreach (var staticColumn in ImportViewModel.StaticColumns)
                        {
                            columnValues[staticColumn] = new List<string>();
                        }

                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            var header = worksheet.Cells[1, col].Text;
                            if (ImportViewModel.StaticColumns.Contains(header))
                            {
                                headers.Add(header);
                            }
                        }

                        for (int row = rowNumber; row <= worksheet.Dimension.End.Row; row++)
                        {
                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                var header = worksheet.Cells[1, col].Text;
                                var cellValue = worksheet.Cells[row, col].Text;
                                if (ImportViewModel.StaticColumns.Contains(header) &&
                                    !string.IsNullOrWhiteSpace(cellValue) &&
                                    !columnValues[header].Contains(cellValue))
                                {
                                    columnValues[header].Add(cellValue);
                                }
                            }
                        }

                        model.Headers = headers;
                        model.ColumnValues = columnValues;
                        model.StartingRow = rowNumber;
                    }
                }
            }

            model.IsFileUploaded = true;
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult ValidateFile(int rowNumber, ImportViewModel model)
        {
            var exampleRow = new Dictionary<string, string>();
            var totalRows = 0;
            var validRowCount = 0;

            var uploadedFile = HttpContext.Session.Get("UploadedFile");
            if (uploadedFile != null)
            {
                using (var stream = new MemoryStream(uploadedFile))
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        totalRows = worksheet.Dimension.End.Row - rowNumber + 1;

                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            var header = worksheet.Cells[1, col].Text;
                            if (model.Headers.Contains(header))
                            {
                                var cellValue = worksheet.Cells[rowNumber, col].Text;
                                exampleRow[header] = cellValue;
                            }
                        }

                        for (int row = rowNumber; row <= worksheet.Dimension.End.Row; row++)
                        {
                            bool isValidRow = false;
                            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                            {
                                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                                {
                                    isValidRow = true;
                                    break;
                                }
                            }
                            if (isValidRow)
                            {
                                validRowCount++;
                            }
                        }
                    }
                }
            }

            ViewBag.ExampleRow = exampleRow;
            ViewBag.TotalRows = totalRows;
            ViewBag.ValidRowCount = validRowCount;

            return PartialView("_ValidationResult", model);
        }

        [HttpPost]
        public IActionResult ImportFile(int rowNumber)
        {
            var parsedData = new List<Dictionary<string, string>>();

            var fileBytes = HttpContext.Session.Get("UploadedFile");
            if (fileBytes != null)
            {
                using (var stream = new MemoryStream(fileBytes))
                {
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        int totalRows = worksheet.Dimension.End.Row;
                        int totalCols = worksheet.Dimension.End.Column;

                        for (int row = rowNumber; row <= totalRows; row++)
                        {
                            var rowData = new Dictionary<string, string>();
                            bool isValidRow = false;

                            for (int col = 1; col <= totalCols; col++)
                            {
                                var header = worksheet.Cells[1, col].Text;
                                var cellValue = worksheet.Cells[row, col].Text;

                                if (!string.IsNullOrWhiteSpace(cellValue))
                                {
                                    isValidRow = true;
                                }

                                var transformedHeader = ToCamelCase(RemoveSymbols(header));
                                rowData[transformedHeader] = cellValue;
                            }

                            if (isValidRow)
                            {
                                parsedData.Add(rowData);
                            }
                        }
                    }
                }
            }

            string jsonOutput = JsonConvert.SerializeObject(parsedData, Formatting.Indented);
            return File(new System.Text.UTF8Encoding().GetBytes(jsonOutput), "application/json", "data.json");
        }

        private string RemoveSymbols(string input)
        {
            return Regex.Replace(input, @"[^a-zA-Z0-9]", "");
        }

        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 2)
                return input.ToLower();

            return char.ToLower(input[0]) + input.Substring(1);
        }
    }
}
