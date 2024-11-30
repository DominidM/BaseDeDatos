using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace T3_BD_CSharp
{
    internal static class Program
    {
        // Este método inicia la aplicación y el formulario principal
        [STAThread]
        static void Main()
        {
            // Crear la instancia de la conexión al iniciar la aplicación
            string servidor = @"DESKTOP-5TE4LNL\SQLEXPRESS";  // Nombre de tu servidor
            string baseDeDatos = "BDHOSPITAL";  // Nombre de tu base de datos
            ConexionBD.InicializarConexion(servidor, baseDeDatos);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1()); // Ejecutar Form1
        }
    }
}
