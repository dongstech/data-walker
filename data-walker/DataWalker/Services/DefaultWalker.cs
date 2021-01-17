using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataWalker.Configurations;
using DataWalker.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace DataWalker.Services
{
    internal class DefaultWalker : IDataWalker
    {
        private readonly IExcelValidator _excelValidator;
        private readonly ILogger<DefaultWalker> _logger;
        private readonly DataWalkerOptions _options;

        public DefaultWalker(ILogger<DefaultWalker> logger, IExcelValidator excelValidator,
            IOptions<DataWalkerOptions> options)
        {
            _logger = logger;
            _excelValidator = excelValidator;
            _options = options.Value;
        }

        public void Walk()
        {
            _logger.LogInformation("The default walker is walking...");
            var inputDir = Path.Combine(_options.WorkingDir, _options.InputDir);
            _logger.LogInformation($"WorkingDir= {_options.WorkingDir}");
            _logger.LogInformation($"InputDir= {_options.InputDir}");
            _logger.LogInformation($"OutputDir= {_options.OutputDir}");
            var results = _excelValidator.Validate(inputDir);
            if (results.Count > 0)
            {
                foreach (var result in results) _logger.LogDebug(result.ToString());
                return;
            }

            using var resultFile = new ExcelPackage();
            var filePaths = Directory.EnumerateFiles(inputDir, "*.xlsx");
            foreach (var filePath in filePaths)
            {
                _logger.LogInformation($"[Processing]\t{filePath}");
                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.BaseName();
                if (fileInfo.Exists)
                {
                    using var package = new ExcelPackage(fileInfo);
                    var sheet = package.Workbook.Worksheets[_options.DataSheetName];
                    _logger.LogInformation($"[Sheet]{sheet.Name}");
                    GroupCellRangeByMarker(sheet, resultFile, fileName);
                }
                else
                {
                    _logger.LogInformation($"[Not Exist]\t{filePath}");
                }
            }

            foreach (var workbookWorksheet in resultFile.Workbook.Worksheets) workbookWorksheet.Cells.AutoFitColumns();

            resultFile.SaveAs(new FileInfo(BuildOutputFilePath()));
        }

        private string BuildOutputFilePath()
        {
            return Path.Combine(_options.WorkingDir, _options.OutputDir,
                $"result_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        private void GroupCellRangeByMarker(ExcelWorksheet sheet, ExcelPackage resultFile, string fileName)
        {
            // var workspace = GetWorkspace(sheet);
            _logger.LogInformation("[Finding cell range...]");
            _logger.LogInformation($"[Is begin marker]\tpattern=^{_options.TableCodePattern}");
            var cells = sheet.Cells.Where(_ =>
                    new Regex($"^{_options.TableCodePattern}").IsMatch(_.Text) ||
                    new Regex($"^End_{_options.TableCodePattern}").IsMatch(_.Text))
                .ToList();

            _logger.LogInformation("[Start copying data...");
            for (var i = 0; i < cells.Count; i += 2)
            {
                var cell1 = cells[i];
                var cell2 = cells[i + 1];
                var sourceRange = sheet.Cells[cell1.Start.Row, cell1.Start.Column, cell2.Start.Row, cell2.Start.Column];
                if (IsDataRangeEmpty(sourceRange)) continue;
            }
        }

        private static bool IsDataRangeEmpty(ExcelRange sourceRange)
        {
            return false;
        }
    }
}