/* $Date: 2009-04-17 23:01:17 +0200 (fr, 17 apr 2009) $    $Revision: 4819 $ */
//Namespace.Classname.Method(arg1, arg2, arg3, ... callback, context, onLoading, onError, onTimeout, onStateChanged);

function initAdmin() {
  $("#relatedpages li.selected").each(function(){
    var parent = $(this).parent().parent();
    if (parent.get(0).tagName == "LI")
      parent.addClass("open");
  });
  $("#relatedpages, #allpagestree").treeview({collapsed:true});
}

function bwGetPropId(propname) {
  return document.getElementById(propname + "_propid").value;
}
function bwGetPropName(id) {
  var e = document.getElementsByName('hidden_propid');
  var propname = "";
  for (var i=0; i < e.length && propname.length == 0; i++) {
    if (e[i].value == id)
      propname = e[i].id.substring(0, e[i].id.length-7);
  }
  return propname;
}
function lockProperty(propId, lock) {
  if (lock) NFN.BasePage.LockProperty(propId);
  else NFN.BasePage.UnlockProperty(propId);
}

function unlockAll() {
  var response = NFN.BasePage.CheckLoggedIn();
  if (response.value) {
    var changedProp = new Array();
    var openId = new Array();
    var chCount = 0;
    var openCount = 0;
    var e = document.getElementsByName('hidden_propid');
    for (var i=0; i < e.length; i++) {
      var propname = e[i].id.substring(0, e[i].id.length-7);
      if (document.getElementById(propname + "_static").style.display == "none") {
        if (isChanged(propname)) {
          changedProp[chCount] = propname;
          chCount++;
        }
        openId[openCount] = e[i].value;
        openCount++;
      }
    }
    if (chCount > 0 && confirm(translate("Vill du spara ändringar som är gjorda på sidan") + "?")) {
      for (var i=0; i < changedProp.length; i++) {
        var html = tinyMCE.getContent(changedProp[i] + "_mce");
        NFN.BasePage.SavePropVal(bwGetPropId(changedProp[i]), 'text', html, false);
      }
    }
    for (var i=0; i < openId.length; i++)
      lockProperty(openId[i], false);
  }
}
function checkLoggedIn() {
  var response = NFN.BasePage.CheckLoggedIn();
  if (!response.value)
    window.location.reload();
}


/*** PermissionControl ***/
function __showPermissions(showidx, idlist) {
  var idarr = idlist.split(',');
  for (var i = 0; i < idarr.length; i++) {
    if (i == showidx)
      document.getElementById(idarr[i]).style.display='block';
    else
      document.getElementById(idarr[i]).style.display='none';
  }
}


/*** MoveableControl ***/
function _initMoveable(clientid, propid, isRelative) {
  $('#' + clientid).bind('drag',function( event ){
    $( this ).css({
      top: event.offsetY - (isRelative ? $(this).parent().offset().top : 0),
      left: event.offsetX - (isRelative ? $(this).parent().offset().left : 0)
    });
  });
  $('#' + clientid).bind('dragend',function( event ){
    NFN.BasePage.SaveControlProp(propid, 'left', String(event.offsetX - (isRelative ? $(this).parent().offset().left : 0)));
    NFN.BasePage.SaveControlProp(propid, 'top', String(event.offsetY - (isRelative ? $(this).parent().offset().top : 0)));
  });
}


function __showPermissions(showidx, idlist) {
  var idarr = idlist.split(',');
  for (var i = 0; i < idarr.length; i++) {
    if (i == showidx)
      document.getElementById(idarr[i]).style.display='block';
    else
      document.getElementById(idarr[i]).style.display='none';
  }
}



/*** HighlightItem ***/
function __highLightItem(id, highlight) {
/*  var item = document.getElementById(id);
  if (highlight) item.style.border = 'dashed 1px #ccc';
  else item.style.border = 'none';*/
}


/*** SharedClicked ***/
function __sharedClicked(cb) {
  if (cb.checked) return confirm('Elementets nuvarande egenskaper kan gå förlorade. Ok att dela egenskaper med andra sidor?');
  else return true;
}



/*** User info ***/

function _showUserInfo() {
  $("#createpageform").hide("normal");
  $("#editpageform").hide("normal");
  $("#userinfoform").show("normal");
}

function _cancelUserInfo() {
  $("#userinfoform").hide("normal");
}

function _saveUserInfo() {
  var err = "";
  var uname = $('#span_UserName').html();
  var opw = $("#inp_PWOld").val();
  var pw1 = $("#inp_PWNew1").val();
  var pw2 = $("#inp_PWNew2").val();
  var email = $("#inp_UEmail").val();
  var attribs = new Array();
  $("input[name='ua_field']").each(function(){
    var aname = $(this).attr("id").substring(3);
    var adescr = $(this).parent().prev().html();
    var mandatory = $(this).next().html();
    var regexp = $(this).next().next().html();
    attribs.push({
      name:aname,
      description:adescr,
      mandatory:mandatory,
      regexp:regexp,
      value:$(this).val()
    });
  });
  if (pw1.length > 0) {
    if (opw.length == 0) err += translate("Ange ditt nuvarande lösenord") + "<br />";
    if (pw1 != pw2) err += translate("Lösenorden stämmer inte överens") + "<br />";
  }
  if (email.length > 0 && !$.validateEmail(email)) err += translate("Ange en giltig e-postadress") + "<br />";
  var sattribs = "";
  for (var i=0; i < attribs.length; i++) {
    if (sattribs.length > 0) sattribs += ";";
    sattribs += attribs[i].name + "|" + attribs[i].value;
    if (attribs[i].mandatory == "1" && attribs[i].value.length == 0)
      err += translate("Fältet") + " " + translate(attribs[i].description) + " " + translate("är obligatoriskt") + "<br />";
    if (attribs[i].regexp.length > 0 && attribs[i].value.length > 0) {
      var re = new RegExp(attribs[i].regexp);
      if (!re.test(attribs[i].value)) translate("Värdet i fältet") + " " + translate(attribs[i].description) + " " + translate("är ogiltigt") + "<br />";
    }
  }
  if (err.length > 0) {
    $("#pu_err").html(err).show('normal');
    setTimeout("$('#pu_err').hide('normal')", 3000);
  }
  else
    NFN.BasePage.SaveUserInfo(uname, opw, pw1, email, sattribs, _saveUserInfo2);
}

function _saveUserInfo2(response) {
  if (response.error) {
    $("#pu_err").html(response.error.Message).show('normal');
    setTimeout("$('#pu_err').hide('normal')", 3000);
  }
  else
    $("#userinfoform").hide("normal");
}

/*** Edit page ***/

function _showEditPage() {
  $("#createpageform").hide("normal");
  $("#userinfoform").hide("normal");
  $("#editpageform").show("normal");
}

function _cancelEditPage() {
  if ($("#_publishBtn").length > 0) {
    $("#_publishBtn").css("display","inline")
    $("#_publishTemplate").val("n");
  }
  $("#editpageform").hide("normal");
}

function toggleRelatedPages() {
  $("#relatedpagesdiv").slideToggle("normal");
}

function toggleAllPages() {
  $("#allpagesdiv").slideToggle("normal");
}

function _saveEditPage() {
  var err = "";
  var title = $("#inpPageTitle").val();
  if (title.length == 0) err += translate("Sidans titel får inte vara tom") + "<br />";
  var name = $("#inpPageName").val();
  var isactive = $("#inpPageActive").attr("checked");
  var istimecontrolled = $("#inpPageTimeControlled").attr("checked");
  var startdate = $("#inpPageStartDate").val();
  var enddate = $("#inpPageEndDate").val();
  var template = $("#selPageTemplate").val();
  var place = parseInt($("#selpageOrder").val());
  var attrib = "";
  var publish = $("#_publishTemplate").val() == "y";


  var ares = getFormAttribs("editpageform");
  if (err.length == 0) err = ares.err;

  var related = "";
  $("#relatedpages input:checked").each(function(){
    if (related.length > 0) related += "|";
    related += this.id.substring(3);
  });
  if (err.length > 0) {
    $("#pa_err").html(err).show('normal');
    setTimeout("$('#pa_err').hide('normal')", 3000);
  }
  else
    NFN.BasePage.SavePageAttribs(_pageId, title, name, isactive, istimecontrolled, startdate, enddate, template, publish, parseInt(place), ares.attrib, related, _saveEditPage2);
}

function _saveEditPage2(response) {
  if (response.error)
    alert(response.error.Message);
  else
    document.location.reload();
}


/*** Delete page ***/

function _deletePage() {
  if (confirm(translate("Är du säker på att du vill permanent ta bort sidan, inklusive dess undersidor och information?")))
    NFN.BasePage.DeletePage(_pageId, _deletePage2);
}

function _deletePage2(response) {
  if (response.error) alert(response.error.Message);
  else document.location.href = response.value;
}


/*** Create page ***/

function _showCreatePage() {
  $("#editpageform").hide("normal");
  $("#userinfoform").hide("normal");
  $("#createpageform").show("normal");
}

function _cancelCreatePage() {
  $("#createpageform").hide("normal");
}

function chkAutoIdClicked(chk) {
  if (chk.checked)
    $("#inp_PageId").val($("#hd_PageId").val());
  $("#inp_PageId").attr("disabled",chk.checked);
}

function getFormAttribs(formid) {
  var res = {attrib:"",err:""};
  $("#" + formid).find("[id^='attrib_']").each(function(){
    if (res.attrib.length > 0) res.attrib += ";";
    var id = $(this).attr("id").substring(7);
    var val = $(this).val();
    var type = $(this).attr("name");
    if ($(this).attr("type") == "checkbox")
      val = ($(this).attr("checked") ? "true" : "");
    res.attrib += id + "|" + val;
    var txt = $(this).parent().prev().html();
    if (val.length > 0 && type == "int") {
      var hlp = parseInt(val);
      if (isNaN(hlp)) res.err += txt + " " + translate("måste vara ett heltal");
    }
    if (val.length > 0 && type == "datetime") {
      var adate = Date.parse(val);
      if (adate == null) res.err += txt + " " + translate("måste vara ett giltigt datum");
    }
  });
  return res;
}

function _saveCreatePage() {
  var err = "";
  var id = $("#inp_PageId").val();
  if (id.length == 0) err += translate("Sidans identifierare får inte vara tom") + "<br />";
  var title = $("#inp_PageTitle").val();
  var name = $("#inp_PageName").val();
  if (title.length == 0) err += translate("Sidans titel får inte vara tom") + "<br />";
  var isactive = $("#inp_PageActive").attr("checked");
  var istimecontrolled = $("#inp_PageTimeControlled").attr("checked");
  var startdate = $("#inp_PageStartDate").val();
  var enddate = $("#inp_PageEndDate").val();
  var tmpl = $("#sel_pageTemplate").val();
  var place = parseInt($("#sel_pageOrder").val());

  var ares = getFormAttribs("createpageform");
  if (err.length == 0) err = ares.err;

  if (err.length > 0) {
    $("#pc_err").html(err).show('normal');
    setTimeout("$('#pc_err').hide('normal')", 3000);
  }
  else
    NFN.BasePage.QuickNewPage(_pageId, id, title, name, isactive, istimecontrolled, startdate, enddate, tmpl, place, ares.attrib, _saveCreatePage2);
}
function _saveCreatePage2(response) {
  if (response.error) {
    $("#pc_err").html(response.error.Message).show('normal');
    setTimeout("$('#pc_err').hide('normal')", 3000);
  }
  else
    document.location.href = "/" + response.value + ".aspx";
}

function togglePageTimecontrol(cb) {
  var disp = (cb.checked ? "" : "none");
  $(cb).parent().parent().next().css("display",disp);
  $(cb).parent().parent().next().next().css("display",disp);
}

function selPageDate(a) {
  showCalendar(a.previousSibling, a, false, selPageDateChange);
}

function selPageDateChange(cal, adate) {
  cal.sel.value = adate;
  cal.shownAt.innerHTML = adate;
}

function publishTemplate(a) {
  $("#_publishTemplate").val("y");
  $(a).css("display","none");
}




if (window.attachEvent) window.attachEvent("onunload", unlockAll);
else window.addEventListener("unload", unlockAll, false);

