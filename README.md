# File Processor API

ASP.NET Core 8 Web API scaffold for an API-key-protected CSV file processor.

## Current status

This first scaffold contains the default ASP.NET Core controller template and Swagger in Development. The file-processing endpoint, API-key middleware, processing counter, and processing logs described below are the planned API contract and are not implemented yet.

## Planned API

### `POST /api/files/process`

Uploads a CSV file and returns the average of its `amount` column. The upload will require an API key in the `X-API-Key` request header and a multipart form field named `file`.

Requirements for the CSV:

- Include a header row with an `amount` column.
- Provide numeric values in each non-empty `amount` field.

Example `sample.csv`:

```csv
amount
12.5
7.5
10
```

Example request:

```sh
curl -X POST http://localhost:5163/api/files/process \
  -H "X-API-Key: replace-with-your-api-key" \
  -F "file=@sample.csv;type=text/csv"
```

The planned successful response is JSON in this shape:

```json
{
  "fileName": "sample.csv",
  "rowsProcessed": 3,
  "averageAmount": 10,
  "processingTimeMs": 4
}
```

The implementation will also track the number of processed files and log basic processing details, including the filename and processing time. The counter will be in memory and reset when the service restarts.

## API key configuration

The planned API key setting is `FILE_PROCESSOR_API_KEY`. Configure a non-production local value through an environment variable before the endpoint is implemented:

```powershell
$env:FILE_PROCESSOR_API_KEY = "replace-with-a-long-random-value"
```

Do not commit real API keys to source control. Requests without a matching key will be rejected once API-key middleware is implemented.

## Run locally

Requires the .NET 8 SDK.

```sh
dotnet restore
dotnet run
```

The generated HTTP launch profile uses `http://localhost:5163`. In Development, Swagger UI is available at `http://localhost:5163/swagger`.

## Docker

Build the image from the repository root:

```sh
docker build -t fileprocessor-api .
```

Run it on port 8080:

```sh
docker run --rm -p 8080:8080 \
  -e FILE_PROCESSOR_API_KEY=replace-with-a-long-random-value \
  fileprocessor-api
```

The container listens on port `8080`. The API key environment variable is documented for the planned implementation; the current scaffold does not yet validate it.