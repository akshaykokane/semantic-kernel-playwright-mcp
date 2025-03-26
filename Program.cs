using AIAgentServiceDemo;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Configuration;

namespace AzureAIAgentServiceDemo
{
    class Program
    {

        static async Task Main(string[] args)
        {

            Console.WriteLine("Welcome to My MCP + Semantic Kernel Demo App!");

            await RunApplication();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task RunApplication()
        {

            Console.WriteLine("Running application logic...");
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion("GPT4ov1", "https://<replace>.openai.azure.com", "<replacewithkey>");
            Kernel kernel = builder.Build();

            var mcpClient = await GetMCPClientForPlaywright();

            var KernelFunctions = await mcpClient.MapToFunctionsAsync();
            kernel.Plugins.AddFromFunctions("Browser", KernelFunctions);

            // Enable automatic function calling
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.0,
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            };

            var prompt = "Summarize AI news for me related to MCP on bing news. Open first link and summarize content";
            var result = await kernel.InvokePromptAsync(prompt, new(executionSettings)).ConfigureAwait(false);
            Console.WriteLine($"\n\n{prompt}\n{result}");

        }

        public static async Task<IMcpClient> GetMCPClientForPlaywright()
        {
            McpClientOptions options = new()
            {
                ClientInfo = new() { Name = "Playwright", Version = "1.0.0" }
            };

            var config = new McpServerConfig
            {
                Id = "Playwright",
                Name = "Playwright",
                TransportType = "stdio",
                TransportOptions = new Dictionary<string, string>
                {
                    ["command"] = "npx",
                    ["arguments"] = "-y @playwright/mcp@latest",
                }
            };

            var mcpClient = await McpClientFactory.CreateAsync(
        
                config,
                options
                );

            return mcpClient;
        }


    }
}
