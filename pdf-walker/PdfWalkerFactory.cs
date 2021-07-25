namespace PdfWalker
{
    public class PdfWalkerFactory
    {
        public IPdfWalker Get()
        {
            return new WalkerA();
        }
    }
}