using System.Collections.Generic;
using DataWalker.Models;
using OfficeOpenXml;

namespace DataWalker.Services
{
    public class DefaultTableConverter : ITableConverter
    {
        public Table Convert(TableAddress tableAddress, ExcelWorksheet sourceSheet)
        {
            var sourceTable = sourceSheet.Cells[tableAddress.Address];
            var tableTitle = sourceSheet.Cells[sourceTable.Start.Row, sourceTable.Start.Column, sourceTable.End.Row,
                sourceTable.Start.Column];
            var header = sourceSheet.Cells[sourceTable.Start.Row, sourceTable.Start.Column + 1, sourceTable.Start.Row,
                sourceTable.End.Column];
            var footer = sourceSheet.Cells[sourceTable.End.Row, sourceTable.Start.Column + 1, sourceTable.End.Row,
                sourceTable.End.Column];
            var rows = new List<Row>();
            for (var currentRow = sourceTable.Start.Row + 1; currentRow < sourceTable.End.Row; currentRow++)
            {
                var row = new Row
                {
                    Label = sourceSheet.Cells[currentRow, sourceTable.Start.Column + 1],
                    Value = sourceSheet.Cells[currentRow, sourceTable.Start.Column + 2, currentRow,
                        sourceTable.End.Column]
                };

                rows.Add(row);
            }

            return new Table
            {
                Title = tableTitle,
                Header = header,
                Rows = rows,
                Footer = footer,
                Address = tableAddress,
                SourceSheet = sourceSheet
            };
        }
    }
}