﻿using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json.Linq;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T>
    {
        public async Task<QueriedPaginatedResult<T>> PagedListAsync<Pid>(Pid pk, int limit, string? token)
        {
            var skProperty = entityInfo.Sk();
            if (skProperty == null)
            {
                throw new Exception("Entity requires Sk attribute to use PagedListAsync.");
            }

            var pkProperty = entityInfo.Pk();

            var paginationToken = PaginationToken.FromString(token);
            int currentPage = 1;
            if (paginationToken != null)
            {
                if (paginationToken.Forward)
                {
                    currentPage = paginationToken.CurrentPage + 1;
                }
                else
                {
                    currentPage = paginationToken.CurrentPage - 1;
                }
            }

            var pkValue = PkDictionary(pk).First().Value;
            var request = new QueryRequest
            {
                TableName = entityInfo.TableName,
                ScanIndexForward = paginationToken?.Forward ?? true,
                KeyConditionExpression = $"{pkProperty.Name} = :v_Id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":v_Id", pkValue}
                },
                Limit = limit,
                ExclusiveStartKey = paginationToken != null ? new Dictionary<string, AttributeValue>
                {
                    { pkProperty.Name , pkValue },
                    { skProperty.Name , SkDictionary(paginationToken.Sk).First().Value }
                } : new()
            };
            var queryResult = await this.context.Client.QueryAsync(request);
            var result = new QueriedPaginatedResult<T>(currentPage);

            if (queryResult.ScannedCount == 0)
            {
                return result;
            }

            if (!paginationToken?.Forward ?? false)
            {
                queryResult.Items.Reverse();
            }

            foreach (var item in queryResult.Items)
            {
                var data = DictionaryToEntity(item);
                result.Items.Add(data);
            }

            var firstSk = queryResult.Items.First().Where(a => a.Key == skProperty.Name).First();

            if (paginationToken == null || firstSk.Value.S == paginationToken.FirstSk)
            {
                result.PreviousToken = null;
            }
            else
            {
                result.PreviousToken = (new PaginationToken(paginationToken.FirstSk, false, firstSk.Value.S, currentPage)).ToString();
            }

            if (queryResult.ScannedCount != limit)
            {
                result.NextToken = null;
            }
            else if (paginationToken?.Forward ?? true)
            {
                if (queryResult.LastEvaluatedKey.Count == 0)
                {
                    result.NextToken = null;
                }
                else
                {
                    var sk = queryResult.LastEvaluatedKey.Where(a => a.Key == skProperty.Name).First();
                    result.NextToken = (new PaginationToken(paginationToken?.FirstSk ?? firstSk.Value.S, true, sk.Value.S, currentPage)).ToString();
                }
            }
            else
            {
                var lastSk = queryResult.Items.Last().Where(a => a.Key == skProperty.Name).First();
                result.NextToken = (new PaginationToken(paginationToken.FirstSk, true, lastSk.Value.S, currentPage)).ToString();
            }

            return result;
        }
    }
}
