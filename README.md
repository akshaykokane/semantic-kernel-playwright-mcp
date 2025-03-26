
# Semantic Kernel + Playwright MCP Server Demo

🚀 **Welcome to the MCP + Semantic Kernel Demo App!**

This project demonstrates how to combine [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) with the **Model Context Protocol (MCP)** server using **Playwright** to enable AI-driven browsing and summarization capabilities.

It uses:
- Azure OpenAI (GPT-4)
- Semantic Kernel Function Calling
- Playwright MCP Server
- Automatic browser interaction
- Bing News summarization using AI

---

## 🔧 Features

- Uses **Semantic Kernel** to interact with AI functions
- Leverages **Playwright MCP server** to simulate browser-like capabilities
- Demonstrates **automatic function calling**
- Summarizes AI-related news from **Bing** by navigating and extracting content

---

## 🧠 Prerequisites

Ensure you have the following:

- .NET 8.0 SDK or newer
- Azure OpenAI resource (with deployed model like `gpt-4-1106-preview`)
- Access to Bing News (public)
- Playwright installed via `npx`
- MCP Server libraries (ModelContextProtocol)

---

## 📦 Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/semantic-kernel-playwright-mcp.git
   cd semantic-kernel-playwright-mcp
   ```

2. **Install Dependencies**
   Make sure to restore NuGet packages.

3. **Configure Azure OpenAI**
   Replace the following placeholders in `Program.cs`:
   ```csharp
   builder.AddAzureOpenAIChatCompletion(
       "GPT4ov1", 
       "https://<replace>.openai.azure.com", 
       "<replacewithkey>");
   ```

4. **Ensure Playwright MCP is Installed**
   ```bash
   npx -y @playwright/mcp@latest
   ```

---

## ▶️ Run the App

```bash
dotnet run
```

You'll see output like:

```plaintext
Welcome to My MCP + Semantic Kernel Demo App!
Running application logic...

Summarize AI news for me related to MCP on bing news. Open first link and summarize content
> <summarized result here>
```
## ▶️ Output

![Demo of the app in action](assets/demo.gif)

---

## 🧠 How It Works

1. Initializes the **Semantic Kernel** with Azure OpenAI.
2. Starts a **Playwright MCP Server** in stdio mode.
3. Maps MCP actions (like open page, extract content) to SK functions.
4. Invokes a natural language prompt to fetch and summarize news content.
5. Uses automatic function calling to delegate the task to the browser.

---

## 🛠 Code Highlights

- `GetMCPClientForPlaywright()` creates an MCP client using Playwright.
- `MapToFunctionsAsync()` integrates MCP capabilities into Semantic Kernel.
- Prompts like _"Summarize AI news..."_ trigger both browsing and summarization.

---

## 🧪 Example Use Case

You can modify the prompt to:
```csharp
var prompt = "Search GitHub for latest Semantic Kernel issues and summarize the top 3.";
```

---

## 📁 Project Structure

```
├── Program.cs
├── AzureAIAgentServiceDemo.csproj
├── README.md
└── ...
```

---

## 📌 Future Ideas

- Add UI using Blazor or Console enhancements
- Integrate multi-turn conversations using `AgentGroupChat`
- Extend MCP to handle screenshots or downloads

---

## 🤝 Contributing

Feel free to fork, enhance, or suggest improvements via pull requests!

---

## 📜 License

MIT License. See [LICENSE](LICENSE) file for details.

---

Let me know if you'd like to include diagrams or badge-style highlights too!
