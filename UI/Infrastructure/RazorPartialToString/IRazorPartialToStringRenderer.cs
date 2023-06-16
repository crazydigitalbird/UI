using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UI.Infrastructure
{
    public interface IRazorPartialToStringRenderer
    {
        Task<string> RenderPartialToStringAsync<TModel>(string partialName, TModel model, string viewDataKey = null, object viewDataValue = null);
    }
}
