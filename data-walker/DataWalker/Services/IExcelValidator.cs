using System.Collections.Generic;
using DataWalker.Models;

namespace DataWalker.Services
{
    internal interface IExcelValidator
    {
        List<ExcelValidationResult> Validate(string inputDir);
    }
}