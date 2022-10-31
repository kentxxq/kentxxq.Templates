using kentxxq.Templates.Blazor.UI.Interfaces;

namespace kentxxq.Templates.Blazor.Native.Services;

public class TextService : ITextService
{
    public string GetText()
    {
        return "data from native";
    }
}