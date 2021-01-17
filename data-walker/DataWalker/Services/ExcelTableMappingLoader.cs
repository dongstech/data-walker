using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataWalker.Configurations;
using DataWalker.EPPlusExtension;
using DataWalker.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace DataWalker.Services
{
    internal class ExcelTableMappingLoader : ITableMappingLoader
    {
        private readonly ILogger<ExcelTableMappingLoader> _logger;
        private readonly DataWalkerOptions _options;

        public ExcelTableMappingLoader(ILogger<ExcelTableMappingLoader> logger, IOptions<DataWalkerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public IEnumerable<TableMappingItem> Load()
        {
            _logger.LogInformation("Loading table mapping...");
            using var tableMappingPackage =
                new ExcelPackage(new FileInfo(Path.Combine(_options.WorkingDir, _options.TableMappingFileName)));
            return tableMappingPackage.Workbook.Worksheets[_options.TableMappingSheetName]
                .ToEnumerable<TableMappingItem>().ToList();
        }
    }
}