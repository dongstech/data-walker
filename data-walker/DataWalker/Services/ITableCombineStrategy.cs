using DataWalker.Models;
using OfficeOpenXml;

namespace DataWalker.Services
{
    public interface ITableCombineStrategy
    {
        void Combine(ExcelWorksheet destSheet, TableAddress item, ExcelWorksheet sourceSheet);
    }
}