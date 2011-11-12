using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Net;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;

using mvCentral.LocalMediaManagement;

using NLog;

namespace mvCentral.Utils
{
  internal class Request
  {

    // Create logger
    private static Logger logger = LogManager.GetCurrentClassLogger();

    const string ROOT = "http://ws.audioscrobbler.com/2.0/";

    public string MethodName { get; private set; }
    public Session Session { get; private set; }

    public RequestParameters Parameters { get; private set; }

    internal static DateTime? lastCallTime { get; set; }

    public Request(string methodName, Session session, RequestParameters parameters)
    {
      this.MethodName = methodName;
      this.Session = session;
      this.Parameters = parameters;

      this.Parameters["method"] = this.MethodName;
      this.Parameters["api_key"] = this.Session.APIKey;
      if (Session.Authenticated)
      {
        this.Parameters["sk"] = this.Session.SessionKey;
        signIt();
      }
    }

    internal void signIt()
    {
      // because auth.getSession requires a signature without session key. 
      this.Parameters["api_sig"] = this.getSignature();
    }

    private string getSignature()
    {
      string str = "";
      foreach (string key in this.Parameters.Keys)
        str += key + this.Parameters[key];

      str += this.Session.APISecret;
      return  LastFMScrobble.MD5(str);
    }

    private void delay()
    {
      // If the last call was less than one second ago, it would delay execution for a second.

      if (Request.lastCallTime == null)
        Request.lastCallTime = new Nullable<DateTime>(DateTime.Now);

      if (DateTime.Now.Subtract(Request.lastCallTime.Value) > new TimeSpan(0, 0, 1))
        Thread.Sleep(1000);
    }

    public XmlDocument execute()
    {
      string lfm_request = ROOT;
      lfm_request += Parameters;
      return getXML(lfm_request);
    }

    private void checkForErrors(XmlDocument document)
    {
      XmlNode n = document.GetElementsByTagName("lfm")[0];

      string status = n.Attributes[0].InnerText;

      if (status == "failed")
      {
        XmlNode err = document.GetElementsByTagName("error")[0];
        ServiceExceptionType type = (ServiceExceptionType)Convert.ToInt32(err.Attributes[0].InnerText);
        string description = err.InnerText;

        throw new ServiceException(type, description);
      }
    }

    // given a url, retrieves the xml result set and returns the nodelist of Item objects
    private static XmlDocument getXML(string url)
    {
      XmlDocument xmldoc = new XmlDocument();

      //logger.Debug("Sending the request: " + url.Replace("eadfb84ac56eddbf072efbfc18a90845","<apiKey>"));
      logger.Debug("Sending the request: " + url);

      mvWebGrabber grabber = Utility.GetWebGrabberInstance(url);
      grabber.Encoding = Encoding.UTF8;
      grabber.Timeout = 5000;
      grabber.TimeoutIncrement = 10;
      grabber.Method = "POST";
      if (grabber.GetResponse())
      {
        return grabber.GetXMLDoc();
      }
      else
      {
        logger.Debug("***** API ERROR *****: Code:{0} ({1})", grabber.errorCode, grabber.errorText);
        return null;
      }
    }


  }

}
