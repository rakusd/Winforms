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
using System.Threading;

namespace myGraphEditor
{
    
    public partial class Form1 : Form
    {
        public Vertice currentVertice = null;
        public const int radius = 25;
        public int currentVerticeIndex = -1;
        public Color currentColor = Color.Black;
        public List<Vertice> verticesList = new List<Vertice>();
        public List<(int,int)> edgesList = new List<(int,int)>();
        public int lastX, lastY;
        public string saveOK="Graf zapisano pomyslnie";
        public string openOK="Graf wczytano pomyslnie";
        public string openFAIL = "Blad pliku";
        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            this.DoubleBuffered = true;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButtons.Left:
                    {
                        if(currentVertice==null)//Add new vertice
                        {
                            foreach (Vertice v in verticesList)
                            {
                                if (Math.Sqrt(Math.Pow(v.X - e.X, 2) + Math.Pow(v.Y - e.Y, 2)) < Vertice.radius)
                                    return;
                            }
                            Vertice tmp = new Vertice(e.X, e.Y, currentColor);
                            verticesList.Add(tmp);
                        }
                        else//Add edge or new vertice if second vertice has not been chosen
                        {
                            for(int i=0;i<verticesList.Count;i++)
                            {
                                Vertice v = verticesList[i];
                                if (Math.Sqrt(Math.Pow(v.X - e.X, 2) + Math.Pow(v.Y - e.Y, 2)) < Vertice.radius)
                                    if (v != currentVertice)
                                    {
                                        int smaller = i < currentVerticeIndex ? i : currentVerticeIndex;
                                        int bigger = i < currentVerticeIndex ? currentVerticeIndex : i;
                                        if (edgesList.Contains((smaller, bigger)))
                                            edgesList.Remove((smaller, bigger));
                                        else
                                            edgesList.Add((smaller, bigger));
                                        pictureBox1.Refresh();
                                        return;
                                    }
                                    else
                                        return;
                            }
                            Vertice tmp = new Vertice(e.X, e.Y, currentColor);
                            verticesList.Add(tmp);
                        }
                    }
                    break;

                case MouseButtons.Right://Select/Unselect vertice
                    {
                        currentVertice = null;
                        currentVerticeIndex = -1;
                        button6.Enabled = false;
                        for(int i=0;i<verticesList.Count;i++)
                        {
                            Vertice v = verticesList[i];
                            if (Math.Sqrt(Math.Pow(v.X - e.X, 2) + Math.Pow(v.Y - e.Y, 2)) < Vertice.radius)
                            {
                                currentVertice = v;
                                currentVerticeIndex = i;
                                button6.Enabled = true;
                            }
                        }
                    }
                    break;

                case MouseButtons.Middle://Moving vertice, better implement in MouseMove
                    {
                        lastX = e.X;
                        lastY = e.Y;
                        pictureBox1.Capture = true;
                    }
                    break;
            }
            pictureBox1.Refresh();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Width == 0 || pictureBox1.Height == 0)
                return;
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);//Triggers Paint() event, no need to call Refresh()
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

            Graphics g = e.Graphics;
            if (verticesList.Count == 0)
                return;
            Font myFont = new Font("Arial", 10);
            SizeF stringSize = new SizeF();

            using (Pen p = new Pen(Color.Black, 3))//Painting edges
            {
                foreach ((int, int) edge in edgesList)
                {
                    float x1 = verticesList[edge.Item1].X;
                    float x2 = verticesList[edge.Item2].X;
                    float y1 = verticesList[edge.Item1].Y;
                    float y2 = verticesList[edge.Item2].Y;
                    double angle = Math.Atan((y2 - y1) / (x2 - x1));
                    float addX = (float)(Math.Cos(angle) * (radius / 2 - 1));
                    float addY = (float)(Math.Sin(angle) * (radius / 2 - 1));
                    if (x1 > x2)
                    {
                        addX = -addX;
                        addY = -addY;
                    }
                    g.DrawLine(p, x1 + addX, y1 + addY, x2 - addX, y2 - addY);
                }
            }
            Pen repainter = new Pen(Color.White, 3);
            for (int i=0;i<verticesList.Count;i++)//Painting vertices
            {
                Vertice v = verticesList[i];
                using (Pen p = new Pen(v.color, 3))
                {
                    g.FillEllipse(repainter.Brush, v.X - Vertice.radius / 2 - 1, v.Y - Vertice.radius / 2 - 1, Vertice.radius, Vertice.radius);
                    stringSize = e.Graphics.MeasureString((i + 1).ToString(), myFont);
                    g.DrawEllipse(p, v.X - Vertice.radius / 2 - 1, v.Y - Vertice.radius / 2 - 1, Vertice.radius, Vertice.radius);
                    g.DrawString((i+1).ToString(),myFont,p.Brush,v.X-stringSize.Width/2.0f,v.Y-stringSize.Height/2.0f);
                    
                }
            }

            if(currentVertice!=null)//Painting currently chosen vertice
            {
                using (Pen p = new Pen(Color.White, 3))
                {
                    g.DrawEllipse(p, currentVertice.X - Vertice.radius / 2 - 1, currentVertice.Y - Vertice.radius / 2 - 1, Vertice.radius, Vertice.radius);
                    p.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    p.Color = currentVertice.color;
                    g.DrawEllipse(p, currentVertice.X - Vertice.radius / 2 - 1, currentVertice.Y - Vertice.radius / 2 - 1, Vertice.radius, Vertice.radius);
                }
            }
            repainter.Dispose();
        }

        private void button5_Click(object sender, EventArgs e)//Color Selection
        {
            var temp = new ColorDialog();
            if (temp.ShowDialog() == DialogResult.OK)
            {
                currentColor = temp.Color;
                if (currentVertice != null)
                    currentVertice.color = temp.Color;
                pictureBox2.BackColor = temp.Color;
            }
        }

        private void button6_Click(object sender, EventArgs e)//Delete vertice
        {
            
            Predicate<(int, int)> match = ((int, int) x) => (x.Item1 == currentVerticeIndex || x.Item2 == currentVerticeIndex);
            edgesList.RemoveAll(match);//Deleting edges from chosen vertice

            for(int i=0; i<edgesList.Count;i++)//Adjusting edges to new vertice numeration
            {
                int val1 = edgesList[i].Item1;
                int val2 = edgesList[i].Item2;

                if (val1 > currentVerticeIndex)
                    val1--;
                if (val2 > currentVerticeIndex)
                    val2--;
                edgesList[i] = (val1, val2);
            } 
            verticesList.RemoveAt(currentVerticeIndex);//Deleting the vertice itself
            currentVerticeIndex = -1;
            currentVertice = null;
            button6.Enabled = false;

            pictureBox1.Refresh();
        }

        private void button7_Click(object sender, EventArgs e)//Erase whole graph
        {
            currentVertice = null;
            verticesList = new List<Vertice>();
            edgesList = new List<(int, int)>();

            pictureBox1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)//Polish
        {
            Size currentSize = new Size(Width, Height);
            Color backColor = pictureBox1.BackColor;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pl-PL");
            List<Vertice> currentVerticesList = verticesList;
            List<(int, int)> currentEdgesList = edgesList;
            Controls.Clear();
            InitializeComponent();
            Width = currentSize.Width;
            Height = currentSize.Height;
            verticesList = currentVerticesList;
            edgesList = currentEdgesList;
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            this.DoubleBuffered = true;
            ComponentResourceManager crm = new ComponentResourceManager(typeof(Form1));
            openOK = crm.GetString("openOK");
            saveOK = crm.GetString("saveOK");
            openFAIL = crm.GetString("openFAIL");
        }

        private void button2_Click(object sender, EventArgs e)//English
        {
            Size currentSize = new Size(Width, Height);
            Color backColor = pictureBox1.BackColor;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            List<Vertice> currentVerticesList = verticesList;
            List<(int, int)> currentEdgesList = edgesList;
            Controls.Clear();
            InitializeComponent();
            Width = currentSize.Width;
            Height = currentSize.Height;
            verticesList = currentVerticesList;
            edgesList = currentEdgesList;
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            this.DoubleBuffered = true;
            ComponentResourceManager crm = new ComponentResourceManager(typeof(Form1));
            openOK = crm.GetString("openOK");
            saveOK = crm.GetString("saveOK");
            openFAIL = crm.GetString("openFAIL");
        }

        private void button3_Click(object sender, EventArgs e)//Save data to text file *.graph
        {
            var temp = new SaveFileDialog();
            temp.Filter = "Graph files (*.graph)|*.graph";
            temp.ShowDialog();
            if(temp.FileName!="")
            {
                System.IO.FileStream stream = (System.IO.FileStream)temp.OpenFile();
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach (Vertice v in verticesList)
                        writer.WriteLine($"{v.X},{v.Y},{v.color.ToArgb()}");
                    foreach ((int, int) edge in edgesList)
                        writer.WriteLine($"{edge.Item1},{edge.Item2}");
                }
                stream.Close();
                ComponentResourceManager crm = new ComponentResourceManager();
                MessageBox.Show(saveOK);
            }
        }

        private void button4_Click(object sender, EventArgs e)//Read data from text file *.graph
        {
            var temp = new OpenFileDialog();
            temp.Filter = "Graph files (*.graph)|*.graph";
            List<Vertice> tempVerticesList = new List<Vertice>();
            List<(int, int)> tempEdgesList = new List<(int, int)>();
            if(temp.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                string line;
                string[] parts;
                bool edgesStart = false;
                System.IO.FileStream stream = (System.IO.FileStream)temp.OpenFile();
                using (StreamReader reader = new StreamReader(stream))
                {
                    while(null!=(line=reader.ReadLine()))
                    {
                        parts = line.Split(',');
                        if(parts.Length==3)//Vertice
                        {
                            if (edgesStart)
                            {
                                MessageBox.Show(openFAIL);
                                return;
                            }
                            int x, y, argb;
                            if(!int.TryParse(parts[0],out x) || !int.TryParse(parts[1], out y) || !int.TryParse(parts[2], out argb))
                            {
                                MessageBox.Show(openFAIL);
                                return;
                            }
                            Color tempColor = Color.FromArgb(argb);
                            tempVerticesList.Add(new Vertice(x, y, tempColor));
                        }
                        else if(parts.Length==2)
                        {
                            edgesStart = true;
                            int v1, v2;
                            if(!int.TryParse(parts[0],out v1) || !int.TryParse(parts[1],out v2))
                            {
                                MessageBox.Show(openFAIL);
                                return;
                            }
                            if(v1>v2)
                            {
                                int temporary = v1;
                                v1 = v2;
                                v2 = temporary;
                            }
                            if(v1<0 || v2<0 || v1>=tempVerticesList.Count || v2>=tempVerticesList.Count)
                            {
                                MessageBox.Show(openFAIL);
                                return;
                            }
                            tempEdgesList.Add((v1, v2));

                        }
                        else
                        {
                            MessageBox.Show(openFAIL);
                            return;
                        }
                    }
                }
            }
            edgesList = tempEdgesList;
            verticesList = tempVerticesList;
            pictureBox1.Refresh();
            currentVertice = null;
            MessageBox.Show(openOK);
        }



        private void Form1_KeyDown_1(object sender, KeyEventArgs e)//Deleting selected vertice using DEL key
        {
            if (button6.Enabled && e.KeyCode==Keys.Delete)
                button6_Click(sender, e);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)//moving selected vertice when holding MiddleMouselButton
        {
            if (e.Button != MouseButtons.Middle || currentVertice==null)
                return;
            if (e.Button == MouseButtons.Middle)
                pictureBox1.Capture = false;

            if (currentVertice.X < 0)
                currentVertice.X = 0;
            if (currentVertice.Y < 0)
                currentVertice.Y = 0;
            if (currentVertice.X > pictureBox1.Image.Width)
                currentVertice.X = pictureBox1.Image.Width;
            if (currentVertice.Y > pictureBox1.Image.Height)
                currentVertice.Y = pictureBox1.Image.Height;
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if(currentVertice!=null && e.Button==MouseButtons.Middle)
            {
                currentVertice.X += (e.X - lastX);
                currentVertice.Y += (e.Y - lastY);
            }
            lastX = e.X;
            lastY = e.Y;
            pictureBox1.Refresh();
        }
    }
}
