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
using System.Net;
using mshtml;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace Read_feed
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Stopwatch sw = new Stopwatch();
        WebClient client = new WebClient();
        int countdowmimg = 0,futt=0;
        private ManualResetEvent mre = new ManualResetEvent(true);

        private void Form1_Load(object sender, EventArgs e)
        {
            //ตั้งค่าแบ็คกราวรัน
            bgwork2.WorkerReportsProgress = true;
            bgwork2.DoWork += new DoWorkEventHandler(bgwork2_DoWork);
            bgwork2.ProgressChanged += new ProgressChangedEventHandler(bgwork2_Progresschanged);
            bgwork2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwork2_RunWorkerCompleted);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            retrievHTML();
            futt = 0;
            countdowmimg = 0;
        }
        private void reimg(String htmlContent)
        {
            int countimgwithout=0;
            int countimg = 0;
            string imgsouce;
            string urlfilter = text_urlfilter.Text;
            string regg = "*";
            string allurl = String.Concat(urlfilter, regg); //ต่อข้อความ
            IHTMLDocument2 htmlDocment = (IHTMLDocument2)new mshtml.HTMLDocument();
            htmlDocment.write(htmlContent);
            listBox1.Items.Clear();
            IHTMLElementCollection imgElements = htmlDocment.images;
            foreach (IHTMLImgElement img in imgElements) // วนดึงรูปภาพจนครบ
            {
                imgsouce = img.src;
                Match match = Regex.Match(imgsouce, allurl, RegexOptions.IgnoreCase);
                countimgwithout = countimgwithout + 1;
                if(match.Success)
                {
                    listBox1.Items.Add(imgsouce);
                    countimg = countimg + 1;
                }
                
            }
            label5.Text = Convert.ToString(countimgwithout);
            label6.Text = Convert.ToString(countimg);
        }
        private void retrievHTML()
        {
            Stream data = client.OpenRead(new Uri(texturl.Text));
            StreamReader reader = new StreamReader(data);
            String htmlContent = reader.ReadToEnd();
            reimg(htmlContent);
            
            data.Close();
            reader.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!bgwork2.IsBusy)
                bgwork2.RunWorkerAsync();
            else
                MessageBox.Show("Can't run the worker twice!");
        }
        
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //label12.Text = string.Format("{0} ", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            /*label12.BeginInvoke(new Action(() =>
            {
                label12.Text = string.Format("{0} ", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            }
                ));*/
            bgwork2.ReportProgress(e.ProgressPercentage);
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            mre.Set();
        }

        private void bgwork2_DoWork(object sender, DoWorkEventArgs e)
        {
            string pathString = @"E:\mangas\" + textBox2.Text;
            System.IO.Directory.CreateDirectory(pathString);
            string subfolder = System.IO.Path.Combine(pathString, textBox1.Text);
            System.IO.Directory.CreateDirectory(subfolder);
            subfolder = subfolder + "\\";
            if (futt == 0)
            { 
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    String urlimg = listBox1.Items[i].ToString();
                    String urlsource = subfolder + Convert.ToString(i) + ".jpg";
                    client.DownloadFileAsync(new Uri(urlimg), urlsource);
                    //bgwork2.ReportProgress(0, null);
                    sw.Start();
                    futt = 1;
                    countdowmimg += 1;
                    while (client.IsBusy)
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                }
            }
         }

        private void bgwork2_Progresschanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.BeginInvoke(new Action(() =>
                {
                    progressBar1.Value = e.ProgressPercentage;
                }
                ));
            label12.BeginInvoke(new Action(() =>
            {
                label12.Text = e.ProgressPercentage.ToString();
                label10.Text = Convert.ToString(countdowmimg);
            }
                ));
        }
        private void bgwork2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("เสร็จแล้ว");
        }

        
    }
}
