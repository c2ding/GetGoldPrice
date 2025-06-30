using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewGoldCost
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private int number = 0;

        public int iCurrentCost = 0;
        private static readonly HttpClient httpClient = new HttpClient();

        async Task getGoldCost()
        {
            try
            {
                var internationalPrice = await GetInternationalGoldPrice();

                number = Convert.ToInt32(internationalPrice);
                UpdateTrayIcon();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取金价失败: {ex.Message}");
            }

        }



        // 获取国际金价
        static async Task<double> GetInternationalGoldPrice()
        {
            try
            {
                // 实际应用中替换为真实API地址
                var response = await httpClient.GetStringAsync("https://sapi.k780.com/?app=finance.gold_price&goldid=1051&appkey=76395&sign=1105eef9b0e08845f344bda2ebbb1ce3&format=json");

                // 使用Newtonsoft.Json解析动态JSON数据
                dynamic priceData = JsonConvert.DeserializeObject(response);
                //dynamic priceData = JsonConvert.DeserializeObject(@"{""success"":""1"",""result"":{""dtQuery"":""1051"",""dtCount"":""1"",""dtList"":{""1051"":{""goldid"":""1051"",""variety"":""AuT+D"",""varietynm"":""黄金T+D"",""last_price"":""779.68"",""buy_price"":""779.65"",""sell_price"":""779.7"",""open_price"":""785.02"",""yesy_price"":""768.02"",""high_price"":""787.05"",""low_price"":""778.34"",""change_price"":""11.66"",""change_margin"":""1.52%"",""uptime"":""2025-06-03 14:57:31""}}}}");
                string s = priceData["result"]["dtList"]["1051"]["last_price"].ToString();



                //此处对数字进行显示
                // MessageBox.Show(s.ToString());

                // 根据实际API返回格式调整路径
                return Convert.ToDouble(s); // 示例路径，实际可能为priceData.result.price等
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析金价数据出错: {ex.Message}");
                throw;
            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            timer1.Start();
            getGoldCost();
            this.WindowState = FormWindowState.Minimized;
            Visible= false;
            InitializeTrayIcon();
        }

        private void button1_Click(object sender, EventArgs e)
        {


            getGoldCost();

        }





        private void InitializeTrayIcon()
        {
            // 创建托盘菜单
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("在线金价", null, OnReset);
            trayMenu.Items.Add("退出", null, OnExit);

            // 创建托盘图标
            trayIcon = new NotifyIcon
            {
                Text = "国际金价["+ DateTime.Now.ToString("g") +"]: " + number.ToString("D3") +"元/克",
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            // 更新托盘图标显示
            UpdateTrayIcon();
        }

        private void OnReset(object sender, EventArgs e)
        {


            string websiteUrl = "https://www.huilvbiao.com/gold";
            try
            {
                // 使用 Process 类启动默认浏览器访问网站
                Process.Start(websiteUrl);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开网站时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }

        private void OnExit(object sender, EventArgs e)
        {
            // 隐藏托盘图标并退出应用程序
            trayIcon.Visible = false;
            Application.Exit();
        }

        private void UpdateTrayIcon()
        {
            // 更新托盘图标文本
            trayIcon.Text = "国际金价[" + DateTime.Now.ToString("g") + "]: " + number.ToString("D3") + "元/克";




            // 创建一个新的图标，显示当前三位数字
            using (Bitmap bitmap = new Bitmap(32, 16))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // 绘制背景
                g.Clear(Color.Transparent);

                // 设置字体和画笔
                Font font = new Font("Arial", 16, FontStyle.Bold);
                Brush brush = Brushes.White;
                Brush backgroundBrush = Brushes.Red;

                // 绘制数字背景
               // g.FillRectangle(backgroundBrush, 0, 0, 32, 32);

                // 绘制三位数字
                string numberText = number.ToString("D3");

                SizeF textSize = g.MeasureString(numberText, font);
                float x = (32 - textSize.Width)/2;
                float y = (16 - textSize.Height) / 3;

                if (iCurrentCost < number)
                {
                    g.DrawString(numberText, font, Brushes.OrangeRed, x, y);
                }
                else
                {
                    g.DrawString(numberText, font, Brushes.LawnGreen, x, y);
                }

                iCurrentCost = number;
                    

                // 从位图创建图标
                IntPtr hIcon = bitmap.GetHicon();
                trayIcon.Icon = Icon.FromHandle(hIcon);

                // 释放图标句柄
                Win32.DestroyIcon(hIcon);
            }
        }

        // Win32 API 用于释放图标句柄
        private static class Win32
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool DestroyIcon(IntPtr handle);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
          
            getGoldCost();
        }
    }
}
