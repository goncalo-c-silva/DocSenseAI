using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace DocSenseAI.Services;

public class RagService
{
    private readonly QdrantClient _qdrantClient;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IChatCompletionService _chatService;
    private const string CollectionName = "technical_docs";

    public RagService(
        QdrantClient qdrantClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IChatCompletionService chatService)
    {
        _qdrantClient = qdrantClient;
        _embeddingGenerator = embeddingGenerator;
        _chatService = chatService;
    }

    public async Task SaveDocumentChunksAsync(string documentName, List<string> chunks)
    {
        var collections = await _qdrantClient.ListCollectionsAsync();
        if (!collections.Contains(CollectionName))
        {
            await _qdrantClient.CreateCollectionAsync(
                CollectionName,
                new VectorParams { Size = 768, Distance = Distance.Cosine }
            );
        }

        var embeddings = await _embeddingGenerator.GenerateAsync(chunks);
        var points = new List<PointStruct>();

        for (int i = 0; i < chunks.Count; i++)
        {
            var point = new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = embeddings[i].Vector.ToArray(),
                Payload =
                {
                    ["document_name"] = documentName,
                    ["text"] = chunks[i]
                }
            };
            points.Add(point);
        }

        await _qdrantClient.UpsertAsync(CollectionName, points);
    }

    public async Task<string> AskQuestionAsync(string question)
    {
        var collections = await _qdrantClient.ListCollectionsAsync();
        if (!collections.Contains(CollectionName))
        {
            return "Ainda não foi carregado nenhum documento para pesquisa.";
        }

        var questionEmbedding = await _embeddingGenerator.GenerateVectorAsync(question);
        
        var searchResults = await _qdrantClient.SearchAsync(
            collectionName: CollectionName,
            vector: questionEmbedding.ToArray(),
            limit: 3,
            scoreThreshold: 0.4f
        );

        var contextBlocks = searchResults
            .Where(r => r.Payload.ContainsKey("text"))
            .Select(r => r.Payload["text"].StringValue)
            .ToList();

        if (!contextBlocks.Any())
        {
            return "Não encontrei informação relevante nos documentos carregados para responder a essa pergunta.";
        }

        var context = string.Join("\n---\n", contextBlocks);

        var prompt = $"""
            És um assistente técnico rigoroso. Responde à pergunta do utilizador usando APENAS o contexto fornecido abaixo. 
            Se a resposta não estiver no contexto, diz que não sabes.

            Contexto:
            {context}

            Pergunta: {question}
            """;

        var response = await _chatService.GetChatMessageContentAsync(prompt);
        return response.Content ?? "Sem resposta gerada.";
    }
}
