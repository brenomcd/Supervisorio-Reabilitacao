using System;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Std;
using RosSharp.RosBridgeClient.Protocols;
using System.Threading;

namespace Supervisorio_Reabilitacao
{
    public partial class Form1 : Form
    {

        private const string path_csv = "C:\\Users\\breno\\Desktop\\Dados\\";
        private const string header = "Velocidade voluntário;Velocidade robô;Distancia;Tempo;Modo;";

        private const double distanciaInicio = 0.85;
        private const double distanciaAumentarVel = 0.60;
        private const double distanciaDiminuirVel = 1.15;
        private const double maxVel = 4.0;
        private const double minVel = 1.0;
        private const string rplidarBridgeIP = "ws://172.20.25.193:9090";

        private static float distancia = 0.0f;
        private float[] ultimasDistancias = new float[10];
        private double velocidadeDesejada = 1.0;
        private double velocidadeReal = 0.0;

        private const double velocidadeVirtual1 = 1.3;
        private const double velocidadeVirtual2 = 1.4;
        private const double velocidadeVirtual3 = 1.5;
        private const double velocidadeVirtual4 = 1.2;
        private const double velocidadeVirtual5 = 2.0;
        private int velocidadeVirtualAtual = 1;

        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private bool partidaAutomatica = false;
        private bool iniciou = false;
        private bool iniciou2 = false;

        private RosSocket rosSocket;
        private string publisherId; // esteira
        private string publisherId2; // jogo

        private frmFormulario OutroForm;

        // Instancio um arquivo de dados que será salvo na área de trabalho
        private StreamWriter sw;

        // Instanciando uma classe
        SerialPort SerialCom = new SerialPort();
        string bfRecebe = string.Empty;
        public delegate void Fdelegate(string a);

        private void InitializeTimer()
        {
            // Run this procedure in an appropriate event.  
            timer.Interval = 1000 * 20;
            timer.Enabled = true;
            // Hook up timer's tick event handler.  
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
        }

        private void timer_Tick(object sender, System.EventArgs e)
        {
                if (velocidadeVirtualAtual >= 5)
                {
                    velocidadeVirtualAtual = 1;
                }
                else
                {
                    velocidadeVirtualAtual++;
                }
        }



        // Callback chamado quando uma mensagem do tópico "/distance" é recebida
        private void DistanceCallback(Float32 message)
        {
            // Atualiza a variável global com o valor recebido
            for (int i = 8; i >= 0; i--)
            {
                ultimasDistancias[i + 1] = ultimasDistancias[i];
            }
            ultimasDistancias[0] = distancia;
            distancia = Math.Abs(message.data);
        }

        public Form1()
        {
            InitializeComponent();
            SerialCom.DataReceived += new SerialDataReceivedEventHandler(SerialCom_DataReceived);
        }

        void SerialCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //throw new NotImplementedException();
            bfRecebe = SerialCom.ReadLine();
            this.BeginInvoke(new Fdelegate(recebe_serial), new object[] { bfRecebe });
        }

        public void recebe_serial(string a)
        {
            string txt_rec = string.Empty;
            txt_rec += a;
            double aux;
            textBox2.Text = distancia.ToString();

            if (txt_rec.Length >= 8)
            {
                Float32 msg = new Float32
                {
                    data = (float)velocidadeReal
                };
                Float32 msg2 = new Float32();
                if (iniciou2 == false)
                {
                    msg2.data = 0f;
                }
                else if (velocidadeVirtualAtual == 1)
                    msg2.data = (float)velocidadeVirtual1;
                else if (velocidadeVirtualAtual == 2)
                    msg2.data = (float)velocidadeVirtual2;
                else if (velocidadeVirtualAtual == 3)
                    msg2.data = (float)velocidadeVirtual3;
                else if (velocidadeVirtualAtual == 4)
                    msg2.data = (float)velocidadeVirtual4;
                else if (velocidadeVirtualAtual == 5)
                    msg2.data = (float)velocidadeVirtual5;

                rosSocket.Publish(publisherId, msg);
                rosSocket.Publish(publisherId2, msg2);
                if (txt_rec.Substring(0, 4) == "ADC=")
                {
                    var textTemp = txt_rec.Substring(4, txt_rec.Length - 4); // Retrieves the last characters of input
                    aux = Convert.ToDouble(textTemp);
                    velocidadeReal = aux / 1618.4;
                    txtSpeed.Text = velocidadeReal.ToString();
                    if (partidaAutomatica)
                    {
                        if (distancia == 0.0 && iniciou == true)
                        {
                            // verificar se as ultimas dez posições são zero, evitando erros de reinicialização
                            bool v = true;
                            for (int i = 0; i < 10; i++)
                            {
                                if (ultimasDistancias[i] != 0.0)
                                    v = false;
                            }
                            if (v)
                            {
                                SerialCom.Write("LigaLed4 " + "\r\n");
                                listBox1.Items.Add("Enviado -> " + "LigaLed4 " + "\r\n");
                                iniciou = false;
                                velocidadeDesejada = 0.0;
                            }
                        }
                        else if (distancia > 0.0 && distancia <= distanciaInicio && iniciou == false)
                        {
                            SerialCom.Write("LigaLed3 " + "\r\n");
                            listBox1.Items.Add("Enviado -> " + "LigaLed3 " + "\r\n");
                            iniciou = true;
                            velocidadeDesejada = 1.0;
                        }
                        else if (distancia > 0.0 && distancia < distanciaAumentarVel && iniciou == true)
                        {
                            if (velocidadeDesejada < maxVel)
                                velocidadeDesejada += 0.1;
                        }
                        else if (distancia > distanciaDiminuirVel && iniciou == true)
                        {
                            if (velocidadeDesejada > minVel)
                                velocidadeDesejada -= 0.1;
                        }
                    }
                    if (velocidadeReal >= 0.7 && velocidadeDesejada != 0 && velocidadeReal < 14.8)
                    {
                        if (velocidadeDesejada < velocidadeReal - 0.09)
                        {
                            diminuirVelocidade();
                        }
                        else if (velocidadeDesejada > velocidadeReal + 0.09)
                        {
                            aumentarVelocidade();
                        }
                    }
                }
                // txt_rec = string.Empty
                // toda vez que recebe a velocidade real, escreve uma nova linha no arquivo de dados
                if (iniciou == true)
                {
                    if (velocidadeReal >= 0.7 && iniciou2 == false)
                    {
                        InitializeTimer();
                        iniciou2 = true;
                    }

                    float velocidadeRobo = 0;
                    if (iniciou2 == false)
                    {
                        velocidadeRobo = 0f;
                    }
                    else if (velocidadeVirtualAtual == 1)
                        velocidadeRobo = (float)velocidadeVirtual1;
                    else if (velocidadeVirtualAtual == 2)
                        velocidadeRobo = (float)velocidadeVirtual2;
                    else if (velocidadeVirtualAtual == 3)
                        velocidadeRobo = (float)velocidadeVirtual3;
                    else if (velocidadeVirtualAtual == 4)
                        velocidadeRobo = (float)velocidadeVirtual4;
                    else if (velocidadeVirtualAtual == 5)
                        velocidadeRobo = (float)velocidadeVirtual5;

                    sw.WriteLine(velocidadeReal + ";" + velocidadeRobo + ";" + distancia + ";" + DateTime.Now + ";" + (partidaAutomatica ? "auto" : "manual") + ";");
                }
            }

            listBox1.Items.Add("Recebido <- " + a);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int i = 0;
            foreach (string str in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(str);
                if (str == "COM1") comboBox1.SelectedIndex = i;
                comboBox1.SelectedIndex = i;
                i++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (OutroForm != null)
            {
                if (OutroForm.formularioPreenchido)
                {
                    sw = new StreamWriter(path_csv + OutroForm.nome + ".csv", true);
                    if (SerialCom.IsOpen == true) SerialCom.Close();

                    SerialCom.PortName = comboBox1.Text;
                    SerialCom.BaudRate = 9600;
                    SerialCom.DataBits = 8;
                    SerialCom.StopBits = (StopBits)1;
                    SerialCom.Parity = (Parity)(0);

                    try
                    {
                        // cria o arquivo csv com os dados do formulario
                        // sw = new StreamWriter(path_csv + OutroForm.nome + ".csv", true);
                        sw.WriteLine(" ; ; ; ; ;" + OutroForm.nome + ";" + OutroForm.sexo + ";" + OutroForm.peso + ";" + OutroForm.altura + ";" + OutroForm.idade + ";");
                        SerialCom.Open();
                        // primeira linha do arquivo de dados
                        sw.WriteLine(header);
                        // Criação do objeto RosSocket e conexão ao servidor ROS
                        rosSocket = new RosSocket(new WebSocketNetProtocol(rplidarBridgeIP));
                        publisherId = rosSocket.Advertise<Float32>("/speed_sub");
                        publisherId2 = rosSocket.Advertise<Float32>("/speed2_sub");

                        // Registro do callback para o tópico "/distance"
                        rosSocket.Subscribe<Float32>("/distance", DistanceCallback);
                        button1.Enabled = false;
                        button2.Enabled = true;
                        button4.Enabled = true;
                        toolStripStatusLabel1.Text = "CONECTADO";
                    }
                    catch (Exception w)
                    {
                        //MessageBox.Show(w.ToString());
                        MessageBox.Show("Não foi possivel abrir a porta serial !");
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SerialCom.DiscardInBuffer();
                SerialCom.DiscardOutBuffer();
                SerialCom.Close();
                button1.Enabled = true;
                button2.Enabled = false;
                button4.Enabled = false;
                toolStripStatusLabel1.Text = "DESCONECTADO";
            }
            catch (Exception W)
            {
                MessageBox.Show(W.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write(textBox1.Text + "\r\n");
                listBox1.Items.Add("Enviado -> " + textBox1.Text + "\r\n");
                textBox1.Text = "";
                textBox1.Focus();
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (button1.Enabled == false)
                button2_Click(sender, e);   // Chama o evento de fechar a porta serial
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("LigaLed4 " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LigaLed4 " + "\r\n");
                iniciou = false;
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
            Float32 msg = new Float32();
            msg.data = 0f;
            rosSocket.Publish(publisherId, msg);
            rosSocket.Publish(publisherId2, msg);
            if (sw != null)
                sw.Close();
            Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int i = 0;
            comboBox1.Items.Clear();
            foreach (string str in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(str);
                if (str == "COM1") comboBox1.SelectedIndex = i;
                comboBox1.SelectedIndex = i;
                i++;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        void aumentarVelocidade()
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("LigaLed2 " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LigaLed2 " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        void diminuirVelocidade()
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("LigaLed1 " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LigaLed1 " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (velocidadeDesejada > minVel)
                velocidadeDesejada -= 0.1;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (velocidadeDesejada < maxVel)
                velocidadeDesejada += 0.1;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen && partidaAutomatica == false)
            {
                SerialCom.Write("LigaLed3 " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LigaLed3 " + "\r\n");
                iniciou = true;
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("LigaLed4 " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LigaLed4 " + "\r\n");
                iniciou = false;
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("LigaLed5 " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LigaLed5 " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("LigaLed6 " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LigaLed6 " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("DeslLed1  " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "DeslLed1 " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button17_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("DeslLed2  " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "DeslLed2  " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("DeslLed3  " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "DeslLed3  " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("DeslLed4  " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "DeslLed4  " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("DeslLed5  " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "DeslLed5  " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("DeslLed6  " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "DeslLed6  " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("LigaLedT " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LigaLedT " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            if (SerialCom.IsOpen)
            {
                SerialCom.Write("DesligaLedT " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "DesligaLedT " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            
            {
                if (SerialCom.IsOpen)
                {
                    SerialCom.Write("DesLoopAdc " + "\r\n");
                    listBox1.Items.Add("Enviado -> " + "DesLoopAdc  " + "\r\n");
                }
                else
                    MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {

            if (SerialCom.IsOpen)
            {
                SerialCom.Write("LeAdc " + "\r\n");
                listBox1.Items.Add("Enviado -> " + "LeAdc " + "\r\n");
            }
            else
                MessageBox.Show("A porta não está aberta, clique no botão Abrir Porta !");
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSpeed_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void txtCoef_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            if (groupBox3.Enabled)
            {
                // Desabilitar o evento groupBox3_Enter
                groupBox3.Enter -= groupBox3_Enter;
                groupBox3.Enabled = false;
                partidaAutomatica = true;
                velocidadeDesejada = 0.0;
            }
            else
            {
                // Habilitar o evento groupBox3_Enter
                groupBox3.Enter += groupBox3_Enter;
                groupBox3.Enabled = true;
                partidaAutomatica = false;
                velocidadeDesejada = 1.0;
            }

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button12_Click_1(object sender, EventArgs e)
        {
            OutroForm = new frmFormulario();
            OutroForm.ShowDialog();
        }
    }
}