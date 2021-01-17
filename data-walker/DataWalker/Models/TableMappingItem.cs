using DataWalker.EPPlusExtension;

namespace DataWalker.Models
{
    public class TableMappingItem
    {
        [Column("Code")] public string Code { get; set; }

        [Column("CombineType")] public string CombineType { get; set; }
        [Column("SumAndSort")] public string SumAndSort { get; set; }
    }
}