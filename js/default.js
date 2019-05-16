function initPage() {
  $("#SongText").ajaxcontrol();

  initSongGrid();
  initSpexGrid();
}

var usedView = "view_song";
var searchedText = "";

function getPostData() {
  var res = {
    idcol:"songid",
    cols:"songid,title,singer,firstline,melody,composer,category,spex,spexyear,textpropid,textlength,notes",
    sql:"select * from " + usedView
  };
  if (searchedText.length > 0) res.sql = "select * from " + usedView + " where songtext like '%" + searchedText + "%'"
  if (usedView == "view_songupps")
    res.cols = "songid,title,singer,firstline,melody,composer,category,spex,spexyear,uyear,info,orderno,textpropid,textlength,notes";
  return res;
}

function getColNames() {
  if (usedView == "view_songupps")
    return ['','Titel', 'Rollfigur', 'Förstarad', 'Melodi', 'Kompositör', 'Kategori', 'Spex', 'Premiärår', 'Årtal', 'Uppsättningsinfo', 'Kuplettnr', 'Text', '', 'Noter'];
  else
    return ['','Titel', 'Rollfigur', 'Förstarad', 'Melodi', 'Kompositör', 'Kategori', 'Spex', 'Årtal', 'Text', '', 'Noter'];
}

function getColModel() {
  if (usedView == "view_songupps")
    return [
      {name:'songid', width:30, formatter:songIdColFormat, search:false, hidedlg:true},
      {name:'title',index:'title', width:160 },
      {name:'singer',index:'singer', width:200, hidden:true },
      {name:'firstline',index:'firstline', width:210 },
      {name:'melody',index:'melody', width:205 },
      {name:'composer',index:'composer', width:200, hidden:true },
      {name:'category',index:'category', width:100, hidden:true },
      {name:'spex',index:'spex', width:130, stype:'select', editoptions:{value:spexes}},
      {name:'spexyear',index:'spexyear', width:40, hidden:true },
      {name:'uyear',index:'uyear', width:40},
      {name:'info',index:'info', width:130, hidden:true},
      {name:'orderno',index:'orderno', width:60},
      {name:'textpropid',index:'textpropid', width:40, formatter:textColFormat, search:false},
      {name:'textlength',hidden:true, hidedlg:true },
      {name:'notes',index:'notes', width:50, formatter:notesColFormat, search:false}
    ];
  else
    return [
      {name:'songid', width:30, formatter:songIdColFormat, search:false, hidedlg:true},
      {name:'title',index:'title', width:190 },
      {name:'singer',index:'singer', width:200, hidden:true },
      {name:'firstline',index:'firstline', width:250 },
      {name:'melody',index:'melody', width:245 },
      {name:'composer',index:'composer', width:200, hidden:true },
      {name:'category',index:'category', width:100, hidden:true },
      {name:'spex',index:'spex', width:130, stype:'select', editoptions:{value:spexes}},
      {name:'spexyear',index:'spexyear', width:40, hidden:true },
      {name:'textpropid',index:'textpropid', width:40, formatter:textColFormat, search:false},
      {name:'textlength',hidden:true, hidedlg:true },
      {name:'notes',index:'notes', width:50, formatter:notesColFormat, search:false}
    ];
}


function initSongGrid() {
  $("#songgrid").jqGrid({
    url:'home.aspx',
    postData: getPostData(),
    colNames:getColNames(),
    colModel:getColModel(),
    datatype: "json",
    altRows:false,
    rowNum:20,
    rowList:[10,20,50,100],
    pager: '#songctrl',
    sortname: 'title',
    viewrecords: true,
    sortorder: "asc",
    height:'100%',
    forceFit:true,
    shrinkToFit:false,
    width:970
  });
  $("#songgrid").navGrid(
    '#songctrl',
    {edit:false,add:false,del:false,search:false,refresh:false},
    {},{},{},{}
  ).navButtonAdd(
    "#songctrl", {
      caption:'',
      buttonimg:'',
      onClickButton:function(){
        $("#songgrid").setColumns();
      },
      position:"last",
      title:'Kolumner'
    }
  );
  $("#songgrid").jqGrid('filterToolbar',{
    stringResult:true,
    searchOnEnter:false,
    defaultSearch:'cn'
  });
  $("#songgrid").jqGrid('gridResize',{
    minWidth:350,
    maxWidth:2000,
    minHeight:80,
    maxHeight:3000
  });
}

var spexPostData = {
  idcol:"id",
  cols:"id,title,syear",
  sql:"select * from spex"
};

var uppsPostData = {
  idcol:"id",
  cols:"id,spexid,uyear,info",
  sql:"select * from uppsattningar where spexid=0"
};

function initSpexGrid() {
  $("#spexgrid").jqGrid({
    url:'home.aspx',
    editurl:'home.aspx?grid=spex',
    postData: spexPostData,
    colNames:['Id','Titel', 'Premiärår'],
    colModel:[
      {name:'id', width:50, formatter:spexIdColFormat },
      {name:'title',index:'title', width:330, editable:true },
      {name:'syear',index:'syear', width:100, editable:true }
    ],
    caption:"Spex",
    datatype: "json",
    altRows:false,
    rowNum:10,
    rowList:[10,20,50,100],
    pager: '#spexctrl',
    sortname: 'syear',
    viewrecords: true,
    sortorder: "desc",
    height:'100%',
    forceFit:true,
    shrinkToFit:false,
    subGrid:true,
    width:520,
    subGridRowExpanded: function(subgrid_id, row_id) {
      var subgrid_table_id = subgrid_id + "_t";
      var pager_id = "p_"+subgrid_table_id;
      $("#"+subgrid_id).html("<div><a href='javascript:addUppsattning(\"" + subgrid_table_id + "\")' onfocus='this.blur()' title='Lägg till uppsättning'><img src='gfx/add.gif' border='0' alt='' /></a></div><table id='"+subgrid_table_id+"' class='scroll'></table><div id='"+pager_id+"' class='scroll'></div>");
      $("#"+subgrid_table_id).jqGrid({
        url:'home.aspx',
        editurl:'home.aspx?spex=' + row_id + '&grid=upps',
        postData: {
          idcol:"id",
          cols:"id,uyear,info",
          sql:"select * from uppsattningar where spexid=" + row_id
        },
        datatype: "json",
        colNames: ['', 'Årtal','Info'],
        colModel: [
          {name:'id', width:50, formatter:function(cellval, el, vals){return uppsIdColFormat(subgrid_table_id, cellval, el, vals);} },
          {name:"uyear",index:"uyear",width:80, editable:true},
          {name:"info",index:"info",width:360, editable:true}
        ],
        caption:"Uppsättningar",
        rowNum:20,
        pager: pager_id,
        sortname: 'uyear',
        sortorder: "desc",
        height: '100%'
      });
      $("#"+subgrid_table_id).jqGrid('navGrid',"#"+pager_id,{edit:false,add:false,del:false,search:false,refresh:false})
    },
    subGridRowColapsed: function(subgrid_id, row_id) {
      // this function is called before removing the data
      //var subgrid_table_id;
      //subgrid_table_id = subgrid_id+"_t";
      //jQuery("#"+subgrid_table_id).remove();
    }

  });
  $("#spexgrid").navGrid(
    '#spexctrl',
    {edit:false,add:false,del:false,search:false,refresh:false},
    {},{},{},{}
  );
  $("#spexgrid").jqGrid('gridResize',{
    minWidth:350,
    maxWidth:2000,
    minHeight:80,
    maxHeight:3000
  });
}


function setGridSql(sql) {
  var prm = $("#songgrid").jqGrid().getGridParam("postData");
  prm.sql = sql;
  $("#songgrid").jqGrid().setGridParam({postData:prm}).trigger("reloadGrid")
}

function songIdColFormat(cellval, el, vals) {
  var html = "";
  if (parseInt(cellval) > 0)
    html += "<div style='text-align:center'><a href='javascript:void(0)' onclick='showSongInfo(" + el.rowId + ")' title='Visa all kuplettinfo'><img src='gfx/info.gif' alt='Info' border='0' /></a></div>";
  return html;
}

function spexIdColFormat(cellval, el, vals) {
  var html = "";
  if (parseInt(cellval) > 0)
    html += "<div style='text-align:center'>" +
      "<span class='spex_static'>" +
      "<a href='javascript:void(0)' onclick='editSpex(this, " + el.rowId + ")' title='Redigera spex'><img src='gfx/edit.gif' alt='Redigera' border='0' /></a>" +
      "<a href='javascript:void(0)' onclick='deleteSpex(" + el.rowId + ")' title='Radera spex'><img src='gfx/delete.png' alt='Radera' border='0' /></a>" +
      "</span>" +
      "<span class='spex_edit'>" +
      "<a href='javascript:void(0)' onclick='saveEditSpex(this, " + el.rowId + ")' title='Spara ändringar'><img src='gfx/save.gif' alt='Spara' border='0' /></a>" +
      "<a href='javascript:void(0)' onclick='cancelEditSpex(this, " + el.rowId + ")' title='Avbryt'><img src='gfx/cancel.gif' alt='Avbryt' border='0' /></a>" +
      "</span>" +
      "</div>";
  return html;
}

function uppsIdColFormat(tableid, cellval, el, vals) {
  var html = "";
  if (parseInt(cellval) > 0)
    html += "<div style='text-align:center'>" +
      "<span class='upps_static'>" +
      "<a href='javascript:void(0)' onclick='editUppsattning(this, \"" + tableid + "\", " + el.rowId + ")' title='Redigera uppsättning'><img src='gfx/edit.gif' alt='Redigera' border='0' /></a>" +
      "<a href='javascript:void(0)' onclick='deleteUppsattning(\"" + tableid + "\", " + el.rowId + ")' title='Radera uppsättning'><img src='gfx/delete.png' alt='Radera' border='0' /></a>" +
      "</span>" +
      "<span class='upps_edit'>" +
      "<a href='javascript:void(0)' onclick='saveEditUppsattning(this, \"" + tableid + "\", " + el.rowId + ")' title='Spara ändringar'><img src='gfx/save.gif' alt='Spara' border='0' /></a>" +
      "<a href='javascript:void(0)' onclick='cancelEditUppsattning(this, \"" + tableid + "\", " + el.rowId + ")' title='Avbryt'><img src='gfx/cancel.gif' alt='Avbryt' border='0' /></a>" +
      "</span>" +
      "</div>";
      nfndebug(html);
  return html;
}

function textColFormat(cellval, el, vals) {
  var propid = (usedView == "view_songupps" ? parseInt(vals[13]) : parseInt(vals[10]));
  if (propid > 0) return "<div style='text-align:center'><a href='javascript:void(0)' onclick='openText(" + el.rowId + ")'><img src='gfx/text.gif' alt='Text' border='0' /></a></div>";
  else return "";
}

function notesColFormat(cellval, el, vals) {
  if (cellval.length > 0) {
    var notes = cellval.split(';');
    var html = '';
    for (var i=0; i < notes.length; i++)
      html += "<div style='float:left'><a href='" + notes[i] + "' target='_blank'><img src='gfx/note.gif' alt='Noter' border='0' /></a></div>";
    html += "<div class='clearfloat'></div>";
    return html;
  }
  else return "";
}

function toggleUpps() {
  if (usedView == "view_songupps") usedView = "view_song";
  else usedView = "view_songupps";

  $('#songgrid').jqGrid("GridUnload");

  $('#songgrid').jqGrid('setGridParam',{
    postData: getPostData(),
    colNames:getColNames(),
    colModel:getColModel()
  });
  initSongGrid();
}

function openText(id) {
  DefaultPage.GetTextInfo(id, function(r){openText2(r, id);});
}
function openText2(response, id) {
  $("title").html("Kavalkad online" + "&nbsp;&nbsp;&mdash;&nbsp;&nbsp;" + response.value[0] + "&nbsp;|&nbsp;" + response.value[1]);
  $("#i_title").html(response.value[0]);
  $("#i_spex").html("Ur " + response.value[1]);
  $("#i_melody").html(response.value[2]);
  var propname = "Song_" + id;
  $("#SongText").ajaxcontrol_setPropertyName(propname, openText3);
}
function openText3() {
  $(".pop").hide();
  $("#poptext").slideDown("normal");
}

function closeText() {
  $("title").html("Kavalkad online");
  $('#poptext').slideUp('normal');
}

function showSongInfo(id) {
  DefaultPage.GetSongInfo(id, showSongInfo2);
}
function showSongInfo2(response) {
  $("#songinfo").html(response.value);
  $("#songinfo input").each(function(){
    $(this).val($(this).val().replace(/¤/g,"\'"));
  });
  $("#songinfo span.static").each(function(){
    $(this).html($(this).html().replace(/¤/g,"\'"));
  });
  $(".pop").hide();
  $("#infopop .static").show();
  $("#infopop .edit").hide();
  $("#infopop").slideDown("normal");
}

var sttimer = null;
function searchtext() {
  if (sttimer) clearTimeout(sttimer);
  sttimer = setTimeout(dosearchtext, 1000);
}

function dosearchtext() {
  sttimer = null;
  searchedText = $("#textrow").val();
  $('#songgrid').jqGrid('setGridParam',{postData:getPostData()}).trigger('reloadGrid');
}

function closeInfo() {
  $('#infopop').slideUp('normal')
  $("#songinfo").html("");
}

function closeNew() {
  $('#newpop').slideUp('normal')
  $("#newsonginfo").html("");
}

function editSong() {
  $("#infopop .static").hide();
  $("#infopop .edit").show();
}

function cancelEditSong() {
  $("#infopop .static").show();
  $("#infopop .edit").hide();
  closeInfo();
}

function saveEditSong() {
  saveSong(false);
}

function saveSong(isnew) {
  var songid = $("#songid").val();
  var title = $("#edtitle").val();
  if (title.length == 0) {
    alert("Kuplettiteln kan inte vara tom");
    return;
  }
  var alt_title = $("#edalttitle").val();
  if (!alt_title) alt_title = "";
  var spex = $("#edspex").val();
  if (spex.length == 0) {
    alert("Du måste ange ett spex");
    return;
  }
  var singer = $("#edsinger").val();
  var firstline = $("#edfirstline").val();
  var upps = [];
  $("#uppsdiv div.oneupps").each(function(){
    var id = $(this).find("input.uppsid").val();
    var order = $(this).find("input.uppsorder").val();
    upps.push(id + "|" + order);
  });
  var melody = [];
  $("#melodytable tr").each(function(i){
    if (i > 0) {
      var mel = $(this).find("input.i_mel").val();
      var altmel = $(this).find("input.i_amel").val();
      var comp = $(this).find("input.i_composer").val();
      var cat = $(this).find("select").val();
      melody.push(mel + "|" + altmel + "|" + comp + "|" + cat);
    }
  });
  var notes = [];
  $("#notesdiv div.onenote a.notelink").each(function() {
    notes.push($(this).attr("href"));
  });
  if (isnew)
    DefaultPage.SaveNewSong(title, alt_title, spex, singer, firstline, upps, melody, notes, saveNewSong2);
  else
    DefaultPage.SaveSong(songid, title, alt_title, spex, singer, firstline, upps, melody, notes, saveEditSong2);
}

function saveEditSong2(response) {
  if (response.error) {
    alert(response.error.Message);
    return;
  }
  closeInfo();
  $("#songgrid").trigger("reloadGrid");
}

function saveNewSong2(response) {
  if (response.error) {
    alert(response.error.Message);
    return;
  }
  closeNew();
  $("#songgrid").trigger("reloadGrid");
}

function addSong() {
  DefaultPage.GetNewForm(addSong2);
}
function addSong2(response) {
  $("#newsonginfo").html(response.value);
  $("#newpop .static").remove();
  $(".pop").hide();
  $("#newpop").slideDown("normal");
}

function cancelNewSong() {
  closeNew();
}

function saveNewSong() {
  saveSong(true);
}

function deleteSong() {
  if (confirm("Är du säker på att du vill radera kupletten?")) {
    var songid = $("#songid").val();
    DefaultPage.DeleteSong(songid, deleteSong2);
  }
}

function deleteSong2(response) {
  if (response.error) {
    alert(response.error.Message);
    return;
  }
  closeInfo();
  $("#songgrid").trigger("reloadGrid");
}

function addMelody() {
  DefaultPage.GetCategoryOptions("", addMelody2);
}

function addMelody2(response) {
  var html = "<tr><td class='edit'><span class='edit'><a href='javascript:void(0)' onclick='deleteMelody(this)' title='Ta bort melodi'><img src='gfx/delete.png' alt='' /></a></span></td>" +
    "<td><span class='static'></span><span class='edit'><input type='text' class='i_mel' value='' /></span></td>" +
    "<td><span class='static'></span><span class='edit'><input type='text' class='i_amel' value='' /></span></td>" +
    "<td><span class='static'></span><span class='edit'><input type='text' class='i_composer' value='' /></span></td>" +
    "<td><span class='static'></span><span class='edit'><select>" + response.value + "</select></span></td></tr>";

  $("#melodytable tbody").append(html);
  $("#melodytable .static").hide();
  $("#melodytable .edit").show();
}

function deleteMelody(a) {
  $(a).parent().parent().parent().remove();
}

function addUppsattningToSong() {
  var spexid = $("#edspex").val();
  if (spexid.length > 0)
    DefaultPage.GetUppsattningOptions(spexid, addUppsattningToSong2);
}

function addUppsattningToSong2(response) {
  $("#uppsdd select").html(response.value);
  $("#uppsdd").show();
}

function uppsSelected() {
  var vals = $("#uppsdd select").val().split('|');
  var info = (vals[2].length > 0 ? ", " + vals[2] : "");
  var html = "<div class='oneupps'>" +
    "<span class='edit'><a href='javascript:void(0)' onclick='deleteUppsattningFromSong(this)' title='Ta bort uppsättnig'><img src='gfx/delete.png' alt='' /></a></span>" +
    vals[1] + info + ", Kuplettnummer <span class='static'>0</span>" +
    "<span class='edit'><input type='hidden' class='uppsid' value='" + vals[0] + "' /><input type='text' class='uppsorder' value='' /></span>" +
    "</div>";
  $("#uppsdiv").append(html);
  $("#uppsdiv .static").hide();
  $("#uppsdiv .edit").show();
  $("#uppsdd").hide();
}

function deleteUppsattningFromSong(a) {
  $(a).parent().parent().remove();
}

function openMediabank(callback,params) {
  params = (params != null ? params : "");
  window.open("/admin/DocumentBank/DocumentBank.aspx?callback=" + callback + "&params=" + params, 'MB', 'width=900,height=600,resizable=yes');
}


function addNotes() {
  var params = "";
  window.open("/admin/DocumentBank/DocumentBank.aspx?callback=addNotes2&params=" + params, 'MB', 'width=900,height=600,resizable=yes');
}

function addNotes2(doc) {
  var hlp = doc.url.split('/');
  var html = "<div class='onenote'>" +
    "<span class='edit'><a href='javascript:void(0)' onclick='delNotes(this)' title='Ta bort not'><img src='gfx/delete.png' alt='' /></a></span>" +
    "<a href='" + doc.url + "' class='notelink' target='_blank' onfocus='this.blur()'>" + hlp[hlp.length-1] + "</a>" +
    "</div>";
  $("#notesdiv").append(html);
  $("#notesdiv .static").hide();
  $("#notesdiv .edit").show();
}

function delNotes(a) {
  $(a).parent().parent().remove();
}

function songSpexSelected() {
  $("#uppsdiv").empty();
}


function addSpex() {
  $("#spexgrid").jqGrid('editGridRow',"new",{reloadAfterSubmit:true, afterSubmit:function(xhr,st,err){
    if (xhr.responseText.length > 0) alert(xhr.responseText)
    return xhr.responseText.length == 0;
  }});
}
function deleteSpex(id) {
  $("#spexgrid").jqGrid('delGridRow',id,{
    reloadAfterSubmit:true,
    afterSubmit:function(xhr,st,err){
      if (xhr.responseText.length > 0) alert(xhr.responseText)
      return xhr.responseText.length == 0;
    },
    msg: "Är du säker på att<br />du vill radera spexet?"
  });
}
function editSpex(a, id) {
  $(a).parent().parent().find(".spex_edit").show();
  $(a).parent().parent().find(".spex_static").hide();
  $("#spexgrid").jqGrid('editRow',id);
}
function cancelEditSpex(a, id) {
  $("#spexgrid").jqGrid('restoreRow',id);
  $(a).parent().parent().find(".spex_edit").hide();
  $(a).parent().parent().find(".spex_static").show();
}
function saveEditSpex(a, id) {
  $("#spexgrid").jqGrid('saveRow',id, function(xhr,st,err) {
    if (xhr.responseText.length > 0) alert(xhr.responseText)
  });
  $(a).parent().parent().find(".spex_edit").hide();
  $(a).parent().parent().find(".spex_static").show();
}


function addUppsattning(grid) {
  $("#" + grid).jqGrid('editGridRow',"new",{reloadAfterSubmit:true, afterSubmit:function(xhr,st,err){
    if (xhr.responseText.length > 0) alert(xhr.responseText)
    return xhr.responseText.length == 0;
  }});
}
function deleteUppsattning(tableid, rowid) {
  $("#" + tableid).jqGrid('delGridRow',rowid,{
    reloadAfterSubmit:true,
    afterSubmit:function(xhr,st,err){
      if (xhr.responseText.length > 0) alert(xhr.responseText)
      return xhr.responseText.length == 0;
    },
    msg: "Är du säker på att<br />du vill radera uppsättningen?"
  });
}
function editUppsattning(a, tableid, id) {
  $(a).parent().parent().find(".upps_edit").show();
  $(a).parent().parent().find(".upps_static").hide();
  $("#" + tableid).jqGrid('editRow',id);
}
function cancelEditUppsattning(a, tableid, id) {
  $("#" + tableid).jqGrid('restoreRow',id);
  $(a).parent().parent().find(".upps_edit").hide();
  $(a).parent().parent().find(".upps_static").show();
}
function saveEditUppsattning(a, tableid, id) {
  $("#" + tableid).jqGrid('saveRow',id, function(xhr,st,err) {
    if (xhr.responseText.length > 0) alert(xhr.responseText)
  });
  $(a).parent().parent().find(".upps_edit").hide();
  $(a).parent().parent().find(".upps_static").show();
}

function logout() {
  DefaultPage.Logout();
  document.location.reload();
}
