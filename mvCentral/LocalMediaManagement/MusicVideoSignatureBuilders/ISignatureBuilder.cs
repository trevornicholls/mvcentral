namespace mvCentral.SignatureBuilders {

    public enum SignatureBuilderResult {
        INCONCLUSIVE,
        CONCLUSIVE        
    }
    
    public interface ISignatureBuilder {

        SignatureBuilderResult UpdateSignature(MusicVideoSignature signature);

    }
}
