using DataWalker.Models;
using OfficeOpenXml;

namespace DataWalker.Services
{
    public interface ITableConverter
    {
        public Table Convert(TableAddress tableAddress, ExcelWorksheet cells);
    }
}