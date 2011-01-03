using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;


using NLog;


using System.Runtime.InteropServices;

using Cornerstone.Extensions;

using DirectShowLib;
using DirectShowLib.Dvd;
using DShowNET.Helper;

using Microsoft.Win32;
using MediaPortal.Profile;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using mvCentral.LocalMediaManagement.MusicVideoResources;
using mvCentral.Database;
using mvCentral.Utils;


namespace mvCentral.ConfigScreen.Popups {
    public partial class GrabberPopup : Form
    {

        const int FRAME_STEP_INCREMENT = 5;   // How many frames to step, when frame-stepping.
        const int ONE_MSEC = 10000;   // The number of 100-ns in 1 msec
        bool m_bFrameStepping;   // TRUE if a frame step is in progress.
        protected ulong _offsetseek = 0;
        protected enum PlayState
        {
            Init,
            Menu,
            Playing,
            Paused,
            Stopped
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary> current state of playback (playing/paused/...) </summary>
        protected PlayState _state;
        
        /// <summary> asynchronous command interface. </summary>
        protected IDvdCmd _cmdOption = null;

        /// <summary> asynchronous command pending. </summary>
        protected bool _pendingCmd;

        /// <summary> dvd graph builder interface. </summary>
        protected IDvdGraphBuilder _dvdGraph = null;

        /// <summary> dvd control interface. </summary>
        protected IDvdControl2 _dvdCtrl = null;

        /// <summary> dvd information interface. </summary>
        protected IDvdInfo2 _dvdInfo = null;

        protected IBaseFilter _dvdbasefilter = null;

        /// <summary> dvd video playback window interface. </summary>
        protected IVideoWindow _videoWin = null;

        protected IBasicVideo2 _basicVideo = null;
        protected IGraphBuilder _graphBuilder = null;
        protected IAMLine21Decoder _line21Decoder = null;

        protected DvdPreferredDisplayMode _videoPref = DvdPreferredDisplayMode.DisplayContentDefault;
        protected AspectRatioMode arMode = AspectRatioMode.Stretched;

        /// <summary> control interface. </summary>
        protected IMediaControl _mediaCtrl = null;

        /// <summary> graph event interface. </summary>
        protected IMediaEventEx _mediaEvt = null;

        /// <summary> interface to single-step video. </summary>
        //protected IVideoFrameStep			  videoStep=null;
        protected int _volume = 100;

        //protected OVTOOLLib.OvMgrClass	m_ovMgr=null;

        protected DvdHMSFTimeCode _currTime; // copy of current playback states, see OnDvdEvent()
        protected int _currTitle = 0;
        protected int _currChapter = 0;
        protected DvdDomain _currDomain;
        protected IBasicAudio _basicAudio = null;
        protected IMediaPosition _mediaPos = null;
        protected IMediaSeeking _mediaSeek = null;
        protected IVideoFrameStep _mediaStep = null;
        private VMR7Util _vmr7 = null;
        protected IBaseFilter _vmr9Filter = null;
        protected int _speed = 1;
        protected double _currentTime = 0;
        protected bool _visible = true;
        protected bool _started = false;
        protected bool _IsScrolling = false;
        protected DsROTEntry _rotEntry = null;

        protected int _positionX = 80;
        protected int _positionY = 400;
        protected int _width = 200;
        protected int _height = 100;
        protected int _videoWidth = 100;
        protected int _videoHeight = 100;
        protected bool _updateNeeded = false;
        protected bool _fullScreen = true;
        private string _defaultAudioLanguage = "";
        private string _defaultSubtitleLanguage = "";
        protected bool _forceSubtitles = true;
        protected bool _freeNavigator = false;
        protected int _UOPs;
        protected int buttonCount = 0;
        protected int focusedButton = 0;
        protected string _currentFile;
        protected double _duration;
        protected Geometry.Type _aspectRatio = Geometry.Type.Normal;

        protected const int WM_DVD_EVENT = 0x00008002; // message from dvd graph
        protected const int WS_CHILD = 0x40000000; // attributes for video window
        protected const int WS_CLIPCHILDREN = 0x02000000;
        protected const int WS_CLIPSIBLINGS = 0x04000000;
        protected const int WM_MOUSEMOVE = 0x0200;
        protected const int WM_LBUTTONUP = 0x0202;


        protected bool _cyberlinkDVDNavigator = false;
        
        
        
        // DirectShow stuff
        FilterState state;
        DBTrackInfo mvs;

        private IVMRWindowlessControl9 windowlessCtrl = null;



        private bool handlersAdded = false;
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
                CloseDVDInterfaces();
                mvs = mv;
                BuildGraph();
                btnPlay_Click(null, null);
                UpdateSeekBar();
            }
            catch (Exception) { }
        }


        private void BuildGraph()
        {
            int hr;
            try
            {
                lblTotalTime.Text = mvs.PlayTime.ToString();
                TimeSpan tt = TimeSpan.Parse(mvs.PlayTime);
                DateTime dt = new DateTime(tt.Ticks);
                lblTotalTime.Text = String.Format("{0:HH:mm:ss}", dt);

                if (mvs.LocalMedia[0].IsDVD)
                {
                    FirstPlayDvd(mvs.LocalMedia[0].File.FullName);
                    // Add delegates for Windowless operations
                    AddHandlers();
                    MainForm_ResizeMove(null, null);





                }
                else
                {
                    _graphBuilder = (IFilterGraph2)new FilterGraph();
                    _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder); 
                    _mediaCtrl = (IMediaControl)_graphBuilder;
                    _mediaSeek = (IMediaSeeking)_graphBuilder;
                    _mediaPos = (IMediaPosition)_graphBuilder;
                    _mediaStep = (IVideoFrameStep)_graphBuilder;
                    _vmr9Filter = (IBaseFilter)new VideoMixingRenderer9();
                    ConfigureVMR9InWindowlessMode();
                    AddHandlers();
                    MainForm_ResizeMove(null, null);
                    hr = _graphBuilder.AddFilter(_vmr9Filter, "Video Mixing Render 9");
                    AddPreferedCodecs(_graphBuilder); 
                    DsError.ThrowExceptionForHR(hr);
                    hr = _graphBuilder.RenderFile(mvs.LocalMedia[0].File.FullName, null);
                    DsError.ThrowExceptionForHR(hr);
                }


            }
            catch (Exception e)
            {
                CloseDVDInterfaces();
                logger.ErrorException("An error occured during the graph building : \r\n\r\n",e);
            }
        }

        private void ConfigureVMR9InWindowlessMode()
        {
            int hr = 0;

            IVMRFilterConfig9 _filterConfig = (IVMRFilterConfig9)_vmr9Filter ;

            // Not really needed for VMR9 but don't forget calling it with VMR7
            hr = _filterConfig.SetNumberOfStreams(1);
            DsError.ThrowExceptionForHR(hr);

            // Change VMR9 mode to Windowless
            hr = _filterConfig.SetRenderingMode(VMR9Mode.Windowless);
            DsError.ThrowExceptionForHR(hr);

            windowlessCtrl = (IVMRWindowlessControl9)_vmr9Filter;

            // Set "Parent" window
            hr = windowlessCtrl.SetVideoClippingWindow(this.panVideoWin.Handle);
            DsError.ThrowExceptionForHR(hr);

            // Set Aspect-Ratio
            hr = windowlessCtrl.SetAspectRatioMode(VMR9AspectRatioMode.LetterBox);
            DsError.ThrowExceptionForHR(hr);

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

            try
            {
                int hr = 0;
                if (mvs.LocalMedia[0].IsDVD)
                {



                    hr = _mediaCtrl.Run();
                    hr = _mediaCtrl.Pause();
                    _offsetseek = (ulong)seekbar.Value;
                    TimeSpan t1 = TimeSpan.FromMilliseconds(seekbar.Value);
                    TimeSpan t2 = TimeSpan.Parse(mvs.OffsetTime);
                    t1 = t1.Add(t2);
                    t2 = t2.Add(TimeSpan.Parse(mvs.PlayTime));
                    DvdHMSFTimeCode t3 = mvCentralUtils.ConvertToDvdHMSFTimeCode(t1);
                    DvdHMSFTimeCode t4 = mvCentralUtils.ConvertToDvdHMSFTimeCode(t2);
                    //                if (state == FilterState.Stopped) 
                    hr = _dvdCtrl.PlayPeriodInTitleAutoStop(1, t3, t4, DvdCmdFlags.Flush | DvdCmdFlags.Block, out _cmdOption);
                    DsError.ThrowExceptionForHR(hr);
 //                   hr = _mediaCtrl.Run();
                    label1.Text = t1.ToString();
                    label2.Text = t2.ToString();
                    //               if (state == FilterState.Stopped) hr = _dvdCtrl.PlayChaptersAutoStop(1, mvs.ChapterID, 1, 0, out _cmdOption);
                    DsError.ThrowExceptionForHR(hr);
                    return;
                }

                if (_mediaCtrl != null)
                {
                    hr = _mediaCtrl.Run();
                    DsError.ThrowExceptionForHR(hr);
                }
            }
                catch (Exception ex)
                {
                    logger.ErrorException("Error in play : \r\n\r\n", ex);
                }
        }

        private void StopGraph()
        {
            if (_mediaCtrl != null)
            {
                int hr = _mediaCtrl.Stop();
                DsError.ThrowExceptionForHR(hr);
                state = FilterState.Stopped;
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
            if (state == FilterState.Running) return;
            RunGraph();
            _mediaCtrl.Run();
            state = FilterState.Running;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (state == FilterState.Paused) return;
            if (_mediaCtrl != null)
            {
                int hr = _mediaCtrl.Pause();
                DsError.ThrowExceptionForHR(hr);
                state = FilterState.Paused;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (state == FilterState.Stopped) return;

            if (_mediaCtrl != null)
            {
                int hr = _mediaCtrl.Stop();
                DsError.ThrowExceptionForHR(hr);
                state = FilterState.Stopped;
            }
                        // Seek back to the start.
            if (_mediaPos != null)
            {
                _offsetseek = 0;
                seekbar.Value = 0;
                _mediaPos.put_CurrentPosition(0);
            }
        }

        private void btnFrameStep_Click(object sender, EventArgs e)
        {
            if (_mediaStep != null)
            {
             _mediaStep.Step(FRAME_STEP_INCREMENT,null);
             state = FilterState.Paused;
//            m_bFrameStepping = true;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            StopGraph();
            CloseDVDInterfaces();
            this.Dispose();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (_IsScrolling) return;
                       // If the player can seek, update the seek bar with the current position.
            if (_mediaSeek != null)
            {
                //            if (mediaSeek.GetCapabilitiesControl..CanSeek())
                //            {
                long timeNow;
                try
                {
                    int hr = _mediaSeek.GetCurrentPosition(out timeNow);
                DsError.ThrowExceptionForHR(hr);
                timeNow = timeNow + (long)(_offsetseek * ONE_MSEC);
                    TimeSpan duration = new TimeSpan(timeNow);
                    DateTime dt = new DateTime(duration.Ticks);
                    lblTime.Text = String.Format("{0:HH:mm:ss}", dt);
                    if ((int)(timeNow / ONE_MSEC) < seekbar.Maximum)
                        seekbar.Value = (int)(timeNow / ONE_MSEC);
                    else btnStop_Click(null, null);

                    TimeSpan t1 = TimeSpan.FromMilliseconds(seekbar.Value);
                    if (mvs.OffsetTime.Trim().Length > 0)
                    {
                        TimeSpan t2 = TimeSpan.Parse(mvs.OffsetTime);
                        t1 = t1.Add(t2);
                        //                DvdHMSFTimeCode t3 = ConvertToDvdHMSFTimeCode(t1);
                        label3.Text = t1.ToString();
                    }

                }
                catch (Exception ex)
                {
                    logger.ErrorException("Error in play position : \r\n\r\n", ex);
                }
            }
        }

        private void UpdateSeekBar()
        {
            // If the player can seek, set the seekbar range and start the time.
            // Otherwise, disable the seekbar.
            if (_mediaSeek != null)
            {
                seekbar.Enabled = true;

                long rtDuration;
                _mediaSeek.GetDuration(out rtDuration);
                rtDuration = TimeSpan.Parse(mvs.PlayTime).Ticks;

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
            if (_mediaSeek != null)
            {
                long temp = ONE_MSEC * (long)seekbar.Value;
                try
                {
                    if (!mvs.LocalMedia[0].IsDVD)
                    {
                        //                    btnStop_Click(null, null);
                      int hr = _mediaSeek.SetPositions(temp, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
                      DsError.ThrowExceptionForHR(hr);
                      return;
                    }
//                    if (_IsScrolling) return; 
//                    DvdHMSFTimeCode t5 = ConvertToDvdHMSFTimeCode(TimeSpan.FromMilliseconds(seekbar.Value));

//                    IDvdCmd _cmdOption;

//                    TimeSpan t3 = TimeSpan.Parse(mvs.OffsetTime);
//                    t3 = t3.Add(TimeSpan.FromMilliseconds(seekbar.Value));

//                    DvdHMSFTimeCode t1 = ConvertToDvdHMSFTimeCode(TimeSpan.Parse(mvs.OffsetTime));
//                    DvdHMSFTimeCode t2 = ConvertToDvdHMSFTimeCode(t3);

//                    if (state == FilterState.Stopped) hr = _dvdCtrl.PlayPeriodInTitleAutoStop(1, t1, t2, DvdCmdFlags.None, out _cmdOption);
//                    _IsScrolling = true;
                    RunGraph();
//                    _IsScrolling = false;
                    //                    int hr1 = _dvdCtrl.PlayAtTime(t2, DvdCmdFlags.Flush, out _cmdOption);
                    //                    DsError.ThrowExceptionForHR(hr1);

                    label4.Text = seekbar.Value.ToString();




                }
                catch (Exception ex)
                {
                    logger.ErrorException("Error in seeking : \r\n\r\n", ex);
                }

            }

        }

        private void seekbar_MouseCaptureChanged(object sender, EventArgs e)
        {
            if (_mediaCtrl != null)
            {

                if (state == FilterState.Stopped)
                {
                    _mediaCtrl.Stop();
                }
                else if (state == FilterState.Running)
                {
                    _mediaCtrl.Run();
                }
                else if (state == FilterState.Paused)
                {
                    _mediaCtrl.Pause();
                }
            }
        }

        private void seekbar_MouseDown(object sender, MouseEventArgs e)
        {
            _IsScrolling = true;
            btnPause_Click(sender, e);
            int tr = _mediaCtrl.GetState(0, out state);
        }

        private void seekbar_MouseUp(object sender, MouseEventArgs e)
        {
            btnPlay_Click(sender, e);
            int tr = _mediaCtrl.GetState(0, out state);
            label4.Text = seekbar.Value.ToString();
            _IsScrolling = false;
            
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
                    logger.ErrorException("Failed getting image: ", anyException);

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

        /// <summary> handling the very first start of dvd playback. </summary>
        private bool FirstPlayDvd(string file)
        {
            int hr;

            try
            {
                _pendingCmd = true;
                CloseDVDInterfaces();
                string path = null;
                _currentFile = file;

                if (MediaPortal.Util.VirtualDirectory.IsImageFile(System.IO.Path.GetExtension(file)))
                    file = MediaPortal.Util.DaemonTools.GetVirtualDrive() + @"\VIDEO_TS\VIDEO_TS.IFO";

                // Cyberlink navigator needs E:\\VIDEO_TS formatted path with double \
                path = file.Replace(@"\\", @"\").Replace(Path.GetFileName(file), "").Replace(@"\", @"\\");

                if (!GetDVDInterfaces(path))
                {
                    logger.Error("DVDPlayer:Unable getinterfaces()");
                    CloseDVDInterfaces();
                    return false;
                }



                if (_basicVideo != null)
                {
                    _basicVideo.SetDefaultSourcePosition();
                    _basicVideo.SetDefaultDestinationPosition();
                }
/*                hr = _mediaCtrl.Run();
                if (hr < 0 || hr > 1)
                {
                    MediaPortal.Util.HResult hrdebug = new MediaPortal.Util.HResult(hr);
                    logger.Info(hrdebug.ToDXString());
                    Log.Error("DVDPlayer:Unable to start playing() 0x:{0:X}", hr);
                    CloseDVDInterfaces();
                    return false;
                }
                */
                    DvdDiscSide side;
                    int titles, numOfVolumes, volume;
                    hr = _dvdInfo.GetDVDVolumeInfo(out numOfVolumes, out volume, out side, out titles);
                    if (hr < 0)
                    {
                        logger.Error("DVDPlayer:Unable to get dvdvolumeinfo 0x{0:X}", hr);
                        CloseDVDInterfaces();
                        return false; // can't read disc
                    }
                    else
                    {
                        if (titles <= 0)
                        {
                            logger.Error("DVDPlayer:DVD does not contain any titles? {0}", titles,1);
                            //return false;
                        }
                    }
                hr = _dvdCtrl.SelectVideoModePreference(_videoPref);
                DvdVideoAttributes videoAttr;
                hr = _dvdInfo.GetCurrentVideoAttributes(out videoAttr);
                _videoWidth = videoAttr.sourceResolutionX;
                _videoHeight = videoAttr.sourceResolutionY;

                _state = PlayState.Playing;

                _pendingCmd = false;
                logger.Info("DVDPlayer:Started playing()");
                if (_currentFile == string.Empty)
                {
                    for (int i = 0; i <= 26; ++i)
                    {
                        string dvd = String.Format("{0}:", (char)('A' + i));
                        if (MediaPortal.Util.Utils.IsDVD(dvd))
                        {
                            _currentFile = String.Format(@"{0}:\VIDEO_TS\VIDEO_TS.IFO", (char)('A' + i));
                            if (File.Exists(_currentFile))
                            {
                                break;
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
               
                logger.ErrorException("DVDPlayer:Could not start DVD:{0} {1} {2}", ex);
                CloseDVDInterfaces();
                return false;
            }
        }

        /// <summary> do cleanup and release DirectShow. </summary>
        protected virtual void CloseDVDInterfaces()
        {
            if (_graphBuilder == null)
            {
                return;
            }
            int hr;
            try
            {
                logger.Info("DVDPlayer:cleanup DShow graph");

                if (_mediaCtrl != null)
                {
                    hr = _mediaCtrl.Stop();
                    _mediaCtrl = null;
                }
                _state = PlayState.Stopped;

                _mediaEvt = null;
                _visible = false;
                _videoWin = null;
                //				videoStep	= null;
                _dvdCtrl = null;
                _dvdInfo = null;
                _basicVideo = null;
                _basicAudio = null;
                _mediaPos = null;
                _mediaSeek = null;
                _mediaStep = null;

                if (_vmr7 != null)
                {
                    _vmr7.RemoveVMR7();
                }
                _vmr7 = null;



                if (_vmr9Filter != null)
                {
                    while ((hr = DirectShowUtil.ReleaseComObject(_vmr9Filter)) > 0)
                    {
                        ;
                    }
                    _vmr9Filter = null;
                }

                if (_dvdbasefilter != null)
                {
                    while ((hr = DirectShowUtil.ReleaseComObject(_dvdbasefilter)) > 0)
                    {
                        ;
                    }
                    _dvdbasefilter = null;
                }

                if (_cmdOption != null)
                {
                    DirectShowUtil.ReleaseComObject(_cmdOption);
                }
                _cmdOption = null;
                _pendingCmd = false;
                if (_line21Decoder != null)
                {
                    while ((hr = DirectShowUtil.ReleaseComObject(_line21Decoder)) > 0)
                    {
                        ;
                    }
                    _line21Decoder = null;
                }


                if (_graphBuilder != null)
                {
                    DirectShowUtil.RemoveFilters(_graphBuilder);
                    if (_rotEntry != null)
                    {
                        _rotEntry.Dispose();
                        _rotEntry = null;
                    }
                    while ((hr = DirectShowUtil.ReleaseComObject(_graphBuilder)) > 0)
                    {
                        ;
                    }
                    _graphBuilder = null;
                }

                if (_dvdGraph != null)
                {
                    while ((hr = DirectShowUtil.ReleaseComObject(_dvdGraph)) > 0)
                    {
                        ;
                    }
                    _dvdGraph = null;
                }
                _state = PlayState.Init;

            }
            catch (Exception ex)
            {
                logger.Error("DVDPlayer:exception while cleanuping DShow graph {0} {1}", ex.Message, ex.StackTrace);
            }
        }

        /// <summary> create the used COM components and get the interfaces. </summary>
        protected virtual bool GetDVDInterfaces(string path)
        {
            int hr;
            //Type	            comtype = null;
            object comobj = null;
            _freeNavigator = true;
            _dvdInfo = null;
            _dvdCtrl = null;
            bool useAC3Filter = false;
            string dvdNavigator = "";
            string aspectRatioMode = "";
            string displayMode = "";
            _videoPref = DvdPreferredDisplayMode.DisplayContentDefault;
            using (MediaPortal.Profile.Settings xmlreader = new MPSettings())
            {
                dvdNavigator = xmlreader.GetValueAsString("dvdplayer", "navigator", "DVD Navigator");
                aspectRatioMode = xmlreader.GetValueAsString("dvdplayer", "armode", "").ToLower();
                if (aspectRatioMode == "crop")
                {
                    arMode = AspectRatioMode.Crop;
                }
                if (aspectRatioMode == "letterbox")
                {
                    arMode = AspectRatioMode.LetterBox;
                }
                if (aspectRatioMode == "stretch")
                {
                    arMode = AspectRatioMode.Stretched;
                }
                //if ( aspectRatioMode == "stretch" ) arMode = AspectRatioMode.zoom14to9;
                if (aspectRatioMode == "follow stream")
                {
                    arMode = AspectRatioMode.StretchedAsPrimary;
                }
                useAC3Filter = xmlreader.GetValueAsBool("dvdplayer", "ac3", false);
                displayMode = xmlreader.GetValueAsString("dvdplayer", "displaymode", "").ToLower();
                if (displayMode == "default")
                {
                    _videoPref = DvdPreferredDisplayMode.DisplayContentDefault;
                }
                if (displayMode == "16:9")
                {
                    _videoPref = DvdPreferredDisplayMode.Display16x9;
                }
                if (displayMode == "4:3 pan scan")
                {
                    _videoPref = DvdPreferredDisplayMode.Display4x3PanScanPreferred;
                }
                if (displayMode == "4:3 letterbox")
                {
                    _videoPref = DvdPreferredDisplayMode.Display4x3LetterBoxPreferred;
                }
            }
            try
            {
                _dvdGraph = (IDvdGraphBuilder)new DvdGraphBuilder();

                hr = _dvdGraph.GetFiltergraph(out _graphBuilder);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
                _rotEntry = new DsROTEntry((IFilterGraph)_graphBuilder);

                _vmr9Filter = (IBaseFilter)new VideoMixingRenderer9();
                IVMRFilterConfig9 config = _vmr9Filter as IVMRFilterConfig9;
                hr = config.SetNumberOfStreams(1);
                hr = config.SetRenderingMode(VMR9Mode.Windowless);
                windowlessCtrl = (IVMRWindowlessControl9)_vmr9Filter;
                windowlessCtrl.SetVideoClippingWindow(this.panVideoWin.Handle);
                
                
                //                config.SetRenderingPrefs(VMR9RenderPrefs.

                _graphBuilder.AddFilter(_vmr9Filter, "Video Mixing Renderer 9");
                
 //               _vmr7 = new VMR7Util();
 //               _vmr7.AddVMR7(_graphBuilder);

                try
                {
                    _dvdbasefilter = DirectShowUtil.AddFilterToGraph(_graphBuilder, dvdNavigator);
                    if (_dvdbasefilter != null)
                    {
                        IDvdControl2 cntl = (IDvdControl2)_dvdbasefilter;
                        if (cntl != null)
                        {
                            _dvdInfo = (IDvdInfo2)cntl;
                            _dvdCtrl = (IDvdControl2)cntl;
                            if (path != null)
                            {
                                if (path.Length != 0)
                                {
                                    cntl.SetDVDDirectory(path);
                                }
                            }
                            _dvdCtrl.SetOption(DvdOptionFlag.HMSFTimeCodeEvents, true); // use new HMSF timecode format
                            _dvdCtrl.SetOption(DvdOptionFlag.ResetOnStop, false);

                            AddPreferedCodecs(_graphBuilder);
                            DirectShowUtil.RenderOutputPins(_graphBuilder, _dvdbasefilter);

                            
//                            _videoWin = _graphBuilder as IVideoWindow;
                            _freeNavigator = false;
                        }

                        //DirectShowUtil.ReleaseComObject( _dvdbasefilter); _dvdbasefilter = null;              
                    }
                }
                catch (Exception ex)
                {
                    string strEx = ex.Message;
                }

                Guid riid;

                if (_dvdInfo == null)
                {
                    riid = typeof(IDvdInfo2).GUID;
                    hr = _dvdGraph.GetDvdInterface(riid, out comobj);
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }
                    _dvdInfo = (IDvdInfo2)comobj;
                    comobj = null;
                }

                if (_dvdCtrl == null)
                {
                    riid = typeof(IDvdControl2).GUID;
                    hr = _dvdGraph.GetDvdInterface(riid, out comobj);
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }
                    _dvdCtrl = (IDvdControl2)comobj;
                    comobj = null;
                }

                _mediaCtrl = (IMediaControl)_graphBuilder;
                _mediaEvt = (IMediaEventEx)_graphBuilder;
                _basicAudio = _graphBuilder as IBasicAudio;
                _mediaPos = (IMediaPosition)_graphBuilder;
                _mediaSeek = (IMediaSeeking)_graphBuilder;
                _mediaStep = (IVideoFrameStep)_graphBuilder;
                _basicVideo = _graphBuilder as IBasicVideo2;
                _videoWin = _graphBuilder as IVideoWindow;

                // disable Closed Captions!
                IBaseFilter baseFilter;
                _graphBuilder.FindFilterByName("Line 21 Decoder", out baseFilter);
                if (baseFilter == null)
                {
                    _graphBuilder.FindFilterByName("Line21 Decoder", out baseFilter);
                }
                if (baseFilter != null)
                {
                    _line21Decoder = (IAMLine21Decoder)baseFilter;
                    if (_line21Decoder != null)
                    {
                        AMLine21CCState state = AMLine21CCState.Off;
                        hr = _line21Decoder.SetServiceState(state);
                        if (hr == 0)
                        {
                            logger.Info("DVDPlayer:Closed Captions disabled");
                        }
                        else
                        {
                            logger.Info("DVDPlayer:failed 2 disable Closed Captions");
                        }
                    }
                }
                /*
                        // get video window
                        if (_videoWin==null)
                        {
                          riid = typeof( IVideoWindow ).GUID;
                          hr = _dvdGraph.GetDvdInterface( ref riid, out comobj );
                          if( hr < 0 )
                            Marshal.ThrowExceptionForHR( hr );
                          _videoWin = (IVideoWindow) comobj; comobj = null;
                        }
                  */
                // GetFrameStepInterface();

                DirectShowUtil.SetARMode(_graphBuilder, arMode);
                DirectShowUtil.EnableDeInterlace(_graphBuilder);
                //m_ovMgr = new OVTOOLLib.OvMgrClass();
                //m_ovMgr.SetGraph(_graphBuilder);

                return true;
            }
            catch (Exception)
            {

                //MessageBox.Show( this, "Could not get interfaces\r\n" + ee.Message, "DVDPlayer.NET", MessageBoxButtons.OK, MessageBoxIcon.Stop );
                CloseDVDInterfaces();
                return false;
            }
            finally
            {
                if (comobj != null)
                {
                    DirectShowUtil.ReleaseComObject(comobj);
                }
                comobj = null;
            }
        }


        public void AddPreferedCodecs(IGraphBuilder _graphBuilder)
        {
            // add preferred video & audio codecs
            string strVideoCodec = "";
            string strAudioCodec = "";
            string strAudiorenderer = "";
            int intFilters = 0; // FlipGer: count custom filters
            string strFilters = ""; // FlipGer: collect custom filters
            using (MediaPortal.Profile.Settings xmlreader = new MPSettings())
            {
                // FlipGer: load infos for custom filters
                int intCount = 0;
                while (xmlreader.GetValueAsString("dvdplayer", "filter" + intCount.ToString(), "undefined") != "undefined")
                {
                    if (xmlreader.GetValueAsBool("dvdplayer", "usefilter" + intCount.ToString(), false))
                    {
                        strFilters += xmlreader.GetValueAsString("dvdplayer", "filter" + intCount.ToString(), "undefined") + ";";
                        intFilters++;
                    }
                    intCount++;
                }
                strVideoCodec = xmlreader.GetValueAsString("dvdplayer", "videocodec", "");
                strAudioCodec = xmlreader.GetValueAsString("dvdplayer", "audiocodec", "");
                strAudiorenderer = xmlreader.GetValueAsString("dvdplayer", "audiorenderer", "Default DirectSound Device");
            }
            if (strVideoCodec.Length > 0)
            {
                DirectShowUtil.AddFilterToGraph(_graphBuilder, strVideoCodec);
            }
            if (strAudioCodec.Length > 0)
            {
                DirectShowUtil.AddFilterToGraph(_graphBuilder, strAudioCodec);
            }
            if (strAudiorenderer.Length > 0)
            {
                DirectShowUtil.AddAudioRendererToGraph(_graphBuilder, strAudiorenderer, false);
            }
            // FlipGer: add custom filters to graph
            string[] arrFilters = strFilters.Split(';');
            for (int i = 0; i < intFilters; i++)
            {
                DirectShowUtil.AddFilterToGraph(_graphBuilder, arrFilters[i]);
            }
        }

 

    }
}
