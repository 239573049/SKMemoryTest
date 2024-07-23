using System.Text.Json;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SKMemory;
using SKMemoryTest.Functions;

namespace SKMemoryTest;

public class Tests
{
    const string prompt =
        """"
        使用 <data></data> 标记中的内容作为你的知识:
        <data>
        {{quote}}
        </data>

        回答要求：
        - 如果用户提问的问题并没有在 <data></data> 中提到，你需要澄清。
        - 避免提及你是从 <data></data> 获取的知识。
        - 保持答案与 <data></data> 中描述的一致。
        - 使用 Markdown 语法优化回答格式。
        - 使用与问题相同的语言回答。
        - 如果 Markdown中有图片，链接，代码则正常显示。

        问题:"""{{question}}"""
        """";

    private MemoryServerless _memory;
    private Kernel _kernel;

    [SetUp]
    public void Setup()
    {
        var httpClientHandler = new OpenAIHttpClientHandler("https://api.token-ai.cn/");
        var key = "";
        var embeddingModel = "text-embedding-ada-002";
        var textModel = "gpt-4o";
        _kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: textModel,
                apiKey: key,
                httpClient: new HttpClient(httpClientHandler))
            .Build();

        _kernel.Plugins.AddFromType<ToolFunction>();

        _memory = new KernelMemoryBuilder()
            .WithOpenAITextGeneration(new OpenAIConfig()
            {
                APIKey = key,
                TextModel = textModel
            }, null, new HttpClient(httpClientHandler))
            .WithOpenAITextEmbeddingGeneration(new OpenAIConfig()
            {
                APIKey = key,
                EmbeddingModel = embeddingModel,
            }, null, false, new HttpClient(httpClientHandler))
            .Build<MemoryServerless>();
    }

    /// <summary>
    /// 计算两个数的大小
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task CalcSize()
    {
        var chat = _kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory("如果用户是需要判断哪个值大，那么您的职责是调用函数然后返回函数的结果去告诉用户哪个大");

        chatHistory.AddUserMessage("请问 1.11和1.9哪个大？");
        
        var content = await chat.GetChatMessageContentAsync(chatHistory, new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        }, _kernel);

        // 返回结果
        Console.WriteLine(content);
    }

    /// <summary>
    /// 知识库问答
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task KnowledgeBase()
    {
        // 将 README.md 导入到知识库
        await _memory.ImportDocumentAsync(Path.Combine(Directory.GetCurrentDirectory(), "README.md"));

        string query = "Kernel Memory 导入文档示例";

        var answer = await _memory.SearchAsync(query, limit: 2, minRelevance: 0.5);

        query = prompt.Replace("{{question}}",
            query).Replace("{{quote}}",
            string.Join('\n', answer.Results.SelectMany(x => x.Partitions.Select(i => i.Text))));

        Console.WriteLine(query);

        var chat = _kernel.GetRequiredService<IChatCompletionService>();

        await foreach (var item in chat.GetStreamingChatMessageContentsAsync(query))
        {
            Console.Write(item.Content);
        }
    }

    /// <summary>
    /// 获取天气
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task GetWeather()
    {
        var chat = _kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory("如果用户需要查询天气，那么您的职责是调用函数然后返回函数的结果去告诉用户天气");

        chatHistory.AddUserMessage("请问深圳的天气怎么样？");

        var content = await chat.GetChatMessageContentAsync(chatHistory, new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        }, _kernel);

        // 返回结果
        Console.WriteLine(content);
    }
}