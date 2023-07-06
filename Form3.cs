using System;
using System.Windows.Forms;

namespace Supervisorio_Reabilitacao
{
    public partial class frmFormulario : Form
    {
        public string nome;
        public string idade;
        public string altura;
        public string peso;
        public string sexo;
        public bool formularioPreenchido = false;

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
            textAltura.Clear();
            textIdade.Clear();
            textPeso.Clear();
            textSexo.Clear();            
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
            nome = txtNome.Text;
            idade = textIdade.Text;
            altura = textAltura.Text;
            peso = textPeso.Text;
            sexo = textSexo.Text;

            formularioPreenchido = true;

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

        private void lblIdade_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
