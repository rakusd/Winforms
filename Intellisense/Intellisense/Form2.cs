using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MyWinForms
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public string returnString
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            string temp = textBox1.Text;
            Regex checker = new Regex(@"^[a-zA-Z]+$");
            if(checker.IsMatch(temp))
                this.DialogResult = DialogResult.OK;
            else
            {
                errorProvider1.SetError(textBox1, "Only letters are allowed");
                MessageBox.Show("Validation error!", "", MessageBoxButtons.OK);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
