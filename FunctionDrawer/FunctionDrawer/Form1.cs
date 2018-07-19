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
    public partial class Form1 : Form
    {
        public DataTable[] Tables;//table of x values and function values
        public Random Rnd = new Random();
        public List<(string, Color)> Functions;
        private float Fun1(double x) => (float)(Math.Pow(x, 3) - Math.Pow(x, 2) - x - 1);
        private float start=0, stop=0;
        private float MyResize(float val,float oldmax,float oldmin,float newmin,float newmax)
        {
            return newmin + (val - oldmin) * ((newmax-newmin) / (oldmax-oldmin));
        }
        public float Start
        {
            get => start;
            set
            {
                start = value;
            }
        }//x min value
        public bool TryAddingFunctionToList(string function)//tries to add function written as string to list of functions
        {
            if (function.Length < 0 || String.IsNullOrWhiteSpace(function) || String.IsNullOrEmpty(function))
                return false;
            try
            {
                for (int i = 0; i < Tables.Length; i++)
                {
                    DataColumn column = new DataColumn();
                    column.DataType = typeof(float);
                    column.Expression = function;
                    column.ColumnName = $"Function{Functions.Count}";
                    Tables[i].Columns.Add(column);
                }
            }
            catch(Exception e)//user-typed function is incorrect
            {
                return false;
            }

            Functions.Add((function, Color.FromArgb(Rnd.Next(0,255),Rnd.Next(0,255),Rnd.Next(255))));
            checkedListBox1.Items.Add(function);
            return true;
        }
        public float Stop
        {
            get => stop;
            set
            {
                stop = value;
            }
        }//x max value

        private void CreateAndFillDataTables(float start,float stop)//fills data tables with points and values of functions in that points
        {
            Tables = new DataTable[7];//for Tables with 3,6,12,24,48,96,192 number of points
            int count = 3;
            for(int i=0;i<Tables.Length;i++)
            {
                Tables[i] = new DataTable();
                Tables[i].Columns.Add("x", typeof(float));
                for (int j=0;j<count;j++)
                {
                    DataRow dr = Tables[i].NewRow();
                    dr["x"] = start+(stop-start)*((float)j/(float)(count-1));
                    Tables[i].Rows.Add(dr);
                }
                count *= 2;
            }

            for(int i=0;i<Tables.Length;i++)
            {
                for(int j=0;j<Functions.Count;j++)
                {
                    DataColumn column = new DataColumn();
                    column.DataType = typeof(float);
                    column.Expression = Functions[j].Item1;//Throws exception if expression cannot be evalutaed, empty expression still goes and needs to be coded to not pass
                    column.ColumnName = $"Function{j}";
                    Tables[i].Columns.Add(column);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)//Changes domain of function
        {
            float a, b;
            if(!float.TryParse(textBox1.Text,out a) || !float.TryParse(textBox2.Text,out b))
            {
                MessageBox.Show("Only numerical values are allowed");
                return;
            }
            Start = a;Stop = b;
            CreateAndFillDataTables(Start, Stop);
        }
        private void PaintFunction(int tableIndex,bool paintBlackLine)//Paints selected functions using DataTable with index==tableIndex
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bmp);
            float width = pictureBox1.Width;
            float middleX = pictureBox1.Width / 2;
            float height = pictureBox1.Height;
            float middleY = pictureBox1.Height / 2;
            float myWidth = Start - Stop;
            double integral = 0;
            int pointsCount = Tables[tableIndex].Rows.Count;
            float move = width / (pointsCount-1);


            float min = float.MaxValue;
            float max = float.MinValue;
            foreach(int functionIndex in checkedListBox1.CheckedIndices)
            {
                object minObj = Tables[tableIndex].Compute($"Min(Function{functionIndex})", "");
                object maxObj = Tables[tableIndex].Compute($"Max(Function{functionIndex})", "");

                float tempmin = float.Parse(minObj.ToString());
                float tempmax = float.Parse(maxObj.ToString());
                if (min > tempmin)
                    min = tempmin;
                if (max < tempmax)
                    max = tempmax;
            }

            label3.Text = $"Max Y: {max}";
            label4.Text = $"Min Y: {min}";

            foreach(int functionIndex in checkedListBox1.CheckedIndices)
            {
                using (Pen p = new Pen(Functions[functionIndex].Item2, 1))
                {
                    using (Brush tp = new SolidBrush(Color.FromArgb(128, Functions[functionIndex].Item2)))
                    {
                        DataTable dt = Tables[tableIndex];
                        DataColumn temp = dt.Columns[$"Function{functionIndex}"];
                        for (int i = 0; i < pointsCount - 1; i++)
                        {
                            object prevObj, nextObj;
                            prevObj = dt.Rows[i][temp]; nextObj = dt.Rows[i + 1][temp];
                            float prev = float.Parse(prevObj.ToString());
                            float next = float.Parse(nextObj.ToString());
                            try
                            {
                                g.DrawLine(p, i * move, MyResize(prev, max, min, height, 0), (i + 1) * move, MyResize(next, max, min, height, 0));
                            }
                            catch(Exception e)
                            {
                                MessageBox.Show("Hello");
                                if(double.IsInfinity(prev) && double.IsInfinity(next))
                                {
                                    if (double.IsNegativeInfinity(prev) && double.IsNegativeInfinity(next))
                                        g.DrawLine(p, i * move, height, (i + 1) * move, height);
                                    if(double.IsNegativeInfinity(prev) && double.IsPositiveInfinity(next))
                                        g.DrawLine(p, i * move, height, (i + 1) * move, 0);
                                    if (double.IsPositiveInfinity(prev) && double.IsNegativeInfinity(next))
                                        g.DrawLine(p, i * move, 0, (i + 1) * move, height);
                                    if (double.IsNegativeInfinity(prev) && double.IsPositiveInfinity(next))
                                        g.DrawLine(p, i * move, 0, (i + 1) * move, 0);
                                }
                                else if(double.IsInfinity(prev))
                                {
                                    if(double.IsNegativeInfinity(prev))
                                        g.DrawLine(p, i * move, height, (i + 1) * move, MyResize(next, max, min, height, 0));
                                    if (double.IsPositiveInfinity(prev))
                                        g.DrawLine(p, i * move, 0, (i + 1) * move, MyResize(next, max, min, height, 0));
                                }
                                else if(double.IsInfinity(next))
                                {
                                    if (double.IsNegativeInfinity(next))
                                        g.DrawLine(p, i * move, MyResize(prev, max, min, height, 0), (i + 1) * move, height);
                                    if (double.IsPositiveInfinity(next))
                                        g.DrawLine(p, i * move, MyResize(prev, max, min, height, 0), (i + 1) * move, 0);
                                }
                                else
                                {
                                    MessageBox.Show("Unhandled exception has been thrown");
                                }
                            }
                            
                            if(paintBlackLine)
                            {
                                float help = (float)0.5 * (MyResize(prev, max, min, height, 0) + MyResize(next, max, min, height, 0));
                                if (help - pictureBox1.Height / 2 > 0)
                                {
                                    g.FillRectangle(tp, new RectangleF(i * move, pictureBox1.Height / 2, move, help - pictureBox1.Height / 2));
                                }
                                   
                                else
                                {
                                    g.FillRectangle(tp, new RectangleF(i * move, help, move, -(help - pictureBox1.Height / 2)));
                                }
                                integral += Math.Abs(((double)next + (double)prev) *0.5* ((double)Stop - (double)Start) / (double)pointsCount);

                            }
                            
                        }
                    }
                    
                }
            }

            if(paintBlackLine)
                using (Pen p = new Pen(Color.Black, 3))
                {
                    g.DrawLine(p, 0, pictureBox1.Height / 2, pictureBox1.Width, pictureBox1.Height / 2);
                    label6.Text = $"Integral: {integral}";
                }
            else
            {
                label6.Text = "Integral: ";
            }
            pictureBox1.Image = bmp;
        }

        private void button1_Click(object sender, EventArgs e)//Paints functions
        {
            pictureBox1.Refresh();
            PaintFunction(6,false);
            progressBar1.Value = 0;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)//Paints functions asynchronously with different number of points
        {
            int stepNum = 1;
            while(stepNum<8)
            {
                if (backgroundWorker1.CancellationPending)
                    return;
                
                PaintFunction(stepNum-1,true);
                backgroundWorker1.ReportProgress((int)(100 *(stepNum/7.0)) );//7.0 is number of steps
                System.Threading.Thread.Sleep(1000);
                stepNum++;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)//Updates ProgressBar
        {
            int percent=e.ProgressPercentage;
            progressBar1.Value = percent;
        }

        private void button3_Click(object sender, EventArgs e)//Calls function that draws functions asynchronously
        {
            if(!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        private void button4_Click(object sender, EventArgs e)//Stops drawing functions asynchronously
        {
            backgroundWorker1.CancelAsync();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Owner = this;
            form2.ShowDialog();

        }

        public Form1()
        {
            InitializeComponent();
            checkedListBox1.SetItemChecked(0, true);
            Start = -1; Stop = 3;
            Functions = new List<(string, Color)>();
            Functions.Add(("x*x*x-x*x-x-1", Color.Red));
            CreateAndFillDataTables(Start, Stop);
            CheckForIllegalCrossThreadCalls = false;
        }
    }
}
