using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanProgram
{
    class image_file_info
    {
        public image_file_info()
        {
            file_path = String.Empty;
            file_rgb_raw_path = String.Empty;
            file_rgb_wb_path = String.Empty;
            file_rgb_vignetting_path = String.Empty;
            file_uv_raw_path= String.Empty;
            file_uv_vignetting_path= String.Empty;
        }
        public static string file_path;
        public static string file_rgb_raw_path;
        public static string file_rgb_wb_path;
        public static string file_rgb_vignetting_path;
        public static string file_uv_raw_path;
        public static string file_uv_vignetting_path;



    }
    class art_Info
    {
        public art_Info()
        {
            name = String.Empty;
            author = String.Empty;
            type = String.Empty;
            era = String.Empty;
            size = String.Empty;
            width = String.Empty;
            height = String.Empty;
            memo = String.Empty;
        }

        public static string name;
        public static string author;
        public static string type;
        public static string era;
        public static string size;
        public static string width;
        public static string height;
        public static string memo;
    }
    class device_info
    {
        public device_info()
        {
            camera_type = String.Empty;
            camera_model = String.Empty;
            camera_max_resolution = String.Empty;
            camera_manufact = String.Empty;
            camera_lens_model = String.Empty;
            camera_lens_manufact = String.Empty;
            camera_filter = String.Empty;
            camera_filter_manufact = String.Empty;
            light_type = String.Empty;
            light_intensity = String.Empty;
            image_file_type = String.Empty;
            image_file_path = String.Empty;
            image_file_currentcount = String.Empty;
            image_file_maxcount = String.Empty;
            image_resolution = String.Empty;
            image_size = String.Empty;
            image_date = String.Empty;
            image_time = String.Empty;
            image_partition = String.Empty;
            calibration_status_color = String.Empty;
            calibration_chart_name_color = String.Empty;
            calibration_status_lens = String.Empty;
            calibration_chart_name_lens = String.Empty;
            calibration_chart_model_lens = String.Empty;
            calibration_status_vigntte = String.Empty;
            calibration_chart_name_vigntte = String.Empty;
            calibration_status_wb = String.Empty;
            calibration_chart_name_wb = String.Empty;
            calibration_chart_model_wb = String.Empty;
            memo = String.Empty;

        }
        public static string camera_type;
        public static string camera_model;
        public static string camera_max_resolution;
        public static string camera_manufact;
        public static string camera_lens_model;
        public static string camera_lens_manufact;
        public static string camera_filter;
        public static string camera_filter_manufact;
        public static string light_type;
        public static string light_intensity;
        //public string light_controller_model;
        public static string image_file_type;
        public static string image_file_path;
        public static string image_file_currentcount;
        public static string image_file_maxcount;
        public static string image_resolution;
        public static string image_size;
        public static string image_date;
        public static string image_time;
        public static string image_partition; //A,B,C,D구역
        public static string calibration_status_color;
        public static string calibration_chart_name_color;
        public static string calibration_chart_model_color;
        public static string calibration_status_lens;
        public static string calibration_chart_name_lens;
        public static string calibration_chart_model_lens;
        public static string calibration_status_vigntte;
        public static string calibration_chart_name_vigntte;
        public static string calibration_chart_model_vigntte;
        public static string calibration_status_wb;
        public static string calibration_chart_name_wb;
        public static string calibration_chart_model_wb;
        public static string memo;

    }
}




