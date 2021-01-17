namespace DataWalker.Configurations
{
    internal record DataWalkerOptions
    {
        public string WorkingDir { get; set; }
        public string InputDir { get; set; }
        public string OutputDir { get; set; }
        public string DataSheetName { get; set; }
        public string TableMappingFileName { get; set; }
        public string TableMappingSheetName { get; set; }
        public string TableCodePattern { get; set; }
    }
}