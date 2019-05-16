/* $Date: 2009-11-27 11:25:41 +0100 (fr, 27 nov 2009) $    $Revision: 5672 $ */
function AjaxCalendar(targetdiv, param) {

  var thisref = this;
  this.currDate = new Date();
  this.monthDate = new Date();
  this.param = (param ? param : new Object());
  if (typeof (targetdiv) != "object")
    targetdiv = document.getElementById(targetdiv);
  if (!targetdiv) return;
  this.id = targetdiv.id;

  this.setup = function() {
    if (this.param.calendarStyle == null) this.param.calendarStyle = '/admin/jstools/ajaxcalendar';
    if (this.param.loggedIn == null) this.param.loggedIn = false;
    if (this.param.browseYears == null) this.param.browseYears = true;
    if (this.param.onDayClicked == null) this.param.onDayClicked = null;
    if (this.param.useContentPopup == null) this.param.useContentPopup = true;
    if (this.param.useDateContent == null) this.param.useDateContent = true;

    var sv = ["Stäng", "Föregående månad", "Nästa månad", "Föregående år", "Nästa år", "Mån", "Tis", "Ons", "Tor", "Fre", "Lör", "Sön"];
    var t = translateArr(sv);
    this.translation = new Object();
    for (var i=0; i < t.length; i++)
      this.translation[sv[i]] = t[i];

    var path = this.param.calendarStyle;
    $("head").addCssFile(path + "/styles.css");

    if (this.param.useContentPopup) {
      var adaydiv = N$CA("DIV", targetdiv, {id:"calendar_daydiv"});
      this.dayheader = N$CA("DIV", adaydiv, {id:"calendar_dayheader"});
      var adiv = N$CA("DIV", adaydiv, {id:"calendar_dayclose"});
      var aanc = N$CA("A", adiv, {href:"javascript:void(0)", onfocus:function(){this.blur();}, title:this.translation["Stäng"], onclick:function(){thisref.daydiv.close();}});
      var aimg = N$CA("IMG", aanc, { src:path + "/closeday.gif", onmouseover:function(){this.src=path + "/closeday_over.gif"}, onmouseout:function(){this.src=path + "/closeday.gif"}, alt:"Stäng", border:"0"});
      adiv = N$CA("DIV", adaydiv, {className:"clearfloat"});
      this.daycontent = N$CA("DIV", adaydiv, {id:"calendar_daycontent"});

      this.daydiv = new AnimDiv(adaydiv, null, true);
      this.daydiv.setup();
    }

    this.monthdiv = N$CA("DIV", targetdiv, null, {overflow:"hidden"});

    this.fillCalendar();
    if (this.param.useContentPopup)
      this.conttiny = new AjaxTiny(this.daycontent, { width:targetdiv.offsetWidth, height:targetdiv.offsetHeight, toolbarLocation:'external', contentByAjax:true });
  }

  this.decMonth = function() {
    this.monthDate = this.monthDate.addMonths(-1);
    this.fillCalendar();
  }

  this.incMonth = function() {
    this.monthDate = this.monthDate.addMonths(1);
    this.fillCalendar();
  }

  this.decYear = function() {
    this.monthDate = this.monthDate.addYears(-1);
    this.fillCalendar();
  }

  this.incYear = function() {
    this.monthDate = this.monthDate.addYears(1);
    this.fillCalendar();
  }

  this.setDate = function(adate) {
    var inView = this.markDate(adate);
    this.currDate = adate;
    if (!inView) {
      this.monthDate = adate;
      this.fillCalendar();
      this.markDate(adate);
    }
  }

  this.markDate = function(adate) {
    var currIdx = -1;
    var newIdx = -1;
    for (var i=0; i < this.dates.length && (currIdx < 0 || newIdx < 0); i++) {
      if (this.dates[i].dayEquals(this.currDate)) currIdx = i;
      if (this.dates[i].dayEquals(adate)) newIdx = i;
    }
    if (currIdx >= 0) {
      var oldtd = N$('td_' + currIdx);
      var hlp = oldtd.className.split(' ');
      oldtd.className = hlp[0];
    }
    if (newIdx >= 0)
      N$('td_' + newIdx).className += " selectedday";
    return newIdx >= 0;
  }

  this.fillCalendar = function() {
    var firstInMonth = this.monthDate.clone();
    firstInMonth.setDate(1);
    var lastInMonth = firstInMonth.clone();
    lastInMonth = lastInMonth.addMonths(1).addDays(-1);

    var firstDay = firstInMonth.clone();
    while (firstDay.getDay() != 1)
      firstDay = firstDay.addDays(-1);
    var lastDay = lastInMonth.clone();
    while (lastDay.getDay() != 0)
      lastDay = lastDay.addDays(1);

    if (!this.param.useDateContent || _loggedin)
      this.fillCalendar2(null, firstDay, lastDay, firstInMonth.getMonth());
    else
      NFN.CalendarPage.GetCalendarEvents(_pageId, firstDay, lastDay, function(r){thisref.fillCalendar2(r, firstDay, lastDay, firstInMonth.getMonth());});
  }

  this.fillCalendar2 = function(response, currDay, lastDay, currMonth) {
    var path = this.param.calendarStyle;

    this.monthdiv.innerHTML = "";
    var hdiv = N$CA("DIV", this.monthdiv, {className:"calendar_header" });

    var adiv = N$CA("DIV", hdiv, {className:"ch_left"});
    var aanc = N$CA("A", adiv, { href:"javascript:void(0)", onfocus:function(){this.blur();}, title:this.translation["Föregående månad"], onclick:function(){thisref.decMonth();} } );
    N$CA("IMG", aanc, { src:path + "/left.gif", onmouseover:function(){this.src = path + "/left_over.gif";}, onmouseout:function(){this.src = path + "/left.gif";}, border:"0", alt:this.translation["Föregående månad"] });
    if (this.param.browseYears) {
      aanc = N$CA("A", adiv, { href:"javascript:void(0)", onfocus:function(){this.blur();}, title:this.translation["Föregående år"], onclick:function(){thisref.decYear();} } );
      N$CA("IMG", aanc, { src:path + "/dleft.gif", onmouseover:function(){this.src = path + "/dleft_over.gif";}, onmouseout:function(){this.src = path + "/dleft.gif";}, border:"0", alt:this.translation["Föregående år"] });
    }

    adiv = N$CA("DIV", hdiv, {className:"ch_mid", innerHTML:this.monthDate.monthName() + "&nbsp;" + this.monthDate.getFullYear()});

    adiv = N$CA("DIV", hdiv, {className:"ch_right"});
    if (this.param.browseYears) {
      aanc = N$CA("A", adiv, { href:"javascript:void(0)", onfocus:function(){this.blur();}, title:this.translation["Nästa år"], onclick:function(){thisref.incYear();} } );
      N$CA("IMG", aanc, { src:path + "/dright.gif", onmouseover:function(){this.src = path + "/dright_over.gif";}, onmouseout:function(){this.src = path + "/dright.gif";}, border:"0", alt:this.translation["Nästa år"] });
    }
    aanc = N$CA("A", adiv, { href:"javascript:void(0)", onfocus:function(){this.blur();}, title:this.translation["Nästa månad"], onclick:function(){thisref.incMonth();} } );
    N$CA("IMG", aanc, { src:path + "/right.gif", onmouseover:function(){this.src = path + "/right_over.gif";}, onmouseout:function(){this.src = path + "/right.gif";}, border:"0", alt:this.translation["Nästa månad"] });

    adiv = N$CA("DIV", hdiv, {className:"clearfloat"});

    var mtable = N$CA("TABLE", this.monthdiv, {className:"calendar_month", cellSpacing:"0", cellPadding:"0", border:"0" });
    var mbody = N$CA("TBODY", mtable);
    atr = N$CA("TR", mbody);

    atd = N$CA("TD", atr, {className:"dayname", width:"14%", innerHTML:this.translation["Mån"]});
    atd = N$CA("TD", atr, {className:"dayname", width:"14%", innerHTML:this.translation["Tis"]});
    atd = N$CA("TD", atr, {className:"dayname", width:"14%", innerHTML:this.translation["Ons"]});
    atd = N$CA("TD", atr, {className:"dayname", width:"14%", innerHTML:this.translation["Tor"]});
    atd = N$CA("TD", atr, {className:"dayname", width:"14%", innerHTML:this.translation["Fre"]});
    atd = N$CA("TD", atr, {className:"dayname", width:"15%", innerHTML:this.translation["Lör"]});
    atd = N$CA("TD", atr, {className:"dayname", width:"15%", innerHTML:this.translation["Sön"]});

    var dayidx = 0;
    atr = N$CA("TR", mbody);
    this.dates = new Array();
    var now = new Date();
    while (currDay <= lastDay) {
      this.dates.push(currDay.clone());
      if (dayidx != 0 && dayidx % 7 == 0)
        atr = N$CA("TR", mbody);

      var isToday = currDay.dayEquals(now);
      var css = (isToday ? "today" : (currDay.getMonth() == currMonth ? (dayidx % 7 < 5 ? "inmonth" : "weekend") : "notinmonth"));

      atd = N$CA("TD", atr, {id:"td_" + dayidx, className:css, vAlign:"top", onclick:function (){ thisref.dayClicked(this);} }, {cursor:"pointer"});
      var adiv = N$CA("DIV", atd, { className:"c_d", innerHTML:currDay.getDate() });
      var aid = "s_" + currDay.dateString().replace(/-/g, "");
      adiv = N$CA("DIV", atd, { id:aid, className:"c_dc" });

      if (this.param.useDateContent) {
        if (response) {
          if (response.value[dayidx].length > 0) adiv.innerHTML = response.value[dayidx];
        }
        else new AjaxTiny(adiv, { toolbarLocation:'external', toolbarConfig:"simple", contentByAjax:true, beforeShowEditor:function(){thisref.isEditing(true);}, afterHideEditor:function(){thisref.isEditing(false);} });
      }

      currDay = currDay.addDays(1);
      dayidx++;
    }

    this.markDate(this.currDate);

    if (this.param.useContentPopup)
      this.daydiv.setDimensions({width:String(this.monthdiv.offsetWidth-2) + "px", height:this.monthdiv.offsetHeight + "px" });
  }

  this.isEditing = function(ed) {
    setTimeout(function() {thisref.editingDate = ed;}, 200);
  }

  this.editDay = function(event, a) {
    this.stopProp(event);
    var e = a;
    var s = a.nextSibling;
    var c = a.nextSibling.nextSibling;
    var r = e.parentNode.nextSibling;
    var stat = r.firstChild;
    var edit = r.childNodes[1];

    edit.firstChild.value = stat.innerHTML.replace(/<br \/>/g,'\n').replace(/<br>/g,'\n');
    stat.style.display = "none";
    edit.style.display = "inline";
    e.style.display = "none";
    s.style.display = "inline";
    c.style.display = "inline";
  }

  this.saveDay = function(event, a) {
    this.stopProp(event);
    var e = a.previousSibling;
    var s = a;
    var c = a.nextSibling;
    var r = e.parentNode.nextSibling;
    var stat = r.firstChild;
    var edit = r.childNodes[1];

  }

  this.canceleditDay = function(event, a) {
    this.stopProp(event);
    var e = a.previousSibling.previousSibling;
    var s = a.previousSibling;
    var c = a;
    var r = e.parentNode.nextSibling;
    var stat = r.firstChild;
    var edit = r.childNodes[1];
    stat.style.display = "inline";
    edit.style.display = "none";
    e.style.display = "inline";
    s.style.display = "none";
    c.style.display = "none";
  }

  this.dayClicked = function(td) {
    if (!this.editingDate) {
      var idx = td.id.substring(3);
      this.setDate(this.dates[idx]);
      if (this.param.useContentPopup)
        this.conttiny.setProperty(this.currDate.dateString().replace(/-/g, ""), null, function(r){thisref.dayClicked2(r);});
      if (this.param.onDayClicked)
        this.param.onDayClicked(this.currDate);
    }
  }

  this.dayClicked2 = function(response) {
    this.dayheader.innerHTML = this.currDate.dayName() + " " + this.currDate.getDate() + " " + this.currDate.monthName().toLowerCase();
    this.daydiv.open();
  }

  this.setup();

}