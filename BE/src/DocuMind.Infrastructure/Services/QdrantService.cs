using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Interfaces.IVectorDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
namespace DocuMind.Infrastructure.Services
{
    class QdrantService : IVectorDbService
    {
        private readonly QdrantClient _client;
        private readonly ILogger<QdrantService> _logger;
        private readonly string _collectionName;
        private readonly int _vectorSize;


        public QdrantService(IConfiguration configuration, ILogger<QdrantService> logger)
        {
            _logger = logger;

            var host = configuration["QdrantSettings:Host"] ?? "localhost";
            var port = int.Parse(configuration["QdrantSettings:Port"] ?? "6334");   
            _collectionName = configuration["QdrantSettings:CollectionName"] ?? "documind_vectors";
            _vectorSize = int.Parse(configuration["QdrantSettings:VectorSize"] ?? "3072");

            _client = new QdrantClient(host, port);

            _logger.LogInformation("Qdrant client initialized: {Host}:{Port}", host, port);
        }

        public async Task InitializeCollectionAsync()
        {
            try
            {
                var exists = await CollectionExistsAsync();

                if (!exists)
                {
                    _logger.LogInformation("Creating collection: {CollectionName}", _collectionName);


                    // Create collection with specified vector size and distance metric
                    await _client.CreateCollectionAsync(
                        collectionName: _collectionName,
                        vectorsConfig: new VectorParams
                        {
                            Size = (ulong)_vectorSize,
                            Distance = Distance.Cosine
                        }
                    );

                    // Create payload index for filtering by documentId
                    await _client.CreatePayloadIndexAsync(
                        collectionName: _collectionName,
                        fieldName: "documentId",
                        schemaType: PayloadSchemaType.Integer
                    );

                    _logger.LogInformation("Collection created successfully: {CollectionName}", _collectionName);
                }
                else
                {
                    _logger.LogInformation("Collection already exists: {CollectionName}", _collectionName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Qdrant collection");
                throw;
            }
        }

        public async Task<bool> CollectionExistsAsync()
        {
            try
            {
                var collections = await _client.ListCollectionsAsync();

                return collections != null &&
                       collections.Any(c =>
                           string.Equals(c, _collectionName, StringComparison.Ordinal));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking collection existence");
                return false;
            }
        }

        public async Task DeleteDocumentVectorsAsync(int documentId)
        {
            try
            {
                _logger.LogInformation("Deleting vectors for DocumentId: {DocumentId}", documentId);

                // Delete all points with matching documentId
                await _client.DeleteAsync(
                    collectionName: _collectionName,
                    filter: new Filter
                    {
                        Must =
                        {
                        new Condition
                        {
                            Field = new FieldCondition
                            {
                                Key = "documentId",
                                Match = new Match { Integer = documentId }
                            }
                        }
                        }
                    }
                );

                _logger.LogInformation("Deleted vectors for DocumentId: {DocumentId}", documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vectors for DocumentId: {DocumentId}", documentId);
                throw;
            }
        }


        public async Task<List<SearchResult>> SearchSimilarAsync(float[] queryVector,List<int>? documentIds = null,int limit = 5)
        {
            try
            {
                _logger.LogInformation("Searching for similar vectors (limit: {Limit})", limit);

                Filter? filter = null;

                // If documentIds specified, create filter
                if (documentIds != null && documentIds.Any())
                {
                    filter = new Filter
                    {
                        Should =
                    {
                        documentIds.Select(id => new Condition
                        {
                            Field = new FieldCondition
                            {
                                Key = "documentId",
                                Match = new Match { Integer = id }
                            }
                        })
                    }
                    };

                    _logger.LogDebug("Filtering by DocumentIds: {DocumentIds}",
                        string.Join(", ", documentIds));
                }

                var searchResult = await _client.SearchAsync(
                    collectionName: _collectionName,
                    vector: queryVector,
                    filter: filter,
                    limit: (ulong)limit,
                    scoreThreshold: 0.6f // Minimum similarity threshold
                );

                var results = searchResult.Select(point => new SearchResult
                {
                    DocumentId = (int)point.Payload["documentId"].IntegerValue,
                    ChunkText = point.Payload["chunkText"].StringValue,
                    ChunkIndex = (int)point.Payload["chunkIndex"].IntegerValue,
                    Score = point.Score
                }).ToList();

                _logger.LogInformation("Found {Count} similar chunks", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vectors");
                throw;
            }
        }

        public async Task UpsertVectorsAsync(int documentId, List<string> chunks, List<float[]> vectors)
        {
            try
            {
                if (chunks.Count != vectors.Count)
                {
                    throw new ArgumentException("Number of chunks must match number of vectors");
                }

                _logger.LogInformation("Upserting {Count} vectors for DocumentId: {DocumentId}",
                    vectors.Count, documentId);

                var points = new List<PointStruct>();

                for (int i = 0; i < chunks.Count; i++)
                {
                    // Generate unique point ID
                    // This allows us to identify which document each chunk belongs to
                    ulong pointId = (ulong)(documentId * 10000 + i);


                    var point = new PointStruct
                    {
                        Id = pointId,
                        Vectors = vectors[i],
                        Payload =
                    {
                        ["documentId"] = documentId,
                        ["chunkIndex"] = i,
                        ["chunkText"] = chunks[i],
                        ["timestamp"] = DateTime.UtcNow.ToString("o")
                    }
                    };

                    points.Add(point);
                }

                // Upsert in batches of 100 to avoid timeouts
                const int batchSize = 100;
                for (int i = 0; i < points.Count; i += batchSize)
                {
                    var batch = points.Skip(i).Take(batchSize).ToList();

                    await _client.UpsertAsync(
                        collectionName: _collectionName,
                        points: batch
                    );

                    _logger.LogDebug("Upserted batch {BatchNumber}/{TotalBatches}",
                        (i / batchSize) + 1,
                        (points.Count + batchSize - 1) / batchSize);
                }

                _logger.LogInformation("Successfully upserted {Count} vectors for DocumentId: {DocumentId}",
                    vectors.Count, documentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting vectors for DocumentId: {DocumentId}", documentId);
                throw;
            }
        }
    }
}
