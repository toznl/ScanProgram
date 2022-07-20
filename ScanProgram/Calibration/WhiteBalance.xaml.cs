using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScanProgram.Calibration
{
    /// <summary>
    /// WhiteBalance.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WhiteBalance : Window
    {
        public WhiteBalance()
        {
            InitializeComponent();
           
        }

        private void grabRGB(object sender, EventArgs e)
        {
            //try
            //{
            //    if (MainWindow.cap.img != null)
            //    {
            //        using (MemoryStream memory = new MemoryStream())
            //        {
            //            cap.img.Save(memory, ImageFormat.Bmp);
            //            memory.Position = 0;
            //            BitmapImage bitmapImage = new BitmapImage();
            //            bitmapImage.BeginInit();
            //            bitmapImage.StreamSource = memory;
            //            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //            bitmapImage.EndInit();
            //            view_box.ImageSource = bitmapImage;
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    log.AppendText(ex.ToString());
            //    log.ScrollToEnd();
            //}
        }

    }
}
