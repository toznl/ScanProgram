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
        public static string path;

        public ArtAdd()
        {
            InitializeComponent();
        }
        string jsonStr = "{
                            'ART_INFO':{
                                            'NAME' : "",
                                            'AUTHOR':"",
                                            'TYPE':"",
                                            'ERA':"",
                                            'SIZE':"",
                                            'WIDTH':"",
                                            'HEIGHT':"",
                                            'MEMO':""
                                        }
                            }"";
        private void Create_Button_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            string folderName = "";
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.InitialDirectory = @"C:";
            saveFile.Title = "생성될 Json파일";
            saveFile.DefaultExt = "json";
            saveFile.Filter = "*.json | *.*";
            if (saveFile.ShowDialog().ToString() == "OK")
            {
                fileName = saveFile.FileName.ToString();
                path = fileName;
                image_file_info.file_path = path;
            }
            CreateJson();
        }

        private void Path_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Filter = "*.json | *.*";
            if (dlgOpenFile.ShowDialog().ToString() == "OK")
            {
                image_file_info.file_path = dlgOpenFile.FileName;
                path = image_file_info.file_path;
                if (File.Exists(path))
                {
                    ReadJson();
                }
                else
                {
                    File.Create(path);
                }

            }


        }
        private void CreateJson()
        {

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
            if (path != null)
            {
                //json 파일이 존재 한다면
                if (File.Exists(path))
                {
                    InputJson(path);
                }
            }

        }

        private void InputJson(string path)
        {
            //사용자 정보 배열로 선언
            JObject json = JObject.Parse(jsonStr);


            json["ARTINFO"]["NAME"] = Convert.ToString(Info_art_name.Text);
            //JObject dbSpec = new JObject(
            //    new JProperty("ARTINFO","NAME", ),
            //    new JProperty("ARTINFO", "AUTHOR", Convert.ToString(Info_art_author.Text)),
            //    new JProperty("ARTINFO", "TYPE", Convert.ToString(Info_art_type.Text)),
            //    new JProperty("ARTINFO", "ERA", Convert.ToString(Info_art_era.Text)),
            //    new JProperty("ARTINFO", "SIZE", Convert.ToString(Info_art_size.Text)),
            //    new JProperty("ARTINFO", "WIDTH", Convert.ToString(Info_art_width.Text)),
            //    new JProperty("ARTINFO", "HEIGHT", Convert.ToString(Info_art_height.Text)),
            //    new JProperty("ARTINFO", "MEMO", Convert.ToString(Info_art_memo.Text))
            //    );


            //File.WriteAllText(path, dbSpec.ToString());
        }

        private void ReadJson()
        {
            
            if (path != null)
            {
                //// Json 파일 읽기
                using (StreamReader file = File.OpenText(path))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject json = JObject.Parse(jsonStr);

                    art_Info.name = (string)json["ARTINFO"]["NAME"].ToString();
                    art_Info.author = (string)json["ARTINFO"]["AUTHOR"].ToString();
                    art_Info.type = (string)json["ARTINFO"]["TYPE"].ToString();
                    art_Info.era = (string)json["ARTINFO"]["ERA"].ToString();
                    art_Info.size = (string)json["ARTINFO"]["SIZE"].ToString();
                    art_Info.width = (string)json["ARTINFO"]["WIDTH"].ToString();
                    art_Info.height = (string)json["ARTINFO"]["HEIGHT"].ToString();
                    art_Info.memo = (string)json["ARTINFO"]["MEMO"].ToString();

                    Info_art_name.Text = art_Info.name;
                    Info_art_author.Text = art_Info.author;
                    Info_art_type.Text = art_Info.type;
                    Info_art_era.Text = art_Info.era;
                    Info_art_size.Text = art_Info.size;
                    Info_art_width.Text = art_Info.width;
                    Info_art_height.Text = art_Info.height;
                    Info_art_memo.Text = art_Info.memo;

                }
            }


        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {

            InputJson(path);
        }
    }
}
