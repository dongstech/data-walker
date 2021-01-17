using OfficeOpenXml;

namespace DataWalker.Services
{
    public interface IAddressCalculator
    {
        ExcelAddress Calculate(ExcelWorksheet destSheet, ExcelRange sourceTable);
        ExcelAddress CalculateFromFileAddress(ExcelWorksheet destSheet);
    }
}