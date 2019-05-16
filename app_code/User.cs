/* $Date: 2010-02-12 00:32:36 +0100 (fr, 12 feb 2010) $    $Revision: 5940 $ */
using System;
using System.Web;
using System.Data;
using System.Collections;
using NFN;

namespace NFN {

  public class NFNUser {

    private int id = 0;
    private String usertype;
    private String username;
    private String password;
    private String email;
    private String description;
    private bool approved;
    private ArrayList userroles = new ArrayList();
    private Hashtable attribVals = new Hashtable();
    private String[][] attribFields;


    public NFNUser() {}

    public NFNUser(int id) {
      Id = id;
      Refresh();
    }

    public NFNUser(String username) {
      Id = DB.GetInt("select id from users where username='" + username + "'", "id");
      Refresh();
    }

    public void LogIn(int id) {
      Id = id;
      DB.ExecSql("insert into adminlog (userid, eventtime, eventaction) values (" + Id.ToString() + ", '" + DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat")) + "', 'login' )");
      Refresh();
    }


    /// <summary>Log out the currently logged in user.</summary>
    /// <param name="sessionid">Id string of the current session</param>
    public void LogOut(String sessionid) {
      if (IsAdmin && sessionid.Length > 0) {
        DB.ExecSql("insert into adminlog (userid, eventtime, eventaction) values (" + Id.ToString() + ", '" + DateTime.Now.ToString(CMS.SiteSetting("dateTimeFormat")) + "', 'logout' )");
        // Unlock all properties for this session
        DB.ExecSql("update pageproperty set locked='', locktime=NULL where locked='" + sessionid + "'");
      }
      usertype = "";
      username = "";
      password = "";
      email = "";
      description = "";
      userroles.Clear();
      attribVals.Clear();
      attribFields = null;
      Id = 0;
    }

    /// <summary>True if a user is currently logged in.</summary> 
    public bool LoggedIn {
      get { return Id != 0; }
    }

    /// <summary>Reads information about the currently logged in user from the database.</summary> 
    public void Refresh() {

      if (Id != 0) {
        DataSet ds = DB.GetDS("select * from users where deleted=0 and id=" + Id);
        usertype = DB.GetString(ds, 0, "usertype");
        username = DB.GetString(ds, 0, "username");
        password = DB.GetString(ds, 0, "password");
        email = DB.GetString(ds, 0, "email");
        description = DB.GetString(ds, 0, "description");
        approved = DB.GetString(ds, 0, "approved") == "Y";

        userroles.Clear();
        ds = DB.GetDS("select role from permissions where typeid=1 and id='" + Id + "'");
        for (int i = 0; i < DB.GetRowCount(ds); i++)
          userroles.Add(DB.GetString(ds, i, "role"));

        attribVals.Clear();
        ds = DB.GetDS("select * from userattribfields order by orderno");
        attribFields = new String[DB.GetRowCount(ds)][];
        for (int i = 0; i < DB.GetRowCount(ds); i++) {
          int fid = DB.GetInt(ds, i, "id");
          attribFields[i] = new String[5];
          attribFields[i][0] = fid.ToString();
          attribFields[i][1] = DB.GetString(ds, i, "name");
          attribFields[i][2] = DB.GetString(ds, i, "displayname");
          attribFields[i][3] = DB.GetString(ds, i, "mandatory");
          attribFields[i][4] = DB.GetString(ds, i, "regexp");

          DataSet ds2 = DB.GetDS("select * from userattrib where userid=" + Id + " and fieldid=" + fid);
          if (DB.GetRowCount(ds2) == 0 || DB.IsNull(ds2, 0, "svalue")) attribVals.Add(DB.GetString(ds, i, "name"), "");
          else attribVals.Add(DB.GetString(ds, i, "name"), DB.GetString(ds2, 0, "svalue"));
        }
      }
    }

    public void WriteToDB() {
      String oldpassword = DB.GetString("select password from users where deleted=0 and id=" + Id, "password");

      String sql = "update users set usertype='" + UserType + "', username='" + UserName + "', password='" + Password + "', email='" + Email + "', description='" + Description + "', approved='" + (Approved ? "Y" : "N") + "' where id=" + Id;
      DB.ExecSql(sql);
      for (int i = 0; i < attribFields.Length; i++) {
        String fid = attribFields[i][0];
        String aname = attribFields[i][1];
        String aval = (attribVals[aname] == null ? "null" : "'" + attribVals[aname].ToString() + "'");
        if (DB.RowExists("select userid from userattrib where userid=" + Id + " and fieldid=" + fid))
          sql = "update userattrib set svalue=" + aval + " where userid=" + Id + " and fieldid=" + fid;
        else
          sql = "insert into userattrib (userid, fieldid, svalue) values(" + Id + ", " + fid + ", " + aval + ")";
        DB.ExecSql(sql);
      }

      int ptid = DB.GetInt("select id from permissiontypes where itemtype='User'", "id");
      sql = "delete from permissions where id='" + Id + "' and typeid=" + ptid;
      DB.ExecSql(sql);
      foreach (String role in userroles) {
        sql = "insert into permissions (id, typeid, role, permission) values('" + Id + "', " + ptid + ", '" + role + "', 'Y')";
        DB.ExecSql(sql);
      }

    }

    /// <summary>Id of user.</summary> 
    public int Id {
      get { return this.id; }
      set { this.id = value; }
    }


    /// <summary>Type of user [Normal|Administrator].</summary> 
    public String UserType {
      get { return usertype; }
      set { usertype = value; }
    }

    public bool Approved {
      get { return approved; }
      set { approved = value; }
    }

    /// <summary>True if UserType='Administrator'.</summary> 
    public bool IsAdmin {
      get { return UserType == "Administrator"; }
    }

    public bool HasRole(String aRole) {
      return Roles.Contains(aRole);
    }

    /// <summary>True if user is a system administrator and has full permissions (has role Admin).</summary>
    public bool IsSysAdmin {
      get { return IsAdmin && Roles.Contains("Admin"); }
    }


    /// <summary>Username of user.</summary> 
    public String UserName {
      get { return username; }
      set { username = value; }
    }


    /// <summary>Password for user.</summary> 
    public String Password {
      get { return password; }
      set { password = value; }
    }


    /// <summary>Email address for user.</summary> 
    public String Email {
      get { return email; }
      set { email = value; }
    }


    /// <summary>Description of user.</summary> 
    public String Description {
      get { return description; }
      set { description = value; }
    }


    /// <summary>List with roles assigned to the user.</summary> 
    public ArrayList Roles {
      get { return userroles; }
      set {
        userroles.Clear();
        foreach (String role in value)
          userroles.Add(role);
      }
    }

    public String[][] AttribFields {
      get { return attribFields; }
    }

    public String GetAttribValue(String name) {
      Object attrib = attribVals[name];
      if (attrib == null) return "";
      else return attrib.ToString();
    }

    public void SetAttrib(String name, String value) {
      if (!attribVals.ContainsKey(name)) throw new Exception("Attribute does not exist");
      attribVals[name] = value;
    }
  }
}