using Hacienda.Domain.Entities;

namespace Hacienda.Application.Interfaces;

public interface IDataSeeder
{
    Task CargarDatosAsync();
}