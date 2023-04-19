using Core.Models.Sheets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Encodings.Web;
using UI.Infrastructure.API;
using UI.Models;
using UI.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace UI.Infrastructure.Hubs
{
    [Authorize]
    public class DialoguesHub : Hub
    {
        private readonly ISheetClient _sheetClient;

        public DialoguesHub(ISheetClient sheetClient)
        {
            _sheetClient = sheetClient;
        }

        public async Task Dialogues(int sheetId, string criteria, string cursor)
        {
            var sheet = await _sheetClient.GetSheetAsync(sheetId);
            if (sheet != null)
            {
                var stringViewComponent = await RenderViewComponent("Dialogues", new { sheet, criteria, cursor });
                await this.Clients.Caller.SendAsync("Send", stringViewComponent);
            }            
        }

        //public class MyViewComponentContext
        //{
        //    public HttpContext HttpContext { get; set; }
        //    public ActionContext ActionContext { get; set; }
        //    public ViewDataDictionary ViewData { get; set; }
        //    public ITempDataDictionary TempData { get; set; }
        //}
        //private async Task<string> Render(MyViewComponentContext myViewComponentContext, string viewComponentName, object args)
        //{
        //    using (var writer = new StringWriter())
        //    {
        //        ChatController chatController = new ChatController();
        //        var helper = chatController.GetViewComponentHelper(myViewComponentContext, writer);
        //        var result = await helper.InvokeAsync(viewComponentName, args);  // get an IHtmlContent
        //        result.WriteTo(writer, HtmlEncoder.Default);
        //        await writer.FlushAsync();
        //        return writer.ToString();
        //    }
        //}

        public async Task<string> RenderViewComponent(string viewComponent, object args)
        {
            var sp = Context.GetHttpContext().RequestServices;

            var helper = new DefaultViewComponentHelper(
                sp.GetRequiredService<IViewComponentDescriptorCollectionProvider>(),
                HtmlEncoder.Default,
                sp.GetRequiredService<IViewComponentSelector>(),
                sp.GetRequiredService<IViewComponentInvokerFactory>(),
                sp.GetRequiredService<IViewBufferScope>());

            using (var writer = new StringWriter())
            {
                ChatController chatController = new(null, null, null, null);
                var context = new ViewContext(chatController.ControllerContext, NullView.Instance, null, null, writer, new HtmlHelperOptions());
                helper.Contextualize(context);
                var result = await helper.InvokeAsync(viewComponent, args);
                result.WriteTo(writer, HtmlEncoder.Default);
                await writer.FlushAsync();
                return writer.ToString();
            }
        }
    }
}

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    internal class NullView : IView
    {
        public static readonly NullView Instance = new();

        public string Path => string.Empty;

        public Task RenderAsync(ViewContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.CompletedTask;
        }
    }
}

