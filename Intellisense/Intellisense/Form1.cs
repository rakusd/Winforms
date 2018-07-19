using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace MyWinForms
{
    public partial class Intellisense : Form
    {
        class DescComparer<T>:IComparer<T>
        {
            public int Compare(T x, T y) => Comparer<T>.Default.Compare(y, x);
        }
        public bool reverse = false;
        public SortedSet<string> descending;
        public SortedSet<string> allWords = new SortedSet<string>();
        public string currentWord = "";
        public string temp;
        public BindingSource bindingSource=new BindingSource();
        public Intellisense()
        {
            InitializeComponent();
            allWords = new SortedSet<string>();
            foreach(FontFamily font in System.Drawing.FontFamily.Families)
            {
                toolStripComboBox1.Items.Add(font.Name);
            }
            toolStripComboBox1.SelectedItem = "Calibri";
            richTextBox1.ContextMenuStrip = contextMenuStrip1;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)//Loading dictionary
        {
            var temp = new OpenFileDialog();
            temp.Filter = "Text files (*.txt)|*.txt";
            temp.Title = "Choose dictionary for intellisense";

            if (temp.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Assign the cursor in the Stream to the Form's Cursor property.  
                var stream = temp.OpenFile();
                allWords = new SortedSet<string>();
                using (StreamReader reader = new StreamReader(stream))
                {
                    string s;
                    while ((s = reader.ReadLine()) != null)
                    {
                        allWords.Add(s);
                    }
                }
                stream.Close();
                bindingSource.DataSource = allWords;
                listBox2.DataSource = bindingSource;

            }
            else
            {
                MessageBox.Show("Failed to open file");
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)//Making font bold
        {
            var button = (ToolStripButton)sender;
            if (button.Checked == false)
            {
                button.Checked = true;
                this.richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Bold | richTextBox1.Font.Style);
                return;
            }
            button.Checked = false;
            this.richTextBox1.Font = new Font(richTextBox1.Font, richTextBox1.Font.Style ^ FontStyle.Bold);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)//making font italic
        {
            var button = (ToolStripButton)sender;
            if (button.Checked == false)
            {
                button.Checked = true;
                this.richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Italic | richTextBox1.Font.Style);
                return;
            }
            button.Checked = false;
            this.richTextBox1.Font = new Font(richTextBox1.Font, richTextBox1.Font.Style ^ FontStyle.Italic);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)//underlining font
        {
            var button = (ToolStripButton)sender;
            if (button.Checked == false)
            {
                button.Checked = true;
                this.richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Underline | richTextBox1.Font.Style);
                return;
            }
            button.Checked = false;
            this.richTextBox1.Font = new Font(richTextBox1.Font, richTextBox1.Font.Style ^ FontStyle.Underline);
        }

        private void toolStripButton4_Click(object sender, EventArgs e)//Choosing font color
        {
            var temp = new ColorDialog();
            if (temp.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.ForeColor = temp.Color;
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)//Choosing background of richTextBox
        {
            var temp = new ColorDialog();
            if (temp.ShowDialog() == DialogResult.OK)
            {
                richTextBox1.BackColor = temp.Color;
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)//Saving dictionary as *.txt file
        {
            var temp = new SaveFileDialog();
            temp.Filter = "Text files (*.txt)|*.txt";
            temp.Title = "Save dictionary";
            temp.ShowDialog();
            if (temp.FileName != "")
            {
                System.IO.FileStream stream =(System.IO.FileStream)temp.OpenFile();
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach (string s in allWords)
                        writer.WriteLine(s);
                }
                stream.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)//Switching between ascending and descending order in dictionary
        {
            if(reverse)
            {
                bindingSource.DataSource = null;
                bindingSource.DataSource = allWords;
                reverse = false;
            }
            else
            {
                descending = new SortedSet<string>(allWords, new DescComparer<string>());
                bindingSource.DataSource = null;
                bindingSource.DataSource = descending;
                reverse = true;
            }
        }

        private void listBox2_DragEnter(object sender, DragEventArgs e)//Changing effect when .txt files are being dragged into dictionary
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file).Equals(".txt", StringComparison.InvariantCultureIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void listBox2_DragDrop(object sender, DragEventArgs e)//Loading dictionary from file or multiple files using drag&drop mechanism
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var file in files)
            {
                if (!System.IO.Path.GetExtension(file).Equals(".txt", StringComparison.InvariantCultureIgnoreCase))
                {
                    MessageBox.Show("At least one of files isn't a text file");
                    return;
                }
            }
            
            foreach(var file in files)
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    string s;
                    while ((s = reader.ReadLine()) != null)
                    {
                        if (reverse)
                            descending.Add(s);
                        allWords.Add(s);
                    }
                    reader.Close();
                }
            }
            if(!reverse)
            {
                bindingSource.DataSource = null;
                bindingSource.DataSource = allWords;
            }
            else
            {
                bindingSource.DataSource = null;
                bindingSource.DataSource = descending;
            }
            
            listBox2.DataSource = null;
            listBox2.DataSource = bindingSource;

        }

        private void richTextBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //TAB - finish currently typed word using dictionary
            if (e.KeyCode == Keys.Tab && listBox1.Visible == true)
            {
                int index = richTextBox1.SelectionStart;
                richTextBox1.Text = richTextBox1.Text.Insert(index, ((string)listBox1.SelectedItem).Remove(0, currentWord.Length));
                richTextBox1.SelectionStart = index + ((string)listBox1.SelectedItem).Length - currentWord.Length;
            }
            //Arrow key up - switching highlighted word
            if (e.KeyCode == Keys.Up && listBox1.Visible == true)
            {
                if (listBox1.SelectedIndex > 0)
                    listBox1.SelectedIndex--;
                return;
            }
            //Arrow key down - switching highlighted word
            if (e.KeyCode == Keys.Down && listBox1.Visible == true)
            {
                if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
                    listBox1.SelectedIndex++;
                return;
            }
            //Random keys that are not letters - hide Intellisense
            if (((int)e.KeyCode < 65 || (int)e.KeyCode > 90) && e.Shift == false)
            {
                listBox1.Visible = false;
                currentWord = "";
                return;
            }
            listBox1.Visible = true;
            //Shift+letter combination or just shfift
            if (e.Shift == true)
            {
                temp = e.KeyCode.ToString().ToLower();
                if (temp.Length == 1)
                    currentWord += temp;
                else
                {
                    listBox1.Visible = false;
                    return;
                }
            }
            else//Letters
            {
                currentWord += e.KeyData.ToString().ToLower();
            }

            int i = richTextBox1.SelectionStart;//Setting intellisense position
            Point p = richTextBox1.GetPositionFromCharIndex(i);
            p.Y += (int)(1.5 * listBox1.ItemHeight);
            p.X += richTextBox1.Left;
            p.Y += richTextBox1.Top;
            listBox1.Location = p;

            bool hasStarted = false;

            listBox1.Items.Clear();
            if(!reverse)
            {
                foreach (string s in allWords)
                {
                    if (s.StartsWith(currentWord, StringComparison.CurrentCultureIgnoreCase))
                    {
                        listBox1.Items.Add(s);
                        hasStarted = true;
                    }
                    else
                    {
                        if (hasStarted == true)
                            break;
                    }
                }
            }
            else
            {
                foreach (string s in descending)
                {
                    if (s.StartsWith(currentWord, StringComparison.CurrentCultureIgnoreCase))
                    {
                        listBox1.Items.Add(s);
                        hasStarted = true;
                    }
                    else
                    {
                        if (hasStarted == true)
                            break;
                    }
                }
            }
            
            if (!hasStarted)
            {
                listBox1.Visible = false;
                currentWord = "";
            }
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }


        private void listBox2_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)//Deleting selected word from dictionary using delete key
        {
            if(e.KeyCode==Keys.Delete)
            {
                foreach(string s in listBox2.SelectedItems)
                {
                    allWords.Remove(s);
                    if (reverse)
                        descending.Remove(s);

                }
                if(!reverse)
                {
                    bindingSource.DataSource = null;
                    bindingSource.DataSource = allWords;
                }
                else
                {
                    bindingSource.DataSource = null;
                    bindingSource.DataSource = descending;
                }
            }
        }

        private void richTextBox1_MouseUp(object sender, MouseEventArgs e)//adding or deleting word from Dictionary using MRB
        {
            if (richTextBox1.Text.Length < 1)
            {
                contextMenuStrip1.Visible = false;
                return;
            }
            int i, start, end;
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                
                i = richTextBox1.GetCharIndexFromPosition(e.Location);
                start = end = i;
                if (!char.IsLetter(richTextBox1.Text[i]))//If not char
                {
                    contextMenuStrip1.Visible = false;
                    return;
                }
                while (start>0 && char.IsLetter(richTextBox1.Text[start - 1]) )
                    start--;
                while ( end < richTextBox1.Text.Length - 1 && char.IsLetter(richTextBox1.Text[end + 1]) )
                    end++;
                string temp = richTextBox1.Text.Substring(start, end - start+1);
                if(allWords.Contains(temp))//Deleting words from dictionary
                {
                    contextMenuStrip1.Items[0].Text = "Delete "+temp;
                    contextMenuStrip1.Visible = true;  
                }
                else//Adding words to dictionary
                {
                    contextMenuStrip1.Items[0].Text = "Add "+temp;
                    contextMenuStrip1.Visible = true;
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)//Adding new word to dictionary
        {
            Form2 formAddWord = new Form2();
            if (formAddWord.ShowDialog(this) == DialogResult.OK)
            {
                string s = formAddWord.returnString;
                allWords.Add(s);
                if (reverse)
                    descending.Add(s);
                
            }
            if(!reverse)
            {
                bindingSource.DataSource = null;
                bindingSource.DataSource = allWords;
            }
            else
            {
                bindingSource.DataSource = null;
                bindingSource.DataSource = descending;
            }
            
            listBox2.DataSource = null;
            listBox2.DataSource = bindingSource;
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)//FontSelection
        {
            string font =(string)toolStripComboBox1.SelectedItem;
            richTextBox1.Font = new Font(font, 12, richTextBox1.Font.Style);
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)//Showing contextmenustrip that can add or delete word from dictionary
        {
            string temp;
            if(contextMenuStrip1.Items[0].Text.StartsWith("Add"))
            {
                temp= contextMenuStrip1.Items[0].Text.Substring(4, contextMenuStrip1.Items[0].Text.Length - 4);

                Form2 formAddWord = new Form2();
                formAddWord.returnString = temp;
                if (formAddWord.ShowDialog(this) == DialogResult.OK)
                {
                    string s = formAddWord.returnString;
                    allWords.Add(s);
                    if (reverse)
                        descending.Add(s);

                }
                if (!reverse)
                {
                    bindingSource.DataSource = null;
                    bindingSource.DataSource = allWords;
                }
                else
                {
                    bindingSource.DataSource = null;
                    bindingSource.DataSource = descending;
                }
                listBox2.DataSource = null;
                listBox2.DataSource = bindingSource;
            }
            else
            {
                temp=contextMenuStrip1.Items[0].Text.Substring(7,contextMenuStrip1.Items[0].Text.Length-7);
                allWords.Remove(temp);
                if(!reverse)
                {
                    bindingSource.DataSource = null;
                    bindingSource.DataSource = allWords;
                }
                else
                {
                    descending.Remove(temp);
                    bindingSource.DataSource = null;
                    bindingSource.DataSource = descending;
                }
            }
        }
    }
}
