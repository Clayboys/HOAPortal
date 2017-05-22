using AForge.Video;
using MjpegProcessor;
using System;
using System.Drawing;
using System.Windows.Forms;
using Gmap.net;
using System.Threading;
using System.IO;
using XperiCode.JpegMetadata;
using System.Drawing.Imaging;
using System.Text;

namespace WindowsFormsApp1{
    public partial class Form1 : Form{

        //private System.IO.Ports.SerialPort port;
        private NmeaParser.SerialPortDevice device;
        private static MJPEGStream stream1 = new MJPEGStream() { Source = "" };
        private static MJPEGStream stream2 = new MJPEGStream() { Source = "" };
        private NmeaParser.Nmea.Gps.Gprmc gprmc;
        private delegate void StringArgReturningVoidDelegate();
        System.Collections.Generic.List<object> clients = new System.Collections.Generic.List<object>();
        private string[] ipAddresses = { "169.254.17.23", "169.254.180.49", "169.254.131.67", "169.254.19.136" };
        bool isCapturing = false;
        System.Threading.Timer timer;

        public Form1(){
            InitializeComponent();
            device = new NmeaParser.SerialPortDevice(new System.IO.Ports.SerialPort("COM3", 4800));
            device.MessageReceived += Device_MessageReceived; ;
            device.OpenAsync();
            stream1.NewFrame += Stream1_NewFrame;
            stream2.NewFrame += Stream2_NewFrame;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 1;
            stream1.Start();
            stream2.Start();
            UpdateStreamSources();
            button1.BackColor = Color.Green;
            }
        private void UpdateStreamSources(){
            stream1.Stop();
            stream2.Stop();
            stream1.Source = "http://" + this.ipAddresses[comboBox1.SelectedIndex] + ":8080/?action=stream";
            stream2.Source = "http://" + this.ipAddresses[comboBox2.SelectedIndex] + ":8080/?action=stream";
            stream1.Start();
            stream2.Start();
            }
        private void Stream2_NewFrame(object sender, NewFrameEventArgs e){
            try{
                pictureBox2.Image = new Bitmap(e.Frame);
                }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
                }
            }
        private void Stream1_NewFrame(object sender, NewFrameEventArgs e){
            try{
                pictureBox1.Image = new Bitmap(e.Frame);
                }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
                }
            }
        private void Device_MessageReceived(object sender, NmeaParser.NmeaMessageReceivedEventArgs e){
            if (e.Message is NmeaParser.Nmea.Gps.Gprmc){
                gprmc = e.Message as NmeaParser.Nmea.Gps.Gprmc;
                //try {
                //    timer.Change((int)(1000.0/(gprmc.Speed/0.514444)), (int)(1000.0 / (gprmc.Speed / 0.514444)));
                //    }
                //catch (System.Exception ex) { Console.WriteLine(ex.Message); }
                try
                {
                    new Thread(new ThreadStart(ThreadSafeUpdateLabel1)).Start();
                    }
                catch (System.Exception ex){Console.WriteLine(ex.Message);}
                try{
                    new Thread(new ThreadStart(ThreadSafeUpdateGmap1)).Start();
                    }
                catch (System.Exception ex){Console.WriteLine(ex.Message);}
                }
            }
        private void ThreadSafeUpdateGmap1() {
            try{
                if (this.gMapControl1.InvokeRequired){
                    StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(ThreadSafeUpdateGmap1);
                    this.Invoke(d);
                    }
                else{
                    gMapControl1.Position = new GMap.NET.PointLatLng(gprmc.Latitude, gprmc.Longitude);
                    }
                }
            catch (System.Exception ex) { Console.WriteLine(ex.Message); }
            }
        private void ThreadSafeUpdateLabel1(){
            try{
                if (this.label1.InvokeRequired){
                    StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(ThreadSafeUpdateLabel1);
                    this.Invoke(d);
                    }
                else{
                    this.label1.Text = $"{gprmc.Latitude},{gprmc.Longitude},{gprmc.Speed / 0.514444} m/s"; this.gMapControl1.Position = new GMap.NET.PointLatLng(gprmc.Latitude, gprmc.Longitude);
                    }
                }
            catch (System.Exception ex) { Console.WriteLine(ex.Message); }
            }
        private void GMapControl1_Load(object sender, EventArgs e){
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            }
        private void ThreadSafeGetImage(object args){
            Thread tempThread = new Thread(new ThreadStart(ThreadSafeGetImage1));
            tempThread.Start();
            }
        private void ThreadSafeGetImage1(){
            GetImage(new { });
            }
        private void GetImage(object args) {
            try{
                if (this.gMapControl1.InvokeRequired){
                    StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(ThreadSafeGetImage1);
                    this.Invoke(d);
                    }
                else{
                    try{
                        //Console.Write("trying, ");
                        using (System.Net.WebClient client = new System.Net.WebClient()){
                        ///*
                            client.DownloadFile("http://" + ipAddresses[comboBox1.SelectedIndex] + ":8080/?action=snapshot", @"C:\Users\Paul\Desktop\pics\(Cam1)(" + Math.Floor((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) + ")(" + gprmc.Latitude.ToString() + ", " + gprmc.Longitude.ToString() + ").jpg");
                            client.DownloadFile("http://" + ipAddresses[comboBox2.SelectedIndex] + ":8080/?action=snapshot", @"C:\Users\Paul\Desktop\pics\(Cam2)(" + Math.Floor((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) + ")(" + gprmc.Latitude.ToString() + ", " + gprmc.Longitude.ToString() + ").jpg");
                            //WriteExif();
                            if ((int)(1000.0 / (gprmc.Speed / 0.514444)) > 2000) { Thread.Sleep(2000); }
                            else { Thread.Sleep((int)(1000.0 / (gprmc.Speed / 0.514444))); }
                            }
                        }
                    catch {
                        try{
                            using (System.Net.WebClient client = new System.Net.WebClient()){
                                //*/
                                client.DownloadFile("http://" + ipAddresses[comboBox1.SelectedIndex] + ":8080/?action=snapshot", @"C:\Users\Paul\Desktop\pics\(Cam1)(" + Math.Floor((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) + ")(no gps).jpg");
                                client.DownloadFile("http://" + ipAddresses[comboBox2.SelectedIndex] + ":8080/?action=snapshot", @"C:\Users\Paul\Desktop\pics\(Cam2)(" + Math.Floor((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds) + ")(no gps).jpg");
                                //Thread.Sleep(333);
                                }
                            }
                        catch (System.Exception ex) { throw ex; }
                        }//catch (System.Exception ex) { throw ex; }
                    }
                }
            catch (System.Exception ex) {
                isCapturing = false;
                timer.Change(-1, -1);
                timer.Dispose();
                button1.Text = "start capturing";
                button1.BackColor = Color.Green;
                MessageBox.Show("No camera feed, bruh", "bruh", MessageBoxButtons.OK); 
                }
            }
        private void Button1_Click(object sender, EventArgs e){
            if (isCapturing){ 
                isCapturing = false;
                timer.Change(-1, -1);
                timer.Dispose();
                button1.Text = "start capturing";
                button1.BackColor = Color.Green;
                }
            else{
                try{
                    object obj = new { gprmc.Latitude };
                    }
                catch { MessageBox.Show("No gps, bruh", "bruh", MessageBoxButtons.OK); }
                try{
                    isCapturing = true;
                    timer = new System.Threading.Timer(GetImage, null, TimeSpan.FromSeconds(0.333), TimeSpan.FromSeconds(0.333));
                    //Thread.Sleep(4500);
                    button1.Text = "stop capturing";
                    button1.BackColor = Color.Red;
                    }
                catch (System.Exception ex) { throw ex; }
                }
            }
        /*public void WriteExif(){
            var image1 = Image.FromFile("C:\\Users\\Paul\\Desktop\\pics1\\Cam3image" + imageCount1.ToString() + ".jpg");
            PropertyItem propitem1 = image1.PropertyItems[0];
            propitem1.Id= 0x9286;
            propitem1.Type = 2;
            propitem1.Value = Encoding.ASCII.GetBytes(gprmc.Latitude.ToString() + ", " + gprmc.Longitude.ToString());
            propitem1.Len = propitem1.Value.Length;
            image1.SetPropertyItem(propitem1);
            System.IO.File.Delete("C:\\Users\\Paul\\Desktop\\pics\\Cam1image" + imageCount1.ToString() + ".jpg");
            image1.Save("C:\\Users\\Paul\\Desktop\\pics\\Cam1Image"+imageCount2.ToString()+".jpg");

            image1 = Image.FromFile("C:\\Users\\Paul\\Desktop\\pics\\Cam4image" + imageCount2.ToString() + ".jpg");
            propitem1 = image1.PropertyItems[0];
            propitem1.Id = 0x9286;
            propitem1.Type = 2;
            propitem1.Value = Encoding.ASCII.GetBytes(gprmc.Latitude.ToString() + ", " + gprmc.Longitude.ToString());
            propitem1.Len = propitem1.Value.Length;
            image1.SetPropertyItem(propitem1);
            System.IO.File.Delete("C:\\Users\\Paul\\Desktop\\pics\\Cam2image" + imageCount2.ToString() + ".jpg");
            image1.Save("C:\\Users\\Paul\\Desktop\\pics\\Cam2image" + imageCount2.ToString() + ".jpg");
                
            //Image image2 = Image.FromFile("C:\\Users\\Paul\\Desktop\\pics\\Cam2image" + imageCount2.ToString() + ".jpg");
            //PropertyItem propitem2 = image2.GetPropertyItem(37510);
            //propitem2.Value = Encoding.ASCII.GetBytes(gprmc.Latitude.ToString() + ", " + gprmc.Longitude.ToString());
            //image2.SetPropertyItem(propitem2);
            }*/
        protected override void OnFormClosing(FormClosingEventArgs e){
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.WindowsShutDown) return;
            Environment.Exit(0);
            }
        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e){try{UpdateStreamSources();}catch { }}
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e){try{UpdateStreamSources();}catch { }}
        private void Form1_Load(object sender, EventArgs e){}
        private void PictureBox1_Click(object sender, EventArgs e){ }
        private void PictureBox2_Click(object sender, EventArgs e) { }
        private void Label1_Click(object sender, EventArgs e) { }
        private void Label3_Click(object sender, EventArgs e){}
        }
    }
