using System.Collections.Generic;
using DataWalker.Models;

namespace DataWalker.Services
{
    internal interface ITableHunter
    {
        IEnumerable<TableAddress> ListTableAddress(IEnumerable<string> filePaths);
    }
}