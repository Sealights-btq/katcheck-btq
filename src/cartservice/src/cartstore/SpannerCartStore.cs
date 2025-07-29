// Copyright 2021 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Google.Cloud.Spanner.Data;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace cartservice.cartstore
{
    /// <summary>
    /// Provides cart storage operations using Google Cloud Spanner.
    /// </summary>
    public class SpannerCartStore : ICartStore
    {
        private static readonly string TableName = "CartItems";
        private static readonly string DefaultInstanceName = "onlineboutique";
        private static readonly string DefaultDatabaseName = "carts";
        private readonly string databaseString;
        private readonly ILogger<SpannerCartStore> _logger;

        public SpannerCartStore(IConfiguration configuration, ILogger<SpannerCartStore> logger)
        {
            _logger = logger;

            string spannerProjectId = configuration["SPANNER_PROJECT"];
            string spannerInstanceId = configuration["SPANNER_INSTANCE"];
            string spannerDatabaseId = configuration["SPANNER_DATABASE"];
            string spannerConnectionString = configuration["SPANNER_CONNECTION_STRING"];
            SpannerConnectionStringBuilder builder = new();

            if (!string.IsNullOrEmpty(spannerConnectionString))
            {
                builder.DataSource = spannerConnectionString;
                databaseString = builder.ToString();
                _logger.LogInformation("Using provided Spanner connection string: {ConnectionString}", databaseString);
                return;
            }

            if (string.IsNullOrEmpty(spannerInstanceId))
                spannerInstanceId = DefaultInstanceName;
            if (string.IsNullOrEmpty(spannerDatabaseId))
                spannerDatabaseId = DefaultDatabaseName;

            builder.DataSource =
                $"projects/{spannerProjectId}/instances/{spannerInstanceId}/databases/{spannerDatabaseId}";
            databaseString = builder.ToString();

            _logger.LogInformation("Built Spanner connection string: {ConnectionString}", databaseString);
        }

        /// <inheritdoc />
        public async Task AddItemAsync(string userId, string productId, int quantity)
        {
            _logger.LogInformation("AddItemAsync called for userId={UserId}, productId={ProductId}, quantity={Quantity}",
                userId, productId, quantity);

            try
            {
                using SpannerConnection spannerConnection = new(databaseString);
                await spannerConnection.RunWithRetriableTransactionAsync(async transaction =>
                {
                    int currentQuantity = 0;
                    var quantityLookup = spannerConnection.CreateSelectCommand(
                        $"SELECT * FROM {TableName} WHERE userId = @userId AND productId = @productId",
                        new SpannerParameterCollection
                        {
                            { "userId", SpannerDbType.String },
                            { "productId", SpannerDbType.String }
                        });
                    quantityLookup.Parameters["userId"].Value = userId;
                    quantityLookup.Parameters["productId"].Value = productId;
                    quantityLookup.Transaction = transaction;

                    using var reader = await quantityLookup.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        currentQuantity += reader.GetFieldValue<int>("quantity");
                    }

                    var cmd = spannerConnection.CreateInsertOrUpdateCommand(TableName,
                        new SpannerParameterCollection
                        {
                            { "userId", SpannerDbType.String },
                            { "productId", SpannerDbType.String },
                            { "quantity", SpannerDbType.Int64 }
                        });
                    cmd.Parameters["userId"].Value = userId;
                    cmd.Parameters["productId"].Value = productId;
                    cmd.Parameters["quantity"].Value = currentQuantity + quantity;
                    cmd.Transaction = transaction;

                    await cmd.ExecuteNonQueryAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add item to cart for userId={UserId}", userId);
                throw new RpcException(
                    new Status(StatusCode.FailedPrecondition, $"Can't access cart storage at {databaseString}. {ex}"));
            }
        }

        /// <inheritdoc />
        public async Task<Hipstershop.Cart> GetCartAsync(string userId)
        {
            _logger.LogInformation("GetCartAsync called for userId={UserId}", userId);
            Hipstershop.Cart cart = new();

            try
            {
                using SpannerConnection spannerConnection = new(databaseString);
                var cmd = spannerConnection.CreateSelectCommand(
                    $"SELECT * FROM {TableName} WHERE userId = @userId",
                    new SpannerParameterCollection {
                        { "userId", SpannerDbType.String }
                    });
                cmd.Parameters["userId"].Value = userId;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    cart.UserId = userId;

                    Hipstershop.CartItem item = new()
                    {
                        ProductId = reader.GetFieldValue<string>("productId"),
                        Quantity = reader.GetFieldValue<int>("quantity")
                    };
                    cart.Items.Add(item);
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve cart for userId={UserId}", userId);
                throw new RpcException(
                    new Status(StatusCode.FailedPrecondition, $"Can't access cart storage at {databaseString}. {ex}"));
            }
        }

        /// <inheritdoc />
        public async Task EmptyCartAsync(string userId)
        {
            _logger.LogInformation("EmptyCartAsync called for userId={UserId}", userId);

            try
            {
                using SpannerConnection spannerConnection = new(databaseString);
                var cmd = spannerConnection.CreateDmlCommand(
                    $"DELETE FROM {TableName} WHERE userId = @userId",
                    new SpannerParameterCollection
                    {
                        { "userId", SpannerDbType.String }
                    });
                cmd.Parameters["userId"].Value = userId;

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to empty cart for userId={UserId}", userId);
                throw new RpcException(
                    new Status(StatusCode.FailedPrecondition, $"Can't access cart storage at {databaseString}. {ex}"));
            }
        }

        /// <summary>
        /// Basic liveness check.
        /// </summary>
        public bool Ping()
        {
            _logger.LogDebug("Ping called");
            return true;
        }
    }
}
