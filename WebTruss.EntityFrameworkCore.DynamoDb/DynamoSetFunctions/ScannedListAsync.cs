using Amazon.DynamoDBv2.DocumentModel;

namespace WebTruss.EntityFrameworkCore.DynamoDb.DynamoSetFunctions
{
    public partial class DynamoSet<T>
    {
        /// <summary>
        /// <br> ScannedList is used when the table only has a partition key and no sort key. </br>
        /// <br> Pagination works differently compared to PagedList.
        /// Scan token will get increasingly larger as you traverse pages. </br>
        /// <br> Unlike PagedList, ScannedList will not return a null token when last page is reached. </br>
        /// <br> Once last page is reached, the nextToken you recieve will take you to the first page 
        /// as there is no way to identify here that the last page has been reached. </br>
        /// </summary>
        public async Task<PaginatedResult<T>> ScannedListAsync(int limit, string? token)
        {
            var list = ScanPagination.ListFromToken(token);


            var table = Table.LoadTable(context.Client, entityInfo.TableName);
            Search search = table.Scan(new ScanOperationConfig
            {
                ConsistentRead = true,
                Limit = limit,
                Select = SelectValues.AllAttributes,
                PaginationToken = list.LastOrDefault()
            });

            var searchResult = await search.GetNextSetAsync();

            if(!string.IsNullOrEmpty(token) && searchResult.Count == 0)
            {
                return await this.ScannedListAsync(limit, null);
            }

            var result = new PaginatedResult<T>(list.Count + 1);

            result.NextToken = ScanPagination.TokenFromList(list.Concat(new List<string> { search.PaginationToken }).ToList());

            if (list.Count > 0)
            {
                list.Remove(list.Last());
            }

            result.PreviousToken = ScanPagination.TokenFromList(list);

            foreach (var item in searchResult)
            {
                var data = DictionaryToEntity(item.ToAttributeMap());
                result.Items.Add(data);
            }
            return result;
        }
    }
}
