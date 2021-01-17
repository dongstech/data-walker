using System;
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
    internal class DefaultExcelValidator : IExcelValidator
    {
        private readonly ILogger<DefaultExcelValidator> _logger;
        private readonly DataWalkerOptions _options;

        public DefaultExcelValidator(ILogger<DefaultExcelValidator> logger, IOptions<DataWalkerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public List<ExcelValidationResult> Validate(string inputDir)
        {
            var filePaths = Directory.EnumerateFiles(inputDir, "*.xlsx");
            var results = new List<ExcelValidationResult>();
            foreach (var filePath in filePaths)
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = fileInfo.BaseName();
                using var package = new ExcelPackage(fileInfo);
                if (NotContainsSheet(package))
                {
                    results.Add(
                        new ExcelValidationResult($"Sheet '{_options.DataSheetName}' not exists in [{fileName}]"));
                    continue;
                }

                var sheet = package.Workbook.Worksheets[_options.DataSheetName];
                /*
                 * cases:
                 * 1. [√]duplicate markers
                 * 2. [√] un-closed markers
                 * 3. [√]no markers detected. may be caused by incorrect format
                 * 4. todo illegal marker pair. begin marker should on the top-left side of end maker.
                 */

                var beginMarkersDict = new Dictionary<string, ExcelRangeBase>();
                var endMarkersDict = new Dictionary<string, ExcelRangeBase>();
                var beginMarkers = new List<ExcelRangeBase>();
                var endMarkers = new List<ExcelRangeBase>();
                var duplicateMarkers = new List<Tuple<ExcelRangeBase, ExcelRangeBase>>();

                foreach (var cell in sheet.Cells)
                {
                    if (new Regex($"^{_options.TableCodePattern}").IsMatch(cell.Text))
                    {
                        beginMarkers.Add(cell);
                        if (beginMarkersDict.ContainsKey(cell.Text))
                            duplicateMarkers.Add(
                                new Tuple<ExcelRangeBase, ExcelRangeBase>(cell, beginMarkersDict[cell.Text]));
                        else
                            beginMarkersDict.Add(cell.Text, cell);
                    }

                    if (new Regex($"^End_{_options.TableCodePattern}").IsMatch(cell.Text))
                    {
                        endMarkers.Add(cell);
                        if (endMarkersDict.ContainsKey(cell.Text))
                            duplicateMarkers.Add(
                                new Tuple<ExcelRangeBase, ExcelRangeBase>(cell, endMarkersDict[cell.Text]));
                        else
                            endMarkersDict.Add(cell.Text, cell);
                    }
                }

                if (beginMarkersDict.Count == 0 && endMarkersDict.Count == 0)
                {
                    results.Add(
                        new ExcelValidationResult(
                            $"Did not detect any markers in [{fileName}!{_options.DataSheetName}]."));
                    continue;
                }

                if (duplicateMarkers.Count > 0)
                    results.AddRange(duplicateMarkers.Select(_ =>
                        new ExcelValidationResult(
                            $"Detected duplicate marker '{_.Item1.Text}' in [{fileName}!{_options.DataSheetName}] {_.Item1.Address} <====> {_.Item2.Address}")));

                var unClosedMarkers = new List<ExcelRangeBase>();
                foreach (var beginMarker in beginMarkers)
                {
                    var index = endMarkers.FindIndex(_ => $"End_{beginMarker.Text}" == _.Text);
                    if (index == -1)
                        unClosedMarkers.Add(beginMarker);
                    else
                        endMarkers.RemoveAt(index);
                }

                unClosedMarkers.AddRange(endMarkers);

                if (unClosedMarkers.Count > 0)
                    results.AddRange(unClosedMarkers.Select(_ =>
                        new ExcelValidationResult(
                            $"Detected unclosed marker '{_.Text}' in [{fileName}!{_options.DataSheetName}] {_.Address}")));
            }

            return results;
        }

        private bool NotContainsSheet(ExcelPackage package)
        {
            return package.Workbook.Worksheets.All(_ => _.Name != _options.DataSheetName);
        }
    }
}