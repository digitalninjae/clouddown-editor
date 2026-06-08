using CloudDown.Editor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CloudDown.Editor;

/// <summary>
/// Hosting extensions for registering CloudDown.Editor with a MAUI app.
/// </summary>
public static class MauiAppBuilderExtensions
{
    /// <summary>
    /// Registers CloudDown.Editor services (and, in future, control handlers) with the
    /// MAUI app builder. Call from <c>MauiProgram.CreateMauiApp</c>.
    /// </summary>
    public static MauiAppBuilder ConfigureCloudDownEditor(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IMarkdownService, MarkdownService>();

        // Platform-specific MarkdownEditor handlers will be registered here as they are implemented.
        return builder;
    }
}
