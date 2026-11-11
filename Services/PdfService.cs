using System.Text;
using UglyToad.PdfPig;

namespace DocSenseAI.Services;

public class PdfService
{
    public List<string> ExtractAndChunkText(Stream pdfStream, int chunkSize = 500, int overlap = 50)
    {
        var fullText = new StringBuilder();

        using (var document = PdfDocument.Open(pdfStream))
        {
            foreach (var page in document.GetPages())
            {
                fullText.AppendLine(page.Text);
            }
        }

        var text = fullText.ToString();
        var chunks = new List<string>();

        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        for (int i = 0; i < text.Length; i += (chunkSize - overlap))
        {
            int length = Math.Min(chunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length).Trim());
        }

        return chunks;
    }
}
