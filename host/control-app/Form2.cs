using System;
using System.Windows.Forms;

namespace Control_App
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendLog(message)));
                return;
            }
            richTextBox1.AppendText(message + Environment.NewLine);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }
    }
}