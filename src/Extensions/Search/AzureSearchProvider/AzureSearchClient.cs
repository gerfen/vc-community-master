﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtoCommerce.Search.Providers.Azure
{
    using RedDog.Search;
    using RedDog.Search.Http;
    using RedDog.Search.Model;

    public class AzureSearchClient
    {
        private ApiConnection _connection = null;
        private IndexManagementClient _IndexClient = null;
        private IndexQueryClient _QueryClient = null;
        private IndexManagementClient IndexClient
        {
            get
            {
                if (_IndexClient == null)
                {
                    _IndexClient = new IndexManagementClient(_connection);
                }

                return _IndexClient;
            }
        }

        private IndexQueryClient QueryClient
        {
            get
            {
                if (_QueryClient == null)
                {
                    _QueryClient = new IndexQueryClient(_connection);
                }

                return _QueryClient;
            }
        }

        public AzureSearchClient(ApiConnection connection)
        {
            _connection = connection;
        }

        public async Task<Index> GetIndex(string scope)
        {
            var index = await IndexClient.GetIndexAsync(scope);
            return index.Body;
        }

        public async Task<IApiResponse<Index>> CreateIndex(Index index)
        {
            var result = await IndexClient.CreateIndexAsync(index);
            return result;
        }

        public async Task<IApiResponse<Index>> UpdateIndex(Index index)
        {
            var result = await IndexClient.UpdateIndexAsync(index);
            return result;
        }

        public async Task<IApiResponse<IEnumerable<IndexOperationResult>>> IndexBulk(string indexName, IEnumerable<AzureDocument> documents = null)
        {
            var operations = from d in documents
                select new IndexOperation(IndexOperationType.Upload, AzureDocument.KeyFieldName, d.Id.ToString()) { Properties = d };

            var result = await IndexClient.PopulateAsync(indexName, operations.ToArray());

            return result;
        }

        public async Task<IApiResponse> DeleteIndex(string indexName)
        {
            var response = await IndexClient.DeleteIndexAsync(indexName);
            return response;
        }

        public async Task<IApiResponse<SearchQueryResult>> Search(string indexName, SearchQuery query)
        {
            var response = await QueryClient.SearchAsync(indexName, query);
            return response;
        }
    }
}
