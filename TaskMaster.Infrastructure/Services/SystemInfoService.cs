using System.Reflection;
using TaskMaster.Domain.Interfaces;

namespace TaskMaster.Infrastructure.Services;

public class SystemInfoService : ISystemInfoService
{
    public string GetSystemVersion()
    {
        // Esto es infraestructura porque toca detalles del sistema operativo/ensamblado
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
    }
}
