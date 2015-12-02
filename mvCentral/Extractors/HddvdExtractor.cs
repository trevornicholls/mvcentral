using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mvCentral.Extractors
{
  public class HddvdExtractor : ChapterExtractor
  {
    public override string[] Extensions
    {
      get { return new string[] { }; }
    }

    public override List<ChapterInfo> GetStreams(string location, int numtitle)
    {
      List<ChapterInfo> pgcs = new List<ChapterInfo>();
      string path = Path.Combine(location, "ADV_OBJ");
      if (!Directory.Exists(path))
        throw new FileNotFoundException("Could not find ADV_OBJ folder on HD-DVD disc.");

      ChapterExtractor ex = new XplExtractor();
      ex.StreamDetected += (sender, args) => OnStreamDetected(args.ProgramChain);
      ex.ChaptersLoaded += (sender, args) => OnChaptersLoaded(args.ProgramChain);

      foreach (string file in Directory.GetFiles(path, "*.xpl"))
      {
        pgcs.Add(ex.GetStreams(file , numtitle)[0]);
      }

      pgcs = pgcs.OrderByDescending(p => p.Duration).ToList();
      OnExtractionComplete();
      return pgcs;
    }
  }
}
