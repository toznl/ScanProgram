using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Drawing.Imaging;
using System.Drawing;

using System.Windows.Media.Imaging;
using System.Windows.Interop;
using OpenCvSharp;
using System.IO;
using System.IO.Ports;
using System.Windows.Documents;

using PCOConvertStructures;
using PCOConvertDll;
using scanSnapDll;

namespace ScanProgram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : System.Windows.Window
    {
        int bytesreceived = 0;
        byte[] inputdata = new byte[65535];
        SerialPort Serial_Uart = new SerialPort();

        #region Thread
        private DispatcherTimer kernelTimer = new DispatcherTimer(); //RobotTimer
        private DispatcherTimer cur_Position = new DispatcherTimer(); //Current Position Timer
        private DispatcherTimer snap_pic = new DispatcherTimer(); //Snap Picture Timer
        private DispatcherTimer grab_pic = new DispatcherTimer(); //Grab Picture Timer
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        #endregion
        #region UV Parameters
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
        #endregion
        #region RobotVariables
        UInt16 ProcState = 0;
        UInt32 MaxAxis = 0;
        UInt32 Status = 0;
        string ActPositionX;
        string ActPositionY;
        string ActPositionZ;
        string Robot_Status;
        float MAX_X = 480;
        float MAX_Y = 730;
        float MAX_Z = 220;
        float JoyStick_Speed = 0;
        float positionX;
        float positionY;
        int CountX = 1;
        int CountY = 1;
        int Auto_Zero = 0;
        int capture_Lock = 1;
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
        #region Parameters
        public string fileinfo_header;
        public string fileinfo_filepath;
        public float final_Start_X;
        public float final_Start_Y;
        public float final_Finish_X;
        public float final_Finish_Y;
        BitmapImage bitmapImage = new BitmapImage();
        MemoryStream memory = new MemoryStream();
        public int CameraMod;
        SaperaCapture cap;
        SaperaCapture_Nir cap_nir;
        #endregion
        #region RobotState&Motions
        public UInt32 IndexCal(UInt32 Index)
        {
            return Index * MaxAxis;
        }
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


        #region RobotInit
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
            log.ScrollToEnd();


            cur_Position.Start();
        }
        //Zero Setting
        private void zeroSetting_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_HOME_OUT Out = new MXP.MXP_HOME_OUT { };
            log.AppendText("Homing\r");
            log.ScrollToEnd();
            for (UInt32 i = 0; i < MaxAxis; i++)
            {
                Motion_Function.MXP_MC_Home(i, IndexCal((UInt32)MXP_MotionBlockIndex.mcHome) + i, MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING, false, Out);
            }
            offset_X.Text = "0";
            offset_Y.Text = "0";
            offset_Z.Text = "0";

            log.AppendText("Homing Completed\r");
            log.ScrollToEnd();

        }

        //Zero Return
        private void zeroReturn_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVEABSOLUTE_IN x = new MXP.MXP_MOVEABSOLUTE_IN { };
            MXP.MXP_MOVEABSOLUTE_OUT y = new MXP.MXP_MOVEABSOLUTE_OUT { };

            Motion_Function.MXP_MC_MoveAbsolute(2,
                                     2,
                                     Convert.ToSingle(100),
                                     Convert.ToSingle(offset_X.Text),
                                     Convert.ToSingle(50),
                                     Convert.ToSingle(50),
                                     Convert.ToSingle(500),
                                     MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                     MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                     false, y);

            Motion_Function.MXP_MC_MoveAbsolute(0,
                           0,
                           Convert.ToSingle(100),
                           Convert.ToSingle(offset_Y.Text),
                           Convert.ToSingle(50),
                           Convert.ToSingle(50),
                           Convert.ToSingle(500),
                           MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                           MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                           false, y);

            Motion_Function.MXP_MC_MoveAbsolute(1,
                          1,
                          Convert.ToSingle(100),
                          Convert.ToSingle(offset_Z.Text),
                          Convert.ToSingle(50),
                          Convert.ToSingle(50),
                          Convert.ToSingle(500),
                          MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                          MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                          false, y);

            log.AppendText("Zero Return\r");
            log.ScrollToEnd();


        }
        private void connect_device_Click(object sender, RoutedEventArgs e)
        {

            kernelTimer.Tick += new EventHandler(KernelTimer_Tick);
            kernelTimer.Interval = TimeSpan.FromMilliseconds(0.01);

            ProcState = (UInt16)MXP_KernelState.Init;
            kernelTimer.Start();
            Robot_Status = Convert.ToString(Motion_Function.MXP_MC_ReadAxisStatus(2));

        }
        #endregion
        #region 3Dmove Arrow
        private void move_CameraUp_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVERELATIVE_IN x = new MXP.MXP_MOVERELATIVE_IN { };
            MXP.MXP_MOVERELATIVE_OUT y = new MXP.MXP_MOVERELATIVE_OUT { };

            Motion_Function.MXP_MC_MoveRelative(1,
                                    1,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(align_Z_Manual.Text),
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

            Motion_Function.MXP_MC_MoveRelative(1,
                                    1,
                                    Convert.ToSingle(100),
                                    -Convert.ToSingle(align_Z_Manual.Text),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    false, y);
        }

        private void move_up_Click(object sender, RoutedEventArgs e)
        {
            MXP.MXP_MOVERELATIVE_IN x = new MXP.MXP_MOVERELATIVE_IN { };
            MXP.MXP_MOVERELATIVE_OUT y = new MXP.MXP_MOVERELATIVE_OUT { };

            Motion_Function.MXP_MC_MoveRelative(0,
                                    0,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(align_Y_Manual.Text),
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

            Motion_Function.MXP_MC_MoveRelative(0,
                                    0,
                                    Convert.ToSingle(100),
                                    -Convert.ToSingle(align_Y_Manual.Text),
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

            Motion_Function.MXP_MC_MoveRelative(2,
                                    2,
                                    Convert.ToSingle(100),
                                    -Convert.ToSingle(align_X_Manual.Text),
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

            Motion_Function.MXP_MC_MoveRelative(2,
                                    2,
                                    Convert.ToSingle(100),
                                    Convert.ToSingle(align_X_Manual.Text),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(50),
                                    Convert.ToSingle(500),
                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                    false, y);
        }
        #endregion\
        #region Json_Info
        private void art_Add_Click(object sender, RoutedEventArgs e)
        {
            ScanProgram.ArtAdd artAddWindow = new ScanProgram.ArtAdd();

            artAddWindow.Show();
            artAddWindow.Top = this.Top + (this.ActualHeight - artAddWindow.Height) / 2;
            artAddWindow.Left = this.Left + (this.ActualWidth - artAddWindow.Width) / 2;

            log.AppendText("ArtAdd Windows Opened\r");
        }
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            makeSerialInit();

        }
        private void makeSerialInit()
        {
            // Ultra sonic CH1
            foreach (string comport in SerialPort.GetPortNames()) { UART_Port.Items.Add(comport); }
            UART_Port.SelectedIndex = 1; UART_Port.EndInit();

        }
        public static class PelcoD
        {
            // private readonly byte STX = 0xFF;
            private const byte STX = 0xFF;

            #region Pan and Tilt Commands
            #region Command1
            private const byte FocusNear = 0x01;
            private const byte IrisOpen = 0x02;
            private const byte IrisClose = 0x04;
            private const byte CameraOnOff = 0x08;
            private const byte AutoManualScan = 0x10;
            private const byte Sense = 0x80;
            #endregion

            #region Command2
            private const byte PanRight = 0x02;
            private const byte PanLeft = 0x04;
            private const byte TiltUp = 0x08;
            private const byte TiltDown = 0x10;
            private const byte ZoomTele = 0x20;
            private const byte ZoomWide = 0x40;
            private const byte FocusFar = 0x80;
            #endregion

            #region Data1
            private const byte PanSpeedMin = 0x00;
            private const byte PanSpeedMax = 0xFF;
            #endregion

            #region Data2
            private const byte TiltSpeedMin = 0x00;
            private const byte TiltSpeedMax = 0x3F;
            #endregion
            #endregion

            #region Enums
            public enum PresetAction { Set, Clear, Goto }
            public enum AuxAction { Set = 0x09, Clear = 0x0B }
            public enum Action { Start, Stop }
            public enum LensSpeed { Low = 0x00, Medium = 0x01, High = 0x02, Turbo = 0x03 }
            public enum PatternAction { Start, Stop, Run }
            public enum SwitchAction { Auto = 0x00, On = 0x01, Off = 0x02 }
            public enum Switch { On = 0x01, Off = 0x02 }
            public enum Focus { Near = FocusNear, Far = FocusFar }
            public enum Zoom { Wide = ZoomWide, Tele = ZoomTele }
            public enum Tilt { Up = TiltUp, Down = TiltDown }
            public enum Pan { Left = PanLeft, Right = PanRight }
            public enum Scan { Auto, Manual }
            public enum Iris { Open = IrisOpen, Close = IrisClose }
            #endregion

            #region Extended Command Set
            public static byte[] Preset(uint deviceAddress, byte preset, PresetAction action)
            {
                byte m_action;
                switch (action)
                {
                    case PresetAction.Set:
                        m_action = 0x03;
                        break;
                    case PresetAction.Clear:
                        m_action = 0x05;
                        break;
                    case PresetAction.Goto:
                        m_action = 0x07;
                        break;
                    default:
                        m_action = 0x03;
                        break;
                }
                return Message.GetMessage(deviceAddress, 0x00, m_action, 0x00, preset);
            }

            public static byte[] Flip(uint deviceAddress)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x07, 0x00, 0x21);
            }

            public static byte[] ZeroPanPosition(uint deviceAddress)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x07, 0x00, 0x22);
            }

            public static byte[] SetAuxiliary(uint deviceAddress, byte auxiliaryID, AuxAction action)
            {
                if (auxiliaryID < 0x00)
                    auxiliaryID = 0x00;
                else if (auxiliaryID > 0x08)
                    auxiliaryID = 0x08;
                return Message.GetMessage(deviceAddress, 0x00, (byte)action, 0x00, auxiliaryID);
            }

            public static byte[] RemoteReset(uint deviceAddress)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x0F, 0x00, 0x00);
            }
            public static byte[] Zone(uint deviceAddress, byte zone, Action action)
            {
                if (zone < 0x01 & zone > 0x08)
                    throw new Exception("Zone value should be between 0x01 and 0x08 include");
                byte m_action;
                if (action == Action.Start)
                    m_action = 0x11;
                else
                    m_action = 0x13;

                return Message.GetMessage(deviceAddress, 0x00, m_action, 0x00, zone);
            }

            public static byte[] WriteToScreen(uint deviceAddress, string text)
            {
                if (text.Length > 40)
                    text = text.Remove(40, text.Length - 40);
                System.Text.Encoding encoding = System.Text.Encoding.ASCII;
                byte[] m_bytes = new byte[encoding.GetByteCount(text) * 7];
                int i = 0;
                byte m_scrPosition;
                byte m_ASCIIchr;

                foreach (char ch in text)
                {
                    m_scrPosition = Convert.ToByte(i / 7);
                    m_ASCIIchr = Convert.ToByte(ch);
                    Array.Copy(Message.GetMessage(deviceAddress, 0x00, 0x15, m_scrPosition, m_ASCIIchr), 0, m_bytes, i, 7);
                    i = i + 7;
                }
                return m_bytes;
            }

            public static byte[] ClearScreen(uint deviceAddress)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x17, 0x00, 0x00);
            }

            public static byte[] AlarmAcknowledge(uint deviceAddress, uint alarmID)
            {
                if (alarmID < 1 & alarmID > 8)
                    throw new Exception("Only 8 alarms allowed for Pelco P implementation");
                return Message.GetMessage(deviceAddress, 0x00, 0x19, 0x00, Convert.ToByte(alarmID));
            }

            public static byte[] ZoneScan(uint deviceAddress, Action action)
            {
                byte m_action;
                if (action == Action.Start)
                    m_action = 0x1B;
                else
                    m_action = 0x1D;
                return Message.GetMessage(deviceAddress, 0x00, m_action, 0x00, 0x00);
            }

            public static byte[] Pattern(uint deviceAddress, PatternAction action)
            {
                byte m_action;
                switch (action)
                {
                    case PatternAction.Start:
                        m_action = 0x1F;
                        break;
                    case PatternAction.Stop:
                        m_action = 0x21;
                        break;
                    case PatternAction.Run:
                        m_action = 0x23;
                        break;
                    default:
                        m_action = 0x23;
                        break;
                }
                return Message.GetMessage(deviceAddress, 0x00, m_action, 0x00, 0x00);
            }

            public static byte[] SetZoomLensSpeed(uint deviceAddress, LensSpeed speed)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x25, 0x00, (byte)speed);
            }

            public static byte[] SetFocusLensSpeed(uint deviceAddress, LensSpeed speed)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x27, 0x00, (byte)speed);
            }

            public static byte[] ResetCamera(uint deviceAddress)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x29, 0x00, 0x00);
            }
            public static byte[] AutoFocus(uint deviceAddress, SwitchAction action)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x2B, 0x00, (byte)action);
            }
            public static byte[] AutoIris(uint deviceAddress, SwitchAction action)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x2D, 0x00, (byte)action);
            }
            public static byte[] AGC(uint deviceAddress, SwitchAction action)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x2F, 0x00, (byte)action);
            }
            public static byte[] BackLightCompensation(uint deviceAddress, Switch action)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x31, 0x00, (byte)action);
            }
            public static byte[] AutoWhiteBalance(uint deviceAddress, Switch action)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x33, 0x00, (byte)action);
            }

            public static byte[] EnableDevicePhaseDelayMode(uint deviceAddress)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x35, 0x00, 0x00);
            }
            public static byte[] SetShutterSpeed(uint deviceAddress, byte speed)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x37, speed, speed);//Not sure about
            }
            public static byte[] AdjustLineLockPhaseDelay(uint deviceAddress)
            {
                throw new Exception("Did not implemented");
                return Message.GetMessage(deviceAddress, 0x00, 0x39, 0x00, 0x00);
            }
            public static byte[] AdjustWhiteBalanceRB(uint deviceAddress)
            {
                throw new Exception("Did not implemented");
                return Message.GetMessage(deviceAddress, 0x00, 0x3B, 0x00, 0x00);
            }
            public static byte[] AdjustWhiteBalanceMG(uint deviceAddress)
            {
                throw new Exception("Did not implemented");
                return Message.GetMessage(deviceAddress, 0x00, 0x3D, 0x00, 0x00);
            }
            public static byte[] AdjustGain(uint deviceAddress)
            {
                throw new Exception("Did not implemented");
                return Message.GetMessage(deviceAddress, 0x00, 0x3F, 0x00, 0x00);
            }
            public static byte[] AdjustAutoIrisLevel(uint deviceAddress)
            {
                throw new Exception("Did not implemented");
                return Message.GetMessage(deviceAddress, 0x00, 0x41, 0x00, 0x00);
            }
            public static byte[] AdjustAutoIrisPeakValue(uint deviceAddress)
            {
                throw new Exception("Did not implemented");
                return Message.GetMessage(deviceAddress, 0x00, 0x43, 0x00, 0x00);
            }
            public static byte[] Query(uint deviceAddress)
            {
                throw new Exception("Did not implemented");
                return Message.GetMessage(deviceAddress, 0x00, 0x45, 0x00, 0x00);
            }
            #endregion

            #region Base Command Set

            public static byte[] CameraSwitch(uint deviceAddress, Switch action)
            {
                byte m_action = CameraOnOff;
                if (action == Switch.On)
                    m_action = CameraOnOff + Sense;
                return Message.GetMessage(deviceAddress, m_action, 0x00, 0x00, 0x00);

            }

            public static byte[] CameraIrisSwitch(uint deviceAddress, Iris action)
            {
                return Message.GetMessage(deviceAddress, (byte)action, 0x00, 0x00, 0x00);
            }

            public static byte[] CameraFocus(uint deviceAddress, Focus action)
            {
                if (action == Focus.Near)
                    return Message.GetMessage(deviceAddress, (byte)action, 0x00, 0x00, 0x00);
                else
                    return Message.GetMessage(deviceAddress, 0x00, (byte)action, 0x00, 0x00);
            }

            public static byte[] CameraZoom(uint deviceAddress, Zoom action)
            {
                return Message.GetMessage(deviceAddress, 0x00, (byte)action, 0x00, 0x00);
            }

            public static byte[] CameraTilt(uint deviceAddress, Tilt action, uint speed)
            {
                if (speed < TiltSpeedMin)
                    speed = TiltSpeedMin;
                if (speed < TiltSpeedMax)
                    speed = TiltSpeedMax;

                return Message.GetMessage(deviceAddress, 0x00, (byte)action, 0x00, (byte)speed);
            }

            public static byte[] CameraPan(uint deviceAddress, Pan action, uint speed)
            {
                if (speed < PanSpeedMin)
                    speed = PanSpeedMin;
                if (speed < PanSpeedMax)
                    speed = PanSpeedMax;

                return Message.GetMessage(deviceAddress, 0x00, (byte)action, (byte)speed, 0x00);
            }

            public static byte[] CameraPanTilt(uint deviceAddress, Pan panAction, uint panSpeed, Tilt tiltAction, uint tiltSpeed)
            {
                byte[] m_bytes = new byte[8];
                byte[] m_tiltMessage = CameraTilt(deviceAddress, tiltAction, tiltSpeed);
                byte[] m_panMessage = CameraPan(deviceAddress, panAction, panSpeed);
                /*m_bytes[0] = m_tiltMessage[0];
                m_bytes[1] = m_tiltMessage[1];
                m_bytes[2] = m_tiltMessage[2];
                m_bytes[3] = (byte)(m_tiltMessage[3]+m_panMessage[3]);
                m_bytes[4] = (byte)(m_tiltMessage[4]+m_panMessage[4]);
                m_bytes[5] = (byte)(m_tiltMessage[5]+m_panMessage[5]);
                m_bytes[6] = m_tiltMessage[6];
                m_bytes[7] = m_tiltMessage[7];*/
                m_bytes = Message.GetMessage(deviceAddress, 0x00, (byte)(m_tiltMessage[3] + m_panMessage[3]),
                    m_panMessage[4], m_tiltMessage[5]);
                return m_bytes;
            }

            public static byte[] CameraStop(uint deviceAddress)
            {
                return Message.GetMessage(deviceAddress, 0x00, 0x00, 0x00, 0x00);
            }

            public static byte[] CameraScan(uint deviceAddress, Scan scan)
            {
                byte m_byte = AutoManualScan;
                if (scan == Scan.Auto)
                    m_byte = AutoManualScan + Sense;

                return Message.GetMessage(deviceAddress, m_byte, 0x00, 0x00, 0x00);
            }
            #endregion

            public struct Message
            {
                public static byte Address;
                public static byte CheckSum;
                public static byte Command1, Command2, Data1, Data2;

                public static byte[] GetMessage(uint address, byte command1, byte command2, byte data1, byte data2)
                {
                    if (address < 1 & address > 256)
                        throw new Exception("Protocol Pelco D support 256 devices only");

                    Address = Byte.Parse((address).ToString());
                    Data1 = data1;
                    Data2 = data2;
                    Command1 = command1;
                    Command2 = command2;

                    CheckSum = (byte)(STX ^ Address ^ Command1 ^ Command2 ^ Data1 ^ Data2);

                    return new byte[] { STX, Address, Command1, Command2, Data1, Data2, CheckSum };
                }
            }
        }
        private void Uart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            if (Serial_Uart.IsOpen)
            {
                while (Serial_Uart.BytesToRead != 0)
                {
                    if (bytesreceived == 65534)
                    {
                        inputdata = new byte[65535];
                        bytesreceived = 0;
                    }
                    Serial_Uart.Read(inputdata, bytesreceived, 1);
                    bytesreceived++;
                }
                /* // getdata output
                // Report completion.
                bool uiAccess = TextData.Dispatcher.CheckAccess();
                string msg = System.BitConverter.ToString(inputdata, 0, bytesreceived);

                if (uiAccess)
                    TextData.Text += msg;
                else
                    TextData.Dispatcher.Invoke(() => { TextData.Text += msg; });

               */
                string msg_str = "";

                if (inputdata[0] == (byte)0xff)  // Sync Frame
                {
                    //checksum 
                    byte SyncByte = 0xff;
                    byte Address;
                    byte CheckSum;
                    
                    byte Command1, Command2, Data1, Data2;
                    Address = inputdata[1];
                    Data1 = inputdata[4];
                    Data2 = inputdata[5];
                    Command1 = inputdata[2];
                    Command2 = inputdata[3];
                    MXP.MXP_MOVEABSOLUTE_OUT y = new MXP.MXP_MOVEABSOLUTE_OUT { };

                    // CheckSum = (byte)(SyncByte ^ Address ^ Command1 ^ Command2 ^ Data1 ^ Data2);
                    // if (CheckSum == inputdata[6]) // Check sum OK
                    {
                        if (Command2 == 0x02)  // PanRight
                        {
                            msg_str = "PanRight : ";
                            int getdata = (Data2 << 8) + Data1;
                            string msg = getdata.ToString();
                            JoyStick_Speed = (Convert.ToSingle(msg)/63)*40 ;

                            ActPositionX = (Convert.ToSingle(ActPositionX) + 10).ToString();
                            if (Convert.ToSingle(ActPositionX) >= MAX_X)
                            {
                                ActPositionX = MAX_X.ToString();
                            }

                            Motion_Function.MXP_MC_MoveAbsolute(2,
                                                    2,
                                                    JoyStick_Speed,
                                                    Convert.ToSingle(ActPositionX),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(500),
                                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                                    MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                                    false, y);
                            
                            msg_str += msg;

                        }
                        else if (Command2 == 0x04)  // PanLeft 
                        {
                            msg_str = "PanLeft :";
                            int getdata = (Data2 << 8) + Data1;
                            string msg = getdata.ToString();
                            JoyStick_Speed = (Convert.ToSingle(msg) / 63) * 40;

                            ActPositionX = (Convert.ToSingle(ActPositionX) - 10).ToString();
                            if (Convert.ToSingle(ActPositionX) <0)
                            {
                                Convert.ToSingle(0).ToString();
                            }

                            Motion_Function.MXP_MC_MoveAbsolute(2,
                                                    2,
                                                    JoyStick_Speed,
                                                    Convert.ToSingle(ActPositionX),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(500),
                                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                                    MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                                    false, y);

                            msg_str += msg;
                        }
                        else if (Command2 == 0x08)  // TiltUp 
                        {
                            msg_str = "TiltUp :";

                            JoyStick_Speed = (Convert.ToSingle(Data2) / 63) * 40;
                            

                            ActPositionY = (Convert.ToSingle(ActPositionY) + 10).ToString();
                                if (Convert.ToSingle(ActPositionY) >= MAX_Y)
                                {
                                    ActPositionY = MAX_Y.ToString();
                                }
                                
                                Motion_Function.MXP_MC_MoveAbsolute(0,
                                                        0,
                                                        JoyStick_Speed,
                                                        Convert.ToSingle(ActPositionY),
                                                        Convert.ToSingle(50),
                                                        Convert.ToSingle(50),
                                                        Convert.ToSingle(500),
                                                        MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                                        MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                                        false, y);
                                int getdata = (Data2 << 8) + Data1;
                                string msg = getdata.ToString();
                                msg_str += msg;
                            
                        }
                        else if (Command2 == 0x10)  // TiltDown 
                        {
                            JoyStick_Speed = (Convert.ToSingle(Data2) / 63) * 40;


                            ActPositionY = (Convert.ToSingle(ActPositionY) - 10).ToString();
                            if (Convert.ToSingle(ActPositionY) < 0)
                            {
                                ActPositionY = Convert.ToSingle(0).ToString();
                            }

                            Motion_Function.MXP_MC_MoveAbsolute(0,
                                                    0,
                                                    JoyStick_Speed,
                                                    Convert.ToSingle(ActPositionY),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(500),
                                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                                    MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                                    false, y);
                            int getdata = (Data2 << 8) + Data1;
                            string msg = getdata.ToString();
                            msg_str += msg;
                        }
                        else if (Command2 == 0x20)  // ZoomTele 
                        {
                            msg_str = "ZoomTele";
                            JoyStick_Speed = 40;
                            
                            ActPositionZ = (Convert.ToSingle(ActPositionZ) + 1).ToString();
                            if (Convert.ToSingle(ActPositionZ) >= MAX_Z)
                            {
                                ActPositionZ = (Convert.ToSingle(ActPositionZ) + 1).ToString();
                            }
                            Motion_Function.MXP_MC_MoveAbsolute(1,
                                                    1,
                                                    JoyStick_Speed,
                                                    Convert.ToSingle(ActPositionZ),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(500),
                                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                                    MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                                    false, y);
                            //log.AppendText(JoyStick_Speed);
                        }
                        else if (Command2 == 0x40)  // ZoomWide 
                        {

                            JoyStick_Speed = 10;
                            
                            ActPositionZ = (Convert.ToSingle(ActPositionZ) - 1).ToString();
                            if (Convert.ToSingle(ActPositionZ) <= 0)
                            {
                                ActPositionZ = (Convert.ToSingle(ActPositionZ) + 1).ToString();
                            }
                            Motion_Function.MXP_MC_MoveAbsolute(1,
                                                    1,
                                                    JoyStick_Speed,
                                                    Convert.ToSingle(ActPositionZ),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(50),
                                                    Convert.ToSingle(500),
                                                    MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                                    MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                                    false, y);
                            log.Dispatcher.Invoke(() => { log.AppendText(msg_str); });
                            log.Dispatcher.Invoke(() => { log.ScrollToEnd(); });
                            msg_str = "ZoomWide";
                        }
                        else if (Command2 == 0x80)  // FocusFar 
                        {
                            msg_str = "FocusFar";
                        }
                        else if (Command1 == 0x01)  // FocusNear 
                        {
                            msg_str = "FocusNear";
                        }
                        else if (Command1 == 0x02)  // Iris Open 
                        {
                            msg_str = "Iris Open ";
                        }
                        else if (Command1 == 0x04)  // Iris Close 
                        {
                            msg_str = "Iris Close ";
                        }
                        
                    }
                }
                bool uiAccess = log.Dispatcher.CheckAccess();
                
                if (uiAccess)
                {
                     log.AppendText(msg_str+"\r");
                    log.AppendText(JoyStick_Speed.ToString()+"\r");
                    log.ScrollToEnd();
                }
                else
                {
                    log.Dispatcher.Invoke(() => { log.AppendText(msg_str + "\r"); });
                    log.Dispatcher.Invoke(() => { log.AppendText(JoyStick_Speed.ToString() + "\r"); });
                    log.Dispatcher.Invoke(() => { log.ScrollToEnd(); });
                }

                bytesreceived = 0;
            }
        }
        private void emergencyStop_Click(object sender, RoutedEventArgs e)
        {
            ProcState = (UInt16)MXP_KernelState.Close;
            log.AppendText("Emergency Stop\r");
            log.ScrollToEnd();
        }
        public void GetCurrentPosition(object sender, EventArgs e)
        {
            ActPositionX = (Motion_Function.MXP_MC_ReadActualPosition(2) - Convert.ToSingle(offset_X.Text)).ToString("F2");
            ActPositionY = (Motion_Function.MXP_MC_ReadActualPosition(0) - Convert.ToSingle(offset_Y.Text)).ToString("F2");
            ActPositionZ = (Motion_Function.MXP_MC_ReadActualPosition(1) - Convert.ToSingle(offset_Z.Text)).ToString("F2");

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
                            log.ScrollToEnd();
                        }
                        else
                        {
                            ProcState = (UInt16)MXP_KernelState.Idle;
                            log.AppendText("Fail to initialize MXP!!!\r");
                            log.ScrollToEnd();
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
                            log.ScrollToEnd();
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
                            log.ScrollToEnd();
                        }
                        else
                        {
                            log.AppendText("Fail to reset MXP!!!\r");
                            log.ScrollToEnd();
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
                                log.ScrollToEnd();
                            }
                            else
                            {
                                log.AppendText("Fail to stop MXP!!!\r");
                                log.ScrollToEnd();
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
                            log.ScrollToEnd();
                        }
                        else
                        {
                            log.AppendText("Fail to close MXP!!!\r");
                            log.ScrollToEnd();
                        }
                        break;
                    }
            }
        }
        public void auto_capture_method(object sender, EventArgs e)
        {
            fileinfo_header = "ART4";
            MXP.MXP_MOVEABSOLUTE_OUT x = new MXP.MXP_MOVEABSOLUTE_OUT { };
            MXP.MXP_MOVEABSOLUTE_OUT y = new MXP.MXP_MOVEABSOLUTE_OUT { };
            MXP.MXP_READSTATUS_IN In_X;
            MXP.MXP_READSTATUS_OUT Out_X;
            MXP.MXP_READSTATUS_IN In_Y;
            MXP.MXP_READSTATUS_OUT Out_Y;

            int status_x = 0;
            int status_y = 0;

            In_X.Axis.AxisNo = 2;
            In_X.Enable = 1;
            In_Y.Axis.AxisNo = 0;
            In_Y.Enable = 1;

            status_x = Motion_Function.MXP_MC_ReadAxisStatus(2);
            status_y = Motion_Function.MXP_MC_ReadAxisStatus(0);

            MXP.MXP_ReadStatus(ref In_X, out Out_X);
            MXP.MXP_ReadStatus(ref In_Y, out Out_Y);
            if(capture_Lock ==0)
            {
                if (Out_X.Standstill == 1)
                {
                    if (CameraMod == 4)
                    {

                        cap_nir.Snap(CountY, CountX, "ddd");
                    }

                    capture_Lock = 1;
                }
                
            }
            else if (capture_Lock == 1)
            {
                if (Auto_Zero == 0)
                {
                    Motion_Function.MXP_MC_MoveAbsolute(2,
                                        2,
                                        Convert.ToSingle(100),
                                        Convert.ToSingle(start_X.Text) + Convert.ToSingle(offset_X.Text),
                                        Convert.ToSingle(50),
                                        Convert.ToSingle(50),
                                        Convert.ToSingle(500),
                                        MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                        MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                        false, y);
                    Motion_Function.MXP_MC_MoveAbsolute(0,
                                            0,
                                            Convert.ToSingle(100),
                                            Convert.ToSingle(start_Y.Text) + Convert.ToSingle(offset_Y.Text),
                                            Convert.ToSingle(50),
                                            Convert.ToSingle(50),
                                            Convert.ToSingle(500),
                                            MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                            MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                            false, y);

                    positionX = Convert.ToSingle(start_X.Text) + Convert.ToSingle(offset_X.Text);
                    positionY = Convert.ToSingle(start_Y.Text) + Convert.ToSingle(offset_Y.Text);
                    Auto_Zero = 1;
                    capture_Lock = 0;
                }
                else if (Auto_Zero == 1)
                {
                    if (positionX < Convert.ToSingle(finish_X.Text) + Convert.ToSingle(offset_X.Text))
                    {
                        positionX = positionX + Convert.ToSingle(align_X.Text);
                        Motion_Function.MXP_MC_MoveAbsolute(2,
                                       2,
                                       Convert.ToSingle(100),
                                       Convert.ToSingle(positionX),
                                       Convert.ToSingle(50),
                                       Convert.ToSingle(50),
                                       Convert.ToSingle(500),
                                       MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                       MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                       false, y);
                        CountX++;
                        capture_Lock = 0;
                    }
                    else if (positionY > Convert.ToSingle(finish_Y.Text) + Convert.ToSingle(offset_Y.Text))
                    {
                        CountX = 1;
                        CountY++;
                        Motion_Function.MXP_MC_MoveAbsolute(2,
                                      2,
                                      Convert.ToSingle(100),
                                       Convert.ToSingle(start_X.Text) + Convert.ToSingle(offset_X.Text),
                                      Convert.ToSingle(50),
                                      Convert.ToSingle(50),
                                      Convert.ToSingle(500),
                                      MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                      MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                      false, y);
                        positionY = positionY - Convert.ToSingle(align_Y.Text);
                        Motion_Function.MXP_MC_MoveAbsolute(0,
                                       0,
                                       Convert.ToSingle(100),
                                       Convert.ToSingle(positionY),
                                       Convert.ToSingle(50),
                                       Convert.ToSingle(50),
                                       Convert.ToSingle(500),
                                       MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                       MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                       false, y);
                        positionX = Convert.ToSingle(start_X.Text) + Convert.ToSingle(offset_X.Text);
                    }
                    else if (positionX >= Convert.ToSingle(finish_X.Text) + Convert.ToSingle(offset_X.Text) && positionY <= Convert.ToSingle(finish_Y.Text) + Convert.ToSingle(offset_Y.Text))
                    {
                        Motion_Function.MXP_MC_MoveAbsolute(2,
                                        2,
                                        Convert.ToSingle(100),
                                        Convert.ToSingle(start_X.Text) + Convert.ToSingle(offset_X.Text),
                                        Convert.ToSingle(50),
                                        Convert.ToSingle(50),
                                        Convert.ToSingle(500),
                                        MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                        MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                        false, y);
                        Motion_Function.MXP_MC_MoveAbsolute(0,
                                                0,
                                                Convert.ToSingle(100),
                                                Convert.ToSingle(start_Y.Text) + Convert.ToSingle(offset_Y.Text),
                                                Convert.ToSingle(50),
                                                Convert.ToSingle(50),
                                                Convert.ToSingle(500),
                                                MXP.MXP_BUFFERMODE_ENUM.MXP_ABORTING,
                                                MXP.MXP_DIRECTION_ENUM.MXP_POSITIVE_DIRECTION,
                                                false, y);
                        Auto_Zero = 0;
                        CountX = 1;
                        CountY = 1;
                        grab_pic.Stop();
                        log.AppendText("Auto Capture Finished\r");
                        log.ScrollToEnd();



                    }
                }

            }
        }
        private void auto_Capture_Click(object sender, RoutedEventArgs e)
        {
            grab_pic.Tick += new EventHandler(auto_capture_method);
            grab_pic.Interval = TimeSpan.FromMilliseconds(1000);

            grab_pic.Start();
            log.AppendText("Auto Capture Start\r");
            log.ScrollToEnd();
        }
        #region Grab Method For Cameras
        private void grabRGB(object sender, EventArgs e)
        {
            try
            {
                if (cap.img != null)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        cap.img.Save(memory, ImageFormat.Bmp);
                        memory.Position = 0;
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        view_box.Source = bitmapImage;
                    }
                }

            }
            catch (Exception ex)
            {
                log.AppendText(ex.ToString());
                log.ScrollToEnd();
            }
        }
        private void grabNIR(object sender, EventArgs e)
        {
            try
            {
                if (cap_nir.img != null)
                {
                    using (MemoryStream memory = new MemoryStream())
                    {
                        cap_nir.img.Save(memory, ImageFormat.Bmp);
                        memory.Position = 0;
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        view_box.Source = bitmapImage;
                    }
                }

            }
            catch (Exception ex)
            {
                log.AppendText(ex.ToString());
                log.ScrollToEnd();
            }
        }
        private void grabUV(object sender, EventArgs e)
        {
            int err = 0;
            int size;
            System.IntPtr evhandle;
            Bitmap imagebmp;
            UIntPtr buf;
            bool bauto = true;              // set this to true to get auto min max
            UInt16 width = 0;
            UInt16 height = 0;
            UInt16 widthmax = 0;
            UInt16 heightmax = 0;
            int ishift = 16 - pcoDescr.wDynResDESC;
            int ipadd = width / 4;
            int iconvertcol = pcoDescr.wColorPatternDESC / 0x1000;
            int max;
            int min;
            int ival;
            ipadd *= 4;
            ipadd = width - ipadd;

            err = PCO_SDK_LibWrapper.PCO_GetSizes(cameraHandle, ref width, ref height, ref widthmax, ref heightmax);
            size = width * height * 2;

            buf = UIntPtr.Zero;
            evhandle = IntPtr.Zero;
            if ((bufwidth != width) || (bufheight != height))
            {
                if (bufnr != -1)
                {
                    PCO_SDK_LibWrapper.PCO_FreeBuffer(cameraHandle, bufnr);
                }
                bufnr = -1;
                imagedata = new byte[(width + ipadd) * height * 3];

                err = PCO_SDK_LibWrapper.PCO_AllocateBuffer(cameraHandle, ref bufnr, size, ref buf, ref evhandle);
                if (err == 0)
                {
                    bufwidth = width;
                    bufheight = height;
                }
            }
            else
                err = PCO_SDK_LibWrapper.PCO_GetBuffer(cameraHandle, bufnr, ref buf, ref evhandle);

            //Mandatory for Cameralink and GigE. Don't care for all other interfaces, so leave it intact here.

            err = PCO_SDK_LibWrapper.PCO_AddBufferEx(cameraHandle, 0, 0, bufnr, (UInt16)width, (UInt16)height, (UInt16)pcoDescr.wDynResDESC);

            // There are two possibilities to synch. with the camera. Either by polling or by event.
            // To use polling uncomment the Polling Block and comment the Event Block
            // Begin Polling Block
            // UInt32 dwStatusDll = 0, dwStatusDrv = 0;
            // do
            // {
            //   err = PCO_SDK_LibWrapper.PCO_GetBufferStatus(cameraHandle, bufnr, ref dwStatusDll, ref dwStatusDrv);
            // } while ((dwStatusDll & 0x8000) == 0);
            // End Polling Block

            //// Begin Event Block
            //bool bImageIsOk = false;
            //uint res = WaitForSingleObject(evhandle, 3000);
            //if (res == 0)
            //{
            //    bImageIsOk = true;
            //}
            //if (!bImageIsOk)
            //    return;
            // End Event Block

            unsafe
            {
                Int16* bufi = (Int16*)buf.ToPointer();
                max = 1500;
                min = 100;
                for (int i = 10 * width; i < height * width; i++)
                {
                    if (bufi[i] > max)
                        max = bufi[i];
                    if (bufi[i] < min)
                        min = bufi[i];
                }
                max >>= ishift;
                min >>= ishift;
                if (max <= min)
                    max = min + 1;
            }
            PCO_Convert_LibWrapper.PCO_Convert16TOCOL(convertHandle, 0, iconvertcol, width, height,
                buf, imagedata);

            if ((convertDialog != IntPtr.Zero) && (convertHandle != IntPtr.Zero))
            {
                PCO_Convert_LibWrapper.PCO_SetDataToDialog(convertDialog, width, height, buf, imagedata);
            }

            if (bauto)
            {
                PCO_ConvertStructures.PCO_Display strDisplay = new PCO_ConvertStructures.PCO_Display(1);

                strDisplay.wSize = (ushort)Marshal.SizeOf(typeof(PCO_ConvertStructures.PCO_Display));

                err = PCO_Convert_LibWrapper.PCO_ConvertGetDisplay(convertHandle, ref strDisplay);
                strDisplay.iScale_min = min;
                strDisplay.iScale_max = max;

                err = PCO_Convert_LibWrapper.PCO_ConvertSetDisplay(convertHandle, ref strDisplay);
                err = PCO_Convert_LibWrapper.PCO_SetConvertDialog(convertDialog, convertHandle);
            }

            imagebmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle dimension = new Rectangle(0, 0, imagebmp.Width, imagebmp.Height);
            BitmapData picData = imagebmp.LockBits(dimension, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr pixelStartAddress = picData.Scan0;

            //Copy the pixel data into the bitmap structure
            System.Runtime.InteropServices.Marshal.Copy(imagedata, 0, pixelStartAddress, imagedata.Length);

            //[uv image save]

            //

            imagebmp.UnlockBits(picData);

            using (MemoryStream memory = new MemoryStream())
            {
                imagebmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                view_box.Source = bitmapImage;
            }

        }
        #endregion
        #region Snap Method For Cameras
        private void snapUV(int ii, int xx, string y)
        {
            int err = 0;
            int size;
            String fileName = ii.ToString("D2") + "_" + xx.ToString("D2") + "_" + y;
            System.IntPtr evhandle;
            Bitmap imagebmp;
            UIntPtr buf;
            bool bauto = true;              // set this to true to get auto min max
            UInt16 width = 0;
            UInt16 height = 0;
            UInt16 widthmax = 0;
            UInt16 heightmax = 0;
            int ishift = 16 - pcoDescr.wDynResDESC;
            int ipadd = width / 4;
            int iconvertcol = pcoDescr.wColorPatternDESC / 0x1000;
            int max;
            int min;
            int ival;
            ipadd *= 4;
            ipadd = width - ipadd;

            err = PCO_SDK_LibWrapper.PCO_GetSizes(cameraHandle, ref width, ref height, ref widthmax, ref heightmax);
            size = width * height * 2;

            buf = UIntPtr.Zero;
            evhandle = IntPtr.Zero;
            if ((bufwidth != width) || (bufheight != height))
            {
                if (bufnr != -1)
                {
                    PCO_SDK_LibWrapper.PCO_FreeBuffer(cameraHandle, bufnr);
                }
                bufnr = -1;
                imagedata = new byte[(width + ipadd) * height * 3];

                err = PCO_SDK_LibWrapper.PCO_AllocateBuffer(cameraHandle, ref bufnr, size, ref buf, ref evhandle);
                if (err == 0)
                {
                    bufwidth = width;
                    bufheight = height;
                }
            }
            else
                err = PCO_SDK_LibWrapper.PCO_GetBuffer(cameraHandle, bufnr, ref buf, ref evhandle);

            //Mandatory for Cameralink and GigE. Don't care for all other interfaces, so leave it intact here.

            err = PCO_SDK_LibWrapper.PCO_AddBufferEx(cameraHandle, 0, 0, bufnr, (UInt16)width, (UInt16)height, (UInt16)pcoDescr.wDynResDESC);

            // There are two possibilities to synch. with the camera. Either by polling or by event.
            // To use polling uncomment the Polling Block and comment the Event Block
            // Begin Polling Block
            // UInt32 dwStatusDll = 0, dwStatusDrv = 0;
            // do
            // {
            //   err = PCO_SDK_LibWrapper.PCO_GetBufferStatus(cameraHandle, bufnr, ref dwStatusDll, ref dwStatusDrv);
            // } while ((dwStatusDll & 0x8000) == 0);
            // End Polling Block

            //// Begin Event Block
            //bool bImageIsOk = false;
            //uint res = WaitForSingleObject(evhandle, 3000);
            //if (res == 0)
            //{
            //    bImageIsOk = true;
            //}
            //if (!bImageIsOk)
            //    return;
            // End Event Block

            unsafe
            {
                Int16* bufi = (Int16*)buf.ToPointer();
                max = 1500;
                min = 100;
                for (int i = 10 * width; i < height * width; i++)
                {
                    if (bufi[i] > max)
                        max = bufi[i];
                    if (bufi[i] < min)
                        min = bufi[i];
                }
                max >>= ishift;
                min >>= ishift;
                if (max <= min)
                    max = min + 1;
            }
            PCO_Convert_LibWrapper.PCO_Convert16TOCOL(convertHandle, 0, iconvertcol, width, height,
                buf, imagedata);

            if ((convertDialog != IntPtr.Zero) && (convertHandle != IntPtr.Zero))
            {
                PCO_Convert_LibWrapper.PCO_SetDataToDialog(convertDialog, width, height, buf, imagedata);
            }

            if (bauto)
            {
                PCO_ConvertStructures.PCO_Display strDisplay = new PCO_ConvertStructures.PCO_Display(1);

                strDisplay.wSize = (ushort)Marshal.SizeOf(typeof(PCO_ConvertStructures.PCO_Display));

                err = PCO_Convert_LibWrapper.PCO_ConvertGetDisplay(convertHandle, ref strDisplay);
                strDisplay.iScale_min = min;
                strDisplay.iScale_max = max;

                err = PCO_Convert_LibWrapper.PCO_ConvertSetDisplay(convertHandle, ref strDisplay);
                err = PCO_Convert_LibWrapper.PCO_SetConvertDialog(convertDialog, convertHandle);
            }

            imagebmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle dimension = new Rectangle(0, 0, imagebmp.Width, imagebmp.Height);
            BitmapData picData = imagebmp.LockBits(dimension, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr pixelStartAddress = picData.Scan0;

            //Copy the pixel data into the bitmap structure
            System.Runtime.InteropServices.Marshal.Copy(imagedata, 0, pixelStartAddress, imagedata.Length);

            //[uv image save]
            imagebmp.Save(fileName + ".tiff", ImageFormat.Tiff);
            //



        }
        private void Camera_Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Radio_Camera_RGB_Camera.IsChecked == true)
            {
                if (cap == null)
                {
                    cap = new SaperaCapture();
                    if (cap.location == null)
                    {
                        log.AppendText("Camera연결에 실패했습니다\r");
                        log.ScrollToEnd();
                    }
                }

                CameraMod = 1;
                //cap.RGB_Cameara_Init();
                Radio_Camera_RGB_Camera.IsEnabled = false;
                Radio_Camera_VNIR_Camera.IsEnabled = false;
                Radio_Camera_UV_Camera.IsEnabled = false;
                Radio_Camera_SWIR_Camera.IsEnabled = false;
                Radio_Camera_NIR_Camera.IsEnabled = false;

                button_cali_WB.IsEnabled = true;
                button_cali_Light.IsEnabled = true;
                button_cali_Lens.IsEnabled = true;
                button_cali_Color.IsEnabled = true;

                snap_pic.Tick += new EventHandler(grabRGB);
                snap_pic.Interval = TimeSpan.FromMilliseconds(100);
                snap_pic.Start();

            }
            else if (Radio_Camera_VNIR_Camera.IsChecked == true)
            {
                CameraMod = 2;
                //SnapScan snapScan = new SnapScan();
                string path_snapscan_file = Environment.CurrentDirectory + "\\Snapscan\\C100U-0021.xml";
                byte[] bytes = Encoding.ASCII.GetBytes(path_snapscan_file);
                sbyte[] sbytes = new sbyte[bytes.Length];
                unsafe
                {
                    fixed (sbyte* sbyte_point = sbytes)
                    {
                        sbyte* sbyte_pointer = sbyte_point;
                        // snapScan.Example_CubeAcquisition_MinimumRequired(sbyte_point);
                    }

                }
            }
            else if (Radio_Camera_UV_Camera.IsChecked == true)
            {
                CameraMod = 3;
                Radio_Camera_RGB_Camera.IsEnabled = false;
                Radio_Camera_VNIR_Camera.IsEnabled = false;
                Radio_Camera_UV_Camera.IsEnabled = false;
                Radio_Camera_SWIR_Camera.IsEnabled = false;
                snap_pic.Tick -= new EventHandler(grabUV);
                snap_pic.Interval = TimeSpan.FromMilliseconds(100);
                snap_pic.Start();
            }
            else if (Radio_Camera_NIR_Camera.IsChecked == true)
            {
                if (cap_nir == null)
                {
                    cap_nir = new SaperaCapture_Nir();
                }
                CameraMod = 4;
                Radio_Camera_RGB_Camera.IsEnabled = false;
                Radio_Camera_VNIR_Camera.IsEnabled = false;
                Radio_Camera_UV_Camera.IsEnabled = false;
                Radio_Camera_SWIR_Camera.IsEnabled = false;
                Radio_Camera_NIR_Camera.IsEnabled = false;

                snap_pic.Tick -= new EventHandler(grabNIR);
                snap_pic.Interval = TimeSpan.FromMilliseconds(100);
                snap_pic.Start();
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

                pcoCameraType = new PCO_CameraType();
                pcoDescr = new PCO_Description();
                pcoStorage = new PCO_Storage();
                pcoImage = new PCO_Image();

                pcoDescr.wSize = (ushort)Marshal.SizeOf(pcoDescr);
                pcoStorage.wSize = (ushort)Marshal.SizeOf(pcoStorage);
                pcoImage.wSize = (ushort)Marshal.SizeOf(pcoImage);


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

                err = PCO_SDK_LibWrapper.PCO_GetCameraName(cameraHandle, szCameraName, 30);
                cameraname = System.Text.Encoding.Default.GetString(szCameraName);

                Setupconvert();



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




            else if (cap_nir == null && CameraMod == 4)
            {
                cap_nir = new SaperaCapture_Nir();
            }
            log.AppendText(CameraMod.ToString());
            log.ScrollToEnd();
        }
        private void Camera_Disconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CameraMod == 1)
            {
                cap.DestroyObjects();
                cap.DisposeObjects();
                snap_pic.Stop();
                view_box.Source = null;
            }
            else if (CameraMod == 3)
            {

            }
            else if (CameraMod == 4)
            {
                cap_nir.DestroyObjects();
                cap_nir.DisposeObjects();
                snap_pic.Stop();
                view_box.Source = null;
            }
            Radio_Camera_RGB_Camera.IsEnabled = true;
            Radio_Camera_VNIR_Camera.IsEnabled = true;
            Radio_Camera_UV_Camera.IsEnabled = true;
            Radio_Camera_SWIR_Camera.IsEnabled = true;
            Radio_Camera_NIR_Camera.IsEnabled = true;

        }
        #endregion
        private void Setupconvert()
        {
            pcoDescr = new PCO_Description();
            pcoDescr.wSize = (ushort)Marshal.SizeOf(pcoDescr);
            int err = 0;

            err = PCO_SDK_LibWrapper.PCO_GetCameraDescription(cameraHandle, ref pcoDescr);
            PCO_ConvertStructures.PCO_SensorInfo strsensorinf = new PCO_ConvertStructures.PCO_SensorInfo();
            PCO_ConvertStructures.PCO_Display strDisplay = new PCO_ConvertStructures.PCO_Display();
            strsensorinf.wSize = (ushort)Marshal.SizeOf(strsensorinf);
            strDisplay.wSize = (ushort)Marshal.SizeOf(strDisplay);
            strsensorinf.wDummy = 0;
            strsensorinf.iConversionFactor = 0;
            strsensorinf.iDataBits = pcoDescr.wDynResDESC;
            strsensorinf.iSensorInfoBits = 1;
            strsensorinf.iDarkOffset = 100;
            strsensorinf.dwzzDummy0 = 0;
            strsensorinf.strColorCoeff.da11 = 1.0;
            strsensorinf.strColorCoeff.da12 = 0.0;
            strsensorinf.strColorCoeff.da13 = 0.0;
            strsensorinf.strColorCoeff.da21 = 0.0;
            strsensorinf.strColorCoeff.da22 = 1.0;
            strsensorinf.strColorCoeff.da23 = 0.0;
            strsensorinf.strColorCoeff.da31 = 0.0;
            strsensorinf.strColorCoeff.da32 = 0.0;
            strsensorinf.strColorCoeff.da33 = 1.0;
            strsensorinf.iCamNum = 0;
            strsensorinf.hCamera = cameraHandle;

            int errorCode;
            /* We created a pointer to a convert object here */
            errorCode = PCO_Convert_LibWrapper.PCO_ConvertCreate(ref convertHandle, ref strsensorinf, PCO_Convert_LibWrapper.PCO_COLOR_CONVERT);

            PCO_ConvertStructures.PCO_Convert pcoConv = new PCO_ConvertStructures.PCO_Convert(); ;

            pcoConv.wSize = (ushort)Marshal.SizeOf(pcoConv);
            errorCode = PCOConvertDll.PCO_Convert_LibWrapper.PCO_ConvertGet(convertHandle, ref pcoConv);
            pcoConv.wSize = (ushort)Marshal.SizeOf(pcoConv);

            IntPtr debugIntPtr = convertHandle;
            PCO_ConvertStructures.PCO_Convert pcoConvertlocal = (PCO_ConvertStructures.PCO_Convert)Marshal.PtrToStructure(debugIntPtr, typeof(PCO_ConvertStructures.PCO_Convert));

        }
        #region Calibration
        private void UV_Camera_Grab(object sender, RoutedEventArgs e)
        {


            int err = 0;
            int size;
            System.IntPtr evhandle;
            Bitmap imagebmp;
            UIntPtr buf;
            bool bauto = true;              // set this to true to get auto min max
            UInt16 width = 0;
            UInt16 height = 0;
            UInt16 widthmax = 0;
            UInt16 heightmax = 0;
            int ishift = 16 - pcoDescr.wDynResDESC;
            int ipadd = width / 4;
            int iconvertcol = pcoDescr.wColorPatternDESC / 0x1000;
            int max;
            int min;
            int ival;
            ipadd *= 4;
            ipadd = width - ipadd;

            err = PCO_SDK_LibWrapper.PCO_GetSizes(cameraHandle, ref width, ref height, ref widthmax, ref heightmax);
            size = width * height * 2;

            buf = UIntPtr.Zero;
            evhandle = IntPtr.Zero;
            if ((bufwidth != width) || (bufheight != height))
            {
                if (bufnr != -1)
                {
                    PCO_SDK_LibWrapper.PCO_FreeBuffer(cameraHandle, bufnr);
                }
                bufnr = -1;
                imagedata = new byte[(width + ipadd) * height * 3];

                err = PCO_SDK_LibWrapper.PCO_AllocateBuffer(cameraHandle, ref bufnr, size, ref buf, ref evhandle);
                if (err == 0)
                {
                    bufwidth = width;
                    bufheight = height;
                }
            }
            else
                err = PCO_SDK_LibWrapper.PCO_GetBuffer(cameraHandle, bufnr, ref buf, ref evhandle);

            //Mandatory for Cameralink and GigE. Don't care for all other interfaces, so leave it intact here.

            err = PCO_SDK_LibWrapper.PCO_AddBufferEx(cameraHandle, 0, 0, bufnr, (UInt16)width, (UInt16)height, (UInt16)pcoDescr.wDynResDESC);

            // There are two possibilities to synch. with the camera. Either by polling or by event.
            // To use polling uncomment the Polling Block and comment the Event Block
            // Begin Polling Block
            // UInt32 dwStatusDll = 0, dwStatusDrv = 0;
            // do
            // {
            //   err = PCO_SDK_LibWrapper.PCO_GetBufferStatus(cameraHandle, bufnr, ref dwStatusDll, ref dwStatusDrv);
            // } while ((dwStatusDll & 0x8000) == 0);
            // End Polling Block

            //// Begin Event Block
            //bool bImageIsOk = false;
            //uint res = WaitForSingleObject(evhandle, 3000);
            //if (res == 0)
            //{
            //    bImageIsOk = true;
            //}
            //if (!bImageIsOk)
            //    return;
            // End Event Block

            unsafe
            {
                Int16* bufi = (Int16*)buf.ToPointer();
                max = 1500;
                min = 100;
                for (int i = 10 * width; i < height * width; i++)
                {
                    if (bufi[i] > max)
                        max = bufi[i];
                    if (bufi[i] < min)
                        min = bufi[i];
                }
                max >>= ishift;
                min >>= ishift;
                if (max <= min)
                    max = min + 1;
            }
            PCO_Convert_LibWrapper.PCO_Convert16TOCOL(convertHandle, 0, iconvertcol, width, height,
                buf, imagedata);

            if ((convertDialog != IntPtr.Zero) && (convertHandle != IntPtr.Zero))
            {
                PCO_Convert_LibWrapper.PCO_SetDataToDialog(convertDialog, width, height, buf, imagedata);
            }

            if (bauto)
            {
                PCO_ConvertStructures.PCO_Display strDisplay = new PCO_ConvertStructures.PCO_Display(1);

                strDisplay.wSize = (ushort)Marshal.SizeOf(typeof(PCO_ConvertStructures.PCO_Display));

                err = PCO_Convert_LibWrapper.PCO_ConvertGetDisplay(convertHandle, ref strDisplay);
                strDisplay.iScale_min = min;
                strDisplay.iScale_max = max;

                err = PCO_Convert_LibWrapper.PCO_ConvertSetDisplay(convertHandle, ref strDisplay);
                err = PCO_Convert_LibWrapper.PCO_SetConvertDialog(convertDialog, convertHandle);
            }

            imagebmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle dimension = new Rectangle(0, 0, imagebmp.Width, imagebmp.Height);
            BitmapData picData = imagebmp.LockBits(dimension, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr pixelStartAddress = picData.Scan0;

            //Copy the pixel data into the bitmap structure
            System.Runtime.InteropServices.Marshal.Copy(imagedata, 0, pixelStartAddress, imagedata.Length);

            //[uv image save]
            imagebmp.Save("c:\\test\\button.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //

            imagebmp.UnlockBits(picData);

            using (MemoryStream memory = new MemoryStream())
            {
                imagebmp.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                view_box.Source = bitmapImage;
            }




        }

        private void cali_WB_Click(object sender, RoutedEventArgs e)
        {
            ScanProgram.Calibration.WhiteBalance wb_Window = new ScanProgram.Calibration.WhiteBalance();

            wb_Window.Show();
            wb_Window.Top = this.Top + (this.ActualHeight - wb_Window.Height) / 2;
            wb_Window.Left = this.Left + (this.ActualWidth - wb_Window.Width) / 2;
            log.AppendText("WhiteBalance Window Opened\r");
            log.ScrollToEnd();
        }

        private void button_cali_Light_Click(object sender, RoutedEventArgs e)
        {
            ScanProgram.Calibration.LightControl lc_Window = new ScanProgram.Calibration.LightControl();

            lc_Window.Show();
            lc_Window.Top = this.Top + (this.ActualHeight - lc_Window.Height) / 2;
            lc_Window.Left = this.Left + (this.ActualWidth - lc_Window.Width) / 2;
            log.AppendText("LightControl Window Opened\r");
            log.ScrollToEnd();
        }

        private void button_cali_Lens_Click(object sender, RoutedEventArgs e)
        {
            ScanProgram.Calibration.Lens lens_Window = new ScanProgram.Calibration.Lens();

            lens_Window.Show();
            lens_Window.Top = this.Top + (this.ActualHeight - lens_Window.Height) / 2;
            lens_Window.Left = this.Left + (this.ActualWidth - lens_Window.Width) / 2;
            log.AppendText("Lens Calibration Window Opened\r");
            log.ScrollToEnd();
        }

        private void button_cali_Color_Click(object sender, RoutedEventArgs e)
        {
            ScanProgram.Calibration.Color color_Window = new ScanProgram.Calibration.Color();

            color_Window.Show();
            color_Window.Top = this.Top + (this.ActualHeight - color_Window.Height) / 2;
            color_Window.Left = this.Left + (this.ActualWidth - color_Window.Width) / 2;
            log.AppendText("Lens Calibration Window Opened\r");
            log.ScrollToEnd();
        }
        #endregion
        private void setOffsetCurrentPos_Click(object sender, RoutedEventArgs e)
        {
            offset_X.Text = Motion_Function.MXP_MC_ReadActualPosition(2).ToString("F2");
            offset_Y.Text = Motion_Function.MXP_MC_ReadActualPosition(0).ToString("F2");
            offset_Z.Text = Motion_Function.MXP_MC_ReadActualPosition(1).ToString("F2");

            log.AppendText("Set Current Position to Offset\r");
            log.ScrollToEnd();
        }

        private void home_Click(object sender, RoutedEventArgs e)
        {

        }

        private void offset_X_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void setStartPoint_Click(object sender, RoutedEventArgs e)
        {
            start_X.Text = (Motion_Function.MXP_MC_ReadActualPosition(2) - Convert.ToSingle(offset_X.Text)).ToString("F2");
            start_Y.Text = (Motion_Function.MXP_MC_ReadActualPosition(0) - Convert.ToSingle(offset_Y.Text)).ToString("F2");
            start_Z.Text = (Motion_Function.MXP_MC_ReadActualPosition(1) - Convert.ToSingle(offset_Z.Text)).ToString("F2");

            log.AppendText("Set Current Position to StartPoint\r");
            log.ScrollToEnd();
        }

        private void setEndPoint_Click(object sender, RoutedEventArgs e)
        {
            finish_X.Text = (Motion_Function.MXP_MC_ReadActualPosition(2) - Convert.ToSingle(offset_X.Text)).ToString("F2");
            finish_Y.Text = (Motion_Function.MXP_MC_ReadActualPosition(0) - Convert.ToSingle(offset_Y.Text)).ToString("F2");
            finish_Z.Text = (Motion_Function.MXP_MC_ReadActualPosition(1) - Convert.ToSingle(offset_Z.Text)).ToString("F2");

            log.AppendText("Set Current Position to EndPoint\r");
            log.ScrollToEnd();

        }

        private void button_cali_Vignette_Click(object sender, RoutedEventArgs e)
        {

        }

        private void JoyStickOpen_Click(object sender, RoutedEventArgs e)
        {
            Serial_Uart.PortName = UART_Port.Text;       // Port Name
            Serial_Uart.BaudRate = (int)9600;

            Serial_Uart.DataBits = (int)8;
            Serial_Uart.Parity = Parity.None;
            Serial_Uart.StopBits = StopBits.One;
            Serial_Uart.Open();
            if (Serial_Uart.IsOpen)
            {
                Serial_Uart.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Uart_DataReceived);
            }

            if (Serial_Uart.IsOpen)
            {
                //Serial_Uart.Close();
            }
        }

        private void Camera_Grab_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CameraMod == 1)
            {

            }
            else if (CameraMod == 3)
            {
                snap_pic.Tick += new EventHandler(grabUV);
            }
            else if (CameraMod == 4)
            {

            }
        }


    }
}
