using System.Text.Json;
using FileProcessorApi.Interfaces;

namespace FileProcessorApi.Services;

public sealed class JsonProcessFileService : IProcessFile
{
    public async Task<object> ProcessAsync(Stream input, CancellationToken cancellationToken)
    {
        using var document = await JsonDocument.ParseAsync(
            input,
            cancellationToken: cancellationToken);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("JSON file must contain an array of objects.");

        var activeRecords = new List<JsonElement>();

        foreach (var record in document.RootElement.EnumerateArray())
        {
            if (record.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException("JSON array entries must be objects.");

            if (record.TryGetProperty("active", out var active)
                && active.ValueKind == JsonValueKind.True)
            {
                activeRecords.Add(record.Clone());
            }
        }

        return activeRecords;
    }
}
