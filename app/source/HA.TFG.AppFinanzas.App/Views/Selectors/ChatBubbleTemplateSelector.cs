using HA.TFG.AppFinanzas.Core.Features.Recomendaciones;

namespace HA.TFG.AppFinanzas.App.Views.Selectors;

public sealed class ChatBubbleTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserTemplate { get; set; }
    public DataTemplate? AssistantTemplate { get; set; }

    protected override DataTemplate? OnSelectTemplate(object item, BindableObject container)
        => item is ChatMessage { IsUser: true } ? UserTemplate : AssistantTemplate;
}
