using DocSenseAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocSenseAI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly PdfService _pdfService;
    private readonly RagService _ragService;

    public DocumentsController(PdfService pdfService, RagService ragService)
    {
        _pdfService = pdfService;
        _ragService = ragService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0 || !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Por favor, envia um ficheiro PDF válido.");

        using var stream = file.OpenReadStream();
        var chunks = _pdfService.ExtractAndChunkText(stream);

        await _ragService.SaveDocumentChunksAsync(file.FileName, chunks);

        return Ok(new { Message = $"Documento '{file.FileName}' processado com sucesso.", ChunksSaved = chunks.Count });
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] QuestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("A pergunta não pode estar vazia.");

        var answer = await _ragService.AskQuestionAsync(request.Question);
        return Ok(new { Question = request.Question, Answer = answer });
    }
}

public record QuestionRequest(string Question);
