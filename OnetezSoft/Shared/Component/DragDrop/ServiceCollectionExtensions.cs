using Microsoft.Extensions.DependencyInjection;
using OnetezSoft.Shared.Component.DragDrop;

namespace OnetezSoft.Shared.Component.DragDrop;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomDragDrop(this IServiceCollection services)
    {
        return services.AddScoped(typeof(DragDropService<>));
    }
}
