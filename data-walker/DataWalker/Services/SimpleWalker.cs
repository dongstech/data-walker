using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using DataWalker.Configurations;
using DataWalker.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace DataWalker.Services
{
    internal class SimpleWalker : IDataWalker
    {
        private readonly IExcelValidator _excelValidator;
        private readonly ILogger<SimpleWalker> _logger;
        private readonly DataWalkerOptions _options;
        private readonly ITableHunter _tableHunter;
        private readonly ITableMappingLoader _tableMappingLoader;
        private readonly TableCombineStrategyProvider _tableCombineStrategyProvider;

        public SimpleWalker(ILogger<SimpleWalker> logger, IOptions<DataWalkerOptions> options,
            IExcelValidator excelValidator, ITableMappingLoader tableMappingLoader, ITableHunter tableHunter,
            TableCombineStrategyProvider tableCombineStrategyProvider)
        {
            _logger = logger;
            _excelValidator = excelValidator;
            _tableMappingLoader = tableMappingLoader;
            _tableHunter = tableHunter;
            _tableCombineStrategyProvider = tableCombineStrategyProvider;
            _options = options.Value;
        }

        public void Walk()
        {
            _logger.LogInformation($"The {GetType().Name} is walking...");
            var inputDir = Path.Combine(_options.WorkingDir, _options.InputDir);
            _logger.LogInformation($"WorkingDir= {_options.WorkingDir}");
            _logger.LogInformation($"InputDir= {_options.InputDir}");
            _logger.LogInformation($"OutputDir= {_options.OutputDir}");
            var results = _excelValidator.Validate(inputDir);
            if (results.Count > 0)
            {
                foreach (var result in results) _logger.LogError(result.ToString());
                return;
            }

            var tableMapping = _tableMappingLoader.Load().ToDictionary(item => item.Code, item => item);
            var filePaths = Directory.EnumerateFiles(inputDir, "*.xlsx");
            var tableAddresses = _tableHunter.ListTableAddress(filePaths);
            var groups = tableAddresses.GroupBy(_ => _.File);
            using var resultFile = new ExcelPackage();

            foreach (var group in groups)
            {
                _logger.LogInformation($"Processing {group.Key}");
                var sourceFilePath = group.Key;
                using var sourceFile = new ExcelPackage(new FileInfo(sourceFilePath));
                foreach (var item in group)
                {
                    _logger.LogInformation($"Processing {item.Address}");
                    var destSheet = resultFile.Workbook.Worksheets.SingleOrDefault(_ => _.Name == item.Type.Value) ??
                                    resultFile.Workbook.Worksheets.Add(item.Type.Value);
                    var sheet = sourceFile.Workbook.Worksheets[item.Sheet];
                    var combineStrategy = _tableCombineStrategyProvider.Get(tableMapping, item.Type.Value);
                    combineStrategy.Combine(destSheet, item, sheet);
                }
            }

            PostCombine(tableAddresses, tableMapping, resultFile);
            AutoFitColumn(resultFile);
            SaveResult(resultFile);
        }

        private static void AutoFitColumn(ExcelPackage resultFile)
        {
            const int spanColumnWidth = 2;
            foreach (var workbookWorksheet in resultFile.Workbook.Worksheets)
            {
                workbookWorksheet.Cells[workbookWorksheet.Dimension.Address].Style.WrapText = false;
                workbookWorksheet.Cells[workbookWorksheet.Dimension.Address].AutoFitColumns(spanColumnWidth);
            }
        }

        private static void PostCombine(IEnumerable<TableAddress> tableAddresses,
            Dictionary<string, TableMappingItem> tableMapping, ExcelPackage resultFile)
        {
            SumAndSort(tableAddresses, tableMapping, resultFile);
            SumForSimpleCombined(tableAddresses, tableMapping, resultFile);
        }

        private static void SumForSimpleCombined(IEnumerable<TableAddress> tableAddresses,
            Dictionary<string, TableMappingItem> tableMapping, ExcelPackage resultFile)
        {
            var addressList = tableAddresses.Where(_ =>
            {
                var tableCode = _.Type.Value;

                return tableMapping.ContainsKey(tableCode) && tableMapping[tableCode].CombineType ==
                    CombineType.SimpleHorizontal.ToString().ToLower();
            });
            var countByTableType = addressList.GroupBy(_ => _.Type.Value).ToDictionary(g => g.Key, g => g.Count());
            var sheetNames = addressList.Select(_ => _.Type.Value).Distinct();
            const int spanColumnNumber = 1;
            const int headerRowIndex = 2;
            const int prefixColumnNumber = 2;
            const int sumStartColumn = prefixColumnNumber + 1;
            foreach (var sheetName in sheetNames)
            {
                var sheet = resultFile.Workbook.Worksheets[sheetName];
                var tableNumber = countByTableType[sheetName];
                var columnNumber = int.Parse(sheet.Dimension.Columns - 2 + "") / tableNumber;
                var valueColumnNumber = columnNumber / (1 + spanColumnNumber);
                var columnNumberToInsert = 2 * valueColumnNumber;
                var sheetEndRow = sheet.Dimension.End.Row;

                sheet.InsertColumn(sumStartColumn, columnNumberToInsert, 2);
                sheet.Cells[headerRowIndex, sumStartColumn, headerRowIndex, sumStartColumn + columnNumberToInsert - 1]
                    .Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[headerRowIndex, sumStartColumn, headerRowIndex, sumStartColumn + columnNumberToInsert - 1]
                    .Style.Fill.BackgroundColor.SetColor(Color.LawnGreen);
                sheet.Cells[sheetEndRow, sumStartColumn, sheetEndRow, sumStartColumn + columnNumberToInsert - 1]
                    .Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[sheetEndRow, sumStartColumn, sheetEndRow, sumStartColumn + columnNumberToInsert - 1]
                    .Style.Fill.BackgroundColor.SetColor(Color.LawnGreen);
                for (var index = 0; index < valueColumnNumber; index++)
                {
                    var valueColumnIndex = 2 * (index + 1);
                    var valueColumnAbstractIndex = prefixColumnNumber + columnNumberToInsert + valueColumnIndex;
                    var columnName = sheet.Cells[headerRowIndex, valueColumnAbstractIndex].Text;
                    var sumColumnIndex = prefixColumnNumber + 2 * (index + 1);

                    // set sum column name
                    sheet.SetValue(headerRowIndex, sumColumnIndex, $"{columnName}汇总");
                    // set sum column value
                    for (var rowIndex = headerRowIndex + 1; rowIndex <= sheetEndRow; rowIndex++)
                    {
                        if (string.IsNullOrEmpty(sheet.Cells[rowIndex, prefixColumnNumber].Text))
                        {
                            continue;
                        }

                        var formulaString = new StringBuilder();
                        for (var index2 = 0; index2 < tableNumber; index2++)
                        {
                            var valueColumnAbstractIndex2 = valueColumnAbstractIndex + columnNumber * index2;
                            if (formulaString.Length > 0)
                            {
                                formulaString.Append(',');
                            }

                            formulaString.Append(sheet.Cells[rowIndex, valueColumnAbstractIndex2].Address);
                        }

                        var sumCell = sheet.Cells[rowIndex, sumColumnIndex];
                        sumCell.Formula = $"=SUM({formulaString})";
                        sumCell.Style.Numberformat.Format = "_ * #,##0.00_ ;_ * -#,##0.00_ ;_ * \"-\"??_ ;_ @_ ";
                        sumCell.Style.WrapText = false;
                    }
                }
            }
        }

        private static void SumAndSort(IEnumerable<TableAddress> tableAddresses,
            Dictionary<string, TableMappingItem> tableMapping, ExcelPackage resultFile)
        {
            var addressList = tableAddresses.Where(_ =>
            {
                var tableCode = _.Type.Value;

                return tableMapping.ContainsKey(tableCode) && tableMapping[tableCode].SumAndSort == "Y";
            });
            var countByTableType = addressList.GroupBy(_ => _.Type.Value).ToDictionary(g => g.Key, g => g.Count());


            var sheetNames = addressList.Select(_ => _.Type.Value).Distinct();

            foreach (var sheetName in sheetNames)
            {
                const int headerRowIndex = 2;
                const int labelColIndex = 2;
                const int spanRow = 1;
                const int spanColumn = 1;

                var sheet = resultFile.Workbook.Worksheets[sheetName];
                var tableRowNumber = sheet.Dimension.Rows - 1;

                // vertically sum
                for (var index = labelColIndex + 1 + spanColumn; index <= sheet.Dimension.End.Column; index += 2)
                {
                    var addr = sheet.Cells[headerRowIndex + 1, index, sheet.Dimension.End.Row - 1, index];
                    sheet.Cells[sheet.Dimension.End.Row, index].Formula = $"=SUM({addr.Address})";
                }
                // vertically sum end

                // separate by column
                //copy title to the same column below
                var destStartRow = headerRowIndex + tableRowNumber + spanRow + 1;
                var destEndRow = destStartRow + tableRowNumber;

                sheet.Cells[headerRowIndex, labelColIndex, headerRowIndex + tableRowNumber, labelColIndex]
                    .Copy(sheet.Cells[destStartRow, labelColIndex, destEndRow, labelColIndex]);

                //move column 2
                var count = countByTableType[sheetName];
                const int dataCols = 1;
                for (var index = 0; index < count; index++)
                {
                    var srcCol = labelColIndex + 2 * (1 + index) + 1;
                    var destCol = labelColIndex + 2 * index + 1;
                    sheet.Cells[headerRowIndex, srcCol, headerRowIndex + tableRowNumber, srcCol + dataCols]
                        .Copy(sheet.Cells[destStartRow, destCol, destEndRow, destCol + dataCols]);
                    for (var i = 0; i <= dataCols; i++)
                    {
                        sheet.DeleteColumn(srcCol);
                    }
                }
                // separate by column end

                // sum horizontal-combined tables
                const int sumColumnIndex = 4;
                sheet.InsertColumn(labelColIndex + 1, 2, 2);
                var column = sheet.Dimension.End.Column;

                // sum first table
                var verticalSumColumnName = $"{sheet.Cells[headerRowIndex, sumColumnIndex + 2].Text}汇总";
                sheet.SetValue(headerRowIndex, sumColumnIndex, verticalSumColumnName);
                sheet.Cells[headerRowIndex, sumColumnIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[headerRowIndex, sumColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LawnGreen);
                sheet.Cells[headerRowIndex + tableRowNumber - 1, sumColumnIndex].Style.Fill.PatternType =
                    ExcelFillStyle.Solid;
                sheet.Cells[headerRowIndex + tableRowNumber - 1, sumColumnIndex].Style.Fill.BackgroundColor
                    .SetColor(Color.LawnGreen);

                for (var index = 1; index <= tableRowNumber - 1; index++)
                {
                    var rowIndex = headerRowIndex + index;
                    var row = sheet.Cells[rowIndex, sumColumnIndex + 1, rowIndex, column];
                    var label = sheet.Cells[rowIndex, labelColIndex].Text;
                    if (string.IsNullOrEmpty(label))
                    {
                        continue;
                    }

                    var sumCell = sheet.Cells[rowIndex, sumColumnIndex];
                    sumCell.Formula = $"=SUM({row.Address})";
                    sumCell.Style.Numberformat.Format = "_ * #,##0.00_ ;_ * -#,##0.00_ ;_ * \"-\"??_ ;_ @_ ";
                    sumCell.Style.WrapText = false;
                }

                // sum second table
                verticalSumColumnName = $"{sheet.Cells[destStartRow, sumColumnIndex + 2].Text}汇总";
                sheet.SetValue(destStartRow, sumColumnIndex, verticalSumColumnName);
                sheet.Cells[destStartRow, sumColumnIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[destStartRow, sumColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LawnGreen);
                sheet.Cells[destStartRow + tableRowNumber - 1, sumColumnIndex].Style.Fill.PatternType =
                    ExcelFillStyle.Solid;
                sheet.Cells[destStartRow + tableRowNumber - 1, sumColumnIndex].Style.Fill.BackgroundColor
                    .SetColor(Color.LawnGreen);
                for (var index = 1; index <= tableRowNumber - 1; index++)
                {
                    var rowIndex = destStartRow + index;
                    var row = sheet.Cells[rowIndex, sumColumnIndex + 1, rowIndex, column];
                    var label = sheet.Cells[rowIndex, labelColIndex].Text;
                    if (string.IsNullOrEmpty(label))
                    {
                        continue;
                    }

                    var sumCell = sheet.Cells[rowIndex, sumColumnIndex];
                    sumCell.Formula = $"=SUM({row.Address})";
                    sumCell.Style.Numberformat.Format = "_ * #,##0.00_ ;_ * -#,##0.00_ ;_ * \"-\"??_ ;_ @_ ";
                    sumCell.Style.WrapText = false;
                }
            }
        }

        private void SaveResult(ExcelPackage resultFile)
        {
            var outputDirPath = Path.Combine(_options.WorkingDir, _options.OutputDir);
            var outputDir = new DirectoryInfo(outputDirPath);
            if (!outputDir.Exists) outputDir.Create();

            resultFile.SaveAs(new FileInfo(Path.Combine(outputDirPath, $"result_{DateTime.Now:yyyyMMddHHmmss}.xlsx")));
        }
    }
}