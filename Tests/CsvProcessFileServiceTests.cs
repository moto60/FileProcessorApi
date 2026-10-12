using System.Text;
using System.Text.Json;
using FileProcessorApi.Services;
using Xunit;

namespace FileProcessorApi.Tests;

public sealed class CsvProcessFileServiceTests
{
    private readonly CsvProcessFileService _service = new();

    [Fact]
    public async Task ProcessAsync_ReturnsAverageForAmountColumn()
    {
        var result = await ProcessAsync("id,amount\n1,10\n2,20\n3,30");

        Assert.Equal("amount", result.GetProperty("column").GetString());
        Assert.Equal(3, result.GetProperty("count").GetInt32());
        Assert.Equal(20m, result.GetProperty("average").GetDecimal());
    }

    [Fact]
    public async Task ProcessAsync_MatchesAmountHeaderIgnoringCaseAndSkipsBlankValues()
    {
        var result = await ProcessAsync("id,AMOUNT\n1,10\n2,\n3,20");

        Assert.Equal(2, result.GetProperty("count").GetInt32());
        Assert.Equal(15m, result.GetProperty("average").GetDecimal());
    }

    [Theory]
    [InlineData("", "CSV file is empty.")]
    [InlineData("id,total\n1,10", "CSV file must contain an 'amount' column.")]
    [InlineData("amount\nnot-a-number", "CSV 'amount' values must be numeric.")]
    [InlineData("amount\n", "CSV file contains no numeric 'amount' values.")]
    [InlineData("id,amount\n1,10,extra", "CSV row has a different number of columns than the header.")]
    public async Task ProcessAsync_RejectsInvalidCsv(string csv, string expectedMessage)
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(
            () => ProcessStreamAsync(csv));

        Assert.Equal(expectedMessage, exception.Message);
    }

    private async Task<JsonElement> ProcessAsync(string csv)
    {
        var result = await ProcessStreamAsync(csv);
        return JsonSerializer.SerializeToElement(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private async Task<object> ProcessStreamAsync(string csv)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        return await _service.ProcessAsync(stream, CancellationToken.None);
    }
}
