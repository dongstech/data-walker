namespace DataWalker.Models
{
    public class ExcelValidationResult
    {
        private readonly string _content;

        public ExcelValidationResult(string content)
        {
            _content = content;
        }

        public override string ToString()
        {
            return _content;
        }
    }
}