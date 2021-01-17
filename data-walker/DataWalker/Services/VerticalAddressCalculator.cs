using DataWalker.Models;
using OfficeOpenXml;

namespace DataWalker.Services
{
    internal class VerticalAddressCalculator : IAddressCalculator
    {
        public ExcelAddress Calculate(ExcelWorksheet destSheet, ExcelRange sourceTable)
        {
            var fromRow = destSheet.Dimension?.End.Row + 1 ?? 1;
            var fromCol = destSheet.Dimension?.Start.Column + 1 ?? 2;
            var toRow = fromRow + sourceTable.Rows - 1;
            var toCol = fromCol + sourceTable.Columns - 1;
            return new ExcelAddress(fromRow, fromCol, toRow, toCol);
        }

        public ExcelAddress CalculateFromFileAddress(ExcelWorksheet destSheet)
        {
            var row = destSheet.Dimension?.End.Row + 1 ?? 1;
            var col = destSheet.Dimension?.Start.Column ?? 1;
            return new ExcelAddress(row, col, row, col);
        }
    }
}