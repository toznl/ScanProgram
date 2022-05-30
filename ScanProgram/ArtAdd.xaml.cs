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
        String jsonStr;
        public ArtAdd()
        {
            InitializeComponent();
        }

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

        public class jsonROOT
        {
            public List<json_Art_Info> json_AI;
            public List<json_Device_Info> json_DI;
            public List<json_Calibration_Info> json_CI;
            public List<json_File_Info> json_FI;

        }

        public class json_Art_Info
        {
            public String json_Art_Info_NAME;
            public String json_Art_Info_AUTHOR;
            public String json_Art_Info_TYPE;
            public String json_Art_Info_ERA;
            public String json_Art_Info_SIZE;
            public String json_Art_Info_WIDTH;
            public String json_Art_Info_HEIGHT;
            public String json_Art_Info_FRAME;
            public String json_Art_Info_MEMO;
        }
        public class json_Device_Info
        {
            public String json_Device_Info_CAMERA_TYPE;
            public String json_Device_Info_CAMERA_MODEL;
            public String json_Device_Info_CAMERA_MAX_RESOLUTION;
            public String json_Device_Info_CAMERA_MANUFACT;
            public String json_Device_Info_CAMERA_LENS_MODEL;
            public String json_Device_Info_CAMERA_LENS_MANUFACT;
            public String json_Device_Info_CAMERA_FILTER;
            public String json_Device_Info_CAMERA_FILTER_MANUFACT;
            public String json_Device_Info_LIGHT_TYPE;
            public String json_Device_Info_LIGHT_INTENSITY;
        }
        public class json_Calibration_Info
        {
            public String json_Calibration_Info_STATUS_COLOR;
            public String json_Calibration_Info_CHART_NAME_COLOR;
            public String json_Calibration_Info_CHART_MODEL_COLOR;
            public String json_Calibration_Info_STATUS_LENS;
            public String json_Calibration_Info_CHART_NAME_LENS;
            public String json_Calibration_Info_CHART_MODEL_LENS;
            public String json_Calibration_Info_STATUS_VIGNETTE;
            public String json_Calibration_Info_CHART_NAME_VIGNETTE;
            public String json_Calibration_Info_CHART_MODEL_VIGNETTE;
            public String json_Calibration_Info_STATUS_WB;
            public String json_Calibration_Info_CHART_NAME_WB;
            public String json_Calibration_Info_CHART_MODEL_WB;
            public String json_Calibration_Info_MEMO;

        }
        public class json_File_Info
        {
            public String json_File_Info_TYPE;
            public String json_File_Info_PATH;
            public String json_File_Info_RESOLUTION;
            public String json_File_Info_SIZE;
            public String json_File_Info_DATE;
            public String json_File_Info_TIME;
            public String json_File_Info_COUNT_HORIZONTAL;
            public String json_File_Info_COUNT_VERTICAL;
            public String json_File_Info_COUNT_TOTAL;
            public String json_File_Info_PARTITION;
        }

        string json = "" +
            "{ " +
            "  'ART_INFO': [ " +
            "               { 'NAME': '이삭 줍는 여인들', " +
            "                 'AUTHOR': '장프랑수아 밀레', " +
            "                 'TYPE': '유화', " +
            "                 'ERA': '1857년도', " +
            "                 'SIZE': '50호', " +
            "                 'WIDTH': '84', " +
            "                 'HEIGHT': '112', " +
            "                 'FRAME': '유', " +
            "                 'MEMO': '테스트용 그림', " +
            "               }, " +
            "             ] " +
            "  'DEVICE_INFO': [ " +
            "               { 'CAMERA_TYPE': 'RGB', " +
            "                 'CAMERA_MODEL': 'Nano-C4020', " +
            "                 'CAMERA_MAX_RESOLUTION': '4112 x 3008', " +
            "                 'CAMERA_MANUFACT': 'Teledyne', " +
            "                 'CAMERA_LENS_MODEL': 'C VIS-NIR', " +
            "                 'CAMERA_LENS_MANUFACT': 'Edmund', " +
            "                 'CAMERA_FILTER': 'N/A', " +
            "                 'CAMERA_FILTER_MANUFACT': 'Edmund', " +
            "                 'LIGHT_TYPE': 'LED', " +
            "                 'LIGHT_INTENSITY': 'N/A', " +
            "               }, " +
            "             ] " +
            "  'CALIBRATION_INFO': [ " +
            "               { 'STATUS_COLOR': 'N/A', " +
            "                 'CHART_NAME_COLOR': '', " +
            "                 'CHART_MODEL_COLOR': '', " +
            "                 'STATUS_LENS': 'N/A', " +
            "                 'CHART_NAME_LENS': '', " +
            "                 'CHART_MODEL_LENS': '', " +
            "                 'STATUS_VIGNETTE': 'N/A', " +
            "                 'CHART_NAME_VIGNETTE': '', " +
            "                 'CHART_MODEL_VIGNETTE': '', " +
            "                 'STATUS_WB': 'N/A', " +
            "                 'CHART_NAME_WB': '', " +
            "                 'CHART_MODEL_WB': '', " +
            "                 'MEMO': '', " +
            "               }, " +
            "             ] " +
            "  'FILE_INFO': [ " +
            "               { 'TYPE': '.tiff', " +
            "                 'PATH': 'C:\\ART1', " +
            "                 'RESOLUTION': '4112 x 3008', " +
            "                 'SIZE': '35.4MB', " +
            "                 'DATE': '2022/05/27', " +
            "                 'TIME': '15:00', " +
            "                 'COUNT_HORIZONTAL': '9', " +
            "                 'COUNT_VERTICAL': '9', " +
            "                 'COUNT_TOTAL': '81', " +
            "                 'PARTITON': 'N/A', " +
            "               }, " +
            "             ] " +
            "}";
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
            JObject jObject = JObject.Parse(json);
            jObject["ARTINFO"][0]["NAME"][0] = Convert.ToString(Info_art_name.Text);
            //JObject dbSpec = new JObjec t(
            //    new JProperty("NAME", Convert.ToString(Info_art_name.Text)),
            //    new JProperty("AUTHOR", Convert.ToString(Info_art_author.Text)),
            //    new JProperty("TYPE", Convert.ToString(Info_art_type.Text)),
            //    new JProperty("ERA", Convert.ToString(Info_art_era.Text)),
            //    new JProperty("SIZE", Convert.ToString(Info_art_size.Text)),
            //    new JProperty("WIDTH", Convert.ToString(Info_art_width.Text)),
            //    new JProperty("HEIGHT", Convert.ToString(Info_art_height.Text)),
            //    new JProperty("MEMO", Convert.ToString(Info_art_memo.Text))
            //    );


            File.WriteAllText(path, jObject.ToString());
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
