using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace T3_BD_CSharp
{
    public class ConexionBD
    {
        private static ConexionBD instanciaUnica = null;
        private static string servidor;
        private static string baseDeDatos;
        private string connectionString;
        private SqlConnection conexion;

        // Constructor privado para evitar instanciación directa
        private ConexionBD()
        {
            // Verificar que los parámetros están inicializados antes de usarlos
            if (string.IsNullOrEmpty(servidor) || string.IsNullOrEmpty(baseDeDatos))
            {
                throw new InvalidOperationException("Debe inicializar la conexión antes de obtener la instancia.");
            }

            // Crear la cadena de conexión
            connectionString = $"Server={servidor};Database={baseDeDatos};Integrated Security=True";

            // Inicializar la conexión
            conexion = new SqlConnection(connectionString);

            // Intentar abrir la conexión al crear la instancia
            ObtenerConexion();
        }

        // Método para inicializar la conexión con los parámetros de servidor y base de datos
        public static void InicializarConexion(string servidor, string baseDeDatos)
        {
            // Verificamos que los parámetros no sean nulos o vacíos
            if (string.IsNullOrEmpty(servidor) || string.IsNullOrEmpty(baseDeDatos))
            {
                throw new ArgumentException("El servidor y la base de datos no pueden ser nulos o vacíos.");
            }

            // Asignar los valores estáticos
            ConexionBD.servidor = servidor;
            ConexionBD.baseDeDatos = baseDeDatos;
        }

        // Método para obtener la única instancia de ConexionBD
        public static ConexionBD ObtenerInstancia()
        {
            if (instanciaUnica == null)
            {
                // Si los parámetros de conexión no se han inicializado, lanzamos una excepción
                if (string.IsNullOrEmpty(servidor) || string.IsNullOrEmpty(baseDeDatos))
                {
                    throw new InvalidOperationException("Debe inicializar la conexión antes de obtener la instancia.");
                }

                instanciaUnica = new ConexionBD(); // Crear la instancia solo si los parámetros están inicializados
            }
            return instanciaUnica;
        }

        // Método para obtener la conexión
        public SqlConnection ObtenerConexion()
        {
            try
            {
                // Verificar si la conexión está cerrada antes de abrirla
                if (conexion.State == System.Data.ConnectionState.Closed)
                {
                    conexion.Open();
                }
                return conexion;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // Método para obtener la instancia de la conexión (si se necesita)
        public SqlConnection ObtenerConexionInstance()
        {
            return conexion;
        }
        public void Dispose()
        {
            if (conexion != null)
            {
                if (conexion.State == System.Data.ConnectionState.Open)
                {
                    conexion.Close();
                }
                conexion.Dispose();
            }
        }
    }

}