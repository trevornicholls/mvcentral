#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using MediaPortal.Configuration;
using MediaPortal.ServiceImplementations;
using MediaPortal.Profile;
using MediaPortal.Services;
using MediaPortal.Util;
using MediaPortal.Utils;
using NLog;


namespace mvCentral.Utils
{
  

  public class VideoThumbCreator
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();

    private const string ExtractApp = "ffmpeg.exe";
    private static readonly string ExtractorPath = MediaPortal.Configuration.Config.GetFile(MediaPortal.Configuration.Config.Dir.Base, "MovieThumbnailer", ExtractApp);
    private static int _previewColumns = 2;
    private static int _previewRows = 2;

    public string threadId
    {
      get
      {
        return Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
      }
    }

    #region Public methods

    public static bool CreateVideoThumb(string aVideoPath, string aThumbPath)
    {
      if (String.IsNullOrEmpty(aVideoPath) || String.IsNullOrEmpty(aThumbPath))
      {
        logger.Warn("VideoThumbCreator: Invalid arguments to generate thumbnails of your video!");
        return false;
      }
      if (!MediaPortal.Util.Utils.FileExistsInCache(aVideoPath))
      {
        logger.Warn("VideoThumbCreator: File {0} not found!", aVideoPath);
        return false;
      }
      if (!MediaPortal.Util.Utils.FileExistsInCache(ExtractorPath))
      {
        logger.Warn("VideoThumbCreator: No {0} found to generate thumbnails of your video!", ExtractApp);
        return false;
      }

      IVideoThumbBlacklist blacklist = GlobalServiceProvider.Get<IVideoThumbBlacklist>();
      if (blacklist != null && blacklist.Contains(aVideoPath))
      {
        logger.Debug("Skipped creating thumbnail for {0}, it has been blacklisted because last attempt failed", aVideoPath);
        return false;
      }

      // Params for ffmpeg
      // string ExtractorArgs = string.Format(" -i \"{0}\" -vframes 1 -ss {1} -s {2}x{3} \"{4}\"", aVideoPath, @"00:08:21", (int)Thumbs.ThumbLargeResolution, (int)Thumbs.ThumbLargeResolution, aThumbPath);

      // Params for mplayer (outputs 00000001.jpg in video resolution into working dir) -vf scale=600:-3
      //string ExtractorArgs = string.Format(" -noconsolecontrols -nosound -vo jpeg:quality=90 -vf scale -frames 1 -ss {0} \"{1}\"", "501", aVideoPath);

      // Params for mtm (http://moviethumbnail.sourceforge.net/usage.en.html)
      //   -D 8         : edge detection; 0:off >0:on; higher detects more; try -D4 -D6 or -D8
      //   -B 420/E 600 : omit this seconds from the beginning / ending TODO: use pre- / postrecording values
      //   -c 2 / r 2   : # of column / # of rows
      //   -b 0.60      : skip if % blank is higher; 0:skip all 1:skip really blank >1:off
      //   -h 100       : minimum height of each shot; will reduce # of column to fit
      //   -t           : time stamp off
      //   -i           : info text off
      //   -w 0         : width of output image; 0:column * movie width
      //   -n           : run at normal priority
      //   -W           : dont overwrite existing files, i.e. update mode
      //   -P           : dont pause before exiting; override -p
      //   -O directory : save output files in the specified directory

      // Use this for the working dir to be on the safe side
      var tempPath = Path.GetTempPath();


      _previewColumns = (int)mvCentralCore.Settings["videoThumbNail_cols"].Value;
      _previewRows = (int)mvCentralCore.Settings["videoThumbNail_rows"].Value;


      //string ExtractorArgs =         string.Format(" -D 0 -c {0} -r {1} -t -i -w {2} -n -O \"{3}\" -P \"{4}\"", PreviewColumns, PreviewRows, 0, TempPath, aVideoPath);
      // Honour we are using a unix app
      //ExtractorArgs = ExtractorArgs.Replace('\\', '/');

      const int preGapSec = 5;
      int postGapSec = 5;

      var strFilenamewithoutExtension = Path.ChangeExtension(aVideoPath, null);
      if (strFilenamewithoutExtension != null)
        strFilenamewithoutExtension = Path.Combine(tempPath, Path.GetFileName(strFilenamewithoutExtension));

      string ffmpegArgs = string.Format("select=isnan(prev_selected_t)+gte(t-prev_selected_t" + "\\" + ",5),yadif=0:-1:0,scale=600:337,setsar=1:1,tile={0}x{1}", _previewColumns, _previewRows);
      string extractorArgs = string.Format("-loglevel quiet -ss {0} -i \"{1}\" -vf {2} -vframes 1 -vsync 0 -an \"{3}_s.jpg\"", preGapSec, aVideoPath, ffmpegArgs, strFilenamewithoutExtension);
      string extractorFallbackArgs = string.Format("-loglevel quiet -ss {0} -i \"{1}\" -vf {2} -vframes 1 -vsync 0 -an \"{3}_s.jpg\"", 5, aVideoPath, ffmpegArgs, strFilenamewithoutExtension);

      
      try
      {
        
        string outputFilename = Path.Combine(tempPath, Path.GetFileName(aVideoPath));
        string outputThumb = string.Format("{0}_s{1}", Path.ChangeExtension(outputFilename, null), ".jpg");
        
        logger.Debug("ThreadID: {0} - About to start MTN process with {1}",Thread.CurrentThread.ManagedThreadId.ToString() , extractorArgs);
        Process  processStatus = MediaPortal.Util.Utils.StartProcess(ExtractorPath, extractorArgs, true, true);

        if (!processStatus.HasExited)
          logger.Debug("ThreadID:{0} - ffmpeg process not exited Status:{0)", Thread.CurrentThread.ManagedThreadId.ToString(),processStatus.ExitCode);
        else
          logger.Debug("ThreadID: {0} - Finished ffmpeg call with exit code:{1) using arguments {2}", Thread.CurrentThread.ManagedThreadId.ToString(), processStatus.ExitCode, extractorArgs);

        // give the system a few IO cycles
        Thread.Sleep(500);

        if (!File.Exists(outputThumb))
          logger.Debug("*** ERROR *** - After ffmpeg the file {0} from Video {1} does not exist", Path.GetFileName(outputThumb), Path.GetFileName(aVideoPath));

        try
        {
          // remove the _s which mdn appends to its files
          File.Move(outputThumb, aThumbPath);
        }
        catch (FileNotFoundException)
        {
          logger.Debug("VideoThumbCreator: {0} did not extract a thumbnail to: {1}", ExtractApp, outputThumb);
        }
        catch (Exception e)
        {
          try
          {
            // Clean up
            logger.DebugException("VideoThumbCreator: Exception in file move",e);
            File.Delete(outputThumb);
            Thread.Sleep(50);
          }
          catch (Exception) { }
        }

        Thread.Sleep(30);

      }
      catch (Exception ex)
      {
        logger.Debug("VideoThumbCreator: Thumbnail generation failed - {0}!", ex.ToString());
      }
      if (MediaPortal.Util.Utils.FileExistsInCache(aThumbPath))
      {
        return true;
      }
      else
      {
        if (blacklist != null)
        {
          blacklist.Add(aVideoPath);
        }
        return false;
      }
    }

    public static string GetThumbExtractorVersion()
    {
      try
      {
        //System.Diagnostics.FileVersionInfo newVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(ExtractorPath);
        //return newVersion.FileVersion;
        // mtn.exe has no version info, so let's use "time modified" instead
        FileInfo fi = new FileInfo(ExtractorPath);
        return fi.LastWriteTimeUtc.ToString("s"); // use culture invariant format
      }
      catch (Exception ex)
      {
        logger.DebugException("GetThumbExtractorVersion failed:",ex);
        return "";
      }
    }

    #endregion

  }
}