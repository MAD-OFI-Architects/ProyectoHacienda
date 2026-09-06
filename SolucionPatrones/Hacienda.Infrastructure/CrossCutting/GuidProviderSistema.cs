using Hacienda.Domain.Interfaces;

namespace Hacienda.Infrastructure.CrossCutting;

public class GuidProviderSistema : IGuidProvider
{
    public Guid Nuevo() => Guid.NewGuid();
}