using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Korzh.EasyQuery;
using Korzh.EasyQuery.Services;

using EqDemo.Security;

namespace EqDemo.Services
{
    /// <summary>
    /// Wraps another query store (e.g. FileQueryStore) and refuses model/query ids that
    /// are not plain identifiers, so they can never be used as file path segments.
    /// </summary>
    public class ValidatingQueryStore : IQueryStore
    {
        private readonly IQueryStore _inner;

        public ValidatingQueryStore(IQueryStore inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public Task<bool> AddQueryAsync(Query query, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(query.Id)) {
                query.Id = Guid.NewGuid().ToString();
            }

            EnsureValid(query.Id);
            return _inner.AddQueryAsync(query, ct);
        }

        public Task<IEnumerable<QueryListItem>> GetAllQueriesAsync(string modelId, CancellationToken ct = default)
        {
            EnsureValid(modelId);
            return _inner.GetAllQueriesAsync(modelId, ct);
        }

        public Task<bool> LoadQueryAsync(Query query, string queryId, CancellationToken ct = default)
        {
            EnsureValid(queryId);
            return _inner.LoadQueryAsync(query, queryId, ct);
        }

        public Task<bool> RemoveQueryAsync(string modelId, string queryId, CancellationToken ct = default)
        {
            EnsureValid(modelId);
            EnsureValid(queryId);
            return _inner.RemoveQueryAsync(modelId, queryId, ct);
        }

        public Task<bool> SaveQueryAsync(Query query, bool createIfNotExist = true, CancellationToken ct = default)
        {
            EnsureValid(query.Id);
            return _inner.SaveQueryAsync(query, createIfNotExist, ct);
        }

        private static void EnsureValid(string id)
        {
            if (!StoreIdValidator.IsValid(id)) {
                throw new ArgumentException("Invalid model or query id", nameof(id));
            }
        }
    }
}
