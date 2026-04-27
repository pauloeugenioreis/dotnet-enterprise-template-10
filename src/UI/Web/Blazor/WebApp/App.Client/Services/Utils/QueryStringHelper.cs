using Microsoft.AspNetCore.WebUtilities;

namespace BlazorApp.Client.Services.Utils;

public static class QueryStringHelper
{
    public static string ToQueryString(string baseUrl, object? filters)
    {
        if (filters == null) return baseUrl;

        var query = new Dictionary<string, string?>();
        var properties = filters.GetType().GetProperties();

        foreach (var prop in properties)
        {
            var value = prop.GetValue(filters);
            if (value != null)
            {
                if (value is DateTime dt)
                {
                    query[prop.Name] = dt.ToString("yyyy-MM-dd");
                }
                else
                {
                    query[prop.Name] = value.ToString();
                }
            }
        }

        return QueryHelpers.AddQueryString(baseUrl, query);
    }
}
