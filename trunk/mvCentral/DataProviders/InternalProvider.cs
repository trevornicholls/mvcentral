using System.Collections.Generic;
using mvCentral.Database;
using mvCentral.SignatureBuilders;

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
                return "Music Videos Team";
            }
        }

        public virtual string Version {
            get {
                return "Internal";
            }
        }

    }
}
