/* $Date: 2009-11-09 16:51:58 +0100 (må, 09 nov 2009) $    $Revision: 5608 $ */

function initPage() {
  new RollOverButton("btn_excel", { rolloverimageurl: "/admin/gfx/excel_over.gif", clickimageurl: "/admin/gfx/excel_sel.gif", onclick: function() { exportTable('news') } });
  initTiny();
}


function initTiny() {
  var tinyConfigs = {
    mode:'exact',
    relative_urls:true,
    convert_urls:true,
    theme:'advanced',
    language:'sv',
    content_css:'../css/base.css,../css/tiny.css',
    width:'550px',
    height:'400px',
    plugins:'inlinepopups,contextmenu,media,style,table,template,advlink,advimage,paste',
    media_use_script:false,
    extended_valid_elements:'script[charset|defer|language|src|type]',
    //strict_loading_mode : false,
    template_templates: htmlTemplates,
    theme_advanced_toolbar_location: 'top',
    theme_advanced_containers_default_align:'left',
    theme_advanced_buttons1:'formatselect,styleselect,separator,bold,italic,separator,justifyleft,justifycenter,justifyright,justifyfull,separator,cut,copy,paste,pastetext,pasteword,selectall,separator,undo,redo',
    theme_advanced_buttons2:'link,unlink,separator,docbank,image,media,separator,template,separator,table,delete_col,delete_row,col_after,col_before,row_after,row_before,separator,styleprops,removeformat,separator,cleanup,code',
    theme_advanced_buttons3:'',
    theme_advanced_styles:tinyStyles,
    file_browser_callback:function(field_name, url, type, win) { openDocumentBank(field_name, url, type, win); }
  };
  $("#txtContent, #txtSummary").tinymce(tinyConfigs);
}

function openDocumentBank(field_name, url, type, win) {
  tinyMCE.activeEditor.windowManager.open({
    file : "/admin/DocumentBank/DocumentBank.aspx?fromtiny=y",
    title : translate('Mediabank'),
    width : 900,
    height : 600,
    resizable : "yes",
    inline : "yes",
    close_previous : "yes"
  },
  {
    window : win,
    input : field_name,
    allowedtype : type
  });
  return false;
}


function getHtmlTemplates() {
  var response = NewsAdmin.GetHtmlTemplates();
  var res = new Array(response.value.length);
  for (var i=0; i < response.value.length; i++) {
    var vals = response.value[i].split('|');
    var d = (vals.length > 2 ? vals[1] : vals[0]);
    var v = (vals.length > 2 ? vals[2] : vals[1]);
    res[i] = {title:vals[0], value:v, description:d};
  }
  return res;
}

function getStyles() {
  var response = NewsAdmin.GetStyles();
  return response.value;
}

function cal_onChange(cal, adate) {
  cal.sel.value = adate;
  cal.shownAt.innerHTML = adate;
}

function show_cal(a) {
  showCalendar(a.previousSibling, a, true, cal_onChange);
}

function showTiny() {
  var contpos = getAbsolutePosition(N$("contdiv"));
  var sumpos = getAbsolutePosition(N$("summarydiv"));
  N$("tplTinyC").style.left = contpos.left + "px";
  N$("tplTinyC").style.top = contpos.top + "px";
  N$("tplTinyS").style.left = sumpos.left + "px";
  N$("tplTinyS").style.top = sumpos.top + "px";
}

function hideTiny() {
  N$S("tplTinyC").left = "-1000px";
  N$S("tplTinyS").left = "-1000px";
}

function onBeforeRowEdit(record) {
  N$('aDate').innerHTML = record.newsdate;
  tinyMCE.editors["txtContent"].setContent(record.content);
  tinyMCE.editors["txtSummary"].setContent(record.summary);
  return true;
}

function onRowEdit(record) {
  showTiny();
  return true;
}

function beforeUpdate(record) {
  N$("e_content").value = tinyMCE.editors["txtContent"].getContent().replace(/\n/g, "");
  N$("e_summary").value = tinyMCE.editors["txtSummary"].getContent().replace(/\n/g, "");
  hideTiny();
  return true;
}

function beforeAddRow() {
  var now = new Date();
  N$('e_date').value = now.dateString();
  N$('aDate').innerHTML = now.dateString();
  tinyMCE.editors["txtContent"].setContent("");
  tinyMCE.editors["txtSummary"].setContent("");
  return true;
}

function onAddRow() {
  showTiny();
  return true;
}

function onDelete(record) {
  return confirm(translate("Är du säker på att du vill ta bort nyhet") + " " + record["headline"] + "?");
}

function gotImage(doc) {
  N$('e_image').value = doc.url;
}