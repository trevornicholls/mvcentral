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

    bool ProvidesDetails { get; }
    bool ProvidesAlbumArt { get; }
    bool ProvidesArtistArt { get; }
    bool ProvidesTrackArt { get; }

    List<DBTrackInfo> Get(MusicVideoSignature mvSignature);
    UpdateResults Update(DBTrackInfo movie);
    bool GetDetails(DBBasicInfo mv);
    bool GetArtistArt(DBArtistInfo mv);
    bool GetAlbumArt(DBAlbumInfo mv);
    bool GetTrackArt(DBTrackInfo mv);
    bool GetAlbumDetails(DBBasicInfo basicInfo, string albumTitle, string AlbumMBID);
  }

  public interface IScriptableMusicVideoProvider : IMusicVideoProvider
  {
    int ScriptID { get; }
    DateTime? Published { get; }

    bool Load(string script);
    bool DebugMode { get; set; }
  }

  public enum DataType { DETAIL, ARTIST, ALBUM, TRACK }
  public enum UpdateResults { SUCCESS, FAILED_NEED_ID, FAILED }
}
