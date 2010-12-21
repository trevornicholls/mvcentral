using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;


using System.Runtime.InteropServices;

using Cornerstone.Extensions;

using DirectShowLib;

using Microsoft.Win32;

using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.Database;

namespace mvCentral.ConfigScreen.Popups {
    public partial class GrabberPopup : Form {


        const int FRAME_STEP_INCREMENT = 5;   // How many frames to step, when frame-stepping.
        const int ONE_MSEC = 10000;   // The number of 100-ns in 1 msec
        bool m_bFrameStepping;   // TRUE if a frame step is in progress.

        // DirectShow stuff
        FilterState state;
        DBTrackInfo mvs;
        private IFilterGraph2 graphBuilder = null;
        private IMediaControl mediaControl = null;
        private IMediaSeeking mediaSeek = null;
        private IMediaPosition mediaPos = null;
        private IVideoFrameStep mediaStep = null;
        private IBaseFilter vmr9 = null;
        private IVMRWindowlessControl9 windowlessCtrl = null;
        private bool handlersAdded = false;
        private SaveFileDialog saveFileDialog;
        private TableLayoutPanel tableLayoutPanelMain;
        private TableLayoutPanel tableLayoutPanelButtons;
//        private Button btnSnap;
//        private Button btnOpen;
//        private Button btnPlay;
//        private Button btnPause;
//        private Panel panel1; // Needed to remove delegates



        public GrabberPopup() {
            InitializeComponent();

            // We paint the windows ourself
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

        }

        public GrabberPopup(DBTrackInfo mv) {
            InitializeComponent();
            try {
                CloseInterfaces();
                mvs = mv;
                BuildGraph(mv.LocalMedia[0].File.FullName);
                RunGraph();
                UpdateSeekBar();
            }
            catch (Exception) { }
        }

        private void CloseInterfaces()
        {
            if (mediaControl != null)
                mediaControl.Stop();

            if (handlersAdded)
                RemoveHandlers();

            if (vmr9 != null)
            {
                Marshal.ReleaseComObject(vmr9);
                vmr9 = null;
                windowlessCtrl = null;
            }

            if (graphBuilder != null)
            {
                Marshal.ReleaseComObject(graphBuilder);
                graphBuilder = null;
                mediaControl = null;
                mediaPos = null;
                mediaSeek = null;
                mediaStep = null;
            }

        }

        private void BuildGraph(string fileName)
        {
            int hr = 0;

            try
            {
                graphBuilder = (IFilterGraph2)new FilterGraph();
                mediaControl = (IMediaControl)graphBuilder;
                mediaSeek = (IMediaSeeking)graphBuilder;
                mediaPos = (IMediaPosition)graphBuilder;
                mediaStep = (IVideoFrameStep)graphBuilder;

                vmr9 = (IBaseFilter)new VideoMixingRenderer9();

                ConfigureVMR9InWindowlessMode();

                hr = graphBuilder.AddFilter(vmr9, "Video Mixing Renderer 9");
                DsError.ThrowExceptionForHR(hr);

                hr = graphBuilder.RenderFile(fileName, null);
                DsError.ThrowExceptionForHR(hr);
            }
            catch (Exception e)
            {
                CloseInterfaces();
                MessageBox.Show("An error occured during the graph building : \r\n\r\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureVMR9InWindowlessMode()
        {
            int hr = 0;

            IVMRFilterConfig9 filterConfig = (IVMRFilterConfig9)vmr9;

            // Not really needed for VMR9 but don't forget calling it with VMR7
            hr = filterConfig.SetNumberOfStreams(1);
            DsError.ThrowExceptionForHR(hr);

            // Change VMR9 mode to Windowless
            hr = filterConfig.SetRenderingMode(VMR9Mode.Windowless);
            DsError.ThrowExceptionForHR(hr);

            windowlessCtrl = (IVMRWindowlessControl9)vmr9;

            // Set "Parent" window
            hr = windowlessCtrl.SetVideoClippingWindow(this.panVideoWin.Handle);
            DsError.ThrowExceptionForHR(hr);

            // Set Aspect-Ratio
            hr = windowlessCtrl.SetAspectRatioMode(VMR9AspectRatioMode.LetterBox);
            DsError.ThrowExceptionForHR(hr);

            // Add delegates for Windowless operations
            AddHandlers();

            // Call the resize handler to configure the output size
            MainForm_ResizeMove(null, null);
        }

        private void AddHandlers()
        {
            // Add handlers for VMR purpose
            this.Paint += new PaintEventHandler(GrabberPopup_Paint); // for WM_PAINT
            this.Resize += new EventHandler(MainForm_ResizeMove); // for WM_SIZE
            this.Move += new EventHandler(MainForm_ResizeMove); // for WM_MOVE
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged); // for WM_DISPLAYCHANGE
            handlersAdded = true;
        }

        private void RemoveHandlers()
        {
            // remove handlers when they are no more needed
            handlersAdded = false;
            this.Paint -= new PaintEventHandler(GrabberPopup_Paint);
            this.Resize -= new EventHandler(MainForm_ResizeMove);
            this.Move -= new EventHandler(MainForm_ResizeMove);
            SystemEvents.DisplaySettingsChanged -= new EventHandler(SystemEvents_DisplaySettingsChanged);
        }

        private void RunGraph()
        {
            if (mediaControl != null)
            {
                int hr = mediaControl.Run();
                DsError.ThrowExceptionForHR(hr);
            }
        }

        private void StopGraph()
        {
            if (mediaControl != null)
            {
                int hr = mediaControl.Stop();
                DsError.ThrowExceptionForHR(hr);
            }
        }

        private void pictureBox_Click(object sender, EventArgs e) {
            this.Close();
        //    pictureBox.Image.Dispose();
        }


        private void GrabberPopup_Paint(object sender, PaintEventArgs e)
        {
            if (windowlessCtrl != null)
            {
                IntPtr hdc = e.Graphics.GetHdc();
                int hr = windowlessCtrl.RepaintVideo(this.panVideoWin.Handle, hdc);
                e.Graphics.ReleaseHdc(hdc);
            }
        }

        private void MainForm_ResizeMove(object sender, EventArgs e)
        {
            if (windowlessCtrl != null)
            {
                int hr = windowlessCtrl.SetVideoPosition(null, DsRect.FromRectangle(this.panVideoWin.ClientRectangle));
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            if (windowlessCtrl != null)
            {
                int hr = windowlessCtrl.DisplayModeChanged();
            }
        }

        private void GrabberPopup_KeyPress(object sender, KeyPressEventArgs e) {
            this.Close();
         //   pictureBox.Image.Dispose();
        }

        private void GrabberPopup_Deactivate(object sender, EventArgs e) {
            this.Close();
           // pictureBox.Image.Dispose();
        }

        // if we have been launched as a dialog and we have an owner, center
        // on the owning form.
        private void GrabberPopup_Shown(object sender, EventArgs e) {

        }

        private void GrabberPopup_Load(object sender, EventArgs e) {
            if (Owner == null)
                return;

            Point center = new Point();
            center.X = Owner.Location.X + (Owner.Width / 2);
            center.Y = Owner.Location.Y + (Owner.Height / 2);

            Point newLocation = new Point();
            newLocation.X = center.X - (Width / 2);
            newLocation.Y = center.Y - (Height / 2);

            Location = newLocation;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            RunGraph();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (mediaControl != null)
            {
                int hr = mediaControl.Pause();
                DsError.ThrowExceptionForHR(hr);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (mediaControl != null)
            {
                int hr = mediaControl.Stop();
                DsError.ThrowExceptionForHR(hr);
            }
                        // Seek back to the start.
            if (mediaPos != null)
            {
                mediaPos.put_CurrentPosition(0);
            }
        }

        private void btnFrameStep_Click(object sender, EventArgs e)
        {
            if (mediaStep != null)
            {
             mediaStep.Step(FRAME_STEP_INCREMENT,null);

//            m_bFrameStepping = true;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            StopGraph();
            CloseInterfaces();
            this.Dispose();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
                       // If the player can seek, update the seek bar with the current position.
            if (mediaSeek != null)
            {
//            if (mediaSeek.GetCapabilitiesControl..CanSeek())
//            {
                long timeNow;

                mediaSeek.GetCurrentPosition(out timeNow);
                seekbar.Value = (int)(timeNow / ONE_MSEC);
            }
        }

        private void UpdateSeekBar()
        {
            // If the player can seek, set the seekbar range and start the time.
            // Otherwise, disable the seekbar.
            if (mediaSeek != null)
            {
                seekbar.Enabled = true;

                long rtDuration;
                mediaSeek.GetStopPosition(out rtDuration);

                seekbar.Maximum = (int)(rtDuration / ONE_MSEC);
                seekbar.LargeChange = seekbar.Maximum / 10;

                // Start the timer
                timer1.Enabled = true;
            }
            else
            {
                seekbar.Enabled = false;

                // Stop the old timer, if any.
                timer1.Enabled = false;
            }
        }



        private void seekbar_Scroll(object sender, EventArgs e)
        {
            // Update the position continuously.
            if (mediaSeek != null)
            {
                int tr = mediaControl.GetState(0, out state);
                mediaSeek.SetPositions(ONE_MSEC * seekbar.Value, 0,0,0);
            }
        }

        private void seekbar_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (mediaControl != null)
            {

                if (state == FilterState.Stopped)
                {
                    mediaControl.Stop();
                }
                else if (state == FilterState.Running)
                {
                    mediaControl.Run();
                }
                else if (state == FilterState.Paused)
                {
                    mediaControl.Pause();
                }
            }
        }

        private void seekbar_MouseDown(object sender, MouseEventArgs e)
        {
            int tr = mediaControl.GetState(0, out state);
            btnPause_Click(sender, e);
        }

        private void snapImage(string outFileName)
        {
            if (windowlessCtrl != null)
            {
                IntPtr currentImage = IntPtr.Zero;
                Bitmap bmp = null;

                try
                {
                    int hr = windowlessCtrl.GetCurrentImage(out currentImage);
                    DsError.ThrowExceptionForHR(hr);

                    if (currentImage != IntPtr.Zero)
                    {
                        BitmapInfoHeader structure = new BitmapInfoHeader();
                        Marshal.PtrToStructure(currentImage, structure);

                        bmp = new Bitmap(structure.Width, structure.Height, (structure.BitCount / 8) * structure.Width, System.Drawing.Imaging.PixelFormat.Format32bppArgb, new IntPtr(currentImage.ToInt64() + 40));
                        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                        bmp.Save(outFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
                catch (Exception anyException)
                {
                    MessageBox.Show("Failed getting image: " + anyException.Message);
                }
                finally
                {
                    if (bmp != null)
                    {
                        bmp.Dispose();
                    }

                    Marshal.FreeCoTaskMem(currentImage);
                }
            }
        }

        private void btnGrabFrame_Click(object sender, EventArgs e)
        {
            string artFolder = mvCentralCore.Settings.TrackArtFolder;
            string safeName = mvs.Track.Replace(' ', '.').ToValidFilename();
            string filename1 = Path.GetTempPath()+"\\{" + safeName + "} [" + new Random().Next(0xFFFFFFF).ToString() + "].jpg";
            snapImage(filename1);
            bool i1 ;
            i1 = mvs.AddArtFromFile(filename1);
            File.Delete(filename1);
            if (i1 == true)
            {
                mvs.Commit();
            }
        }

    }
}
