using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Testa_Kit_QSPIC40
{
    public partial class frmFormulario : Form
    {
        private object cmbSexo;
        private object txtIdade;
        private object txtAltura;
        private object txtPeso;
        private bool simSelecionado;

        public frmFormulario()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnApagar_Click(object sender, EventArgs e)
        {
            txtNome.Clear();
            txtContato.Clear();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            comboBox3.Items.Clear();
            comboBox4.Items.Clear();

            
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (simSelecionado)
            {
                // Lógica para quando o RadioButton "Sim" estiver selecionado
                MessageBox.Show("Sim foi selecionado");
            }
            else
            {
                // Lógica para quando o RadioButton "Não" estiver selecionado
                MessageBox.Show("Não foi selecionado");
            }
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            string nome = txtNome.Text;
            int idade = int.Parse((string)txtIdade);
            float altura = float.Parse((string)txtAltura);
            float peso = float.Parse((string)txtPeso);
            string sexo = cmbSexo.ToString();

            string mensagem = $"Nome: {nome}\nIdade: {idade}\nAltura: {altura}\nPeso: {peso}\nSexo: {sexo}";
            MessageBox.Show(mensagem, "Dados do formulário");
            MessageBox.Show("O participante foi cadastrado");
        }

        private void rbtNao_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtNao.Checked)
            {
                simSelecionado = false;
            }
            else
            {
                simSelecionado = true;
            }
        }

        private void rbtSim_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtSim.Checked)
            {
                simSelecionado = true;
            }
            else
            {
                simSelecionado = false;
            }
        }

        private void btnFechar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtNome_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
