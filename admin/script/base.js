/* $Date: 2010-11-22 14:32:20 +0100 (må, 22 nov 2010) $    $Revision: 7099 $ */

function initMaster() {
  if (typeof AjaxPro != "undefined" && AjaxPro !== null) {
    AjaxPro.timeoutPeriod = 1000 * 10;
    AjaxPro.onTimeout = function(b, res) { ajaxTimeout(b, res); }
    AjaxPro.onLoading = function(b) { showBusy(b); }
    AjaxPro.onError = function(res) { ajaxError(res); }
  }
  try { initPage(); }
  catch (e) { };
}

function showBusy(busy) { document.getElementById("Busy").style.display = (busy ? "block" : "none"); }
function ajaxTimeout(b, res) { showBusy(false); alert("Timeout i funktionen " + res.method); }
function ajaxError(res) { showBusy(false); alert(res.Message); }

function onCallbackError(e) {
  alert(e);
}

function onDoubleClick(iRecordIndex) {
  ItemGrid.editRecord(iRecordIndex);
}

function translate(txt) {
  var response = AdminBase.Translate(txt);
  if (response.error) return txt;
  else return response.value;
}

function exportTable(name) {
  ItemGrid.exportToExcel(name, false, true, false, true);
}

function getAbsolutePosition(div) {
  var currdiv = div;
  var apos = {left:currdiv.offsetLeft, top:currdiv.offsetTop};
  while (currdiv.offsetParent) {
    currdiv = currdiv.offsetParent;
    apos.left += currdiv.offsetLeft;
    apos.top += currdiv.offsetTop;
  }
  return apos;
}

function openMediabank(callback,params) {
  params = (params != null ? params : "");
  window.open("/admin/DocumentBank/DocumentBank.aspx?callback=" + callback + "&params=" + params, 'MB', 'width=900,height=600,resizable=yes');
}

$(document).ready(function(){
   initMaster();
});
