/* $Date: 2010-03-15 01:16:02 +0100 (må, 15 mar 2010) $    $Revision: 6083 $ */
using System;
using System.Data;
using System.Collections;
using AjaxPro;

namespace NFN {

  public class CalendarPage : BasePage {

    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);
      AjaxPro.Utility.RegisterTypeForAjax(typeof(CalendarPage), this);
    }

    private String GetDateNr(int nr) {
      if (nr < 10) return "0" + nr.ToString();
      else return nr.ToString();
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String[] GetCalendarEvents(String pageId, DateTime first, DateTime last) {
      ArrayList res = new ArrayList();
//      res.Add(Cms.User.IsAdmin ? "Y" : "N");
      
      DateTime day = first;
      WebPage aPage = Cms.GetPageById(pageId);
      while (day <= last) {
        String dateStr = day.Year.ToString() + GetDateNr(day.Month) + GetDateNr(day.Day);

        bool hasEvent = aPage.MainProp.HasProperty("s_" + dateStr);
        String eventTxt = (hasEvent ? aPage.MainProp.GetProperty("s_" + dateStr).ControlTypeProperty.Value : "");

        res.Add(eventTxt);
        day = day.AddDays(1);
      }
      String[] events = (String[])res.ToArray( typeof( string ) );
      return events;
    }
      
    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void CreateEvent(String pageId, String dateStr) {
      WebPage aPage = Cms.GetPageById(pageId);
      PageProperty aProp = aPage.MainProp.GetProperty("s_" + dateStr);
      aProp.ControlTypeProperty.SetValuesDirectly("Ny hndelse", "Ny hndelse");
      aProp.ControlTypeProperty.WriteToDB();
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public void DeleteEvent(String pageId, String dateStr) {
      WebPage aPage = Cms.GetPageById(pageId);
      if (aPage.MainProp.HasProperty("s_" + dateStr))
        aPage.MainProp.GetProperty("s_" + dateStr).Delete();
      if (aPage.MainProp.HasProperty(dateStr))
        aPage.MainProp.GetProperty(dateStr).Delete();
    }

    [AjaxPro.AjaxMethod(HttpSessionStateRequirement.Read)]
    public String SaveEvent(String pageId, String dateStr, String value) {
      WebPage aPage = Cms.GetPageById(pageId);
      PageProperty aProp = aPage.MainProp.GetProperty("s_" + dateStr);
      String htmlval = value.Replace("\r\n", "<br>").Replace("\n", "<br>");
      aProp.ControlTypeProperty.SetValuesDirectly(htmlval, htmlval);
      aProp.ControlTypeProperty.WriteToDB();
      return htmlval;
    }

  }

}
