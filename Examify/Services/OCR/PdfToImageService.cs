using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Examify.Services.OCR;

public class PdfToImageService
{
    public List<byte[]> ConvertPdfToImages(Stream pdfStream)
    {
        var images = new List<byte[]>();
        using var ms = new MemoryStream();
        pdfStream.CopyTo(ms);
        var bytes = ms.ToArray();
        
        using var library = DocLib.Instance;
        using var docReader = library.GetDocReader(bytes, new PageDimensions(4096, 4096));
        
        for (int i = 0; i < docReader.GetPageCount(); i++)
        {
            using var pageReader = docReader.GetPageReader(i);
            var text = pageReader.GetText();
            
            // Skip empty pages (only footer or whitespace)
            if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 50)
                continue;
            
            var rawBytes = pageReader.GetImage();
            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();
            
            using var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
            using var whiteBackground = new Image<Bgra32>(width, height, new Bgra32(255, 255, 255, 255));
            whiteBackground.Mutate(ctx => ctx.DrawImage(image, 1f));
            
            using var output = new MemoryStream();
            whiteBackground.Save(output, new PngEncoder());
            images.Add(output.ToArray());
        }
        
        return images;
    }
}
