using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id, int usuarioId); // Modificado
        Task<int> ContarUsuarioId(int usuarioId);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacionViewModel);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioCategorias: IRepositorioCategorias
    {
        private readonly string connectionString;
        public RepositorioCategorias(IConfiguration configuration) 
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO Categorias (Nombre,TipoOperacionId,UsuarioId)
            VALUES (@Nombre,@TipoOperacionId,@UsuarioId)

            SELECT SCOPE_IDENTITY();", categoria);

            categoria.Id = id;  
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, PaginacionViewModel paginacionViewModel)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(
                @$"SELECT * FROM Categorias WHERE UsuarioId = @UsuarioId
                ORDER BY Nombre
                OFFSET {paginacionViewModel.RecordsASaltar} ROWS FETCH NEXT {paginacionViewModel.RecordsPorPagina} ROWS ONLY",
                new { usuarioId }
            );
        }

        public async Task<int> ContarUsuarioId(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.ExecuteScalarAsync<int>(@"SELECT COUNT(*) FROM Categorias WHERE UsuarioId = @UsuarioId", new { usuarioId });
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacion)
        {
            using var connection = new SqlConnection(connectionString);
            var query = @"SELECT Id, Nombre FROM Categorias 
                  WHERE UsuarioId = @UsuarioId AND TipoOperacionId = @TipoOperacionId";

            var categorias = await connection.QueryAsync<Categoria>(query, new { UsuarioId = usuarioId, TipoOperacionId = tipoOperacion });

            Console.WriteLine($"Categorías encontradas: {categorias.Count()}"); // Debug en consola

            return categorias;
        }



        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection( connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(
                @"SELECT * FROM Categorias WHERE Id = @Id AND UsuarioId = @UsuarioId", new { id, usuarioId });
        }
        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@" UPDATE Categorias 
            SET Nombre = @Nombre, TipoOperacionId = @TipoOperacionId WHERE Id = @Id", categoria);
        }
        public async Task Borrar(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("DELETE FROM Categorias WHERE Id = @Id AND UsuarioId = @UsuarioId",
                                          new { id,  usuarioId });
        }


    }
}
