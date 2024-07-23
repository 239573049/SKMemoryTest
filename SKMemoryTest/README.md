# Kernel Memory 介绍

![License: MIT](https://camo.githubusercontent.com/89a8d22b231b7871fef4d4054a5cd2c4128814f9606c0aea6af9b69af2d25a4b/68747470733a2f2f696d672e736869656c64732e696f2f6769746875622f6c6963656e73652f6d6963726f736f66742f6b65726e656c2d6d656d6f7279) [![Discord](https://camo.githubusercontent.com/8d3cbbec5b72e7dce145a1736c86762dd58ef88d56de9f221668ba3f4e520421/68747470733a2f2f696d672e736869656c64732e696f2f646973636f72642f313036333135323434313831393934323932323f6c6162656c3d446973636f7264266c6f676f3d646973636f7264266c6f676f436f6c6f723d776869746526636f6c6f723d643832363739)](https://aka.ms/KMdiscord)

**Kernel Memory** (KM) 是一个**多模态 AI 服务**，专注于通过自定义的连续数据混合管道来高效地索引数据集，支持 **[检索增强生成](https://en.wikipedia.org/wiki/Prompt_engineering#Retrieval-augmented_generation)** (RAG)、合成记忆、提示工程和自定义语义记忆处理。KM 提供了多种集成方式，包括**Web 服务**、**[Docker 容器](https://hub.docker.com/r/kernelmemory/service)**、**[插件](https://learn.microsoft.com/copilot/plugins/overview)**（适用于 ChatGPT/Copilot/Semantic Kernel）和 .NET 库（用于嵌入式应用）。

KM 利用高级嵌入和 LLMs（大语言模型）实现自然语言查询，以从索引数据中获取答案，并附带引用和原始来源链接。

![image](https://private-user-images.githubusercontent.com/371009/253485255-31894afa-d19e-4e9b-8d0f-cb889bf5c77f.png?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3MjE2OTk4OTgsIm5iZiI6MTcyMTY5OTU5OCwicGF0aCI6Ii8zNzEwMDkvMjUzNDg1MjU1LTMxODk0YWZhLWQxOWUtNGU5Yi04ZDBmLWNiODg5YmY1Yzc3Zi5wbmc_WC1BbXotQWxnb3JpdGhtPUFXUzQtSE1BQy1TSEEyNTYmWC1BbXotQ3JlZGVudGlhbD1BS0lBVkNPRFlMU0E1M1BRSzRaQSUyRjIwMjQwNzIzJTJGdXMtZWFzdC0xJTJGczMlMkZhd3M0X3JlcXVlc3QmWC1BbXotRGF0ZT0yMDI0MDcyM1QwMTUzMThaJlgtQW16LUV4cGlyZXM9MzAwJlgtQW16LVNpZ25hdHVyZT01NjNmYmEwYzk3NDBkZjhkOWU2NWY0NDA0OTc5ODYwMzAxYWVkZmZlZTNjMjI4NzRhZGVkMDY3NDk3YTYwODdlJlgtQW16LVNpZ25lZEhlYWRlcnM9aG9zdCZhY3Rvcl9pZD0wJmtleV9pZD0wJnJlcG9faWQ9MCJ9.9EJKOwIXaG1FPJAWbjBlb0Vcr3x0OaHVkxEO_Irx8FY)

KM 旨在与 [Semantic Kernel](https://github.com/microsoft/semantic-kernel)、Microsoft Copilot 和 ChatGPT 无缝集成，提升在大多数流行 AI 平台上的数据驱动功能。

## 同步内存 API（即“无服务器”模式）

Kernel Memory 最佳的运行和扩展方式是作为异步**Web 服务**，允许在不阻塞应用的情况下处理数千个文档和信息。然而，Kernel Memory 也可以在无服务器模式下运行，在 .NET 后端/控制台/桌面应用中嵌入 `MemoryServerless` 类实例。这种方法也适用于 ASP.NET Web APIs 和 Azure Functions。每个请求会立即处理，但调用客户端需要处理瞬时错误。

![image](https://github.com/microsoft/kernel-memory/raw/main/docs/infra-sync.png)

### 导入文档示例

```csharp
var memory = new KernelMemoryBuilder()
    .WithOpenAIDefaults(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    .Build<MemoryServerless>();

// 导入单个文件
await memory.ImportDocumentAsync("meeting-transcript.docx", tags: new() { { "user", "Blake" } });

// 导入多个文件并应用多个标签
await memory.ImportDocumentAsync(new Document("file001")
    .AddFile("business-plan.docx")
    .AddFile("project-timeline.pdf")
    .AddTag("user", "Blake")
    .AddTag("collection", "business")
    .AddTag("collection", "plans")
    .AddTag("fiscalYear", "2023"));
```

### 提问示例

```csharp
var answer1 = await memory.AskAsync("How many people attended the meeting?");
var answer2 = await memory.AskAsync("what's the project timeline?", filter: new MemoryFilter().ByTag("user", "Blake"));
```

上述示例使用默认文档摄取管道：

1. **提取文本**：识别文件格式并提取信息
2. **划分文本**：将文本划分为小块，以优化搜索
3. **提取嵌入**：使用 LLM 嵌入生成器提取嵌入
4. **保存嵌入**：将嵌入保存到向量索引中，例如 [Azure AI Search](https://learn.microsoft.com/azure/search/vector-search-overview)、[Qdrant](https://qdrant.tech/) 或其他数据库。

在示例中，通过标签组织记忆，以保护私人信息。此外，记忆可以使用**标签**进行分类和结构化，从而通过分面导航实现高效搜索和检索。

### 数据谱系、引用、来源

所有记忆和答案都与提供的数据完全相关。在生成答案时，Kernel Memory 包括验证其准确性所需的所有信息：

```csharp
await memory.ImportFileAsync("NASA-news.pdf");

var answer = await memory.AskAsync("Any news from NASA about Orion?");

Console.WriteLine(answer.Result + "/n");

foreach (var x in answer.RelevantSources)
{
    Console.WriteLine($"  * {x.SourceName} -- {x.Partitions.First().LastUpdate:D}");
}
```

> 是的，NASA 有关于猎户座航天器的新闻。NASA 邀请媒体查看一个新的测试版本 [......] 有关 Artemis 项目的更多信息，请访问 NASA 网站。
>
> - **NASA-news.pdf -- 2023年8月1日**

## 作为服务的内存 - 异步 API

根据你的场景，你可能希望在**本地进程内**或通过**异步和可扩展的服务**来运行所有代码。

![image](https://github.com/microsoft/kernel-memory/raw/main/docs/infra-async.png)

如果你只需导入小文件并且仅使用 C#，可以使用上述的 **MemoryServerless** 进行本地进程执行，这样可以阻塞进程。但是，如果你遇到以下情况：

- 我只想要一个 Web 服务来导入数据和发送查询
- 我的应用使用 **TypeScript、Java、Rust 或其他语言**
- 我正在导入 **可能需要几分钟处理的大文件**，并且不想阻塞用户界面
- 我需要内存导入**独立运行，支持故障和重试逻辑**
- 我想定义**混合多种语言**的自定义管道，例如 Python、TypeScript 等

那么你可以将 Kernel Memory 部署为后台服务，插件中包括默认处理程序或自定义 Python/TypeScript/Java 等处理程序，利用异步非阻塞的内存编码过程，通过 **MemoryWebClient** 发送文档和提问。

[这里](https://github.com/microsoft/kernel-memory/blob/main/service/Service/README.md) 可以找到关于如何将 Kernel Memory 部署为后台 Web 服务的更多信息。

## 多语言支持和插件集成

Kernel Memory 是一个通用的多语言平台，通过 API 支持主流的语言，配有多种插件和集成选项来扩展功能。例如：

- **Python**：通过 SDK、插件和自定义管道集成
- **TypeScript/JavaScript**：适用于浏览器和服务器端的插件
- **Java/C++**：通过 REST API 支持更多语言的集成

## 参考文献

- [Kernel Memory GitHub](https://github.com/microsoft/kernel-memory)
- [Microsoft Copilot](https://github.com/microsoft/semantic-kernel)
- [ChatGPT Plugin Guide](https://learn.microsoft.com/copilot/plugins/overview)部署 KM 服务的指南和所有相关详细信息。

### 服务端代码示例

```python
from flask import Flask, request, jsonify
import kernel_memory

app = Flask(__name__)

# Configure Kernel Memory Client
client = kernel_memory.Client(api_key="YOUR_API_KEY")

@app.route('/import', methods=['POST'])
def import_document():
    file = request.files['file']
    tags = request.form.get('tags', '')
    response = client.import_document(file, tags)
    return jsonify(response)

@app.route('/query', methods=['GET'])
def query_memory():
    query = request.args.get('query')
    filter_tags = request.args.get('tags', '')
    response = client.ask(query, filter_tags)
    return jsonify(response)

if __name__ == '__main__':
    app.run(port=5000)
```

## 记忆增强生成（RAG）

RAG 是一种先进的生成模型设计，结合了检索和生成能力来创建更精确和上下文相关的回答。在 Kernel Memory 中，你可以利用 RAG 模式来将嵌入和上下文信息与生成器结合，以提供更丰富的回答。

### RAG 核心概念

1. **检索（Retrieval）**：从记忆库中检索与查询相关的文档或片段。
2. **生成（Generation）**：使用检索到的信息生成自然语言响应，通常结合了预训练的生成模型（如 GPT-3/4）。

### RAG 工作流程

1. **查询输入**：用户提出问题或请求。
2. **检索阶段**：根据查询从数据集中检索相关文档片段。
3. **生成阶段**：将检索到的信息输入生成模型，生成最终的回答或文本。

### 使用示例

```csharp
var rMemory = new KernelMemoryBuilder()
    .WithOpenAIDefaults(Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    .WithRAGModel()
    .Build<MemoryServerless>();

var answer = await rMemory.AskAsync("What were the main points discussed in the project meeting?");
```

## 总结

Kernel Memory 提供了一种灵活的方式来管理和利用大规模数据集，无论是在本地环境还是作为后台服务。它支持多种数据导入和检索策略，并提供强大的 RAG 生成能力以提高回答的相关性和准确性。无论你是需要快速本地执行还是需要扩展的异步服务，Kernel Memory 都能够满足你的需求。

如果你还有其他问题或需要更多的帮助，随时问我！