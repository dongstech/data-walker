using DataWalker.Models;
using OfficeOpenXml;

namespace DataWalker.Services
{
    internal class HorizontalAddressCalculator : IAddressCalculator
    {
        public ExcelAddress Calculate(ExcelWorksheet destSheet, ExcelRange sourceTable)
        {
            var fromRow = destSheet.Dimension?.Start.Row + 1 ?? 2;
            var fromCol = destSheet.Dimension?.End.Column + 1 ?? 1;
            var toRow = fromRow + sourceTable.Rows - 1;
            var toCol = fromCol + sourceTable.Columns - 1;
            return new ExcelAddress(fromRow, fromCol, toRow, toCol);
        }

        public ExcelAddress CalculateFromFileAddress(ExcelWorksheet destSheet)
        {
            var row = destSheet.Dimension?.Start.Row ?? 1;
            var col = destSheet.Dimension?.End.Column + 1 ?? 1;
            return new ExcelAddress(row, col, row, col);
        }
    }
}