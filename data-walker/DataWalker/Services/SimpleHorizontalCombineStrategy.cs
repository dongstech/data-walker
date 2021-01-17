using System;
using System.Linq;
using DataWalker.Models;
using OfficeOpenXml;

namespace DataWalker.Services
{
    public class SimpleHorizontalCombineStrategy : ITableCombineStrategy
    {
        private readonly ITableConverter _tableConverter = new DefaultTableConverter();

        public void Combine(ExcelWorksheet destSheet, TableAddress item, ExcelWorksheet sourceSheet)
        {
            var tableObj = _tableConverter.Convert(item, sourceSheet);
            const int headerRowIndex = 2;
            if (null == destSheet.Dimension)
            {
                //copy title
                const int titleStartRow = 2;
                const int titleStartColumn = 1;
                tableObj.Title.Copy(destSheet.Cells[titleStartRow, titleStartColumn,
                    titleStartRow + tableObj.Title.Rows - 1, titleStartColumn + tableObj.Title.Columns - 1]);
                //copy the first column of header
                tableObj.SourceSheet.Cells[tableObj.Header.Start.Address]
                    .Copy(destSheet.Cells[headerRowIndex, destSheet.Dimension.End.Column + 1]);
                //copy primary key column
                var rowLabelIndex = 3;
                foreach (var row in tableObj.Rows)
                {
                    row.Label.Copy(destSheet.Cells[rowLabelIndex, destSheet.Dimension.End.Column]);
                    rowLabelIndex++;
                }

                // copy the first column of footer
                tableObj.SourceSheet.Cells[tableObj.Footer.Start.Address]
                    .Copy(destSheet.Cells[rowLabelIndex, destSheet.Dimension.End.Column]);
            }

            var startColumn = int.Parse(destSheet.Dimension.End.Column + 1 + "");
            // set table source
            destSheet.SetValue(1, startColumn + 1, item.FileName);
            //copy the left headers
            tableObj.SourceSheet
                .Cells[tableObj.Header.Start.Row, tableObj.Header.Start.Column + 1, tableObj.Header.Start.Row,
                    tableObj.Header.End.Column]
                .Copy(destSheet.Cells[headerRowIndex, startColumn, headerRowIndex,
                    startColumn + tableObj.Header.Columns - 1 - 1]);
            //copy the values
            var rowValueIndex = 3;
            foreach (var row in tableObj.Rows)
            {
                row.Value.Copy(destSheet.Cells[rowValueIndex, startColumn, rowValueIndex,
                    startColumn + row.Value.Columns - 1], ExcelRangeCopyOptionFlags.ExcludeFormulas);
                rowValueIndex++;
            }

            //copy the left columns of footer
            tableObj.SourceSheet.Cells[tableObj.Footer.Start.Row, tableObj.Footer.Start.Column + 1,
                    tableObj.Footer.Start.Row, tableObj.Footer.End.Column]
                .Copy(destSheet.Cells[rowValueIndex, startColumn, rowValueIndex,
                    startColumn + tableObj.Footer.Columns - 1 - 1]);
        }
    }
}