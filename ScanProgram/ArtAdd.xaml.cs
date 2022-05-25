using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Forms;
using System;
using System.Data;

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace ScanProgram
{
    /// <summary>
    /// ArtAdd.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ArtAdd : Window
    {
        art_Info art_info = new art_Info();
        
        
        public ArtAdd()
        {
            InitializeComponent();

            art_Info.name = "dd";
        }

        private void Create_Button_Click(object sender, RoutedEventArgs e)
        {
            WrtieJson();
        }

        private void Path_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Filter = "*.json | *.*";
            if (dlgOpenFile.ShowDialog().ToString() == "OK")
            {
                
            }
            CreateJson();
            
        }
        private void CreateJson()
        {
            string path=@"C:\test\test.json"; 

            if (!File.Exists(path))
            {
                using (File.Create(path))
                {


                }
            }
            else
            {

            }
            
        }
        private void WrtieJson()
        {
            string path = @"C:\test\test.json";

            //json 파일이 존재 한다면
            if (File.Exists(path))
            {
                ReadJson();
            }
        }

        private void InputJson(string path)
        {
            //사용자 정보 배열로 선언

            JObject dbSpec = new JObject(
                new JProperty("NAME", Convert.ToString(Info_art_name.Text)),
                new JProperty("AUTHOR", Convert.ToString(Info_art_author.Text)),
                new JProperty("TYPE", Convert.ToString(Info_art_type.Text)),
                new JProperty("ERA", Convert.ToString(Info_art_era.Text)),
                new JProperty("SIZE", Convert.ToString(Info_art_size.Text)),
                new JProperty("WIDTH", Convert.ToString(Info_art_width.Text)),
                new JProperty("HEIGHT", Convert.ToString(Info_art_height.Text)),
                new JProperty("MEMO", Convert.ToString(Info_art_memo.Text))
                );


            File.WriteAllText(path, dbSpec.ToString());
        }

        private void ReadJson()
        {
            string jsonFilePath = @"C:\test\test.json";
            string json = File.ReadAllText(jsonFilePath);

            art_Info[] arrItemDatas = JsonConvert.DeserializeObject<art_Info[]>(json);

            Dictionary<int, art_Info> dicItemDatas = new Dictionary<int, art_Info>();

     

            foreach (KeyValuePair<int, art_Info> pair in dicItemDatas)
            {
            }

        }
    }
}
