using System.Windows;
using System.Windows.Forms;

namespace ScanProgram
{
    /// <summary>
    /// ArtAdd.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ArtAdd : Window
    {
        public ArtAdd()
        {
            InitializeComponent();
        }

        private void Create_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Path_Button_Click(object sender, RoutedEventArgs e)
        {
            file_info file_Info = new file_info();  
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Filter = "*.json | *.*";
            if (dlgOpenFile.ShowDialog().ToString() == "OK")
            {
                file_Info.filePath = dlgOpenFile.FileName;
                
            }

        }
    }
}
