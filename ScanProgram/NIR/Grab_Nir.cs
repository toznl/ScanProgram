using DALSA.SaperaLT.SapClassBasic;
using System;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

using OpenCvSharp;
namespace ScanProgram
{
    public class SaperaCapture_Nir : MainWindow
    {
        string serverName = "";
        public SapLocation location = null;
        SapAcqDevice device = null;
        SapBuffer buffer = null;
        SapTransfer transfer = null;
        SapView view = null;
        SapProcessing m_pro = null;
        
        public Image img = null;

        // Create camera index parameter
        [Description("The index of the camera from which to acquire images.")]
        public int Index { get; set; }
        
        // Function used for destroying and disposing of sapera class objects
        public void DestroyObjects()
        {
            if (transfer != null && transfer.Initialized)
                transfer.Destroy();
            if (m_pro != null && m_pro.Initialized)
                m_pro.Destroy();
            if (view != null && view.Initialized)
                view.Destroy();
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
            if (buffer != null && buffer.Initialized)
            { buffer.Destroy(); buffer = null; }
            if (device != null && device.Initialized)
            { device.Destroy(); device = null; }
        }

        static void ProCallback(object sender, SapProcessingDoneEventArgs pInfo)
        {
            SaperaCapture_Nir demo = pInfo.Context as SaperaCapture_Nir;

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
                int height = buffer.Height; //2800
                int weight = buffer.Width; //4096
                int pixd = buffer.PixelDepth; //8


                // Read pictures from memory and convert them into bitmap Formats, creating palettes, printing to PictureBox
                PixelFormat pf = PixelFormat.Format8bppIndexed;
                Bitmap bmp = new Bitmap(weight, height, buffer.Pitch, pf, addr);
            ColorPalette _palette = bmp.Palette;
            Color[] _entries = _palette.Entries;
            for (int i = 0; i < 256; i++)
            {
                Color b = new Color();
                b = Color.FromArgb((byte)i, (byte)i, (byte)i);
                _entries[i] = b;
            }
            bmp.Palette = _palette;

            img = (Image)bmp;

        }
        public SaperaCapture_Nir()
        {
            // Get server count
            int serverCount = SapManager.GetServerCount();

            // Check if index variable is valid
            if (Index >= 0 && Index < serverCount - 1)
            {
                // Find the name of the server
                serverName = "Nano-M2590-NIR_1";
            }
            else
            {
                log.AppendText("서버를 찾지 못했습니다");
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
            m_pro = new SapMyProcessing(buffer);

            //view = new SapView(buffer);

            // Initialize frame handler for end of frame events
            transfer.Pairs[0].EventType = SapXferPair.XferEventType.EndOfFrame;
            transfer.XferNotify += new SapXferNotifyHandler(Xfer_XferNotify);
            transfer.XferNotifyContext = this;

            #region Create Objects
            // Check if device was created
            if (device != null && !device.Initialized)
            {
                if (!device.Create())
                {
                    DestroyObjects();
                    location.Dispose();
                    return;
                }

            }
            // Enable/Disable bayer conversion
            // This call may require to modify the acquisition output format.
            // For this reason, it has to be done after creating the acquisition object but before
            // creating the output buffer object.

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
            // Create view object
            if (view != null && !view.Initialized)
            {
                // Set buffer to be viewed
                // When using hardware color decoder, view the acquired RGB buffer,
                // otherwise, for software color conversion, view the converted RGB buffer.
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
            }

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



                transfer.Grab();
         
        }

        public void Snap(int i, int x, string y)
        {
            string fileName;


            fileName = i.ToString() + "_" + x.ToString() + "_" + y;
            buffer.Save(fileName + ".tif", "-format tif");

        }

        public void Grab()
        {
            //Grab_View = new SapView(buffer);
        }
    }
}