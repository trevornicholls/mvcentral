using System;
using System.Collections.Generic;
using System.Text;
using mvCentral.Database;
using mvCentral.SignatureBuilders;

namespace mvCentral.DataProviders
{
  public interface IMusicVideoProvider
  {
    string Name { get; }
    string Version { get; }
    string Author { get; }
    string Description { get; }
    string Language { get; }
    string LanguageCode { get; }
    List<string> LanguageCodeList { get; }

    bool ProvidesTrackDetails { get; }
    bool ProvidesArtistDetails { get; }
    bool ProvidesAlbumDetails { get; }
    bool ProvidesAlbumArt { get; }
    bool ProvidesArtistArt { get; }
    bool ProvidesTrackArt { get; }

    List<DBTrackInfo> GetTrackDetail(MusicVideoSignature mvSignature);
    DBTrackInfo GetArtistDetail(DBTrackInfo mv);
    DBTrackInfo GetAlbumDetail(DBTrackInfo mv);

    UpdateResults UpdateTrack(DBTrackInfo trackData);
    bool GetDetails(DBBasicInfo mv);
    bool GetArtistArt(DBArtistInfo mv);
    bool GetAlbumArt(DBAlbumInfo mv);
    bool GetTrackArt(DBTrackInfo mv);
    bool GetAlbumDetails(DBBasicInfo basicInfo, string albumTitle, string AlbumMBID);

    event EventHandler ProgressChanged;
  }

  public class ProgressEventArgs : EventArgs
  {
      public string Text { get; set; }
  }

  public interface IScriptableMusicVideoProvider : IMusicVideoProvider
  {
    int ScriptID { get; }
    DateTime? Published { get; }

    bool Load(string script);
    bool DebugMode { get; set; }
  }

  public enum DataType { TRACKDETAIL, ARTISTDETAIL, ALBUMDETAIL, ARTISTART, ALBUMART, TRACKART }
  public enum UpdateResults { SUCCESS, FAILED_NEED_ID, FAILED }
}
