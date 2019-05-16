/* $Date: 2011-01-12 20:07:44 +0100 (on, 12 jan 2011) $    $Revision: 7273 $ */
namespace NFN {

  using System;
  using System.Web;
  using System.Web.UI;
  using System.Web.SessionState;
  using System.Net.Mail;
  using System.IO;
  using System.Configuration;
  using System.Text.RegularExpressions;
  using System.Data;
  using System.Data.Odbc;
  using System.Data.SqlClient;
  using System.Collections;
  using System.Drawing;
  using System.Net;
  using System.Globalization;
  using NFN;
  using NFN.Controls;

  using System.Net.Sockets;
  using System.Text;
  using System.Diagnostics;
  using System.Threading;

  /********************************************************************************
  *
  * Class Util
  *
  *********************************************************************************/

  /// <summary>Contains static general methods</summary>
  public class Util {

    private static String GetResourceFiles(String tag) {
      String res = "";
      StreamReader sr = File.OpenText(HttpContext.Current.Server.MapPath("resources.txt"));

      String aline = sr.ReadLine();
      while(aline != null && !aline.StartsWith(tag)) aline = sr.ReadLine();
      if (aline != null) aline = sr.ReadLine();

      while(aline != null && !aline.StartsWith("<")) {
        if (aline.Length > 0) { 
          if (res.Length > 0) res += ",";
          res += aline;
        }
        aline = sr.ReadLine();
      }
      sr.Close();
      return res;
    }
    
    public static String[] GetCSSFiles() {
      CMS cms = (CMS)HttpContext.Current.Session["CMS"];
      String files = Util.GetResourceFiles("<CSS - Common>");
      if (cms.User.LoggedIn) {
        String help = Util.GetResourceFiles("<CSS - Loggedin>");
        if (help.Length > 0) files += "," + help;
      }
      if (cms.User.IsAdmin) {
        String help = Util.GetResourceFiles("<CSS - Admin>");
        if (help.Length > 0) files += "," + help;
      }
      return files.Split(',');
    }

    public static String[] GetJSFiles() {
      CMS cms = (CMS)HttpContext.Current.Session["CMS"];
      String files = Util.GetResourceFiles("<JS - Common>");
      if (cms.User.LoggedIn) {
        String help = Util.GetResourceFiles("<JS - Loggedin>");
        if (help.Length > 0) files += "," + help;
      }
      if (cms.User.IsAdmin) {
        String help = Util.GetResourceFiles("<JS - Admin>");
        if (help.Length > 0 && help.IndexOf("admin/tinymce") < 0) files += "," + help;
      }
      return files.Split(',');
    }

    public static bool IsValidEmail(String email, bool emptyok) {
      return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
    }

    public static int GetWeekNumber(DateTime adate) {
      CultureInfo ciCurr = CultureInfo.CurrentCulture;
      int weekNum = ciCurr.Calendar.GetWeekOfYear(adate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
      return weekNum;
    }

    private static String GetData(NetworkStream oStream) {
      byte[] bResponse = new byte[1024];
      String sResponse = "";
      int lenStream = oStream.Read(bResponse, 0, 1024);
      if (lenStream > 0)
        sResponse = Encoding.ASCII.GetString(bResponse, 0, 1024);
      return sResponse;
    }

    private static String SendData(NetworkStream oStream, String sToSend) {
      byte[] bArray = Encoding.ASCII.GetBytes(sToSend.ToCharArray());
      oStream.Write(bArray, 0, bArray.Length);
      return GetData(oStream);
    }

    private static bool ValidResponse(String sResult) {
      bool bResult = false;
      int iFirst = 100;

      if (sResult.Length > 1) {
        iFirst = Convert.ToInt32(sResult.Substring(0, 1));
        if (iFirst < 3) bResult = true;
      }
      return bResult;
    }

    private static String NSLookup(String sDomain) {
      ProcessStartInfo info = new ProcessStartInfo();
      info.UseShellExecute = false;
      info.RedirectStandardInput = true;
      info.RedirectStandardOutput = true;
      info.FileName = "nslookup";
      info.Arguments = "-type=MX " + sDomain.ToUpper().Trim();

      Process ns = Process.Start(info);
      StreamReader sout = ns.StandardOutput;

      //String sreg = "mail exchanger = (?<server>[^\\\s]+)";
      String sreg = "mail exchanger = (?<server>[^\\\\s]+)";
      Regex reg = new Regex(sreg);
      String mailserver = "";
      String response = "";

      bool found = false;
      while (!found && sout.Peek() > -1) {
        response = sout.ReadLine();

        Match amatch = reg.Match(response);

        if (amatch.Success) {
          mailserver = amatch.Groups["server"].Value;
          found = true;
        }
      }
      return mailserver;
    }

    public enum ValidEmailResponse : int {
      OK = 0,
      EMPTY = 1,
      MALFORMED = 2,
      DNS_FAILED = 3,
      ADDRESS_FAILED = 4,
      ERROR = 5
    }

    public static ValidEmailResponse IsValidEmail(String email, bool emptyok, bool CheckDomain, bool CheckSMTP) {
      if (email.Length == 0) return (emptyok ? ValidEmailResponse.OK : ValidEmailResponse.EMPTY);

      if (!IsValidEmail(email, false)) return ValidEmailResponse.MALFORMED;

      if (CheckDomain) {
        String to = "<" + email + ">";
        String[] hlp = email.Split('@');
        String mserver = NSLookup(hlp[1]);
        if (mserver.Length == 0)
          return ValidEmailResponse.DNS_FAILED;

        if (CheckSMTP) {
          String remote_addr = "040.se";
          String from = "<info@" + remote_addr + ">";

          TcpClient tcp = new TcpClient();
          try {
            tcp.SendTimeout = 3000;
            tcp.Connect(mserver, 25);
            NetworkStream stream = tcp.GetStream();
            String response = GetData(stream);
            response = SendData(stream, "HELO " + remote_addr + "\r\n");
            response = SendData(stream, "MAIL FROM: " + from + "\r\n");

            if (ValidResponse(response)) {
              response = SendData(stream, "RCPT TO: " + to + "\r\n");
              if (ValidResponse(response)) return ValidEmailResponse.OK;
              else return ValidEmailResponse.ADDRESS_FAILED;
            }

            SendData(stream, "QUIT" + "\r\n");
            tcp.Close();
            stream = null;
          }
          catch (Exception ex) {
            String mess = ex.Message;
            return ValidEmailResponse.ERROR;
          }
        }
      }
      return ValidEmailResponse.OK;
    }

    public static bool IsValidDate(String date) {
      try {
        DateTime.Parse(date);
        return true;
      }
      catch {
        return false;
      }
    }

    public static void Debug(String txt) {
      HttpServerUtility server = HttpContext.Current.Server;
      StreamWriter sw = File.AppendText(server.MapPath("/debug.txt"));
      sw.WriteLine(txt);
      sw.Close();    
    }

    public static String SendMail(String from, String to, String subject, String content, bool html) {
      try {
        MailMessage message = new MailMessage();
        
        if (from == null || from.Length == 0) {
          System.Net.Configuration.SmtpSection smtpSect = (System.Net.Configuration.SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
          from = smtpSect.From;
        }
        message.From = new MailAddress(from);
        message.To.Add(new MailAddress(to));
        message.Subject = subject;
        message.SubjectEncoding = System.Text.Encoding.Default;
        message.BodyEncoding =  System.Text.Encoding.Default;
        message.IsBodyHtml = html;
        message.Body = content;

        SmtpClient client = new SmtpClient();
        client.Send(message);
        return "";
      }
      catch (Exception ex) {
        return ex.Message;
      }
    }

    public static void SendMail(String from, String[] to, String subject, String content, bool html) {
      for (int i=0; i < to.Length; i++)
        SendMail(from, to[i], subject, content, html);
    }

    public static String GetNow() {
      return DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat"));
    }

    public static String HtmlRoot {
      get { return (HttpRuntime.AppDomainAppVirtualPath.Length > 1 ? HttpRuntime.AppDomainAppVirtualPath : ""); }
    }

    public static String GetBaseUrl(HttpRequest req) {
      return "http://" + req.ServerVariables["HTTP_HOST"] + "/";
    }
    
    public static String GetBaseUrl() {
      //return HttpContext.Current.Request.RawUrl;
      return HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
    }
    
    private static bool HeaderHasControlWithId(Page page, String id) {
      bool found = false;
      for (int i = 0; i < page.Header.Controls.Count && !found; i++) 
        found = (page.Header.Controls[i].ID == id);
      return found;
    }

    public static bool ConfigStringMatches(String configstr, String matches) {
      if (ConfigurationManager.AppSettings[configstr] == null) return false;
      return (ConfigurationManager.AppSettings[configstr].ToLower() == matches.ToLower());
    }

    /// <summary>Replaces any swedish characters with their html equivalents</summary>
    /// <param name="txt">String in which replacement sould be done.</param>
    public static String ReplaceHtmlChars(String txt) {
      return txt.Replace("å", "&aring;").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("Å", "&Aring;").Replace("Ä", "&Auml;").Replace("Ö", "&Ouml;").Replace("æ", "&aelig;").Replace("ø", "&oslash;").Replace("Æ", "&AElig;").Replace("Ø", "&Oslash;").Replace("ü", "&uuml;").Replace("Ü", "&Uuml;").Replace("é", "&eacute;").Replace("É", "&Eacute;");
    }


    private static Control FindInControl(Control ctrl, String id) {
      Control resctrl = null;
      for (int i = 0; i < ctrl.Controls.Count && resctrl == null; i++) {
        resctrl = ctrl.Controls[i].FindControl(id);
        if (resctrl == null)
          resctrl = FindInControl(ctrl.Controls[i], id);
      }
      return resctrl;
    }

    public static Control FindInPage(Page page, String id) {
      return FindInControl(page, id);
    }
    
    public static String GetThumbnail(String url, int width, int height) {
      return GetThumbnail(url, width, height, true, null);
    }
    
    public static String GetThumbnailExact(String url, int newwidth, int newheight) {
      return GetThumbnailExact(url, newwidth, newheight, null);
    }

    public static String GetThumbnailExact(String url, int newwidth, int newheight, String errgif) {
      String absUrl;
      if (url.StartsWith("/")) {
        if (url.StartsWith(NFN.Util.HtmlRoot)) absUrl = url;
        else absUrl = NFN.Util.HtmlRoot + url;
      }
      else
        absUrl = NFN.Util.HtmlRoot + "/" + url;

      HttpServerUtility server = HttpContext.Current.Server;
      if (!File.Exists(server.MapPath(absUrl))) return errgif;

      try{
        Bitmap origBitmap = new Bitmap(server.MapPath(absUrl));
        int oriheight = origBitmap.Height;
        int oriwidth = origBitmap.Width;

        if (Convert.ToDouble(oriheight)/Convert.ToDouble(oriwidth) > Convert.ToDouble(newheight)/Convert.ToDouble(newwidth)) 
          return NFN.Util.GetThumbnail(url, newwidth, 0, true, errgif);
        else 
          return NFN.Util.GetThumbnail(url, 0, newheight, true, errgif);
      }
      catch {
        return errgif;
      }
    }

    public static String GetThumbnail(String url, int width, int height, bool keepaspect, String errthumb) {
    
      HttpServerUtility server = HttpContext.Current.Server;
      if (errthumb == null) errthumb = "admin/DocumentBank/gfx/thumberror.gif";
      String absUrl;
      if (url.StartsWith("/")) {
        if (url.StartsWith(HtmlRoot)) absUrl = url;
        else absUrl = HtmlRoot + url;
      }
      else
        absUrl = NFN.Util.HtmlRoot + "/" + url;
      String fpath = absUrl.Substring(0, absUrl.Length - Path.GetFileName(absUrl).Length);

      String fname = Path.GetFileNameWithoutExtension(absUrl);
      String thumbName = fpath + "thumbs/" + fname + "_" + width + "x" + height + Path.GetExtension(absUrl);

      if (File.Exists(server.MapPath(thumbName))) 
        return thumbName;
      
      if (!File.Exists(server.MapPath(absUrl))) 
        return HttpRuntime.AppDomainAppVirtualPath + errthumb;
      
      
      try{
        Bitmap origBitmap = new Bitmap(server.MapPath(absUrl));
        int origHeight = origBitmap.Height;
        int origWidth = origBitmap.Width;
        int reqHeight = origHeight;
        int reqWidth = origWidth;
        int newHeight = origHeight;
        int newWidth = origWidth;
        
        if (origWidth <= width && origHeight <= height)
          return url;

        if (height != 0) reqHeight = height;
        if(width != 0) reqWidth = width;

        if (keepaspect) {
          double aspect = (double)origHeight / (double)origWidth;
          if (reqWidth <= newWidth) {
            newWidth = reqWidth;
            newHeight = Convert.ToInt32(Math.Round(newWidth*aspect));
          }
          if (reqHeight < newHeight) {
            newHeight = reqHeight;
            newWidth = Convert.ToInt32(Math.Round(1/aspect*newHeight));
          }
        }
        else {
          newHeight = reqHeight;
          newWidth = reqWidth;
        }
        if (newHeight <= 0) newHeight = 1;
        if (newWidth <= 0) newWidth = 1;

        Bitmap outputImage = new Bitmap(newWidth, newHeight);
        try {
          Graphics g = Graphics.FromImage(outputImage);
          g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
          g.FillRectangle(Brushes.White, 0, 0, newWidth, newHeight);
          g.DrawImage(origBitmap, 0, 0, newWidth, newHeight);
        }
        catch {
          Graphics g = Graphics.FromImage(outputImage);
          g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
          g.FillRectangle(Brushes.Gray, 0, 0, newWidth, newHeight);
        }

        Hashtable imageOutputFormatsTable = new Hashtable();
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Gif.Guid, System.Drawing.Imaging.ImageFormat.Gif);
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Jpeg.Guid, System.Drawing.Imaging.ImageFormat.Jpeg);
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Bmp.Guid, System.Drawing.Imaging.ImageFormat.Gif);
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Tiff.Guid, System.Drawing.Imaging.ImageFormat.Jpeg);
        imageOutputFormatsTable.Add(System.Drawing.Imaging.ImageFormat.Png.Guid, System.Drawing.Imaging.ImageFormat.Png);

        System.Drawing.Imaging.ImageFormat outputFormat = (System.Drawing.Imaging.ImageFormat)imageOutputFormatsTable[origBitmap.RawFormat.Guid];

        if (!Directory.Exists(server.MapPath(fpath + "thumbs")))
          Directory.CreateDirectory(server.MapPath(fpath + "thumbs"));

        outputImage.Save(server.MapPath(thumbName), outputFormat);
        outputImage.Dispose();
        origBitmap.Dispose();
        
        int tries = 0;
        while (!File.Exists(server.MapPath(thumbName)) && tries < 100) {
          System.Threading.Thread.Sleep(100);
          tries++;
        }

        return thumbName;
      }
      catch {
        return HttpRuntime.AppDomainAppVirtualPath + errthumb;
      }
    
    }

    private delegate IPHostEntry GetHostEntryHandler(string ip);

    public static String GetReverseDNS(string ip, int timeout) {
      try {
        GetHostEntryHandler callback = new GetHostEntryHandler(Dns.GetHostEntry);
        IAsyncResult result = callback.BeginInvoke(ip, null, null);
        if (result.AsyncWaitHandle.WaitOne(timeout, false))
          return callback.EndInvoke(result).HostName;
        else
          return ip;
      }
      catch (Exception) {
        return ip;
      }
    }
    
    public static String GetReverseDNS(String ip) {
      try {
        IPAddress myIP = IPAddress.Parse(ip);
        IPHostEntry GetIPHost = Dns.GetHostByAddress(myIP);
        return GetIPHost.HostName;
      }
      catch {
        return "Error";
      }
    }
    

  }

  /********************************************************************************
  *
  * Class Scheduler
  *
  *********************************************************************************/

  public class Scheduler {

    private Thread athread;
    private int interval;
    private bool running = false;

    public Scheduler(int interval) {
      this.interval = interval;
      athread = new Thread(new ThreadStart(this.Timer));
    }

    public void Start() {
      if (!this.running) {
        this.running = true;
        athread.Start();
      }
    }

    public void Stop() {
      this.running = false;
    }

    private void Timer() {
      while (this.running) {
        EventArgs e = new EventArgs();
        OnEvent(e);
        Thread.Sleep(this.interval * 1000);
      }
    }

    public event EventHandler ScheduledEvent;

    protected virtual void OnEvent(EventArgs e) {
      if (ScheduledEvent != null) ScheduledEvent(this, e);
    }
  }


  /********************************************************************************
  *
  * Class UrlRewriter
  *
  *********************************************************************************/

  /// <summary>Class for handling http requests and perform url rewrite</summary>
  public class UrlRewriter : IHttpModule {

    /// <summary></summary>
    public void Init(HttpApplication app) {
      app.BeginRequest += new EventHandler(OnBeginRequest);
    }

    /// <summary></summary>
    public void Dispose() { }

    private String TransformUrl(String url) {

      String[] split = url.Split('?');

      String ext = Path.GetExtension(split[0]);
      if ((split.Length > 1 && split[1].StartsWith("pageid=")) || split[0].IndexOf('/', 1) != -1 || split[0].Length < 5 || ext != ".aspx") {
        if (split[0].IndexOf("ajaxpro") == -1 && !File.Exists(HttpContext.Current.Server.MapPath("~/" + split[0]))) {
          if (File.Exists(HttpContext.Current.Server.MapPath("~/404.aspx")))
            return "~/404.aspx";
          else {
            String defaultPage = CMS.SiteSetting("defaultPage");
            String fileName = DB.GetString("select filename from webpage where deleted=0 and id='" + defaultPage + "'", "filename");
            return fileName + "?pageid=" + defaultPage;
          }
        }
        else return "";
      }
      else {
        String pageId = Path.GetFileNameWithoutExtension(split[0]);
        if (pageId.ToLower() == "default") 
          pageId = CMS.SiteSetting("defaultPage");
        String param = (split.Length < 2 ? "" : "&" + split[1]);

        String defaultPage = CMS.SiteSetting("defaultPage");

        String fileName = DB.GetString("select filename from webpage where deleted=0 and id='" + pageId + "'", "filename");
        if (fileName.Length == 0) {
          if (File.Exists(HttpContext.Current.Server.MapPath("~/" + split[0]))) return "";
          if (File.Exists(HttpContext.Current.Server.MapPath("~/404.aspx")))
            return "~/404.aspx";
          else {
            fileName = DB.GetString("select filename from webpage where deleted=0 and id='" + defaultPage + "'", "filename");
            pageId = defaultPage;
          }
        }

        return "~/" + fileName + "?pageid=" + pageId + param;
      }
    }

    /// <summary></summary>
    public void OnBeginRequest(Object s, EventArgs e) {
      String url = HttpContext.Current.Request.RawUrl;
      url = url.Substring(HttpRuntime.AppDomainAppVirtualPath.Length);
      if (!url.StartsWith("/")) url = "/" + url;

      String redirect = TransformUrl(url);
      if (redirect.Length > 0)
        HttpContext.Current.RewritePath(redirect);
    }

  }
  
  
  /********************************************************************************
  *
  * Class RemotePost
  *
  *********************************************************************************/

  public class RemotePost {
    private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();
    public string Url = "";
    public string Method = "post";
    public string FormName = "form1";

    public void Add(string name,string value) {
      Inputs.Add(name,value);
    }
  
    public void Post() {
      System.Web.HttpContext.Current.Response.Clear();

      System.Web.HttpContext.Current.Response.Write("<html><head>");

      System.Web.HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit();window.history.back(); \">",FormName));
      System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" target=\"_blank\" action=\"{2}\" >",FormName,Method,Url));
      for(int i=0;i< Inputs.Keys.Count;i++){
        System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">",Inputs.Keys[i],Inputs[Inputs.Keys[i]]));
      }
      System.Web.HttpContext.Current.Response.Write("</form>");
      System.Web.HttpContext.Current.Response.Write("</body></html>");

      System.Web.HttpContext.Current.Response.End();
    }
  }


}
