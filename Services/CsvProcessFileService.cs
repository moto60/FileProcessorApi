using System.Globalization;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using FileProcessorApi.Interfaces;

namespace FileProcessorApi.Services;

public sealed class CsvProcessFileService : IProcessFile
{
    public async Task<object> ProcessAsync(Stream input, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            input,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: true);
        var csv = await reader.ReadToEndAsync(cancellationToken);

        using var textReader = new StringReader(csv);
        using var parser = new TextFieldParser(textReader)
        {
            HasFieldsEnclosedInQuotes = true,
            TrimWhiteSpace = true
        };
        parser.SetDelimiters(",");

        string[]? headers;
        try
        {
            headers = parser.ReadFields();
        }
        catch (MalformedLineException exception)
        {
            throw new InvalidDataException("CSV file contains a malformed header.", exception);
        }

        if (headers is null)
            throw new InvalidDataException("CSV file is empty.");

        var amountColumnIndex = Array.FindIndex(
            headers,
            header => string.Equals(header, "amount", StringComparison.OrdinalIgnoreCase));

        if (amountColumnIndex < 0)
            throw new InvalidDataException("CSV file must contain an 'amount' column.");

        var total = 0m;
        var count = 0;

        try
        {
            while (!parser.EndOfData)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fields = parser.ReadFields();
                if (fields is null || fields.Length != headers.Length)
                    throw new InvalidDataException("CSV row has a different number of columns than the header.");

                var amount = fields[amountColumnIndex];
                if (string.IsNullOrWhiteSpace(amount))
                    continue;

                if (!decimal.TryParse(
                        amount,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var value))
                {
                    throw new InvalidDataException("CSV 'amount' values must be numeric.");
                }

                total += value;
                count++;
            }
        }
        catch (MalformedLineException exception)
        {
            throw new InvalidDataException("CSV file contains a malformed row.", exception);
        }

        if (count == 0)
            throw new InvalidDataException("CSV file contains no numeric 'amount' values.");

        return new CsvAverageResult("amount", count, total / count);
    }

    private sealed record CsvAverageResult(string Column, int Count, decimal Average);
}
