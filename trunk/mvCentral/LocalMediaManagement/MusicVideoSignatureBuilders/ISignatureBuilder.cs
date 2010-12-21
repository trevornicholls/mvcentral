using System;
using System.Collections.Generic;
using System.Text;

namespace mvCentral.SignatureBuilders {

    public enum SignatureBuilderResult {
        INCONCLUSIVE,
        CONCLUSIVE        
    }
    
    public interface ISignatureBuilder {

        SignatureBuilderResult UpdateSignature(MusicVideoSignature signature);

    }
}
