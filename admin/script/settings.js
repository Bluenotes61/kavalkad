/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */


/*** General settings ***/

function editSetting(a) {
  $(a).parent().prev().children(":first-child").hide();
  $(a).parent().prev().children(":last-child").show();
  $(a).hide().next().show().next().show();
}

function cancelSetting(a) {
  $(a).parent().prev().children(":first-child").show();
  $(a).parent().prev().children(":last-child").hide();
  $(a).hide().prev().hide().prev().show();
}

function saveSetting(a) {
  var key = $(a).parent().prev().children(":first-child").attr("id");
  var val = $(a).parent().prev().children(":last-child").val();
  Settings.SaveSetting(key, val, function(r){saveSetting2(r, a, key, val);});
}
function saveSetting2(response, a, key, val) {
  $("#" + key).html(val);
  $(a).parent().prev().children(":first-child").show();
  $(a).parent().prev().children(":last-child").hide();
  $(a).hide().prev().show().next().next().hide();
}



/*** Recover ***/

function cal_onChange(cal, adate) {
  cal.sel.value = adate;
  cal.shownAt.innerHTML = adate;
}

function show_cal(a) {
  showCalendar(a.previousSibling, a, true, cal_onChange);
}

function recover() {
  var adate = $('#e_recover').val();
  if (confirm(translate("Är du säker på att du vill återställa sajten till") + " " + adate + "?"))
    Settings.Recover(adate, recover2);
}

function recover2(response) {
  if (response.error)
    alert(response.error.Message);
  else {
    $('#recoverok').show('normal');
    setTimeout("$('#recoverok').hide('normal')", 3000);
  }
}



/*** Templates table ***/

function onDeleteTemplate(record) {
  return confirm(translate("Är du säker på att du vill ta bort sidmallen") + " " + record["filename"] + "?");
}

function onAddTemplate(record) {
  $("#idtmpl").show();
  $("#idtmplspan").hide();
}

function onEditTemplate(record) {
  $("#idtmplspan").html($("#idtmpl").val()).show();
  $("#idtmpl").hide();
}

function openAttributes(a) {
  var key = $(a).parent().parent().parent().children("td:nth-child(3)").children("div:first-child").html();
  $("#attribhl").html(translate("Sidattribut för") + " " + key);
  $("#attribfor").val(key);
  //AttribsGrid.removeFilter();
  AttribsGrid.addFilterCriteria("filename", OboutGridFilterCriteria.EqualTo, key);
  AttribsGrid.executeFilter();
  $("#attribPop").css("top", $(a).offset().top).show('normal');
}

function moveTemplate(dir) {
  if ( TemplatesGrid.SelectedRecords[0] == null ) {
    alert(translate("Markera en post att flytta") + ".");
    return;
  }
  var filename = TemplatesGrid.SelectedRecords[0].filename;
  Settings.MoveTemplateRow(dir, filename);
  TemplatesGrid.refresh();
}



/*** Attributes table ***/

function onDeleteAttrib(record) {
  return confirm(translate("Är du säker på att du vill ta bort attributet") + " " + record["id"] + "?");
}

function onAddAttrib(record) {
  $("#idattrib").show();
  $("#idattribspan").hide();
}

function onEditAttrib(record) {
  $("#idattribspan").html($("#idattrib").val()).show();
  $("#idattrib").hide();
}

function onPopulateControls(record) {
  record.filename = $("#attribfor").val();
}

function moveAttrib(dir) {
  if ( AttribsGrid.SelectedRecords[0] == null ) {
    alert(translate("Markera en post att flytta") + ".");
    return;
  }
  var filename = $("#attribfor").val();
  var id = AttribsGrid.SelectedRecords[0].id;
  Settings.MoveAttribRow(dir, filename, id);
  AttribsGrid.refresh();
}


/*** Css table ***/

function onDeleteCss(record) {
  return confirm(translate("Är du säker på att du vill ta bort stilmallen") + " " + record["name"] + "?");
}

function onAddCss(record) {
  $("#idcss").show();
  $("#idcssspan").hide();
}

function onEditCss(record) {
  $("#idcssspan").html($("#idcss").val()).show();
  $("#idcss").hide();
}

function moveCss(dir) {
  if ( CssGrid.SelectedRecords[0] == null ) {
    alert(translate("Markera en post att flytta") + ".");
    return;
  }
  var name = CssGrid.SelectedRecords[0].name;
  Settings.MoveExtraRow(dir, "Css", name);
  CssGrid.refresh();
}


