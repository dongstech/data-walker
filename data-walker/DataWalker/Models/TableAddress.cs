namespace DataWalker.Models
{
    public class TableAddress
    {
        internal TableType Type { get; set; }
        internal string File { get; set; }
        internal string Sheet { get; set; }
        internal string Address { get; set; }
        internal string FileName { get; set; }
    }
}