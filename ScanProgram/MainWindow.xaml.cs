using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ScanProgram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : System.Windows.Window
    {
        #region Thread
        private DispatcherTimer kernelTimer = new DispatcherTimer();
        private DispatcherTimer cur_Position = new DispatcherTimer();
        private DispatcherTimer snap_pic = new DispatcherTimer();
        private DispatcherTimer grab_pic = new DispatcherTimer();

        public int camera_Mode;
        public string fileinfo_header;
        public string fileinfo_filepath;
        public file_info prop;

        public float final_Start_X;
        public float final_Start_Y;
        public float final_Finish_X;
        public float final_Finish_Y;

        public int CameraMod;
        private System.ComponentModel.Container components = null;
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        PCO_Description pcoDescr;
        PCO_Storage pcoStorage;
        PCO_Image pcoImage;
        PCO_CameraType pcoCameraType;

        IntPtr cameraHandle = IntPtr.Zero;
        IntPtr convertHandle = IntPtr.Zero;
        IntPtr convertDialog = IntPtr.Zero;
        IntPtr camDialog = IntPtr.Zero;
        private const int WM_APPp100 = 0x8000 + 100;
        private const int WM_APPp101 = 0x8000 + 101;
        private const int WM_APPp102 = 0x8000 + 102;
        int bufwidth = 0, bufheight = 0;
        byte[] imagedata;
        short bufnr = -1;



        SaperaCapture cap;
        SaperaCapture_Nir cap_nir;


        #endregion 
        #region RobotVariables
        UInt16 ProcState = 0;
        UInt32 MaxAxis = 0;
        UInt32 Status = 0;
        string ActPositionX;
        string ActPositionY;
        string ActPositionZ;


        #endregion
        #region PCO_Struct_Import
        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_SC2_Hardware_DESC
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public String szName;               // string with board name
            public ushort wBatchNo;             // production batch no
            public ushort wRevision;            // use range 0 to 99
            public ushort wVariant;             // variant    // 22
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public ushort[] ZZwDummy;             //            // 62
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_SC2_Firmware_DESC
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public String szName;                // string with device name
            public byte bMinorRev;              // use range 0 to 99
            public byte bMajorRev;              // use range 0 to 255
            public ushort wVariant;             // variant    // 20
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
            public ushort[] ZZwDummy;             //            // 64
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_HW_Vers
        {
            public ushort BoardNum;       // number of devices
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 10)]
            public PCO_SC2_Hardware_DESC[] Board;// 622
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_FW_Vers
        {
            public ushort DeviceNum;       // number of devices
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 10)]
            public PCO_SC2_Firmware_DESC[] Device;// 642
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_CameraType
        {
            public ushort wSize;                   // Sizeof this struct
            public ushort wCamType;                // Camera type
            public ushort wCamSubType;             // Camera sub type
            public ushort ZZwAlignDummy1;
            public UInt32 dwSerialNumber;          // Serial number of camera // 12
            public UInt32 dwHWVersion;             // Hardware version number
            public UInt32 dwFWVersion;             // Firmware version number
            public ushort wInterfaceType;          // Interface type          // 22
            public PCO_HW_Vers strHardwareVersion;      // Hardware versions of all boards // 644
            public PCO_FW_Vers strFirmwareVersion;      // Firmware versions of all devices // 1286
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 39)]
            public ushort[] ZZwDummy;                                          // 1364
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_Description
        {
            public ushort wSize;                   // Sizeof this struct
            public ushort wSensorTypeDESC;         // Sensor type
            public ushort wSensorSubTypeDESC;      // Sensor subtype
            public ushort wMaxHorzResStdDESC;      // Maxmimum horz. resolution in std.mode
            public ushort wMaxVertResStdDESC;      // Maxmimum vert. resolution in std.mode
            public ushort wMaxHorzResExtDESC;      // Maxmimum horz. resolution in ext.mode
            public ushort wMaxVertResExtDESC;      // Maxmimum vert. resolution in ext.mode
            public ushort wDynResDESC;             // Dynamic resolution of ADC in bit
            public ushort wMaxBinHorzDESC;         // Maxmimum horz. binning
            public ushort wBinHorzSteppingDESC;    // Horz. bin. stepping (0:bin, 1:lin)
            public ushort wMaxBinVertDESC;         // Maxmimum vert. binning
            public ushort wBinVertSteppingDESC;    // Vert. bin. stepping (0:bin, 1:lin)
            public ushort wRoiHorStepsDESC;        // Minimum granularity of ROI in pixels
            public ushort wRoiVertStepsDESC;       // Minimum granularity of ROI in pixels
            public ushort wNumADCsDESC;            // Number of ADCs in system
            public ushort ZZwAlignDummy1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public uint[] dwPixelRateDESC;       // Possible pixelrate in Hz
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public uint[] ZZdwDummypr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] wConvFactDESC;       // Possible conversion factor in e/cnt
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public ushort[] ZZdwDummycv;
            public ushort wIRDESC;                 // IR enhancment possibility
            public ushort ZZwAlignDummy2;
            public uint dwMinDelayDESC;          // Minimum delay time in ns
            public uint dwMaxDelayDESC;          // Maximum delay time in ms
            public uint dwMinDelayStepDESC;      // Minimum stepping of delay time in ns
            public uint dwMinExposureDESC;       // Minimum exposure time in ns
            public uint dwMaxExposureDESC;       // Maximum exposure time in ms
            public uint dwMinExposureStepDESC;   // Minimum stepping of exposure time in ns
            public uint dwMinDelayIRDESC;        // Minimum delay time in ns
            public uint dwMaxDelayIRDESC;        // Maximum delay time in ms
            public uint dwMinExposureIRDESC;     // Minimum exposure time in ns
            public uint dwMaxExposureIRDESC;     // Maximum exposure time in ms
            public ushort wTimeTableDESC;          // Timetable for exp/del possibility
            public ushort wDoubleImageDESC;        // Double image mode possibility
            public short sMinCoolSetDESC;         // Minimum value for cooling
            public short sMaxCoolSetDESC;         // Maximum value for cooling
            public short sDefaultCoolSetDESC;     // Default value for cooling
            public ushort wPowerDownModeDESC;      // Power down mode possibility 
            public ushort wOffsetRegulationDESC;   // Offset regulation possibility
            public ushort wColorPatternDESC;       // Color pattern of color chip
                                                   // four nibbles (0,1,2,3) in ushort 
                                                   //  ----------------- 
                                                   //  | 3 | 2 | 1 | 0 |
                                                   //  ----------------- 
                                                   //   
                                                   // describe row,column  2,2 2,1 1,2 1,1
                                                   // 
                                                   //   column1 column2
                                                   //  ----------------- 
                                                   //  |       |       |
                                                   //  |   0   |   1   |   row1
                                                   //  |       |       |
                                                   //  -----------------
                                                   //  |       |       |
                                                   //  |   2   |   3   |   row2
                                                   //  |       |       |
                                                   //  -----------------
                                                   // 
            public ushort wPatternTypeDESC;        // Pattern type of color chip
                                                   // 0: Bayer pattern RGB
                                                   // 1: Bayer pattern CMY
            public ushort wDSNUCorrectionModeDESC; // DSNU correction mode possibility
            public ushort ZZwAlignDummy3;          //
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public UInt32[] dwReservedDESC;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public UInt32[] ZZdwDummy;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_Description2
        {
            public ushort wSize;                   // Sizeof this struct
            public ushort ZZwAlignDummy1;
            public uint dwMinPeriodicalTimeDESC2;// Minimum periodical time tp in (nsec)
            public uint dwMaxPeriodicalTimeDESC2;// Maximum periodical time tp in (msec)        (12)
            public uint dwMinPeriodicalConditionDESC2;// System imanent condition in (nsec)
                                                      // tp - (td + te) must be equal or longer than
                                                      // dwMinPeriodicalCondition
            public uint dwMaxNumberOfExposuresDESC2;// Maximum number of exporures possible        (20)
            public int lMinMonitorSignalOffsetDESC2;// Minimum monitor signal offset tm in (nsec)
                                                    // if(td + tstd) > dwMinMon.)
                                                    //   tm must not be longer than dwMinMon
                                                    // else
                                                    //   tm must not be longer than td + tstd
            public uint dwMaxMonitorSignalOffsetDESC2;// Maximum -''- in (nsec)                      
            public uint dwMinPeriodicalStepDESC2;// Minimum step for periodical time in (nsec)  (32)
            public uint dwStartTimeDelayDESC2; // Minimum monitor signal offset tstd in (nsec)
                                               // see condition at dwMinMonitorSignalOffset
            public uint dwMinMonitorStepDESC2; // Minimum step for monitor time in (nsec)     (40)
            public uint dwMinDelayModDESC2;    // Minimum delay time for modulate mode in (nsec)
            public uint dwMaxDelayModDESC2;    // Maximum delay time for modulate mode in (msec)
            public uint dwMinDelayStepModDESC2;// Minimum delay time step for modulate mode in (nsec)(52)
            public uint dwMinExposureModDESC2; // Minimum exposure time for modulate mode in (nsec)
            public uint dwMaxExposureModDESC2; // Maximum exposure time for modulate mode in (msec)(60)
            public uint dwMinExposureStepModDESC2;// Minimum exposure time step for modulate mode in (nsec)
            public uint dwModulateCapsDESC2;   // Modulate capabilities descriptor
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public UInt32[] dwReservedDESC;                                                    //(132)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
            public UInt32[] ZZdwDummy;                                                         // 296};
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_DescriptionEx
        {
            public ushort wSize;                   // Sizeof this struct
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_Storage
        {
            public ushort wSize;                   // Sizeof this struct
            public ushort ZZwAlignDummy1;
            public UInt32 dwRamSize;               // Size of camera ram in pages
            public ushort wPageSize;               // Size of one page in pixel       // 10
            public ushort ZZwAlignDummy4;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UInt32[] dwRamSegSize;          // Size of ram segment 1-4 in pages // 28
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public UInt32[] ZZdwDummyrs;                                                // 108
            public ushort wActSeg;                 // no. (0 .. 3) of active segment  // 110
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 39)]
            public ushort[] ZZwDummy;                                     // 188
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_Segment
        {
            public ushort wSize;                   // Sizeof this struct
            public ushort wXRes;                   // Res. h. = resulting horz.res.(sensor resolution, ROI, binning)
            public ushort wYRes;                   // Res. v. = resulting vert.res.(sensor resolution, ROI, binning)
            public ushort wBinHorz;                // Horizontal binning
            public ushort wBinVert;                // Vertical binning                // 10
            public ushort wRoiX0;                  // Roi upper left x
            public ushort wRoiY0;                  // Roi upper left y
            public ushort wRoiX1;                  // Roi lower right x
            public ushort wRoiY1;                  // Roi lower right y
            public ushort ZZwAlignDummy1;                                             // 20
            public UInt32 dwValidImageCnt;         // no. of valid images in segment
            public UInt32 dwMaxImageCnt;           // maximum no. of images in segment // 28
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public ushort[] ZZwDummy;                                         // 188
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_Image
        {
            public ushort wSize;      // Sizeof this struct
            public ushort ZZwAlignDummy1;                                    // 4
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
            public PCO_Segment[] strSegment;// Segment info                      // 436
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            public PCO_Segment[] ZZstrDummySeg;// Segment info dummy            // 2164
            public ushort wBitAlignment;// Bitalignment during readout. 0: MSB, 1: LSB aligned
            public ushort wHotPixelCorrectionMode;   // Correction mode for hotpixel
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 38)]
            public ushort[] ZZwDummy;                                              // 2244
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct PCO_OpenStruct
        {
            public ushort wSize;        // Sizeof this struct
            public ushort wInterfaceType;
            public ushort wCameraNumber;
            public ushort wCameraNumAtInterface; // Current number of camera at the interface
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 10)]
            public ushort[] wOpenFlags;   // [0]: moved to dwnext to position 0xFF00
                                          // [1]: moved to dwnext to position 0xFFFF0000
                                          // [2]: Bit0: PCO_OPENFLAG_GENERIC_IS_CAMLINK
                                          //            Set this bit in case of a generic Cameralink interface
                                          //            This enables the import of the additional three camera-
                                          //            link interface functions.

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 5)]
            public UInt32[] dwOpenFlags;// [0]-[4]: moved to strCLOpen.dummy[0]-[4]
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6)]
            public IntPtr[] wOpenPtr;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            public ushort[] zzwDummy;     // 88 - 64bit: 112
        };

        class PCO_SDK_LibWrapper
        {
            [DllImport("sc2_cam.dll", EntryPoint = "PCO_OpenCamera",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_OpenCamera(ref IntPtr pHandle, UInt16 wCamNum);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_OpenCameraEx",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_OpenCameraEx(ref IntPtr pHandle, PCO_OpenStruct strOpen);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_CloseCamera",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_CloseCamera(IntPtr pHandle);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_ResetLib",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_ResetLib();

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetCameraDescription",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetCameraDescription(IntPtr pHandle, ref PCO_Description strDescription);

            // In C# it is hard to deal with pointer to structures with different sizes.
            // Thus it is easier to setup a similar function call for each available structure.
            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetCameraDescriptionEx",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetCameraDescriptionEx(IntPtr pHandle, ref PCO_Description strDescription, UInt16 wType);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetCameraDescriptionEx",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetCameraDescriptionEx2(IntPtr pHandle, ref PCO_Description2 strDescription, UInt16 wType);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetCameraHealthStatus",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetCameraHealthStatus(IntPtr pHandle, ref UInt32 dwWarn, ref UInt32 dwError, ref UInt32 dwStatus);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_AllocateBuffer",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_AllocateBuffer(IntPtr pHandle, ref short sBufNr, int size, ref UIntPtr wBuf, ref IntPtr hEvent);
            //HANDLE ph,SHORT* sBufNr,DWORD size,WORD** wBuf,HANDLE *hEvent

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetBuffer",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetBuffer(IntPtr pHandle, short sBufNr, ref UIntPtr wBuf, ref IntPtr hEvent);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_FreeBuffer",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_FreeBuffer(IntPtr pHandle, short sBufNr);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_ArmCamera",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_ArmCamera(IntPtr pHandle);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_CamLinkSetImageParameters",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_CamLinkSetImageParameters(IntPtr pHandle, UInt16 wXRes, UInt16 wYRes);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_SetRecordingState",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_SetRecordingState(IntPtr pHandle, UInt16 wRecState);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetRecordingState",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetRecordingState(IntPtr pHandle, ref UInt16 wRecState);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_CancelImages",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_CancelImages(IntPtr pHandle);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_AddBuffer",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_AddBuffer(IntPtr pHandle, UInt32 dwFirstImage, UInt32 dwLastImage, short sBufNr);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_AddBufferEx",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_AddBufferEx(IntPtr pHandle, UInt32 dwFirstImage, UInt32 dwLastImage, short sBufNr, UInt16 wXRes, UInt16 wYRes, UInt16 wBitPerPixel);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetBufferStatus",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetBufferStatus(IntPtr pHandle, short sBufNr, ref UInt32 dwStatusDll, ref UInt32 dwStatusDrv);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetStorageStruct",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetStorageStruct(IntPtr pHandle, ref PCO_Storage strStorage);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetImageStruct",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetImageStruct(IntPtr pHandle, ref PCO_Image strImage);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetCameraType",
               ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetCameraType(IntPtr pHandle, ref PCO_CameraType strCameraType);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetCameraName",
                ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetCameraName(IntPtr pHandle, byte[] szCameraName, ushort wSZCameraNameLen);

            [DllImport("sc2_cam.dll", EntryPoint = "PCO_SetTriggerMode",
                ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_SetTriggerMode(IntPtr pHandle, ushort wTriggerMode);
            [DllImport("sc2_cam.dll", EntryPoint = "PCO_GetSizes",
                ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_GetSizes(IntPtr hCamDialog,
                                      ref UInt16 wXResAct, // Actual X Resolution
                                      ref UInt16 wYResAct, // Actual Y Resolution
                                      ref UInt16 wXResMax, // Maximum X Resolution
                                      ref UInt16 wYResMax); // Maximum Y Resolution

            [DllImport("sc2_Dlg.dll", EntryPoint = "PCO_OpenDialogCam",
                ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_OpenDialogCam(ref IntPtr hCamDialog, IntPtr pHandle, IntPtr parent, UInt32 uiFlags, UInt32 uiMsgArm, UInt32 uiMsgCtrl, int xpos, int ypos, [MarshalAs(UnmanagedType.LPStr)] string title);

            [DllImport("sc2_Dlg.dll", EntryPoint = "PCO_CloseDialogCam",
                ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_CloseDialogCam(IntPtr hCamDialog);

            [DllImport("sc2_Dlg.dll", EntryPoint = "PCO_EnableDialogCam",
                ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
            public static extern int PCO_EnableDialogCam(IntPtr hCamDialog, bool bEnable);
        };
        #endregion

        void calibration_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Filter = "*.json | *.*";
            if (dlgOpenFile.ShowDialog().ToString() == "OK")
            {
                prop.filePath = dlgOpenFile.FileName;
                log.AppendText($"Art Information File Loaded : [{prop.filePath}]\r");
            }

        }
        public UInt32 IndexCal(UInt32 Index)
        {
            return Index * MaxAxis;
        }
        #region RobotState&Motions
        public enum MXP_KernelState
        {
            Idle = 0,
            Init,
            Initing,
            Inited,
            Run,
            Running,
            Runed,
            Reset,
            Close,
            Destory
        }
        public enum MXP_MotionBlockIndex
        {
            mcPower = 0,
            mcReset,
            mcStop,
            mcHalt,
            mcHome,
            mcMoveAbsolute,
            mcMoveRelative,
            mcMoveVelocity,
            mcMoveLinearAbsolute,
            mcMoveLinearRelative,
            mcMoveCircularAbsolute,
            mcMoveCircularRelative,
            mcGroupStop,
            mcWriteParameter,
            mcWriteBoolParameter,
            mcET_ReadParameter,
            mcET_WriteParameter,
            mcWriteOutputs,
            mcWriteDigitalOutputs,
            mcGearIn,
            mcGearOut,
            mcGearInPos,
            mcCamIn,
            mcCamOut,
            mcCamTableSelect,
            mcSetTouchProbe,
            mcDirectTorqueControl
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }
        private void emergencyStop_Click(object sender, RoutedEventArgs e)
        {
            ProcState = (UInt16)MXP_KernelState.Close;
            log.AppendText("Emergency Stop\r");
        }
        private void file_Setting_Click(object sender, RoutedEventArgs e)
        {
            ScanProgram.FileSetting fileSettingWindow = new ScanProgram.FileSetting();

            fileSettingWindow.Show();
            log.AppendText("FileSetting Windows Opened\r");
        }

        private void art_Add_Click(object sender, RoutedEventArgs e)
        {
            ScanProgram.ArtAdd artAddWindow = new ScanProgram.ArtAdd();

            artAddWindow.Show();
            log.AppendText("ArtAdd Windows Opened\r");
        }

        private void zeroReturn_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_HOME_OUT Out = new MXP.MXP_HOME_OUT { };

            for (UInt32 i = 0; i < MaxAxis; i++)
            {
                Motion_Function.MXP_MC_Home(i, IndexCal((UInt32)MXP_MotionBlockIndex.mcHome) + i, MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING, false, Out);
            }

            log.AppendText("Zero Return\r");

        }

        private void zeroSetting_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_HOME_OUT Out = new MXP.MXP_HOME_OUT { };

            for (UInt32 i = 0; i < MaxAxis; i++)
            {
                Motion_Function.MXP_MC_Home(i, IndexCal((UInt32)MXP_MotionBlockIndex.mcHome) + i, MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING, false, Out);
            }

            log.AppendText("Homing Completed\r");
        }

        private void connect_device_Click(object sender, RoutedEventArgs e)
        {

            kernelTimer.Tick += new EventHandler(KernelTimer_Tick);
            kernelTimer.Interval = TimeSpan.FromMilliseconds(0.01);

            ProcState = (UInt16)MXP_KernelState.Init;
            kernelTimer.Start();
        }
        private void power_Robot_Click(object sender, RoutedEventArgs e)
        {
            cur_Position.Tick += new EventHandler(GetCurrentPosition);
            cur_Position.Interval = TimeSpan.FromMilliseconds(0.01);

            MXP.MXP_POWER_OUT Out = new MXP.MXP_POWER_OUT { };

            for (UInt32 i = 0; i < MaxAxis; i++)
            {
                Motion_Function.MXP_MC_Power(i, IndexCal((UInt32)MXP_MotionBlockIndex.mcPower) + i, 1, false, Out);
            }

            log.AppendText("Robot_Servo_On\r");

            cur_Position.Start();
        }
        public void GetCurrentPosition(object sender, EventArgs e)
        {
            ActPositionX = Motion_Function.MXP_MC_ReadActualPosition(0).ToString("F2");
            ActPositionY = Motion_Function.MXP_MC_ReadActualPosition(1).ToString("F2");
            ActPositionZ = Motion_Function.MXP_MC_ReadActualPosition(2).ToString("F2");

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    //your code
                    currentPosition.Text = ($"X: {ActPositionX}, Y: {ActPositionY}, Z: {ActPositionZ}");
                }));

        }

        private void KernelTimer_Tick(object sender, EventArgs e)
        {
            switch (ProcState)
            {
                case (UInt16)MXP_KernelState.Idle:
                    {
                        break;
                    }
                case (UInt16)MXP_KernelState.Init:
                    {
                        UInt32 status = 0;
                        Int32 InitError;

                        InitError = MXP.MXP_InitKernel(ref status);

                        if (InitError == MXP.MXP_ret.RET_NO_ERROR)
                        {
                            ProcState = (UInt16)MXP_KernelState.Initing;
                            log.AppendText("Succeed to initialize MXP.\r");
                        }
                        else
                        {
                            ProcState = (UInt16)MXP_KernelState.Idle;
                            log.AppendText("Fail to initialize MXP!!!\r");
                        }
                        break;
                    }
                case (UInt16)MXP_KernelState.Initing:
                    {
                        ProcState = (UInt16)MXP_KernelState.Inited;

                        break;
                    }
                case (UInt16)MXP_KernelState.Inited:
                    {
                        ProcState = (UInt16)MXP_KernelState.Run;
                        break;
                    }
                case (UInt16)MXP_KernelState.Run:
                    {
                        if (MXP.MXP_SystemRun() == MXP.MXP_ret.RET_NO_ERROR)
                        {
                            ProcState = (UInt16)MXP_KernelState.Running;
                            Status++;
                            if (Status > 3)
                            {
                            }
                        }
                        else
                        {
                            log.AppendText("Fail to run MXP!!!\r");
                        }
                        break;
                    }
                case (UInt16)MXP_KernelState.Running:
                    {
                        UInt32 usNumOfSlave = 0;
                        if (MXP.MXP_GetSlaveCount(0/*1: get number of axis, 0: get number of all slaves.*/, out usNumOfSlave) == MXP.MXP_ret.RET_NO_ERROR)
                        {
                            MaxAxis = usNumOfSlave;
                            ProcState = (UInt16)MXP_KernelState.Inited;
                        }

                        UInt32 status = 0;
                        MXP.MXP_GetOnlineMode(ref status);
                        if (status == (UInt32)MXP.MXP_ONLINESTATE_ENUM.NET_STATE_OP)
                        {
                            ProcState = (UInt16)MXP_KernelState.Runed;
                        }
                        break;
                    }
                case (UInt16)MXP_KernelState.Runed:
                    {
                        //러닝중
                        Status = 0;
                        break;
                    }
                case (UInt16)MXP_KernelState.Reset:
                    {
                        if (MXP.MXP_SystemReset() == MXP.MXP_ret.RET_NO_ERROR)
                        {
                            ProcState = (UInt16)MXP_KernelState.Running;
                            log.AppendText("Succeed to reset MXP.\r");
                        }
                        else
                        {
                            log.AppendText("Fail to reset MXP!!!\r");
                        }
                        break;
                    }
                case (UInt16)MXP_KernelState.Close:
                    {
                        Int32 Status = 0;
                        MXP.MXP_GetKernelStatus(out Status);
                        if (Status >= MXP.MXP_SysStatus.Initialized)
                        {
                            if (MXP.MXP_SystemStop() == MXP.MXP_ret.RET_NO_ERROR)
                            {
                                ProcState = (UInt16)MXP_KernelState.Destory;
                            }
                            else if (Status == 0)
                            {
                                ProcState = (UInt16)MXP_KernelState.Idle;
                                log.AppendText("Already destroy MXP\r");
                            }
                            else
                            {
                                log.AppendText("Fail to stop MXP!!!\r");
                            }
                        }
                        break;
                    }
                case (UInt16)MXP_KernelState.Destory:
                    {
                        if (MXP.MXP_Destroy() == MXP.MXP_ret.RET_NO_ERROR)
                        {
                            ProcState = (UInt16)MXP_KernelState.Idle;
                            log.AppendText("Succeed to close MXP.\r");
                        }
                        else
                        {
                            log.AppendText("Fail to close MXP!!!\r");
                        }
                        break;
                    }
            }
        }
        #region 2Dmove Arrow
        private void move_up_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVERELATIVE_IN x = new MXP.MXP_MOVERELATIVE_IN { };
            MXP.MXP_MOVERELATIVE_OUT y = new MXP.MXP_MOVERELATIVE_OUT { };

            Motion_Function.MXP_MC_MoveRelative(1,
                                    1,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(align_Y.Text),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    false, y);
        }

        private void move_down_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVERELATIVE_IN x = new MXP.MXP_MOVERELATIVE_IN { };
            MXP.MXP_MOVERELATIVE_OUT y = new MXP.MXP_MOVERELATIVE_OUT { };

            Motion_Function.MXP_MC_MoveRelative(1,
                                    1,
                                    Convert.ToSingle(100),
                                    -Convert.ToSingle(align_Y.Text),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    false, y);
        }

        private void move_left_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVERELATIVE_IN x = new MXP.MXP_MOVERELATIVE_IN { };
            MXP.MXP_MOVERELATIVE_OUT y = new MXP.MXP_MOVERELATIVE_OUT { };

            Motion_Function.MXP_MC_MoveRelative(0,
                                    0,
                                    Convert.ToSingle(100),
                                    -Convert.ToSingle(align_X.Text),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    false, y);
        }

        private void move_right_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVERELATIVE_IN x = new MXP.MXP_MOVERELATIVE_IN { };
            MXP.MXP_MOVERELATIVE_OUT y = new MXP.MXP_MOVERELATIVE_OUT { };

            Motion_Function.MXP_MC_MoveRelative(0,
                                    0,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(align_X.Text),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    false, y);
        }
        #endregion
        public void auto_capture_method(object sender, EventArgs e)
        {
            fileinfo_header = "ART4";

            int CountX = 1;
            int CountY = 1;
            MXP.MXP_MOVEABSOLUTE_OUT y = new MXP.MXP_MOVEABSOLUTE_OUT { };
            log.AppendText("Auto Capture Started\r");
            Motion_Function.MXP_MC_MoveAbsolute(0,
                                    0,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(start_X.Text),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                    false, y);
            Motion_Function.MXP_MC_MoveAbsolute(1,
                                    1,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(start_Y.Text),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                    false, y);
            Thread.Sleep(2000);

            int status_x = 0;
            int status_y = 0;

            status_x = Motion_Function.MXP_MC_ReadAxisStatus(0);
            status_y = Motion_Function.MXP_MC_ReadAxisStatus(1);


            for (int positionY = Convert.ToInt32(start_Y.Text); positionY > Convert.ToInt32(finish_Y.Text); positionY = positionY - Convert.ToInt32(align_Y.Text))
            {
                for (int positionX = Convert.ToInt32(start_X.Text); positionX < Convert.ToInt32(finish_X.Text); positionX = positionX + Convert.ToInt32(align_X.Text))
                {
                    Motion_Function.MXP_MC_MoveAbsolute(0,
                                   0,
                                   Convert.ToSingle(100),
                                   Convert.ToSingle(positionX),
                                   Convert.ToSingle(50),
                                   Convert.ToSingle(50),
                                   Convert.ToSingle(500),
                                   MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                   MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                   false, y);

                    Motion_Function.MXP_MC_MoveAbsolute(1,
                                   1,
                                   Convert.ToSingle(100),
                                   Convert.ToSingle(positionY),
                                   Convert.ToSingle(50),
                                   Convert.ToSingle(50),
                                   Convert.ToSingle(500),
                                   MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                   MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                   false, y);

                    Thread.Sleep(2000);
                    Thread.Sleep(3000);
                    if (CameraMod == 1)
                    {
                        cap.Snap(CountY, CountX, fileinfo_header);
                    }
                    else if (CameraMod == 2)
                    {
                        cap_nir.Snap(CountY, CountX, fileinfo_header);

                    }
                    CountX++;
                }
                Motion_Function.MXP_MC_MoveAbsolute(0,
                                   0,
                                   Convert.ToSingle(100),
                                   Convert.ToSingle(start_X.Text),
                                   Convert.ToSingle(50),
                                   Convert.ToSingle(50),
                                   Convert.ToSingle(500),
                                   MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                   MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                   false, y);
                CountX = 1;
                CountY++;
                Thread.Sleep(15000);
                log.AppendText("AutoCapture Finished\r");
            }
        }
        private void auto_Capture_Click(object sender, RoutedEventArgs e)
        {
            grab_pic.Tick += new EventHandler(auto_capture_method);
            grab_pic.Interval = TimeSpan.FromMilliseconds(2);

            grab_pic.Start();
        }

        private void Edit_Velocity_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }



        private void log_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void save_Setting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void move_CameraUp_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVERELATIVE_IN x = new MXP.MXP_MOVERELATIVE_IN { };
            MXP.MXP_MOVERELATIVE_OUT y = new MXP.MXP_MOVERELATIVE_OUT { };

            Motion_Function.MXP_MC_MoveRelative(0,
                                    0,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(-10),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    false, y);
        }

        private void move_CameraDown_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVERELATIVE_IN x = new MXP.MXP_MOVERELATIVE_IN { };
            MXP.MXP_MOVERELATIVE_OUT y = new MXP.MXP_MOVERELATIVE_OUT { };

            Motion_Function.MXP_MC_MoveRelative(0,
                                    0,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(10),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    false, y);
        }



        private void Camera_Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (RGB_Camera.IsChecked == true)
            {
                cap = new SaperaCapture();
                CameraMod = 1;
                cap.Grab();

            }
            else if (NIR_Camera.IsChecked == true)
            {
                CameraMod = 2;
            }
            else if (UV_Camera.IsChecked == true)
            {
                CameraMod = 3;
            }
            else
            {
                System.Windows.MessageBox.Show("Camera Mode를 선택하셔야합니다.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            if (cap == null && CameraMod == 1)
            {
                cap = new SaperaCapture();
            }

            else if (cap == null && CameraMod == 2)
            {
                cap_nir = new SaperaCapture_Nir();
            }
            else if (cap == null && CameraMod == 3)
            {
                int err = 0;
                ushort boardNum = 0;

                cameraHandle = IntPtr.Zero;
                convertHandle = IntPtr.Zero;
                convertDialog = IntPtr.Zero;
                camDialog = IntPtr.Zero;
                bufwidth = 0;
                bufheight = 0;

                // Verify board number validity
                // Open a handle to the camera
                    err = PCO_SDK_LibWrapper.PCO_OpenCamera(ref cameraHandle, boardNum);
                if (err == 0)
                {
                    UInt16 wrecstate = 0;



                    PCO_SDK_LibWrapper.PCO_GetRecordingState(cameraHandle, ref wrecstate);
                    if (wrecstate != 0)
                        PCO_SDK_LibWrapper.PCO_SetRecordingState(cameraHandle, 0);

                }
                else
                {
                    err = PCO_SDK_LibWrapper.PCO_ResetLib();
                }
            }

            log.AppendText(CameraMod.ToString());
        }

        private void Camera_Grab_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CameraMod == 3)
            {
                pcoCameraType = new PCO_CameraType();
                pcoDescr = new PCO_Description();
                pcoStorage = new PCO_Storage();
                pcoImage = new PCO_Image();

                pcoDescr.wSize = (ushort)Marshal.SizeOf(pcoDescr);
                pcoStorage.wSize = (ushort)Marshal.SizeOf(pcoStorage);
                pcoImage.wSize = (ushort)Marshal.SizeOf(pcoImage);

                int err = 0;

                err = PCO_SDK_LibWrapper.PCO_GetCameraDescription(cameraHandle, ref pcoDescr);

                err = PCO_SDK_LibWrapper.PCO_GetStorageStruct(cameraHandle, ref pcoStorage);

                pcoImage.strSegment = new PCO_Segment[4];

                pcoImage.strSegment[0].wSize = (ushort)Marshal.SizeOf(typeof(PCO_Segment));
                pcoImage.strSegment[1].wSize = (ushort)Marshal.SizeOf(typeof(PCO_Segment));
                pcoImage.strSegment[2].wSize = (ushort)Marshal.SizeOf(typeof(PCO_Segment));
                pcoImage.strSegment[3].wSize = (ushort)Marshal.SizeOf(typeof(PCO_Segment));

                err = PCO_SDK_LibWrapper.PCO_GetImageStruct(cameraHandle, ref pcoImage);

                ushort usfwsize;// = (ushort)Marshal.SizeOf(typeof(PCO_SC2_Firmware_DESC));

                usfwsize = (ushort)Marshal.SizeOf(typeof(PCO_FW_Vers));
                pcoCameraType.strHardwareVersion.Board = new PCO_SC2_Hardware_DESC[10];
                pcoCameraType.strFirmwareVersion.Device = new PCO_SC2_Firmware_DESC[10];
                for (int i = 0; i < 10; i++)
                {
                    pcoCameraType.strHardwareVersion.Board[i].szName = "123456789012345";
                    pcoCameraType.strFirmwareVersion.Device[i].szName = "123456789012345";
                }
                pcoCameraType.wSize = (ushort)Marshal.SizeOf(pcoCameraType);

                err = PCO_SDK_LibWrapper.PCO_GetCameraType(cameraHandle, ref pcoCameraType);

                byte[] szCameraName;
                szCameraName = new byte[30];
                string cameraname;

                //err = PCO_SDK_LibWrapper.PCO_GetCameraName(cameraHandle, szCameraName, 30);
                cameraname = System.Text.Encoding.Default.GetString(szCameraName);

                UInt32 dwWarn = 0, dwError = 0, dwStatus = 0;
                UInt16 width = 0;
                UInt16 height = 0;
                UInt16 widthmax = 0;
                UInt16 heightmax = 0;

                // It is recommended to call this function in order to get information about the camera internal state
                err = PCO_SDK_LibWrapper.PCO_GetCameraHealthStatus(cameraHandle, ref dwWarn, ref dwError, ref dwStatus);
                PCO_SDK_LibWrapper.PCO_SetTriggerMode(cameraHandle, 0);
                err = PCO_SDK_LibWrapper.PCO_ArmCamera(cameraHandle);
                err = PCO_SDK_LibWrapper.PCO_GetSizes(cameraHandle, ref width, ref height, ref widthmax, ref heightmax);
                err = PCO_SDK_LibWrapper.PCO_CamLinkSetImageParameters(cameraHandle, (UInt16)width, (UInt16)height);

                err = PCO_SDK_LibWrapper.PCO_SetRecordingState(cameraHandle, 1);
                if (camDialog != IntPtr.Zero)
                {
                    PCO_SDK_LibWrapper.PCO_EnableDialogCam(camDialog, false);
                }
            }
        }
    }
}
