namespace ScanProgram
{

    public struct art_info
    {
        public string name;
        public string author;
        public string type;
        public string era;
        public string size;
        public double width;
        public double height;
        public string memo;
    }

    public struct device_info
    {
        public string camera_type;
        public string camera_model;
        public string camera_max_resolution;
        public string camera_manufact;
        public string camera_lens_model;
        public string camera_lens_manufact;
        public string camera_filter;
        public string camera_filter_manufact;
        public string light_type;
        public string light_intensity;
        //public string light_controller_model;
        public string image_file_type;
        public string image_file_path;
        public string image_file_currentcount;
        public string image_file_maxcount;
        public string image_resolution;
        public string image_size;
        public string image_date;
        public string image_time;
        public string image_partition; //A,B,C,D구역
        public string calibration_status_color;
        public string calibration_chart_name_color;
        public string calibration_chart_model_color;
        public string calibration_status_lens;
        public string calibration_chart_name_lens;
        public string calibration_chart_model_lens;
        public string calibration_status_vigntte;
        public string calibration_chart_name_vigntte;
        public string calibration_chart_model_vigntte;
        public string calibration_status_wb;
        public string calibration_chart_name_wb;
        public string calibration_chart_model_wb;
        public string memo;

    }

}
