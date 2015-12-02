using Cornerstone.Database;
using Cornerstone.Database.Tables;

using System.IO;

namespace mvCentral.Database
{
    public enum MenuBackdropType { RANDOM, MOVIE, FILE }

    [DBTable("musicvideo_node_settings")]
    public class DBMusicVideoNodeSettings : DatabaseTable
    {
        [DBField]
        public MenuBackdropType BackdropType {
            get { return _backdropType; }
            set { 
                _backdropType = value;
                commitNeeded = true;
            }
        } private MenuBackdropType _backdropType = MenuBackdropType.RANDOM;

        [DBField (Default=null)]
        public DBTrackInfo BackdropMusicVideo
        {
            get { return _backdropMusicVideo; }
            set {
                _backdropMusicVideo = value;
                commitNeeded = true;
            }
        } private DBTrackInfo _backdropMusicVideo;

        public FileInfo BackdropFile {
            get { return fileInfo; }
            set {
                fileInfo = value;
                commitNeeded = true;
            }
        }
        private FileInfo fileInfo;

        [DBField]
        public string BackdropFilePath {
            get {
                if (fileInfo == null)
                    return "";

                return fileInfo.FullName;
            }

            set {
                if (value.Trim() == "")
                    fileInfo = null;
                else
                    fileInfo = new FileInfo(value);

                if (fileInfo != null && !fileInfo.Exists)
                    fileInfo = null;

                commitNeeded = true;
            }
        }

        [DBField(Default="true")]
        public bool UseDefaultSorting {
            get { return _useDefaultSorting; }
            set {
                _useDefaultSorting = value;
                commitNeeded = true;
            }
        } private bool _useDefaultSorting = true;

/*        [DBField(Default="Title")]
        public SortingFields SortField {
            get { return _sortingField; }
            set {
                _sortingField = value;
                commitNeeded = true;
            }
        } private SortingFields _sortingField = SortingFields.Title;

        [DBField]
        public SortingDirections SortDirection {
            get { return _sortDirection; }
            set {
                _sortDirection = value;
                commitNeeded = true;
            }
        } private SortingDirections _sortDirection = SortingDirections.Ascending;

        // movie view assigned to the category
        // possible options include PARENT, LIST, SMALLICON, LARGEICON, FILMSTRIP, LASTUSED
        [DBField]
        public BrowserViewMode MusicVideoView
        {
            get { return _musicvideoView; }
            set {
                if (value == BrowserViewMode.PARENT
                    || value == BrowserViewMode.LIST
                    || value == BrowserViewMode.SMALLICON
                    || value == BrowserViewMode.LARGEICON
                    || value == BrowserViewMode.FILMSTRIP
                    || value == BrowserViewMode.LASTUSED
                    ) {
                    _musicvideoView = value;
                    commitNeeded = true;
                }
            }
        } private BrowserViewMode _musicvideoView = BrowserViewMode.PARENT;

        // view that is used when the movie view is set to "lastused"
        // possible options include LIST, SMALLICON, LARGEICON, FILMSTRIP
        [DBField]
        public Nullable<BrowserViewMode> LastMusicVideoView
        {
            get { return _lastMusicVideoView; }
            set {
                if (value == BrowserViewMode.LIST
                    || value == BrowserViewMode.SMALLICON
                    || value == BrowserViewMode.LARGEICON
                    || value == BrowserViewMode.FILMSTRIP
                    ) {
                        _lastMusicVideoView = value;
                    commitNeeded = true;
                }
            }
        } private Nullable<BrowserViewMode> _lastMusicVideoView = null;
*/    }
}
