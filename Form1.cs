using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Security.Permissions;

namespace PVHLF0002
{
    public partial class frm_PanelMensajes : Form
    {
        System.Net.Sockets.TcpClient clientSocket; 
        Thread tEscuchar;
        Thread tConectar;
        public frm_PanelMensajes()
        {
            InitializeComponent();
            tEscuchar = new Thread(new ThreadStart(mEscuchar));
            tConectar = new Thread(new ThreadStart(mConectar));
        }      

        private void frm_PanelMensajes_Load(object sender, EventArgs e)
        {   
            
            richTextBox1.SelectionAlignment = HorizontalAlignment.Center;
            CDI_P_CentrarLayout();
            CheckForIllegalCrossThreadCalls = false;
            try
            {
         
                mConectar();
            }
            catch (SocketException se)
            {
                tsslStatus.Text = "Servidor de mensajes desconectado";
                mDesconectar();
            }
        }
        public void msg(string mesg)
        {
            lblMensaje.Text = mesg;
            richTextBox1.Text=mesg;
            
            //.Text + Environment.NewLine + " >> " + mesg;
            CDI_P_CentrarTexto(ref lblMensaje);
        }
        public void msgInicial(string mesg)
        {
            lblMensaje.Text = mesg;
            richTextBox1.Text=mesg;
            CDI_P_CentrarTexto(ref lblMensaje);
        }
       
        private DataSet CDI_F_LeerXML()
        {
            DataSet ds = new DataSet();//Se crea un dataset
            ds.ReadXml(Application.StartupPath+"\\configuracion.xml");//Dirección del archivo xml
            return ds;
        }
        private void CDI_P_CentrarTexto(ref Label objeto)
        {   
            int heVentana=panel1.Size.Height;
            int wiVentana=panel1.Size.Width;
            int heObjeto = objeto.Size.Height;
            int wiObjeto = objeto.Size.Width;
            //Point pLocation = new Point(((wiVentana - wiObjeto) / 2),((heVentana-heObjeto)/2));
            Point pLocation = new Point(((wiVentana - wiObjeto) / 2), heObjeto);
            objeto.Location=pLocation;
            //objeto.Left = ((wiVentana - wiObjeto) / 2);
        }
        private void CDI_P_CentrarLayout()
        {
            int heVentana = panel1.Size.Height;
            int wiVentana = panel1.Size.Width;
            tableLayoutPanel1.Size = new System.Drawing.Size(wiVentana, heVentana);
        }

        private void frm_PanelMensajes_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                mKillThread();
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
        private void mConectar()
        {
            try
            {
                clientSocket = new System.Net.Sockets.TcpClient();
                msgInicial("Cliente iniciado");
                DataSet ds = new DataSet();
                ds = CDI_F_LeerXML();
                clientSocket.Connect(ds.Tables[0].Rows[0]["ip"].ToString(), int.Parse(ds.Tables[0].Rows[0]["puerto"].ToString()));
                //clientSocket.Connect("172.17.7.31", 8888);
                tsslStatus.Text = "Pantalla mensajes - Servidor Conectado ...";
                tEscuchar.IsBackground = true;
                tEscuchar.Start();                
            }catch(Exception ex){
                tsslStatus.Text = "Servidor de mensajes desconectado";
                
            }
        }
        private void mEscuchar()
        {
            while ((true))
            {
                try
                {
                    NetworkStream serverStream = clientSocket.GetStream();
                    byte[] outStream = System.Text.Encoding.ASCII.GetBytes("true$");
                    serverStream.Write(outStream, 0, outStream.Length);
                    serverStream.Flush();

                    byte[] inStream = new byte[10025];
                    serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
                    string returndata = System.Text.Encoding.ASCII.GetString(inStream);
                    msg(returndata);
                }
                catch (Exception se)
                {
                    tsslStatus.Text = "Servidor de mensajes desconectado";
                    
                }
            }
        }
        private void mDesconectar()
        {
            tEscuchar.Abort();
            clientSocket.Close();
            clientSocket = null;
            //mConectar();

        }
//        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        private void mKillThread()
        {   
            tEscuchar.Abort(tEscuchar);
        }
    }
}

