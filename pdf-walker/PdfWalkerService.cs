using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Filter;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Path = System.IO.Path;

namespace PdfWalker
{
    internal class PdfWalkerService : IHostedService
    {
        private static readonly Tesseract4OcrEngineProperties tesseract4OcrEngineProperties =
            new Tesseract4OcrEngineProperties();

        private readonly PdfWalkerOptions _pdfWalkerOptions;
        private readonly ILogger<PdfWalkerService> _logger;

        public PdfWalkerService(IOptions<PdfWalkerOptions> options, ILogger<PdfWalkerService> logger)
        {
            _pdfWalkerOptions = options.Value;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    var workingDir = _pdfWalkerOptions.WorkingDir;
                    _logger.LogInformation($"WorkingDir: {workingDir}");
                    var inputDir = Path.Combine(workingDir, _pdfWalkerOptions.InputDir);
                    _logger.LogInformation($"InputDir: {inputDir}");
                    var filePaths = Directory.EnumerateFiles(inputDir, "*.pdf").ToList();
                    _logger.LogInformation($"Find {filePaths.Count()} files.");

                    foreach (var filePath in filePaths)
                    {
                        _logger.LogInformation($"Find file: {filePath}");

                        var tesseractReader = new Tesseract4LibOcrEngine(tesseract4OcrEngineProperties);
                        tesseract4OcrEngineProperties.SetPathToTessData(new FileInfo(TESS_DATA_FOLDER));


                        using var pdfDoc = new PdfDocument(new PdfReader(filePath));
                        Rectangle rect = new Rectangle(0, 0, 595, 841);
                        var filter = new TextRegionEventFilter(rect);
                        FilteredEventListener listener = new FilteredEventListener();

                        // Create a text extraction renderer
                        LocationTextExtractionStrategy extractionStrategy = listener
                            .AttachEventListener(new LocationTextExtractionStrategy(), filter);

                        // Note: If you want to re-use the PdfCanvasProcessor, you must call PdfCanvasProcessor.reset()
                        new PdfCanvasProcessor(listener).ProcessPageContent(pdfDoc.GetFirstPage());

                        // Get the resultant text after applying the custom filter
                        String actualText = extractionStrategy.GetResultantText();

                        // See the resultant text in the console
                        _logger.LogInformation($"Recognized text is:\n{actualText}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}