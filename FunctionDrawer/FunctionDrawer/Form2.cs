using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Surprise
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.ActiveControl = textBox1;
            Function = null;
        }
        public string Function { get; set; }
        private void button1_Click(object sender, EventArgs e)//OK button, tries to parse function and add it to list of functions
        {
            Form1 owner = this.Owner as Form1;
            if (!owner.TryAddingFunctionToList(textBox1.Text))
            {
                MessageBox.Show("Function cannot be parsed");
                return;
            }
            else
                this.Close();
        }

        private void button2_Click(object sender, EventArgs e)//Cancel button, closes form
        {
            this.Close();
        }


        private void Form2_KeyDown(object sender, KeyEventArgs e)//Enter-OK button, ESC - Close button
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    button1_Click(sender, null);
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }
        }
    }
}
