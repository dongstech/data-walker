using OfficeOpenXml;

namespace DataWalker.Models
{
    public class Row
    {
        public ExcelRange Label { get; set; }
        public ExcelRange Value { get; set; }
    }
}