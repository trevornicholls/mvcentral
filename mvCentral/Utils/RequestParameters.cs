using System.Collections.Generic;
using System.Text;
using System.Web;

namespace mvCentral.Utils
{
  internal class RequestParameters : SortedDictionary<string, string>
  {
    public override string ToString()
    {
      string values = "";

      values = "?" + "method=" + this["method"] + "&";

      foreach (string key in this.Keys)
        if (key != "method")
        {
          values += HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(this[key]) + "&";
        }
      values = values.Substring(0, values.Length - 1);

      return values;
    }

    internal byte[] ToBytes()
    {
      return Encoding.ASCII.GetBytes(ToString());
    }

    internal string serialize()
    {
      string line = "";

      foreach (string key in Keys)
        line += key + "\t" + this[key] + "\t";

      return line;
    }

    internal RequestParameters(string serialization)
      : base()
    {
      string[] values = serialization.Split('\t');

      for (int i = 0; i < values.Length - 1; i++)
      {
        if ((i % 2) == 0)
          this[values[i]] = values[i + 1];
      }
    }

    public RequestParameters()
      : base()
    {
    }
  }
}
