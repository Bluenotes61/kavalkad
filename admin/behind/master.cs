/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using NFN;

public class Master : MasterPage {

  public Repeater Menu;

  protected CMS Cms {
    get { return (CMS)Page.Session["CMS"]; }
  }


  private AdminBase BWPage {
    get { return (AdminBase)Page; }
  }

  protected override void OnLoad(EventArgs e) {
    base.OnLoad(e);
    if (!Cms.User.IsAdmin) Response.Redirect("~/admin/default.aspx");
    if (!IsPostBack) {
      Page.Title = BWPage.BWPageName;
      BindMenu();
    }
  }

  protected virtual void BindMenu() {
    DataTable dt = new DataTable();
    DataRow dr;

    dt.Columns.Add(new DataColumn("Text", typeof(string)));
    dt.Columns.Add(new DataColumn("Tooltip", typeof(string)));
    dt.Columns.Add(new DataColumn("Url", typeof(string)));
    dt.Columns.Add(new DataColumn("Enabled", typeof(bool)));

    dr = dt.NewRow();
    dr[0] = "Till sajten";
    dr[1] = "Tillbaka till sajten";
    dr[2] = "~/";
    dr[3] = true;
    dt.Rows.Add(dr);

    foreach (Hashtable page in Cms.GetBackWebPages()) {
      dr = dt.NewRow();
      dr[0] = page["Text"].ToString();
      dr[1] = page["Description"].ToString();
      dr[2] = "~/" + page["URL"].ToString();
      dr[3] = page["Name"].ToString() != BWPage.BWPageName;
      dt.Rows.Add(dr);
    }
    dr = dt.NewRow();
    dr[0] = "Logga ut";
    dr[1] = "Logga ut";
    dr[2] = "Logout.aspx";
    dr[3] = true;
    dt.Rows.Add(dr);

    Menu.DataSource = new DataView(dt);
    Menu.DataBind();
  }

}
