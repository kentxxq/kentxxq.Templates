using kentxxq.Templates.Blazor.UI.Interfaces;

namespace kentxxq.Templates.Blazor.Web.Client.Services
{
    public class TextService : ITextService
    {
        public string GetText()
        {
            return "data from web.client";
        }
    }
}
