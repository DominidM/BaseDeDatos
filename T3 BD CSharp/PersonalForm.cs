using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace T3_BD_CSharp
{
    public partial class PersonalForm : Form
    {
        public PersonalForm()
        {
            InitializeComponent();
            txtBuscar.TextChanged += new EventHandler(txtBuscar_TextChanged);

        }

        private void PersonalForm_Load(object sender, EventArgs e)
        {
            // Llamar a CargarDatos cuando el formulario se carga
            CargarDatos();
        }

        // Método para cargar los datos en el DataGridView
        public void CargarDatos()
        {
            // Asumiendo que tienes un método que obtiene los datos de la base de datos
            DataTable datos = ObtenerDatosDeBaseDeDatos();

            // Verifica si hay datos para mostrar
            if (datos.Rows.Count == 0)
            {
                MessageBox.Show("No se encontraron datos.");
            }

            // Asigna el DataTable al DataGridView, lo que llenará las columnas automáticamente
            dataGridView1.DataSource = datos;
        }

        // Método para obtener los datos de la base de datos
        public DataTable ObtenerDatosDeBaseDeDatos()
        {
            string query = "SELECT * FROM Personal"; // Limitar a los primeros 10 registros
            SqlDataAdapter da = new SqlDataAdapter(query, ConexionBD.ObtenerInstancia().ObtenerConexionInstance());
            DataTable dt = new DataTable();

            try
            {
                da.Fill(dt);  // Aquí podría ocurrir un error
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error en la consulta: {ex.Message}", "Error de SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }
        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            // Obtener el texto ingresado en el TextBox
            string textoBusqueda = txtBuscar.Text;

            // Llamar al método para obtener los datos filtrados
            DataTable datosFiltrados;

            if (string.IsNullOrEmpty(textoBusqueda))
            {
                // Si no hay texto en el TextBox, obtener todos los registros
                datosFiltrados = ObtenerDatosDeBaseDeDatos(); // Llamamos al método sin filtro
            }
            else
            {
                // Si hay texto, obtener los registros filtrados
                datosFiltrados = ObtenerDatosPersonal(textoBusqueda); // Llamamos al método con filtro
            }

            // Asignar el DataTable al DataGridView
            dataGridView1.DataSource = datosFiltrados;
        }
        public DataTable ObtenerDatosPersonal(string filtro = "")
        {
            string query = "SELECT TOP 10 * FROM Personal"; // Aquí puedes modificar la consulta
            if (!string.IsNullOrEmpty(filtro))
            {
                query += " WHERE Nombre LIKE @Filtro OR ApellidoPaterno LIKE @Filtro OR ApellidoMaterno LIKE @Filtro"; // Filtrado por nombre y apellidos
            }

            SqlDataAdapter da = new SqlDataAdapter(query, ConexionBD.ObtenerInstancia().ObtenerConexionInstance());
            da.SelectCommand.Parameters.AddWithValue("@Filtro", "%" + filtro + "%"); // Parámetro para evitar SQL Injection
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        private void imprimirToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ImprimirPDF();
        }
        private void ImprimirPDF()
        {
            try
            {
                // Solicitar la cantidad de filas que el usuario desea imprimir
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "¿Cuántas filas deseas incluir en el PDF? (Escribe '0' para incluir todo)",
                    "Cantidad de Filas",
                    "0");

                if (!int.TryParse(input, out int cantidadFilas) || cantidadFilas < 0)
                {
                    MessageBox.Show("Por favor, ingrese un número válido.", "Entrada inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Cargar los datos directamente desde la base de datos
                ConexionBD conexionBD = ConexionBD.ObtenerInstancia();
                SqlConnection conn = conexionBD.ObtenerConexion();

                DataTable dataTable = CargarDatosDesdeBD(cantidadFilas, conn);

                if (dataTable.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron datos para generar el PDF.", "Sin Datos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Mostrar un cuadro de diálogo para guardar el archivo
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Archivo PDF (*.pdf)|*.pdf";
                saveFileDialog.Title = "Guardar como PDF";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string rutaArchivo = saveFileDialog.FileName;

                    // Crear el archivo PDF
                    CrearPDFDesdeBaseDeDatos(rutaArchivo, dataTable);
                    MessageBox.Show("PDF guardado exitosamente en: " + rutaArchivo, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear el PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Función para cargar los datos desde la base de datos
        private DataTable CargarDatosDesdeBD(int limite, SqlConnection conn)
        {
            DataTable dt = new DataTable();

            try
            {
                string query = limite > 0
                    ? "SELECT TOP (@Limite) * FROM Personal"
                    : "SELECT * FROM Personal";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (limite > 0)
                        cmd.Parameters.AddWithValue("@Limite", limite);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }

        // Función para crear el PDF desde los datos de la base de datos
        private void CrearPDFDesdeBaseDeDatos(string rutaArchivo, DataTable dataTable)
        {
            try
            {
                // Asegúrate de que la ruta de guardado del archivo sea válida
                // Cambiar a orientación horizontal (landscape)
                Document doc = new Document(PageSize.A4.Rotate()); // .Rotate() pone el tamaño de página en horizontal (paisaje)

                // Usando PdfWriter de iTextSharp.text.pdf
                PdfWriter.GetInstance(doc, new FileStream(rutaArchivo, FileMode.Create));
                doc.Open();

                // Crear una tabla de datos a partir del DataTable
                PdfPTable table = new PdfPTable(dataTable.Columns.Count);

                // Establecer un ancho específico para las columnas (opcional, ajusta según tu necesidad)
                float[] columnWidths = new float[dataTable.Columns.Count];
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    columnWidths[i] = 1f; // Puedes ajustar este valor para cambiar el tamaño de las columnas
                }
                table.SetWidths(columnWidths);

                // Añadir los encabezados de la tabla
                foreach (DataColumn column in dataTable.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER; // Alineación centrada para los encabezados
                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    table.AddCell(cell);
                }

                // Añadir las filas de datos
                foreach (DataRow row in dataTable.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(item.ToString()));
                        cell.Padding = 5; // Añadir un poco de espacio dentro de las celdas
                        cell.HorizontalAlignment = Element.ALIGN_LEFT; // Alineación a la izquierda para los datos
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        table.AddCell(cell);
                    }
                }

                // Agregar la tabla al documento
                doc.Add(table);
                doc.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void eliminarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                // Verificar si se ha seleccionado una fila en el DataGridView
                if (dataGridView1.SelectedCells.Count > 0) // Usamos SelectedCells en lugar de SelectedRows
                {
                    // Obtener la fila seleccionada
                    int rowIndex = dataGridView1.SelectedCells[0].RowIndex;

                    // Verificar que la fila seleccionada no esté en estado de "nueva" o "vacía"
                    if (dataGridView1.Rows[rowIndex].IsNewRow)
                    {
                        MessageBox.Show("No se puede eliminar una fila nueva.", "Selección inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Eliminar la fila del DataGridView
                    dataGridView1.Rows.RemoveAt(rowIndex);

                    // Obtener la instancia única de ConexionBD
                    ConexionBD conexionBD = ConexionBD.ObtenerInstancia();

                    // Eliminar la fila también de la base de datos
                    EliminarFilaDeBD(rowIndex, conexionBD);
                }
                else
                {
                    MessageBox.Show("Por favor, selecciona una fila para eliminar.", "Selección inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la fila: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Método para eliminar la fila de la base de datos (si es necesario)
        private void EliminarFilaDeBD(int rowIndex, ConexionBD conexionBD)
        {
            try
            {
                // Lógica para eliminar la fila de la base de datos según el índice
                int idFila = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["Codigo"].Value); // Suponiendo que hay una columna "ID"

                // Obtener la conexión a la base de datos
                SqlConnection conn = conexionBD.ObtenerConexion();

                // Crear la consulta SQL para eliminar la fila correspondiente en la base de datos
                string query = "DELETE FROM Personal WHERE Codigo = @Codigo"; // Cambia "Paciente" y "ID" según tu tabla
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Codigo", idFila);
                    cmd.ExecuteNonQuery(); // Ejecutar la consulta
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar de la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guardarToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            InsertarDatos();
        }

        public void InsertarDatos()
        {
            // Obtener los textos de los TextBox
            string codigo = textBox1.Text; // Codigo de personal, se asume que lo maneja la base de datos automáticamente como IDENTITY
            string nombre = textBox2.Text;
            string apellidopa = textBox3.Text;
            string apellidoma = textBox4.Text;
            DateTime fechanacimiento = dateTimePicker1.Value; // Corregido a Fecha de Nacimiento
            string genero = comboBox2.Text;
            string ciudad = textBox7.Text;
            string direccion = textBox8.Text;
            string correo = textBox5.Text;
            string cargo = comboBox1.Text;

            // Validar campos obligatorios
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellidopa) || string.IsNullOrEmpty(apellidoma) ||
                string.IsNullOrEmpty(genero) || string.IsNullOrEmpty(ciudad) || string.IsNullOrEmpty(direccion) ||
                string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(cargo))
            {
                MessageBox.Show("Por favor complete todos los campos obligatorios.");
                return;
            }

            // Crear la consulta SQL para insertar los datos en la tabla Personal
            string query = "INSERT INTO Personal (Nombre, ApellidoPaterno, ApellidoMaterno, FechaNacimiento, Genero, Ciudad, Direccion, Correo, Cargo) " +
                           "VALUES (@Nombre, @ApellidoPaterno, @ApellidoMaterno, @FechaNacimiento, @Genero, @Ciudad, @Direccion, @Correo, @Cargo); " +
                           "SELECT SCOPE_IDENTITY();"; // Para obtener el Codigo generado automáticamente (ID)

            // Obtener la conexión desde la instancia de ConexionBD
            SqlConnection conn = ConexionBD.ObtenerInstancia().ObtenerConexionInstance();

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // Agregar los parámetros a la consulta
                cmd.Parameters.AddWithValue("@Nombre", nombre);
                cmd.Parameters.AddWithValue("@ApellidoPaterno", apellidopa);
                cmd.Parameters.AddWithValue("@ApellidoMaterno", apellidoma);
                cmd.Parameters.AddWithValue("@FechaNacimiento", fechanacimiento);
                cmd.Parameters.AddWithValue("@Genero", genero);
                cmd.Parameters.AddWithValue("@Ciudad", ciudad);
                cmd.Parameters.AddWithValue("@Direccion", direccion);
                cmd.Parameters.AddWithValue("@Correo", correo);
                cmd.Parameters.AddWithValue("@Cargo", cargo);

                try
                {
                    // Abrir la conexión
                    conn.Open();

                    // Ejecutar el comando de inserción y obtener el Codigo generado
                    int codigoGenerado = Convert.ToInt32(cmd.ExecuteScalar());

                    // Mostrar el Codigo generado al usuario
                    MessageBox.Show($"Datos insertados correctamente. Codigo Personal: {codigoGenerado}");

                    // Limpiar los TextBox después de insertar
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox3.Clear();
                    textBox4.Clear();
                    dateTimePicker1.Value = DateTime.Now; // Restablecer la fecha
                    textBox7.Clear();
                    textBox8.Clear();
                    textBox5.Clear();
                    comboBox1.SelectedIndex = -1;  // Deselecciona el ComboBox
                    comboBox2.SelectedIndex = -1;  // Deselecciona el ComboBox
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al insertar los datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close(); // Cerrar la conexión si está abierta
                }
            }
        }

        private void nuevoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            dateTimePicker1.Value = DateTime.Now; // Restablecer la fecha
            textBox7.Clear();
            textBox8.Clear();
            textBox5.Clear();
            comboBox1.SelectedIndex = -1;  // Deselecciona el ComboBox
            comboBox2.SelectedIndex = -1;  // Deselecciona el ComboBox
        }
    }
    
}
