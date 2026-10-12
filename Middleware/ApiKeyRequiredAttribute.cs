namespace FileProcessorApi.Middleware;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ApiKeyRequiredAttribute : Attribute;
