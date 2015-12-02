using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mvCentral.Extractors
{
  public class BlurayExtractor : ChapterExtractor
  {
    public override string[] Extensions
    {
      get { return new string[] { }; }
    }

    public override List<ChapterInfo> GetStreams(string location, int numtitle)
    {
      List<ChapterInfo> pgcs = new List<ChapterInfo>();
      string path = Path.Combine(Path.Combine(location, "BDMV"), "PLAYLIST");
      if (!Directory.Exists(path))
        throw new FileNotFoundException("Could not find PLAYLIST folder on BluRay disc.");

      ChapterExtractor ex = new BDInfoExtractor();
      ex.StreamDetected += (sender, args) => OnStreamDetected(args.ProgramChain);
      ex.ChaptersLoaded += (sender, args) => OnChaptersLoaded(args.ProgramChain);

      foreach (string file in Directory.GetFiles(path, "*.mpls"))
      {
        pgcs.Add(ex.GetStreams(file,numtitle)[0]);
      }

      pgcs = pgcs.OrderByDescending(p => p.Duration).ToList();
      OnExtractionComplete();
      return pgcs;
    }
  }
}
