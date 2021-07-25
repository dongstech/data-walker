using System;
using System.Data;
using System.IO;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using OfficeOpenXml;

namespace PdfWalker
{
    public class WalkerB : IPdfWalker
    {
        public void Walk()
        {
            var left = 310f;
            var top = 841.9f - 667.3f;
            var width = 504.6f - 310f;
            var height = 667.3f - 652.3f;
            var resultTable = new DataTable();
            var column_file_name = "file_name";
            var column_content_title = "content_title";
            resultTable.Columns.Add(new DataColumn(column_file_name));
            resultTable.Columns.Add(new DataColumn(column_content_title));

            var filePaths = Directory.EnumerateDirectories("");
            foreach (var file in filePaths)
            {
                using var pdfDoc = new PdfDocument(new PdfReader(file));

                var page = pdfDoc.GetPage(3);
                var pageSize = page.GetPageSize();
                Console.WriteLine($"pageSize: {pageSize}");
                var filterRect = new Rectangle(left, pageSize.GetTop() - top - height, width, height);
                Console.WriteLine($"filterRect {filterRect.GetX()} {filterRect.GetY()}");
                var regionFilter = new TextRegionEventFilter(filterRect);
                var text = PdfTextExtractor.GetTextFromPage(page,
                    new FilteredTextEventListener(new LocationTextExtractionStrategy(), regionFilter));
                var row = resultTable.NewRow();
                row[column_file_name] = file;
                row[column_content_title] = text;
                resultTable.Rows.Add(row);
            }

            using var resultFile = new ExcelPackage();
            resultFile.Workbook.Worksheets.Add("Sheet1").Cells.LoadFromDataTable(resultTable, true);

            // var outputDirPath = Path.Combine(workingDir, _pdfWalkerOptions.OutputDir);
            // var outputDir = new DirectoryInfo(outputDirPath);
            // if (!outputDir.Exists) outputDir.Create();
            // resultFile.SaveAs(
            //     new FileInfo(Path.Combine(outputDirPath, $"result_{DateTime.Now:yyyyMMddHHmmss}.xlsx"))); 
        }
    }
}