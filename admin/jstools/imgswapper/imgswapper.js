/* $Date: 2009-11-09 01:03:45 +0100 (mÃ¥, 09 nov 2009) $    $Revision: 5604 $ */
var imgswaps = new Array();

function flashLoaded(flashid) {
  var swp = null;
  for (var i=0; i < imgswaps.length && !swp; i++)
    if (imgswaps[i].id == flashid)
      swp = imgswaps[i];
  if (swp) swp.sendToFlash();
}

function swp_callbackDB(doc, id) {
  var ids = id.split(';');
  imgswaps[parseInt(ids[0])].callbackDB(doc, ids[1]);
}

function ImageSwapper(id, param) {

  var flashpath = "/admin/flash/img_swap.swf";
  var thisref = this;
  $("head").addCssFile("/admin/jstools/imgswapper/imgswapper.css");
  var swapperdata = null;

  this.setup = function() {
    this.id = id;
    this.maindiv = $("#" + id);
    if (!param) param = new Object();
    if (!param.flashpath) param.flashpath = flashpath;
    if (!param.flashsize) param.flashsize = {width:100, height:100};
    if (!param.flashversion) param.flashversion = 8;
    if (!param.bgcolor) param.bgcolor = "#ffffff";
    if (!param.wmode) param.wmode = "transparent";
    if (!param.data_type) param.data_type = "EXTINT";
    if (!param.paus_on) param.paus_on = 0;
    if (!param.nav_on) param.nav_on = 0;
    if (!param.click_on) param.click_on = 0;
    if (!param.loop_on) param.loop_on = 1;
    if (!param.base_path) param.base_path = "http://localhost";
    if (!param.editorheight) param.editorheight = 300;
    if (!param.thumbsize) param.thumbsize = {width:100, height:100};

    if (_loggedin) {
      var response = NFN.BasePage.GetPermission(this.id, "PageProperty", "edit");
      if (response.value) {
        this.pendiv = $("<div />").css({"position":"absolute", "display":"none"});
        this.pendiv.append(
          $("<div />").addClass('imgswap_pen').append(
            $("<a />").attr("href","javascript:void(0)").click(function(){thisref.showEditor()}).append(
              $("<img />").attr({"src":"/admin/gfx/edit.gif", "alt":"Edit", "border":"0"})
            )
          )
        );

        this.maindiv.append(this.pendiv);
        this.maindiv.mouseover(function(){thisref.pendiv.css("display", "block");});
        this.maindiv.mouseout(function(){thisref.pendiv.css("display", "none");});

        this.editordiv = $("<div />").addClass("imgswap_editordiv").draggable();
        this.maindiv.append(this.editordiv);

        this.editformdiv = $("<div />").attr("id", this.id + "editformdiv").addClass("imgswap_editformdiv").draggable();
        var html = "<input type='hidden' id='" + this.id + "editidx' />" +
          "<table>" +
          "<tr><td>Bild</td><td><input type='text' id='" + this.id + "url' /></td><td><a id='dblink" + this.id + "' href='javascript:void(0)'><img src='/admin/gfx/documentbank.gif' alt='' border='0' /></td></tr>" +
          "<tr><td>Bredd</td><td colspan='2'><input type='text' id='" + this.id + "width' /></td></tr>" +
          "<tr><td>Höjd</td><td colspan='2'><input type='text' id='" + this.id + "height' /></td></tr>" +
          "<tr><td>Pantyp</td><td colspan='2'><select id='" + this.id + "pantype'><option value='none' selected>None</option><option value='left_right'>Left-right</option><option value='mask'>Mask</option></select></td></tr>" +
          "<tr><td>Fadetid</td><td colspan='2'><input type='text' id='" + this.id + "fadetime' /></td></tr>" +
          "<tr><td>Fördröjning</td><td colspan='2'><input type='text' id='" + this.id + "timedelay' /></td></tr>" +
          "<tr><td>Panfördröjning</td><td colspan='2'><input type='text' id='" + this.id + "pandelay' /></td></tr>" +
          "<tr><td>Länk</td><td colspan='2'><input type='text' id='" + this.id + "link' /></td></tr>" +
          "<tr><td>Länkmål</td><td colspan='2'><select id='" + this.id + "target'><option value='_blank' selected>Nytt fönster</option><option value='_this'>Samma fönster</option></select></td></tr>" +
          "<tr><td colspan='3' align='right'><a href='javascript:void(0)' id='save" + this.id + "'>Spara</a>&nbsp;&nbsp;<a href='javascript:void(0)' id='cancel" + this.id + "'>Avbryt</a></td></tr>" +
          "</table>";
        this.editformdiv.append(html);

        this.editordiv.append(this.editformdiv);
        $("#dblink" + thisref.id).click(function(){thisref.openDocBank();});
        $("#save" + thisref.id).click(function(){thisref.saveImage();});
        $("#cancel" + thisref.id).click(function(){thisref.cancelEdit();});


        this.editordiv.append(
          $("<div />").css({"width":"100%","text-align":"right"}).append(
            $("<a />").attr("href","javascript:void(0)").click(function(){thisref.closeEditor()}).append(
              $("<img />").attr({"src":"admin/gfx/closewind.gif", "alt":"Stäng", "border":"0"})
            )
          )
        );

        this.listdiv = $("<div />").addClass("imgswap_editor_scroll").css("height", param.editorheight-50);

        this.editordiv.append(
          $("<div />").addClass("imgswap_editor_inner").append(
            $("<div />").addClass("imgswap_editor_new").append(
              $("<a />").attr("href","javascript:void(0)").click(function(){thisref.newImage()}).append(
                $("<img />").attr({"src":"admin/gfx/new.gif", "alt":"Ny bild", "border":"0"})
              )
            )
          ).append(this.listdiv)
        );

        this.setListHtml();
      }
    }
    var fdiv = $("<div />").attr("id", "flash_" + id).css({"width":param.flashsize.width, "height":param.flashsize.height});
    this.maindiv.append(fdiv);

    this.initFlash();
  }


  this.setListHtml = function(callback) {
    NFN.ImgSwapper.GetSwapperData(_pageId, this.id, param.thumbsize.width, param.thumbsize.height, function(r){thisref.setListHtml2(r, callback);});
  }

  this.setListHtml2 = function(response, callback) {
    swapperdata = response.value;
    this.listdiv.empty();
    for (var i=0; i < swapperdata.length; i++) {
      var params = swapperdata[i].split('|');
      this.listdiv.append(
        $("<div/>").attr("id",this.id + "oneimg_" + i).append(
          $("<input />").attr({"type":"hidden", "id":"url" + this.id + i}).val(params[0])
        ).append(
          $("<div />").addClass("imgswap_editor_info").mouseover(function(){$(this).show();}).mouseout(function(){$(this).hide()}).append(
            $("<table />").addClass("imgswap_infotable").attr({"cellpadding":"5","cellspacing":"0"}).append(
              $("<tr />").append($("<td />").append("Filnamn")).append($("<td />").append($("<span />").append(params[9])))
            ).append(
              $("<tr />").append($("<td />").append("Bredd")).append($("<td />").append($("<span />").attr("id","width" + this.id + i).append(params[1])))
            ).append(
              $("<tr />").append($("<td />").append("Höjd")).append($("<td />").append($("<span />").attr("id","height" + this.id + i).append(params[2])))
            ).append(
              $("<tr />").append($("<td />").append("Pantyp")).append($("<td />").append($("<span />").attr("id","pantype" + this.id + i).append(params[3])))
            ).append(
              $("<tr />").append($("<td />").append("Fadetid")).append($("<td />").append($("<span />").attr("id","fadetime" + this.id + i).append(params[4])))
            ).append(
              $("<tr />").append($("<td />").append("Fördröjning")).append($("<td />").append($("<span />").attr("id","timedelay" + this.id + i).append(params[5])))
            ).append(
              $("<tr />").append($("<td />").append("Panfördröjning")).append($("<td />").append($("<span />").attr("id","pandelay" + this.id + i).append(params[6])))
            ).append(
              $("<tr />").append($("<td />").append("Länk")).append($("<td />").append($("<span />").attr("id","link" + this.id + i).append(params[7])))
            ).append(
              $("<tr />").append($("<td />").append("Target")).append($("<td />").append($("<span />").attr("id","target" + this.id + i).append(params[8])))
            )
          )
        ).append(
          $("<table />").append(
            $("<tr />").append(
              $("<td />").attr("width","50").append(
                $("<a />").attr("href","javascript:void(0)").click(function(){thisref.editImage(this);}).append(
                  $("<img />").attr({"src":"admin/gfx/edit.gif", "alt":"Redigera", "border":"0"})
                )
              ).append("&nbsp;").append(
                $("<a />").attr("href","javascript:void(0)").click(function(){thisref.deleteImage(this);}).append(
                  $("<img />").attr({"src":"admin/gfx/delete.gif", "alt":"Ta bort", "border":"0"})
                )
              ).append("<br /><br />").append(
                $("<a />").attr("href","javascript:void(0)").click(function(){thisref.upImage(this);}).append(
                  $("<img />").attr({"src":"admin/gfx/up.gif", "alt":"Flytta upp", "border":"0"})
                )
              ).append("&nbsp;").append(
                $("<a />").attr("href","javascript:void(0)").click(function(){thisref.downImage(this, response.value.length);}).append(
                  $("<img />").attr({"src":"admin/gfx/down.gif", "alt":"Flytta ner", "border":"0"})
                )
              )
            ).append(
              $("<td />").append(
                $("<img />").attr({"src":params[9], "alt":""}).mouseover(function(){$(this).parent().parent().parent().parent().parent().children(".imgswap_editor_info").show();}).mouseout(function(){$(this).parent().parent().parent().parent().parent().children(".imgswap_editor_info").hide();})
              )
            )
          )
        ).append(
          $("<div />").addClass("clearfloat")
        )
      );
    }
    if (callback) callback();
  }


  this.openDocBank = function() {
    openMediabank("swp_callbackDB", this.arrindex);
  }

  this.callbackDB = function(img) {
    $("#" + this.id + "url").val(img.url);
    $("#" + this.id + "width").val(img.width);
    $("#" + this.id + "height").val(img.height);
  }

  this.editImage = function(elem) {
    var idx = $(elem).parent().parent().parent().parent().parent().attr("id").split('_')[1];
    $("#" + this.id + "url").val($("#url" + this.id + idx).val());
    $("#" + this.id + "width").val($("#width" + this.id + idx).html());
    $("#" + this.id + "height").val($("#height" + this.id + idx).html());
    $("#" + this.id + "pantype").val($("#pantype" + this.id + idx).html());
    $("#" + this.id + "fadetime").val($("#fadetime" + this.id + idx).html());
    $("#" + this.id + "timedelay").val($("#timedelay" + this.id + idx).html());
    $("#" + this.id + "pandelay").val($("#pandelay" + this.id + idx).html());
    $("#" + this.id + "link").val($("#link" + this.id + idx).html());
    $("#" + this.id + "target").val($("#target" + this.id + idx).html());
    $("#" + this.id + "editidx").val(idx);
    this.editformdiv.show();
  }

  this.cancelEdit = function() {
    this.editformdiv.hide();
  }

  this.deleteImage = function(elem) {
    if (confirm("Är du säker på att du vill ta bort bilden?")) {
      var idx = $(elem).parent().parent().parent().parent().parent().attr("id").split('_')[1];
      NFN.ImgSwapper.Delete(_pageId, this.id, idx, function(r){thisref.afterServer(r);});
    }
  }

  this.upImage = function(elem) {
    var idx = parseInt($(elem).parent().parent().parent().parent().parent().attr("id").split('_')[1]);
    if (idx > 0)
      NFN.ImgSwapper.Move(_pageId, this.id, idx, "up", function(r){thisref.afterServer(r);});
  }

  this.downImage = function(elem, maxidx) {
    var idx = parseInt($(elem).parent().parent().parent().parent().parent().attr("id").split('_')[1]);
    if (idx < maxidx)
      NFN.ImgSwapper.Move(_pageId, this.id, idx, "down", function(r){thisref.afterServer(r);});
  }

  this.afterServer = function(response) {
    this.setListHtml(thisref.sendToFlash);
  }

  this.saveImage = function() {
    var idx = $("#" + this.id + "editidx").val();
    var pf = "#" + this.id;
    var vals = $(pf + "url").val() + "|" + $(pf + "width").val() + "|" + $(pf + "height").val() + "|" + $(pf + "pantype").val() + "|" + $(pf + "fadetime").val() + "|" + $(pf + "timedelay").val() + "|" + $(pf + "pandelay").val() + "|" + $(pf + "link").val() + "|" + $(pf + "target").val();
    if (idx == 'new')
      NFN.ImgSwapper.SaveNew(_pageId, this.id, vals, function(r){thisref.saveImage2(r);});
    else
      NFN.ImgSwapper.Save(_pageId, this.id, idx, vals, function(r){thisref.saveImage2(r);});
  }

  this.saveImage2 = function(response) {
    this.setListHtml(thisref.sendToFlash);
    this.editformdiv.hide();
  }

  this.newImage = function() {
    $("#" + this.id + "url").val("");
    $("#" + this.id + "width").val("");
    $("#" + this.id + "height").val("");
    $("#" + this.id + "pantype").val("");
    $("#" + this.id + "fadetime").val("");
    $("#" + this.id + "timedelay").val("");
    $("#" + this.id + "pandelay").val("");
    $("#" + this.id + "link").val("");
    $("#" + this.id + "target").val("");
    $("#" + this.id + "editidx").val("new");
    this.editformdiv.show();
  }

  this.showEditor = function() {
    this.pendiv.hide();
    this.editordiv.show();
  }

  this.closeEditor = function() {
    this.pendiv.show();
    this.editordiv.hide();
  }

  this.initFlash = function() {
    var flashvars = {
      base_path: param.base_path,
      init_val: "",
      data_type: param.data_type,
      paus_on: param.paus_on,
      nav_on: param.nav_on,
      click_on: param.click_on,
      loop_on: param.loop_on,
      flash_id: id
    };
    var params = {
      salign: "lt",
      scale: "noscale",
      allowscriptaccess: "always",
      bgcolor: param.bgcolor,
      wmode: param.wmode
    }
    var attributes = {
      id : "movie_" + id
    };
    swfobject.embedSWF(param.flashpath, "flash_" + id, String(param.flashsize.width), String(param.flashsize.height), String(param.flashversion), "flash/expressInstall.swf", flashvars, params, attributes);
  }

  this.sendToFlash = function() {
    if (!swapperdata)
      NFN.ImgSwapper.GetSwapperData(_pageId, this.id, param.thumbsize.width, param.thumbsize.height, function(r) {thisref.sendToFlash2(r);} );
    else
      thisref.sendToFlash2({value:swapperdata});
  }

  this.sendToFlash2 = function(response) {
    if (response.error) {
      alert(response.error.Message);
      return;
    }
    swapperdata = response.value;
    var imgarr = new Array();
    var p;
    for (var i=0; i < swapperdata.length; i++) {
      var params = swapperdata[i].split('|');
      p = {
        path:params[0],
        w:parseInt(params[1]),
        h:parseInt(params[2]),
        pantype:params[3],
        fadetime:parseInt(params[4]),
        timedelay:parseInt(params[5]),
        pandelay:parseInt(params[6]),
        link:params[7],
        target:params[8]
      }
      imgarr.push(p);
    }
    var thisMovie = N$("movie_" + id);
    if (thisMovie.sendArrToFlash) thisMovie.sendArrToFlash(imgarr);
  }

  this.arrindex = imgswaps.length;
  imgswaps.push(this);

  this.setup();
}