namespace AiAgentAspireApp.Web
{
    public class ProductApiClient(HttpClient httpClient)
    {
        public async Task<ProductModel[]> GetProductAsync(int maxItems = 10, CancellationToken cancellationToken = default)
        {
            List<ProductModel>? products = null;

            await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<ProductModel>("/product/api/products", cancellationToken))
            {
                if (products?.Count >= maxItems)
                {
                    break;
                }
                if (forecast is not null)
                {
                    products ??= [];
                    products.Add(forecast);
                }
            }

            return products?.ToArray() ?? [];
        }
    }

    public record ProductModel(int Id, string Name, string? Desc);
}
