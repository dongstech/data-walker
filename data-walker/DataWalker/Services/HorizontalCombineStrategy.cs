using System.Linq;
using DataWalker.Models;
using OfficeOpenXml;

namespace DataWalker.Services
{
    public class HorizontalCombineStrategy : ITableCombineStrategy
    {
        private readonly IAddressCalculator _addressCalculator = new HorizontalAddressCalculator();

        private readonly ITableConverter _tableConverter = new DefaultTableConverter();

        public void Combine(ExcelWorksheet destSheet, TableAddress item, ExcelWorksheet sourceSheet)
        {
            var table = _tableConverter.Convert(item, sourceSheet);
            const int headerRowIndex = 2;
            if (null == destSheet.Dimension)
            {
                //copy title
                const int titleStartRow = 2;
                const int titleStartColumn = 1;
                table.Title.Copy(destSheet.Cells[titleStartRow, titleStartColumn,
                    titleStartRow + table.Title.Rows - 1, titleStartColumn + table.Title.Columns - 1]);
                //copy the first column of header
                table.SourceSheet.Cells[table.Header.Start.Address]
                    .Copy(destSheet.Cells[headerRowIndex, destSheet.Dimension.End.Column + 1]);
                //copy primary key column
                var rowLabelIndex = 3;
                foreach (var row in table.Rows)
                {
                    row.Label.Copy(destSheet.Cells[rowLabelIndex, destSheet.Dimension.End.Column]);
                    rowLabelIndex++;
                }

                // copy the first column of footer
                table.SourceSheet.Cells[table.Footer.Start.Address]
                    .Copy(destSheet.Cells[rowLabelIndex, destSheet.Dimension.End.Column]);
            }

            var startColumn = int.Parse(destSheet.Dimension.End.Column + 1 + "");

            //set table source
            destSheet.SetValue(1, startColumn + 1, item.FileName);

            // copy the left headers
            table.SourceSheet
                .Cells[table.Header.Start.Row, table.Header.Start.Column + 1, table.Header.Start.Row,
                    table.Header.End.Column]
                .Copy(destSheet.Cells[headerRowIndex, startColumn, headerRowIndex,
                    startColumn + table.Header.Columns - 1]);

            // copy the values 
            table.Rows.ForEach(row =>
            {
                if (string.IsNullOrEmpty(row.Label.Text))
                {
                    return;
                }

                var labels = destSheet.Cells[3, 2, destSheet.Dimension.End.Row - 1, 2];
                var label = labels.FirstOrDefault(_ => _.Text.Equals(row.Label.Text));
                const int labelStartColumn = 2;
                if (null == label)
                {
                    destSheet.InsertRow(destSheet.Dimension.End.Row, 1);
                    var rowIndex = destSheet.Dimension.End.Row - 1;
                    row.Label.Copy(destSheet.Cells[rowIndex, labelStartColumn]);
                    row.Value.Copy(
                        destSheet.Cells[rowIndex, startColumn, rowIndex, startColumn + row.Value.Columns - 1],
                        ExcelRangeCopyOptionFlags.ExcludeFormulas);
                }
                else
                {
                    row.Value.Copy(destSheet.Cells[label.Start.Row, startColumn, label.Start.Row,
                        startColumn + row.Value.Columns - 1], ExcelRangeCopyOptionFlags.ExcludeFormulas);
                }
            });
            // copy the left footers
            table.SourceSheet
                .Cells[table.Footer.Start.Row, table.Footer.Start.Column + 1, table.Footer.Start.Row,
                    table.Footer.End.Column]
                .Copy(destSheet.Cells[destSheet.Dimension.End.Row, startColumn, destSheet.Dimension.End.Row,
                    startColumn + table.Footer.Columns - 1]);
        }
    }
}