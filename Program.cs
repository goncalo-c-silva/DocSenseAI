using DocSenseAI.Services;
using Microsoft.SemanticKernel;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var ollamaEndpoint = new Uri("http://localhost:11434");

// Configurar Ollama (Chat e Embeddings modernos)
builder.Services.AddOllamaChatCompletion(
    modelId: "llama3.2",
    endpoint: ollamaEndpoint
);

builder.Services.AddOllamaEmbeddingGenerator(
    modelId: "nomic-embed-text",
    endpoint: ollamaEndpoint
);

// Configurar Qdrant Vector Store (liga à porta gRPC 6334 do teu contentor Docker)
builder.Services.AddSingleton(new QdrantClient("localhost", 6334));
builder.Services.AddQdrantVectorStore();

// Registar os nossos serviços
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<RagService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
