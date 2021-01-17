using DataWalker.Models;
using OfficeOpenXml;

namespace DataWalker.Services
{
    public class VerticalCombineStrategy : ITableCombineStrategy
    {
        private readonly IAddressCalculator _addressCalculator = new VerticalAddressCalculator();

        public void Combine(ExcelWorksheet destSheet, TableAddress item, ExcelWorksheet sourceSheet)
        {
            destSheet.SetValue(_addressCalculator.CalculateFromFileAddress(destSheet).Address, item.FileName);
            var sourceTable = sourceSheet.Cells[item.Address];
            sourceTable.Copy(destSheet.Cells[_addressCalculator.Calculate(destSheet, sourceTable).Address],
                ExcelRangeCopyOptionFlags.ExcludeFormulas);
        }
    }
}