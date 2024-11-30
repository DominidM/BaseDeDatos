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
    public partial class CitasForm : Form
    {
        public CitasForm()
        {
            InitializeComponent();
            txtBuscar.TextChanged += new EventHandler(txtBuscar_TextChanged);
            combosala.TextChanged += new EventHandler(combosala_TextChanged);
            combosala.DropDownStyle = ComboBoxStyle.DropDown; // Permite que se escriba y se muestren los resultados

        }
        private void CitasForm_Load(object sender, EventArgs e)
        {
            // Llamar a CargarDatos cuando el formulario se carga
            LlenarComboBox();

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
            string query = "SELECT * FROM Citas"; // Limitar a los primeros 10 registros
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
                datosFiltrados = ObtenerDatosCitas(textoBusqueda); // Llamamos al método con filtro
            }

            // Asignar el DataTable al DataGridView
            dataGridView1.DataSource = datosFiltrados;
        }

        public DataTable ObtenerDatosCitas(string filtro = "")
        {
            string query = "SELECT TOP 10 * FROM Citas"; // Consulta básica de las citas
            if (!string.IsNullOrEmpty(filtro))
            {
                query += " WHERE Servicio LIKE @Filtro OR Atencion LIKE @Filtro OR Estado LIKE @Filtro"; // Filtrado por servicio, atención y estado
            }

            SqlDataAdapter da = new SqlDataAdapter(query, ConexionBD.ObtenerInstancia().ObtenerConexionInstance());
            da.SelectCommand.Parameters.AddWithValue("@Filtro", "%" + filtro + "%"); // Parámetro para evitar SQL Injection
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }


        public void LlenarComboBox(string filtro = "")
        {
            string query = "SELECT Codigo FROM Salas"; // Cambia la columna o tabla si es necesario
            if (!string.IsNullOrEmpty(filtro))
            {
                query += " WHERE Codigo LIKE @Filtro"; // Usar LIKE si el filtro es texto
            }

            SqlDataAdapter da = new SqlDataAdapter(query, ConexionBD.ObtenerInstancia().ObtenerConexionInstance());

            // Asegúrate de que el filtro sea un valor numérico antes de agregarlo al parámetro
            if (int.TryParse(filtro, out int codigo))
            {
                da.SelectCommand.Parameters.AddWithValue("@Filtro", codigo); // El filtro es numérico
            }
            else
            {
                da.SelectCommand.Parameters.AddWithValue("@Filtro", "%" + filtro + "%"); // Filtro con LIKE
            }

            DataTable dt = new DataTable();
            da.Fill(dt);

            // Verifica si se obtuvieron datos
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No se encontraron resultados.");
            }

            // Limpiar el ComboBox antes de llenarlo
            combosala.Items.Clear();

            // Llenar el ComboBox con los datos obtenidos
            foreach (DataRow row in dt.Rows)
            {
                combosala.Items.Add(row["Codigo"].ToString()); // Asegúrate de que la columna coincida
            }
        }

        private void combosala_TextChanged(object sender, EventArgs e)
        {
            string textoBusqueda = combosala.Text;

            // Llamar al método para llenar el ComboBox con los resultados filtrados
            LlenarComboBox(textoBusqueda);
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
                    ? "SELECT TOP (@Limite) * FROM Citas"
                    : "SELECT * FROM Citas";

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
                int idFila = Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["CodigoCita"].Value); // Suponiendo que hay una columna "ID"

                // Obtener la conexión a la base de datos
                SqlConnection conn = conexionBD.ObtenerConexion();

                // Crear la consulta SQL para eliminar la fila correspondiente en la base de datos
                string query = "DELETE FROM Citas WHERE CodigoCita = @CodigoCita"; // Cambia "Paciente" y "ID" según tu tabla
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CodigoCita", idFila);
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

        }

        public void InsertarDatos()
        {
            // Obtener los textos de los TextBox y ComboBoxes
            string codigocita = textBox1.Text;
            string codigoexpe = textBox2.Text;
            string estado = comboBox5.Text;
            DateTime fecha = dateTimePicker1.Value;
            string entrada = textBox5.Text;
            string salida = textBox6.Text;
            string seguro = comboBox2.Text;
            string atencion = comboBox3.Text;
            string servicio = comboBox4.Text;
            string personal = comboBox7.Text;
            string sala = combosala.Text;
            string sede = comboBox1.Text;

            // Validar campos obligatorios
            if (string.IsNullOrEmpty(codigocita) || string.IsNullOrEmpty(codigoexpe) || string.IsNullOrEmpty(estado) ||
                string.IsNullOrEmpty(entrada) || string.IsNullOrEmpty(salida) || string.IsNullOrEmpty(personal) ||
                string.IsNullOrEmpty(sala))
            {
                MessageBox.Show("Por favor complete todos los campos obligatorios.");
                return;
            }

            // Validar que los valores de Entrada y Salida sean tiempos válidos
            if (!TimeSpan.TryParse(entrada, out TimeSpan entradaTime) || !TimeSpan.TryParse(salida, out TimeSpan salidaTime))
            {
                MessageBox.Show("Por favor ingrese horarios válidos para la entrada y salida.");
                return;
            }

            // Crear la consulta SQL para insertar los datos en la tabla Citas
            string query = "INSERT INTO Citas (CodigoExpediente, Estado, Fecha, Entrada, Salida, Seguro, Atencion, Servicio, Personal, Sala, Sede) " +
                           "VALUES (@CodigoExpediente, @Estado, @Fecha, @Entrada, @Salida, @Seguro, @Atencion, @Servicio, @Personal, @Sala, @Sede); " +
                           "SELECT SCOPE_IDENTITY();"; // Para obtener el Codigo generado automáticamente (ID)

            // Obtener la conexión desde la instancia de ConexionBD
            SqlConnection conn = ConexionBD.ObtenerInstancia().ObtenerConexionInstance();

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // Agregar los parámetros a la consulta
                cmd.Parameters.AddWithValue("@CodigoExpediente", Convert.ToInt32(codigoexpe)); // Convierte el código de expediente a entero
                cmd.Parameters.AddWithValue("@Estado", estado);
                cmd.Parameters.AddWithValue("@Fecha", fecha);
                cmd.Parameters.AddWithValue("@Entrada", entradaTime); // Asignar el tiempo de entrada
                cmd.Parameters.AddWithValue("@Salida", salidaTime); // Asignar el tiempo de salida
                cmd.Parameters.AddWithValue("@Seguro", seguro);
                cmd.Parameters.AddWithValue("@Atencion", atencion);
                cmd.Parameters.AddWithValue("@Servicio", servicio);
                cmd.Parameters.AddWithValue("@Personal", Convert.ToInt32(personal)); // Asumimos que el Personal es un código numérico
                cmd.Parameters.AddWithValue("@Sala", Convert.ToInt32(sala)); // Asumimos que la Sala es un código numérico
                cmd.Parameters.AddWithValue("@Sede", sede);

                try
                {
                    // Abrir la conexión
                    conn.Open();

                    // Ejecutar el comando de inserción y obtener el Codigo generado
                    int codigoGenerado = Convert.ToInt32(cmd.ExecuteScalar());

                    // Mostrar el Código generado al usuario
                    MessageBox.Show($"Datos insertados correctamente. Codigo Cita: {codigoGenerado}");

                    // Limpiar los TextBox después de insertar
                    textBox1.Clear();
                    textBox2.Clear();
                    textBox5.Clear();
                    textBox6.Clear();
                    comboBox1.SelectedIndex = -1; // Deselecciona el ComboBox de Sede
                    comboBox2.SelectedIndex = -1; // Deselecciona el ComboBox de Seguro
                    comboBox3.SelectedIndex = -1; // Deselecciona el ComboBox de Atención
                    comboBox4.SelectedIndex = -1; // Deselecciona el ComboBox de Servicio
                    comboBox5.SelectedIndex = -1; // Deselecciona el ComboBox de Estado
                    comboBox7.SelectedIndex = -1; // Deselecciona el ComboBox de Personal
                    combosala.SelectedIndex = -1; // Deselecciona el ComboBox de Sala
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
            textBox5.Clear();
            textBox6.Clear();
            comboBox1.SelectedIndex = -1; // Deselecciona el ComboBox de Sede
            comboBox2.SelectedIndex = -1; // Deselecciona el ComboBox de Seguro
            comboBox3.SelectedIndex = -1; // Deselecciona el ComboBox de Atención
            comboBox4.SelectedIndex = -1; // Deselecciona el ComboBox de Servicio
            comboBox5.SelectedIndex = -1; // Deselecciona el ComboBox de Estado
            comboBox7.SelectedIndex = -1; // Deselecciona el ComboBox de Personal
            combosala.SelectedIndex = -1;
        }
    }
}
