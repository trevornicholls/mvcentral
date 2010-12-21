using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using DirectShowLib;

using Microsoft.Win32;

namespace mvCentral.Utils
{
    class FrameGrabber
    {
        // DirectShow stuff
        private IFilterGraph2 graphBuilder = null;
        private IMediaControl mediaControl = null;
        private IMediaSeeking mediaSeeking = null;
        private IMediaPosition mediaPosition = null;
        private IBaseFilter vmr9 = null;
        private IVMRWindowlessControl9 windowlessCtrl = null;
        private Panel panel1 = new Panel();
         public FrameGrabber()
         {

         }


         private void BuildGraph(string fileName)
         {
             int hr = 0;

             try
             {
                 graphBuilder = (IFilterGraph2)new FilterGraph();
                 mediaControl = (IMediaControl)graphBuilder;

                 mediaSeeking = (IMediaSeeking)graphBuilder;
                 mediaPosition = (IMediaPosition)graphBuilder;


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
             hr = windowlessCtrl.SetVideoClippingWindow(this.panel1.Handle);
             DsError.ThrowExceptionForHR(hr);

             // Set Aspect-Ratio
             hr = windowlessCtrl.SetAspectRatioMode(VMR9AspectRatioMode.LetterBox);
             DsError.ThrowExceptionForHR(hr);


         }

         private void CloseInterfaces()
         {
             if (mediaControl != null)
                 mediaControl.Stop();

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
                 mediaSeeking = null;
                 mediaPosition = null;
             }

         }

         public void GrabFrame(string FileName, string outputFileName, double timeindex)
         {
             FilterState state;
             int tr = 0;

             CloseInterfaces();
             BuildGraph(FileName);
             int hr = mediaPosition.put_CurrentPosition(timeindex);// Seeking.   .Run();
             mediaControl.Run();
             tr = mediaControl.GetState(0, out state);
             while (state != FilterState.Running && tr != 0)
             {
                tr = mediaControl.GetState(0, out state);
             };


             mediaControl.Pause();
             tr = mediaControl.GetState(0, out state);
             while (state != FilterState.Running && tr != 0)
             {
                 tr = mediaControl.GetState(0, out state);
             };

//             DsError.ThrowExceptionForHR(hr);
             snapImage(outputFileName);
             CloseInterfaces();
         }


         private void snapImage(string outFileName )
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
    }
}
