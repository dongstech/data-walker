using System.Collections.Generic;
using DataWalker.Models;

namespace DataWalker.Services
{
    internal interface ITableMappingLoader
    {
        IEnumerable<TableMappingItem> Load();
    }
}