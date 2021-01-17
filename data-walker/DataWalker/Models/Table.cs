using System.Collections.Generic;
using OfficeOpenXml;

namespace DataWalker.Models
{
    public class Table
    {
        internal TableAddress Address { get; set; }
        public ExcelRange Title { get; set; }
        public ExcelRange Header { get; set; }
        public List<Row> Rows { get; set; }
        public ExcelRange Footer { get; set; }
        public ExcelWorksheet SourceSheet { get; set; }
    }
}