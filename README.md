# DocSenseAI 

An intelligent Retrieval-Augmented Generation (RAG) Web API built with **C# (.NET)**, **Microsoft Semantic Kernel**, **Qdrant**, and **Ollama**. 

This project demonstrates how to extract knowledge from technical PDF documents and query them using natural language while keeping all data 100% local and private.

## Architecture & Workflow

The system operates in two main phases:

1. **Document Ingestion (`POST /api/documents/upload`)**
   - Extracts raw text from uploaded PDF files using `PdfPig` and splits it into overlapping chunks.
   - Generates vector embeddings (`768` dimensions) locally via **Ollama** (`nomic-embed-text`).
   - Stores the vectors and original text metadata inside a **Qdrant** vector database running on Docker.

2. **Semantic Search & Generation (`POST /api/documents/ask`)**
   - Converts the user's question into a vector embedding.
   - Performs a cosine similarity search in **Qdrant** to retrieve the top 3 most relevant document chunks.
   - Uses **Microsoft Semantic Kernel** and **Ollama** (`llama3.2`) with strict system prompting to generate accurate answers based *only* on the retrieved context.

## Tech Stack

- **Backend:** C#, ASP.NET Core Web API
- **AI Orchestration:** Microsoft Semantic Kernel, Microsoft.Extensions.AI
- **Local LLMs & Embeddings:** Ollama (`llama3.2`, `nomic-embed-text`)
- **Vector Database:** Qdrant (Docker)
- **PDF Processing:** UglyToad.PdfPig

## How to Run Locally

1. **Start Qdrant Vector Database:**
   ```bash
   docker run -d --name qdrant-docs -p 6333:6333 -p 6334:6334 qdrant/qdrant
   ```

2. **Pull required Ollama models:**
   ```bash
   ollama pull llama3.2
   ollama pull nomic-embed-text
   ```

3. **Run the .NET API:**
   ```bash
   dotnet run
   ```
   Access the Swagger UI at `http://localhost:5238/swagger` to upload PDFs and test queries.
