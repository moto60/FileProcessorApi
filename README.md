# File Processor API

ASP.NET Core 8 Web API scaffold for an API-key-protected CSV file processor.

## File processing

## Planned API

### `POST /File/process`

Uploads a CSV or JSON file. The request requires an API key in the `X-API-Key` request header and a multipart form field named `file`.

CSV files return the average of their `amount` column. Include a header row with an `amount` column and numeric values in each non-empty `amount` field.

JSON files must contain an array of objects; the response contains only records whose `active` field is `true`.

Example `sample.csv`:

```csv
amount
12.5
7.5
10
```

Example request:

```sh
curl -X POST http://localhost:5163/File/process \
  -H "X-API-Key: replace-with-your-api-key" \
  -F "file=@sample.csv;type=text/csv"
```

## API key configuration

Set `FILE_PROCESSOR_API_KEY` in the environment before starting the service:

```powershell
$env:FILE_PROCESSOR_API_KEY = "replace-with-a-long-random-value"
```

Do not commit real API keys to source control. The upload endpoint rejects requests without a matching key. If the setting is missing, the endpoint returns an internal server error and logs the configuration problem.

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

The container listens on port `8080` and reads the API key from `FILE_PROCESSOR_API_KEY`.