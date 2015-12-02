using mvCentral.Database;

namespace mvCentral.DataProviders
{
    abstract class InternalProvider {

        public virtual DBSourceInfo SourceInfo {
            get {
                if (_sourceInfo == null)
                    _sourceInfo = DBSourceInfo.GetFromProviderObject((IMusicVideoProvider)this);

                return _sourceInfo;
            }
        } private DBSourceInfo _sourceInfo;        

        public virtual string Author { 
            get { 
                return "mvCentral Team";
            }
        }

        public virtual string Version {
            get {
                return "Internal";
            }
        }

    }
}
