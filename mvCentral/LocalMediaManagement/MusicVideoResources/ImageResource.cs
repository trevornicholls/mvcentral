using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace mvCentral.LocalMediaManagement.MusicVideoResources
{
  public enum ImageLoadResults { SUCCESS, SUCCESS_REDUCED_SIZE, FAILED_TOO_SMALL, FAILED_ALREADY_LOADED, FAILED }

  public class ImageResource : FileBasedResource
  {

    public class ImageSize
    {
      public int Width;
      public int Height;
    }

    public string ThumbFilename
    {
      get;
      set;
    }

    protected void GenerateThumbnail()
    {
      throw new NotImplementedException();
    }

    public ImageLoadResults FromUrl(string url, bool ignoreRestrictions, ImageSize minSize, ImageSize maxSize, bool redownload)
    {
      // if this resource already exists
      if (File.Exists(Filename))
      {
        // if we are redownloading, just delete what we have
        if (redownload)
        {
          try
          {
            File.Delete(Filename);
            File.Delete(ThumbFilename);
          }
          catch (Exception) { }
        }
        // otherwise return an "already loaded" failure
        else
        {
          return ImageLoadResults.FAILED_ALREADY_LOADED;
        }
      }

      // try to grab the image if failed, exit
      if (!Download(url)) return ImageLoadResults.FAILED;

      // verify the image file and resize it as needed
      return VerifyAndResize(minSize, maxSize);
    }

    public ImageLoadResults FromFile(string path, bool ignoreRestrictions, ImageSize minSize, ImageSize maxSize, bool redownload)
    {
      // if this resource already exists
      if (File.Exists(Filename))
      {
        // if we are redownloading, just delete what we have
        if (redownload)
        {
          try
          {
            File.Delete(Filename);
            File.Delete(ThumbFilename);
          }
          catch (Exception) { }
        }
        // otherwise return an "already loaded" failure
        else
        {
          logger.Debug("Filename {2} file exists - retuen already loaded", Filename);
          return ImageLoadResults.FAILED_ALREADY_LOADED;
        }
      }
      // Copy the file to the correct location
      try
      {
        File.Copy(path, Filename);
      }
      catch (System.IO.IOException)
      {
        logger.Debug("Failed to copy file : {0} for path {1}", path, Filename);
        return ImageLoadResults.FAILED;

      }

      // verify the image file and resize it as needed
      if (ignoreRestrictions)
        return ImageLoadResults.SUCCESS;
      else
        return VerifyAndResize(minSize, maxSize);

    }

    protected ImageLoadResults VerifyAndResize(ImageSize minSize, ImageSize maxSize)
    {

      //logger.Debug("Using Min W {0} H {1}", minSize.Width, minSize.Height);

      Image img = null;
      try
      {
        ImageLoadResults rtn = ImageLoadResults.SUCCESS;

        // try to open the image
        img = Image.FromFile(Filename);
        int newWidth = img.Width;
        int newHeight = img.Height;

        //logger.Debug("Filename {0} has W {1} H {2}", Filename, newWidth, newHeight);

        // check if the image is too small
        if (minSize != null)
        {
          if (img.Width < minSize.Width || img.Height < minSize.Height)
          {
            img.Dispose();
            img = null;
            if (File.Exists(Filename)) File.Delete(Filename);
            return ImageLoadResults.FAILED_TOO_SMALL;
          }
        }

        // check if the image is too big
        if (maxSize != null)
        {
          if (img.Width > maxSize.Width || img.Height > maxSize.Height)
          {

            // calculate new dimensions
            newWidth = maxSize.Width;
            newHeight = maxSize.Width * img.Height / img.Width;

            if (newHeight > maxSize.Height)
            {
              newWidth = maxSize.Height * img.Width / img.Height;
              newHeight = maxSize.Height;
            }

            rtn = ImageLoadResults.SUCCESS_REDUCED_SIZE;
          }
        }

        // resize / rebuild image
        Image newImage = (Image)new Bitmap(newWidth, newHeight);
        Graphics g = Graphics.FromImage((Image)newImage);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(img, 0, 0, newWidth, newHeight);
        g.Dispose();
        img.Dispose();
        img = null;

        // determine compression quality
        int quality = mvCentralCore.Settings.JpgCompressionQuality;
        if (quality > 100) quality = 100;
        if (quality < 0) quality = 0;

        // save image as a jpg
        ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
        System.Drawing.Imaging.Encoder qualityParamID = System.Drawing.Imaging.Encoder.Quality;
        EncoderParameter qualityParam = new EncoderParameter(qualityParamID, quality);
        EncoderParameters encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = qualityParam;
        newImage.Save(Filename, jgpEncoder, encoderParams);
        newImage.Dispose();

        return rtn;
      }
      catch (Exception e)
      {
        logger.DebugException("Exception in VerifyAndResize : ", e);

        if (File.Exists(Filename)) File.Delete(Filename);
        return ImageLoadResults.FAILED;
      }
      finally
      {
        if (img != null) img.Dispose();
      }
    }

    private ImageCodecInfo GetEncoder(ImageFormat format)
    {
      ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

      foreach (ImageCodecInfo codec in codecs)
        if (codec.FormatID == format.Guid)
          return codec;

      return null;
    }
  }
}
