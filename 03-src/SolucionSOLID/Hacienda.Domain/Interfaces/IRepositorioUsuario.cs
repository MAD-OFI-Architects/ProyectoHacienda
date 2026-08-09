using Hacienda.Domain.Entities;

namespace Hacienda.Domain.Interfaces;

public interface IRepositorioUsuario
{
    List<Usuario> ObtenerTodos();
    void GuardarTodos(List<Usuario> usuarios);
}