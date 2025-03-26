using System;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Types;
using System.Text.Json;

namespace AIAgentServiceDemo
{
	public static class SemanticKernelExtension
	{
        #region private

        public static async Task<IEnumerable<KernelFunction>> MapToFunctionsAsync(this IMcpClient mcpClient)
        {
            List<KernelFunction> kernelFunctions = new List<KernelFunction>();
            await foreach (var tool in mcpClient.ListToolsAsync())
            {

                kernelFunctions.Add(tool.ToKernelFunction(mcpClient));
            }

            return kernelFunctions;
        }

        public static KernelFunction ToKernelFunction(this Tool tool, IMcpClient mcpClient)
        {
            async Task<string> InvokeToolAsync(Kernel kernel, KernelFunction function, KernelArguments arguments, CancellationToken cancellationToken)
            {
                try
                {
                    // Convert arguments to dictionary format expected by mcpdotnet
                    Dictionary<string, object> mcpArguments = new Dictionary<string, object>();
                    foreach (var arg in arguments)
                    {
                        if (arg.Value is not null)
                        {
                            mcpArguments[arg.Key] = function.ToArgumentValue(arg.Key, arg.Value);
                        }
                    }

                    // Call the tool through mcpdotnet
                    var result = await mcpClient.CallToolAsync(
                        tool.Name,
                        mcpArguments,
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);

                    // Extract the text content from the result
                    return string.Join("\n", result.Content
                        .Where(c => c.Type == "text")
                        .Select(c => c.Text));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error invoking tool '{tool.Name}': {ex.Message}");

                    // Rethrowing to allow the kernel to handle the exception
                    throw;
                }
            }

            return KernelFunctionFactory.CreateFromMethod(
                method: InvokeToolAsync,
                functionName: tool.Name,
                description: tool.Description,
                parameters: tool.ToParameters(),
                returnParameter: ToReturnParameter()
            );
        }

        public static object ToArgumentValue(this KernelFunction function, string name, object value)
        {
            var parameter = function.Metadata.Parameters.FirstOrDefault(p => p.Name == name);
            return parameter?.ParameterType switch
            {
                Type t when Nullable.GetUnderlyingType(t) == typeof(int) => Convert.ToInt32(value),
                Type t when Nullable.GetUnderlyingType(t) == typeof(double) => Convert.ToDouble(value),
                Type t when Nullable.GetUnderlyingType(t) == typeof(bool) => Convert.ToBoolean(value),
                Type t when t == typeof(List<string>) => (value as IEnumerable<object>)?.ToList(),
                Type t when t == typeof(Dictionary<string, object>) => (value as Dictionary<string, object>)?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                _ => value,
            } ?? value;
        }

        public static List<KernelParameterMetadata>? ToParameters(this Tool tool)
        {
            var inputSchema = tool.InputSchema;

            if (inputSchema.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            // Extract the 'properties' object
            if (!inputSchema.TryGetProperty("properties", out JsonElement propertiesElement))
            {
                return null;
            }

            // Get required properties (as HashSet for fast lookup)
            HashSet<string> requiredProperties = new();
            if (inputSchema.TryGetProperty("required", out JsonElement requiredElement) &&
                requiredElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in requiredElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        requiredProperties.Add(item.GetString()!);
                    }
                }
            }

            // Now extract each property and convert
            var parameters = new List<KernelParameterMetadata>();
            foreach (JsonProperty kvp in propertiesElement.EnumerateObject())
            {
                var propName = kvp.Name;
                var propValue = kvp.Value;

                parameters.Add(new KernelParameterMetadata(propName)
                {
                    Description = propValue.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    ParameterType = ConvertParameterDataType(propValue, requiredProperties.Contains(propName)),
                    IsRequired = requiredProperties.Contains(propName)
                });
            }

            return parameters;
        }

        public static KernelReturnParameterMetadata? ToReturnParameter()
        {
            return new KernelReturnParameterMetadata()
            {
                ParameterType = typeof(string),
            };
        }

        public static Type ConvertParameterDataType(JsonElement property, bool required)
        {
            if (!property.TryGetProperty("type", out JsonElement typeElement) || typeElement.ValueKind != JsonValueKind.String)
            {
                return typeof(object); // Fallback
            }

            var typeStr = typeElement.GetString();

            var type = typeStr switch
            {
                "string" => typeof(string),
                "integer" => typeof(int),
                "number" => typeof(double),
                "boolean" => typeof(bool),
                "array" => typeof(List<string>), // Simplified assumption
                "object" => typeof(Dictionary<string, object>),
                _ => typeof(object)
            };

            return !required && type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type;
        }

        #endregion
    }
}

