using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioUsuarios
    {
        Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado);
        Task<int> CrearUsuario(Usuario usuario);
    }
    public class RepositorioUsuarios: IRepositorioUsuarios
    {
        private readonly IConfiguration configuration;
        private readonly string connectionString;

        public RepositorioUsuarios(IConfiguration configuration)
        {
            this.configuration = configuration;
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CrearUsuario(Usuario usuario)
        {
            usuario.EmailNormalizado = usuario.Email.ToUpper();
            using var connection = new SqlConnection(connectionString);
            var usuarioId = await connection.QuerySingleAsync<int>(@"INSERT INTO Usuarios(Email, EmailNormalizado, PasswordHash)
            VALUES (@Email, @EmailNormalizado, @PasswordHash); SELECT SCOPE_IDENTITY()", usuario);

            await connection.ExecuteAsync("Crear_Datos_Usuario_Nuevo", new { usuarioId }, commandType: System.Data.CommandType.StoredProcedure);
            return usuarioId;
        }

        public async Task<Usuario> BuscarUsuarioPorEmail(string emailNormalizado)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<Usuario>("SELECT * FROM Usuarios WHERE EmailNormalizado = @EmailNormalizado",
                new { emailNormalizado });
        }

    }
}
