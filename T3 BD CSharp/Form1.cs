using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace T3_BD_CSharp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void pacienteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Crear una nueva instancia del formulario PacientForm
            PacienteForm pacientForm = new PacienteForm();

            // Establecer que PacientForm se muestra dentro de Form1 (no como una ventana nueva)
            pacientForm.TopLevel = false;
            pacientForm.FormBorderStyle = FormBorderStyle.None; // Eliminar bordes del formulario para que se maximice
            pacientForm.Dock = DockStyle.Fill; // Hace que el formulario ocupe todo el Panel
            pacientForm.Show();

            // Añadir PacientForm al Panel de Form1
            panel1.Controls.Clear(); // Limpiar cualquier formulario anterior (si lo hay)
            panel1.Controls.Add(pacientForm); // Agregar PacientForm al Panel
        }

        private void personalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Crear una nueva instancia del formulario PacientForm
            PersonalForm personalForm = new PersonalForm();

            // Establecer que PacientForm se muestra dentro de Form1 (no como una ventana nueva)
            personalForm.TopLevel = false;
            personalForm.FormBorderStyle = FormBorderStyle.None; // Eliminar bordes del formulario para que se maximice
            personalForm.Dock = DockStyle.Fill; // Hace que el formulario ocupe todo el Panel
            personalForm.Show();

            // Añadir PacientForm al Panel de Form1
            panel1.Controls.Clear(); // Limpiar cualquier formulario anterior (si lo hay)
            panel1.Controls.Add(personalForm); // Agregar PacientForm al Panel
        }

        private void salaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Crear una nueva instancia del formulario PacientForm
            SalasForm salasForm = new SalasForm();

            // Establecer que PacientForm se muestra dentro de Form1 (no como una ventana nueva)
            salasForm.TopLevel = false;
            salasForm.FormBorderStyle = FormBorderStyle.None; // Eliminar bordes del formulario para que se maximice
            salasForm.Dock = DockStyle.Fill; // Hace que el formulario ocupe todo el Panel
            salasForm.Show();

            // Añadir PacientForm al Panel de Form1
            panel1.Controls.Clear(); // Limpiar cualquier formulario anterior (si lo hay)
            panel1.Controls.Add(salasForm); // Agregar PacientForm al Panel
        }

        private void citasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Crear una nueva instancia del formulario PacientForm
            CitasForm citasForm = new CitasForm();

            // Establecer que PacientForm se muestra dentro de Form1 (no como una ventana nueva)
            citasForm.TopLevel = false;
            citasForm.FormBorderStyle = FormBorderStyle.None; // Eliminar bordes del formulario para que se maximice
            citasForm.Dock = DockStyle.Fill; // Hace que el formulario ocupe todo el Panel
            citasForm.Show();

            // Añadir PacientForm al Panel de Form1
            panel1.Controls.Clear(); // Limpiar cualquier formulario anterior (si lo hay)
            panel1.Controls.Add(citasForm); // Agregar PacientForm al Panel
        }

        private void pagosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Crear una nueva instancia del formulario PacientForm
            PagosForm pagosForm = new PagosForm();

            // Establecer que PacientForm se muestra dentro de Form1 (no como una ventana nueva)
            pagosForm.TopLevel = false;
            pagosForm.FormBorderStyle = FormBorderStyle.None; // Eliminar bordes del formulario para que se maximice
            pagosForm.Dock = DockStyle.Fill; // Hace que el formulario ocupe todo el Panel
            pagosForm.Show();

            // Añadir PacientForm al Panel de Form1
            panel1.Controls.Clear(); // Limpiar cualquier formulario anterior (si lo hay)
            panel1.Controls.Add(pagosForm); // Agregar PacientForm al Panel
        }

        private void configuracionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Crear una nueva instancia del formulario PacientForm
            ConfiguracionForm configuracionForm = new ConfiguracionForm();

            // Establecer que PacientForm se muestra dentro de Form1 (no como una ventana nueva)
            configuracionForm.TopLevel = false;
            configuracionForm.FormBorderStyle = FormBorderStyle.None; // Eliminar bordes del formulario para que se maximice
            configuracionForm.Dock = DockStyle.Fill; // Hace que el formulario ocupe todo el Panel
            configuracionForm.Show();

            // Añadir PacientForm al Panel de Form1
            panel1.Controls.Clear(); // Limpiar cualquier formulario anterior (si lo hay)
            panel1.Controls.Add(configuracionForm); // Agregar PacientForm al Panel
        }

        private void cerrarSesionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
