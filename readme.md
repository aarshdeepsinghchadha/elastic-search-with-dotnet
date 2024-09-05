Elasticsearch is a powerful search engine designed for scalable data search and analytics. Integrating it with a .NET Core Web API allows us to perform full-text search and manage data efficiently. In this article, we'll walk through how to set up Elasticsearch with Docker, create a simple service to interact with it, and integrate it into a .NET Core Web API.

## Prerequisites

Before we get started, make sure you have the following installed:

- Docker
- .NET Core SDK
- Basic understanding of Docker, Elasticsearch, and .NET Core Web API

## Setting up Elasticsearch and Kibana with Docker

We will use Docker Compose to create an environment where Elasticsearch and Kibana work together. Kibana provides a user interface to interact with Elasticsearch and monitor its data.

### `docker-compose.yml`

```yaml
version: '3.8'

services:
  elasticsearch:
    container_name: else
    image: elasticsearch:8.15.0
    ports:
      - "9200:9200"
    volumes:
      - elasticsearch-data:/usr/share/elasticsearch/data
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
    networks:
      - elk

  kibana:
    container_name: kibana
    image: kibana:8.15.0
    ports:
      - "5601:5601"
    depends_on:
      - elasticsearch
    environment:
      - ELASTICSEARCH_URL=http://elasticsearch:9200
    networks:
      - elk

networks:
  elk:
    driver: bridge

volumes:
  elasticsearch-data:

```

In this file, we define two services:

1. **Elasticsearch**: Runs on port 9200 and is set to operate in a single-node mode. Security is disabled for simplicity.
2. **Kibana**: Connects to Elasticsearch and runs on port 5601, providing a UI for interacting with Elasticsearch.

To bring up the services, run:

```bash
docker-compose up -d

```

This will pull the required images and start the containers. You can verify by visiting `http://localhost:5601` for Kibana and `http://localhost:9200` for Elasticsearch.

## Creating the ElasticService in .NET Core Web API

Next, we'll create a service in our .NET Core Web API project to interact with Elasticsearch.

### Step 1: Add NuGet Packages

Install the necessary NuGet packages:

```bash
dotnet add package Elasticsearch.Net --version 8.15.0
dotnet add package NEST --version 8.15.0

```

### Step 2: Define ElasticSettings

Add the connection settings for Elasticsearch in your `appsettings.json`:

```json
"ElasticsSearchSettings": {
  "Url": "<http://localhost:9200>",
  "DefaultIndex": "users"
}

```

### Step 3: Create `ElasticService`

The `ElasticService` is responsible for interacting with the Elasticsearch cluster. Here's a basic implementation:

```csharp
public class ElasticService : IElasticService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticSettings _elasticSettings;

    public ElasticService(IOptions<ElasticSettings> elasticSettings)
    {
        _elasticSettings = elasticSettings.Value;

        var settings = new ElasticsearchClientSettings(new Uri(_elasticSettings.Url))
            .DefaultIndex(_elasticSettings.DefaultIndex);

        _client = new ElasticsearchClient(settings);
    }

    public async Task<bool> AddOrUpdate(User user)
    {
        var response = await _client.IndexAsync(user, idx =>
        {
            idx.Index(_elasticSettings.DefaultIndex).OpType(OpType.Index);
        });

        return response.IsValidResponse;
    }

    public async Task<bool> AddOrUpdateBulk(IEnumerable<User> users, string indexName)
    {
        var response = await _client.BulkAsync(x => x.Index(_elasticSettings.DefaultIndex)
        .UpdateMany(users, (ud, u) => ud.Doc(u).DocAsUpsert(true)));

        return response.IsValidResponse;
    }

    public async Task CreateIndexIfNotExistsAsync(string indexName)
    {
        if (!_client.Indices.Exists(indexName).Exists)
        {
            await _client.Indices.CreateAsync(indexName);
        }
    }

    public async Task<User> Get(string key)
    {
        var response = await _client.GetAsync<User>(key, g =>
        {
            g.Index(_elasticSettings.DefaultIndex);
        });

        return response.Source;
    }

    public async Task<List<User>> GetAll()
    {
        var response = await _client.SearchAsync<User>(x => x.Index(_elasticSettings.DefaultIndex));

        return response.IsValidResponse ? response.Documents.ToList() : default;
    }

    public async Task<bool> Remove(string key)
    {
        var response = await _client.DeleteAsync<User>(key, x => x.Index(_elasticSettings.DefaultIndex));

        return response.IsValidResponse;
    }

    public async Task<long?> RemoveAll()
    {
        var response = await _client.DeleteByQueryAsync<User>(d => d.Indices(_elasticSettings.DefaultIndex));

        return response.IsValidResponse ? response.Deleted : default;
    }
}

```

This service contains methods to:

- Add or update documents in Elasticsearch.
- Create indices if they do not exist.
- Retrieve individual or all documents from the index.
- Delete individual or all documents from the index.

### Step 4: Register and Configure Services

In your `Startup.cs` or `Program.cs`, register the `ElasticService` and its settings:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<ElasticSettings>(Configuration.GetSection("ElasticsSearchSettings"));
    services.AddSingleton<IElasticService, ElasticService>();
}

```

## Conclusion

In this article, we've set up Elasticsearch and Kibana using Docker and created a basic .NET Core Web API service to interact with Elasticsearch. With these steps, you can integrate Elasticsearch into your .NET Core applications to provide powerful search functionality. From here, you can further explore advanced features like custom mappings, querying, and full-text search.

## Resources Link!

https://dev.to/midnightasc/elasticsearch-with-net-core-web-api-and-docker-5bjc

https://dev.to/midnightasc/elasticsearch-an-in-depth-explanation-2bpf

https://dev.to/midnightasc/how-to-scale-elasticsearch-3ao3