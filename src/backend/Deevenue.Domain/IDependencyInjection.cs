using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deevenue.Domain;

public interface IDependencyInjection
{
    IServiceCollection Services { get; }

    IConfiguration Config { get; }
}
