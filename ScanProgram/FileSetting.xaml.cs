using System.Windows;
using System.Windows.Forms;

namespace ScanProgram
{
    /// <summary>
    /// FileSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FileSetting : Window
    {
        public FileSetting()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            file_info file_Info = new file_info();  
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            if (dlgOpenFile.ShowDialog().ToString() == "OK")
            {
                file_Info.filePath = dlgOpenFile.FileName;   
            }
        }
    }
}
