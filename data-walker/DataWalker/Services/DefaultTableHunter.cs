using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataWalker.Configurations;
using DataWalker.Extensions;
using DataWalker.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace DataWalker.Services
{
    internal class DefaultTableHunter : ITableHunter
    {
        private readonly ILogger<DefaultTableHunter> _logger;
        private readonly DataWalkerOptions _options;

        public DefaultTableHunter(IOptions<DataWalkerOptions> options, ILogger<DefaultTableHunter> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public IEnumerable<TableAddress> ListTableAddress(IEnumerable<string> filePaths)
        {
            _logger.LogInformation("List table address...");
            var result = new List<TableAddress>();
            foreach (var filePath in filePaths)
            {
                _logger.LogInformation($"Reading {filePath}");
                var fileInfo = new FileInfo(filePath);
                using var package = new ExcelPackage(fileInfo);
                var sheet = package.Workbook.Worksheets[_options.DataSheetName];

                var cells = sheet.Cells.Where(_ =>
                        new Regex($"^{_options.TableCodePattern}").IsMatch(_.Text) ||
                        new Regex($"^End_{_options.TableCodePattern}").IsMatch(_.Text))
                    .ToList();

                var tableAddresses = new List<TableAddress>();
                for (var i = 0; i < cells.Count; i += 2)
                {
                    var cell1 = cells[i];
                    var cell2 = cells[i + 1];
                    var tableType = new TableType(cell1.Text);
                    tableAddresses.Add(new TableAddress
                    {
                        Type = tableType,
                        File = filePath,
                        FileName = fileInfo.BaseName(),
                        Sheet = _options.DataSheetName,
                        Address = sheet.Cells[cell1.Start.Row, cell1.Start.Column + 1, cell2.Start.Row,
                            cell2.Start.Column - 1].Address
                    });
                }

                result.AddRange(tableAddresses);
            }

            return result;
        }
    }
}