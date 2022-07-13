using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using DALSA.SaperaLT.SapClassBasic;

using OpenCvSharp;



namespace ScanProgram
{
    public class SaperaCapture : MainWindow
    {
        string serverName = "";
        public SapLocation location = null;
        SapAcqDevice device = null;
        SapTransfer transfer = null;
        SapBuffer buffer = null;
        SapView view = null;
        SapProcessing m_pro = null;
        SapColorConversion m_conv;
        Mat colorConvGrab = new Mat();
        Mat rawConvGrab = new Mat();
        public Image img = null;
        // Create camera index parameter
        [Description("The index of the camera from which to acquire images.")]
        public int Index { get; set; }

        // Create exposure time parameter
        private double exposureTime;
        [Description("Exposure time (ms). This controls the maximum framerate.")]
        public double ExposureTime { get => exposureTime; set => exposureTime = Math.Round(value, 2); }

        // Create frame rate parameter
        private double frameRate;
        [Description("Desired frame rate (frames / s). This is superceded by the maximum framerate allowed by the exposure time. When UseMaxFrameRate is set to True, FrameRate is set to -1.")]
        public double FrameRate
        {
            get
            {
                return frameRate;
            }
            set
            {
                frameRate = Math.Floor(value * 100) / 100;
            }
        }

        // Create black level parameter
        private double blackLevel;
        [Description("Black level (DN). This controls the analog black level as DC offset applied to the video signal.")]
        public double BlackLevel
        {
            get
            {
                return blackLevel;
            }
            set
            {
                blackLevel = Math.Round(value, 2);
            }
        }

        // Create gain parameter
        private double gain;
        [Description("Adjusts the gain (dB). This controls the gain as an amplification factor applied to the video signal.")]
        public double Gain
        {
            get
            {
                return gain;
            }
            set
            {
                gain = Math.Round(value, 2);
            }
        }

        // Create parameter for determining whether to automatically use the maximum possible frame rate for the current exposure time
        private bool useMaxFrameRate;
        [Description("Whether to set the FPS to the maximum possible value based on the current exposure time.")]
        public bool UseMaxFrameRate
        {
            get
            {
                return useMaxFrameRate;
            }
            set
            {
                useMaxFrameRate = value;
            }
        }

        // Create parameter for determining whether to reset the device before acquiring frames
        private bool resetDevice;
        [Description("Resets the camera before acquiring frames.")]
        public bool ResetDevice
        {
            get
            {
                return resetDevice;
            }
            set
            {
                resetDevice = value;
            }
        }


        // Create variables
        readonly object captureLock = new object();

        // Function used for destroying and disposing of sapera class objects
        public void DestroyObjects()
        {
            if (transfer != null && transfer.Initialized)
                transfer.Destroy();
            if (m_pro != null && m_pro.Initialized)
                m_pro.Destroy();
            if (view != null && view.Initialized)
                view.Destroy();
            if (m_conv != null && m_conv.Initialized)
                m_conv.Destroy();
            if (buffer != null && buffer.Initialized)
                buffer.Destroy();
            if (device != null && device.Initialized)
                device.Destroy();
        }
        public void DisposeObjects()
        {
            if (transfer != null && transfer.Initialized)
            { transfer.Dispose(); transfer = null; }
            if (m_pro != null && m_pro.Initialized)
            { m_pro.Destroy(); m_pro = null; }
            if (view != null && view.Initialized)
            { view.Destroy(); view = null; }
            if (m_conv != null && m_conv.Initialized)
            { m_conv.Destroy(); m_conv = null; }
            if (buffer != null && buffer.Initialized)
            { buffer.Destroy(); buffer = null; }
            if (device != null && device.Initialized)
            { device.Destroy(); device = null; }
        }

        static void ProCallback(object sender, SapProcessingDoneEventArgs pInfo)
        {
            SaperaCapture demo = pInfo.Context as SaperaCapture;

        }
        // Callback function for when a frame is grabbed by the camera
        private void Xfer_XferNotify(object sender, SapXferNotifyEventArgs args)
        {
            SaperaCapture demo = args.Context as SaperaCapture;
            // If grabbing in trash buffer, do not display the image, update the
            // appropriate number of frames on the status bar instead
            if (args.Trash) { return; }
            // Refresh view
            else
            {
                m_pro.ExecuteNext();
            }
            // First, judge whether this frame is an abandoned frame. If so, return immediately and wait for the next frame (but this sentence sometimes) m_Xfer.Snap(n)It will cause frame loss, you can comment it out (try it)
            if (args.Trash) return;

            // obtain buffer As long as you know the address of the picture memory, you can actually have a variety of ways to get the picture (for example, convert to Bitmap)
            IntPtr addr;
            buffer.GetAddress(out addr);

            // observation buffer Some attribute values of the picture in. The values in the comments after the statement are possible values
            int count = buffer.Count; //2
            SapFormat format = buffer.Format; //Uint8
            double rate = buffer.FrameRate; //30.0,This value changes dynamically during continuous acquisition
            int height = buffer.Height; //2800
            int weight = buffer.Width; //4096
            int pixd = buffer.PixelDepth; //8


            // Read pictures from memory and convert them into bitmap Formats, creating palettes, printing to PictureBox
            PixelFormat pf = PixelFormat.Format8bppIndexed;
            Bitmap bmp = new Bitmap(weight, height, buffer.Pitch, pf, addr);
            bmp.SetResolution(height, weight);
            BitmapData bmpData = new BitmapData();

            bmpData = bmp.LockBits(new Rectangle(0, 0, weight, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            ColorPalette palette = bmp.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            bmp.Palette = palette;

           

            bmp.UnlockBits(bmpData);

            //using (Bitmap tempbmp = new Bitmap(1, 1, pf))
            //{
            //    m_grayPalette = tempbmp.Palette;
            //}
            //for (int i = 0; i <= 255; i++)
            //{
            //    m_grayPalette.Entries[i] = Color.FromArgb(i, i, i);
            //}

            //bmp.Palette = m_grayPalette;
            //opencv
            rawConvGrab = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp);

            Cv2.CvtColor(rawConvGrab, colorConvGrab, ColorConversionCodes.BayerRG2RGB_EA);
            bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(colorConvGrab);
            //bmp = Imagefrom
            img = (Image)bmp;
        }

        public SaperaCapture()
        {
            // Get server count
            int serverCount = SapManager.GetServerCount();

            // Check if index variable is valid
            if (Index >= 0 && Index < serverCount - 1)
            {
                // Find the name of the server
                serverName = SapManager.GetServerName(Index + 1);
            }
            else
            {
                return;
            }
            // Find server location
            location = new SapLocation(serverName, 0);
            // Find device
            device = new SapAcqDevice(location, false);
            // Create buffer
            buffer = new SapBufferWithTrash(3, device, SapBuffer.MemoryType.ScatterGather);
            // Initialize transfer between device and buffer
            transfer = new SapAcqDeviceToBuf(device, buffer);

            m_conv = new SapColorConversion(device, buffer);
            m_pro = new SapMyProcessing(buffer, m_conv);
            //view = new SapView(m_conv.OutputBuffer);

            // Initialize frame handler for end of frame events
            transfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            transfer.XferNotify += new SapXferNotifyHandler(Xfer_XferNotify);
            transfer.XferNotifyContext = this;

            #region Create Objects
            // Check if device was created
            if (!device.Create())
            {
                DestroyObjects();
                location.Dispose();
                return;
            }
            // Check if software color conversion is supported
            if (device.RawBayerOutput == false)
            {
                DestroyObjects();
                return;
            }
            // Enable/Disable bayer conversion
            // This call may require to modify the acquisition output format.
            // For this reason, it has to be done after creating the acquisition object but before
            // creating the output buffer object.
            if (m_conv != null && !m_conv.Enable(true, false))
            {
                DestroyObjects();
                return;
            }
            // Create buffer object
            if (buffer != null && !buffer.Initialized)
            {
                if (buffer.Create() == false)
                {
                    DestroyObjects();
                    return;
                }
                buffer.Clear();
            }
            // Create color conversion object
            if (m_conv != null && !m_conv.Initialized)
            {
                if (m_conv.Create() == false)
                {
                    DestroyObjects();
                    return;
                }
            }
            // Create view object
            if (view != null && !view.Initialized)
            {
                if (m_conv != null && m_conv.Initialized)
                {
                    // Set buffer to be viewed
                    // When using hardware color decoder, view the acquired RGB buffer,
                    // otherwise, for software color conversion, view the converted RGB buffer.
                    SapBuffer convBuffer = m_conv.OutputBuffer;
                    if (convBuffer != null && convBuffer.Initialized)
                        view.Buffer = convBuffer;
                    else
                        view.Buffer = buffer;
                }

                if (view.Create() == false)
                {
                    DestroyObjects();
                    return;
                }
            }
            // Create transfer object
            if (transfer != null && !transfer.Initialized)
            {
                if (!transfer.Create())
                {
                    DestroyObjects();
                    return;
                }
                transfer.AutoEmpty = false;
            }
            // Create processing object
            if (m_pro != null && !m_pro.Initialized)
            {
                if (!m_pro.Create())
                {
                    DestroyObjects();
                    return;
                }

                m_pro.AutoEmpty = true;
            }
            #endregion

            m_conv.OutputFormat = SapFormat.RGB8888;


                // Start grabbing frames
                transfer.Grab();
           

        }

        public void Snap(int i, int x, string y)
        {
            string fileName;
            fileName = i.ToString("D2") + "_" + x.ToString("D2") + "_" + y;
            log.AppendText(serverName + location.ToString());
            m_conv.OutputBuffer.Save(fileName + ".tif", "-format tif");
            log.AppendText(serverName);
        }
            
        public void Grab()
        {

        }
    }
}