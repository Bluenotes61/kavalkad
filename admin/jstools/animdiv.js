/* $Date: 2009-06-02 02:28:55 +0200 (ti, 02 jun 2009) $    $Revision: 5013 $ */
var anim_divs = new Array();

function AnimDiv(targetdiv, param, manualsetup) {
  var animTimer = null;
  var slideTimer = null;
  var thisref = this;

  this.manualsetup = manualsetup;
  this.param = param;
  this.setupDone = false;

  this.setup = function(f) {
    if (f) this.param.onSetupDone = f;
    if (this.param == null) this.param = new Object();

    if (typeof (targetdiv) != "object")
      targetdiv = document.getElementById(targetdiv);
    if (!targetdiv) return;
    this.id = targetdiv.id;
    this.maindiv = targetdiv;

    this.currPos = this.getElemAbsPos(targetdiv);
    this.currSize = this.getElemSize(targetdiv);
    this.oriPos = this.getElemAbsPos(targetdiv);

    if (this.param.fade == null) this.param.fade = true;
    if (this.param.growOrigin == null) this.param.growOrigin = "none";  //left, top, right, bottom, topleft, topright, bottomleft, bottomright, center
    if (this.param.visible == null) this.param.visible = false;
    if (this.param.visiblemode == null) this.param.visiblemode = "display";
    if (this.param.steps == null) this.param.steps = 10;
    if (this.param.speed == null) this.param.speed = 20;
    if (this.param.opacity == null) this.param.opacity = 0;
    if (this.param.minOpacity == null) this.param.minOpacity = 0;
    if (this.param.maxOpacity == null) this.param.maxOpacity = 100;
    if (this.param.minwidth == null) this.param.minwidth = 0;
    if (this.param.maxwidth == null) this.param.maxwidth = this.currSize.width;
    if (this.param.minheight == null) this.param.minheight = 0;
    if (this.param.maxheight == null) this.param.maxheight = this.currSize.height;
    if (this.param.beforeOpen == null) this.param.beforeOpen = null;
    if (this.param.onOpen == null) this.param.onOpen = null;
    if (this.param.onOpened == null) this.param.onOpened = null;
    if (this.param.onClosed == null) this.param.onClosed = null;
    if (this.param.afterDrag == null) this.param.afterDrag = null;
    if (this.param.beforeClose == null) this.param.beforeClose = null;
    if (this.param.afterSlide == null) this.param.afterSlide = null;
    if (this.param.onSetupDone == null) this.param.onSetupDone = null;
    if (this.param.onResetCss == null) this.param.onResetCss = null;
    if (this.param.moveable == null) this.param.moveable = false;
    if (this.param.moveDiv == null) this.param.moveDiv = null;
    if (this.param.overflow == null) this.param.overflow = "hidden";
    if (this.param.toggleVisibility == null) this.param.toggleVisibility = true;

    targetdiv.style.overflow = this.param.overflow;
    if (this.param.zIndex)
      targetdiv.style.zIndex = String(this.param.zIndex);

    this.setVisibility(this.param.visible);

    if (this.param.moveable) this.makeMoveable();

    this.setupDone = true;
    if (this.param.onSetupDone) this.param.onSetupDone(this);
  }

  this.getElemAbsPos = function(elem) {
    var adisp = elem.style.display;
    elem.style.display = "block";
    var res = {left:elem.offsetLeft, top:elem.offsetTop};
    elem.style.display = adisp;
    var aelem = elem;
    while (aelem.offsetParent) {
      aelem = aelem.offsetParent;
      res.left += aelem.offsetLeft;
      res.top += aelem.offsetTop;
    }
    return res;
  }

  this.getElemPos = function(elem) {
    var res = {left:parseInt(this.getCssParam(elem, "left")), top:parseInt(this.getCssParam(elem, "top"))};
    if (isNaN(res.left) || isNaN(res.top)) {
      res = {left:elem.offsetLeft, top:elem.offsetTop};
      res.left -= this.getCssParam(elem, "marginLeft", true);
      res.top -= this.getCssParam(elem, "marginTop", true);
    }
    return res;
  }

  this.getElemSize = function(elem) {
    var res = {width:parseInt(this.getCssParam(elem, "width")), height:parseInt(this.getCssParam(elem, "height"))};
    if (isNaN(res.width)|| isNaN(res.height)) {
      var disp = elem.style.display;
      elem.style.display = "block";
      if (isNaN(res.width)) res.width = elem.offsetWidth;
      if (isNaN(res.height)) res.height = elem.offsetHeight;
      elem.style.display = disp;
    }
    return res;
  }

/*
  this.getCssParamAlt = function(elem, styleElem, asNumber) {
    var res = "";
    if(window.getComputedStyle) res = window.getComputedStyle(elem,null)[styleElem];
    else if(elem.currentStyle) res = elem.currentStyle[styleElem];

    if (asNumber) {
      if (res.length == 0) return 0;
      return parseInt(res.substring(0, res.length-2));
    }
    else return res;
  }
*/

  this.getCssParam = function(elem, styleelem, asNumber, ignoreInline) {
    if (!ignoreInline && elem.style[styleelem] && elem.style[styleelem].length > 0)
      return elem.style[styleelem];
    else {
      var res = "";
      try {
        var classes = elem.className.split(" ");
        var cssRules = (document.all ? "rules" : "cssRules");
        for (var i=0; i < document.styleSheets.length; i++){
          for (var j=0; j < document.styleSheets[i][cssRules].length; j++) {
            var sel = document.styleSheets[i][cssRules][j].selectorText;
            if (sel == "#" + elem.id && document.styleSheets[i][cssRules][j].style[styleelem].length > 0)
              res = document.styleSheets[i][cssRules][j].style[styleelem];
            else {
              for (var k=0; k < classes.length; k++)
                if (sel == "." + classes[k]) res = document.styleSheets[i][cssRules][j].style[styleelem];
            }
          }
        }
      }
      catch (e) {}
      if (asNumber) {
        var n = parseInt(res);
        return (isNaN(n) ? 0 : n);
      }
      else
        return res;
    }
  }

  function getEventPos(event) {
    if (Browser.isIE || Browser.isOpera) return {x:window.event.clientX + document.documentElement.scrollLeft + document.body.scrollLeft, y:window.event.clientY + document.documentElement.scrollTop + document.body.scrollTop};
    else return {x:event.clientX + window.scrollX, y:event.clientY + window.scrollY};
  }


  this.reposition = function(pos) {
    if (pos.left) {
      this.currPos.left = pos.left;
      this.oriPos.left = pos.left;
    }
    if (pos.top) {
      this.currPos.top = pos.top;
      this.oriPos.top = pos.top;
    }
    this.setPosition(this.currPos);
  }

  function cssFileLoaded(cssHref) {
    var loaded = false;
    var sheet = null;
    for (var i=0; i < document.styleSheets.length && !sheet; i++)
      if (document.styleSheets[i].href.indexOf(cssHref) >= 0) sheet = document.styleSheets[i];

    if (sheet) {
      var cssRules = (document.all ? "rules" : "cssRules");
      try {
        var len = sheet[cssRules].length;
        if (len > 0 ) loaded = true;
      }
      catch (e) {loaded = false; }
    }
    return loaded;
  }

  this.resetFromCssFile = function(cssHref, f) {
    if (f) this.param.tempOnResetCss = f;

    targetdiv.style.margin = "";
    targetdiv.style.left = "";
    targetdiv.style.top = "";

    this.resetCount = 0;
    this.resetFromCssFile2(cssHref);
  }

  this.resetFromCssFile2 = function(cssHref) {

    this.resetCount++;

    if (!cssFileLoaded(cssHref) && this.resetCount < 5) {
      setTimeout(function(){thisref.resetFromCssFile2(cssHref);}, 100);
      return;
    }
    this.doCssReset();
  }


  this.resetFromCss = function(f) {
    if (f) this.param.tempOnResetCss = f;

    if (targetdiv.style.left.length > 0 || targetdiv.style.top.length > 0 ) {
      this.helppos = this.getElemAbsPos(targetdiv);
      targetdiv.style.margin = "";
      targetdiv.style.left = "";
      targetdiv.style.top = "";
    }

    this.resetCount = 0;
    this.resetFromCss2();
  }

  this.resetFromCss2 = function() {

    this.resetCount++;
    var pos = this.getElemAbsPos(targetdiv);

    if (this.helppos && this.helppos.left == pos.left && this.helppos.top == pos.top && this.resetCount < 5) {
      setTimeout(function(){thisref.resetFromCss2();}, 100);
      return;
    }
  }

  this.doCssReset = function() {

    var width = parseInt(this.getCssParam(targetdiv, "width", false, true));
    if (!isNaN(width)) {
      this.param.maxwidth = width;
      this.currSize.width = width;
    }

    var height = parseInt(this.getCssParam(targetdiv, "height", false, true));
    if (!isNaN(height)) {
      this.param.maxheight = height;
      this.currSize.height = height;
    }
    this.setDimensions({width:this.currSize.width, height:this.currSize.height});

    var pos = this.getElemAbsPos(targetdiv);
    this.reposition(pos);

    if (this.param.tempOnResetCss) {
      this.param.tempOnResetCss(this);
      this.param.tempOnResetCss = null;
    }
    else if (this.param.onResetCss)
      this.param.onResetCss(this);
  }


  this.setOpacity = function(opacity) {
    var opac = parseInt(opacity);
    targetdiv.style.filter = "alpha(opacity:"+opac+")";
    targetdiv.style.KHTMLOpacity = opac/100;
    targetdiv.style.MozOpacity = opac/100;
    targetdiv.style.opacity = opac/100;
  }

  this.setPosition = function(pos) {
    targetdiv.style.left = parseInt(pos.left) + "px";
    targetdiv.style.top = parseInt(pos.top) + "px";
    targetdiv.style.margin = "0";
  }

  this.setDimensions = function(dim) {
    targetdiv.style.width = parseInt(dim.width) + "px";
    targetdiv.style.height = parseInt(dim.height) + "px";
  }

  this.setMargins = function(left, top) {
    if (left && left.length > 0) targetdiv.style.marginLeft = left;
    if (top && top.length > 0) targetdiv.style.marginTop = top;
  }

  this.setContent = function(html) {
    targetdiv.innerHTML = html;
  }



  this.getFadeParams = function(from, to) {
    this.fadeDir = (from < to ? 1 : (from > to ? -1 : 0));
    this.param.opacity = from;
    this.fadeStep = (to - from)*this.fadeDir/this.param.steps;
    this.fadeEnd = to;
  }

  this.getResizeParams = function(from, to) {
    this.growDir = {x:0, y:0};
    if (from.width > to.width) this.growDir.x = -1;
    else if (from.width < to.width) this.growDir.x = 1;
    if (from.height > to.height) this.growDir.y = -1;
    else if (from.height < to.height) this.growDir.y = 1;

    this.currSize = {width:from.width, height:from.height};

    var growRange = {
      width: (to.width-from.width)*this.growDir.x,
      height: (to.height-from.height)*this.growDir.y
    };

    this.growStep = {width:0, height:0};
    if (this.param.growOrigin != "top" && this.param.growOrigin != "bottom")
      this.growStep.width = growRange.width/this.param.steps;
    if (this.param.growOrigin != "left" && this.param.growOrigin != "right")
      this.growStep.height = growRange.height/this.param.steps;

    var moveRange = {x:0, y:0};
    this.moveDir = {x:-this.growDir.x, y:-this.growDir.y};
    if (this.param.growOrigin.indexOf("right") >= 0)
      moveRange.x = growRange.width;
    if (this.param.growOrigin.indexOf("bottom") >= 0)
      moveRange.y = growRange.height;
    if (this.param.growOrigin == "center")
      moveRange = {x: growRange.width / 2, y: growRange.height / 2};
    this.moveStep = {x:moveRange.x/this.param.steps, y:moveRange.y/this.param.steps};

    this.endPos = {
      left: this.currPos.left - moveRange.x*this.growDir.x,
      top: this.currPos.top - moveRange.y*this.growDir.y
    };

    this.endSize = {width:to.width, height:to.height};
    this.doGrow = true;
  }

  this.setVisibility = function(visible) {
    if (this.param.visiblemode == "visibility") targetdiv.style.visibility = (visible ? "visible" : "hidden");
    else targetdiv.style.display = (visible ? "block" : "none");
    this.param.visible = visible;
  }

  this.saveOriParam = function(param) {
    this.oriParam = null;
    if (param) {
      this.oriParam = new Object();
      for (prop in this.param)
        this.oriParam[prop] = this.param[prop];
      for (prop in param)
        this.param[prop] = param[prop];
    }
  }

  this.resize = function(width, height, param) {
    if (this.animating) {
      setTimeout(function(){thisref.open();}, 100);
      return;
    }
    this.saveOriParam(param);
  }

  this.slideTo = function(left, top, param) {
    if (this.animating) {
      setTimeout(function(){thisref.open();}, 100);
      return;
    }
    this.saveOriParam(param);

    this.endSize = {width: this.currSize.width, height:this.currSize.height};
    this.endPos = {left:left, top:top};

    this.growDir = {x:0, y:0};

    this.moveDir = {
      x: (this.endPos.left > this.currPos.left ? 1 : (this.endPos.left < this.currPos.left ? -1 : 0)),
      y: (this.endPos.top > this.currPos.top ? 1 : (this.endPos.top < this.currPos.top ? -1 : 0))
    };
    var moveRange = {
      x: (this.endPos.left - this.currPos.left)*this.moveDir.x,
      y: (this.endPos.top - this.currPos.top)*this.moveDir.y
    }
    this.moveStep = {x:moveRange.x/this.param.steps, y:moveRange.y/this.param.steps};

    this.getFadeParams(this.param.minOpacity, this.param.maxOpacity);

    this.animCallback = this.afterSlide;

    if (animTimer) clearInterval(animTimer);
    animTimer = setInterval(function(){thisref.animate();}, this.param.speed);
  }

  this.afterSlide = function() {
    if (this.param.afterSlide) this.param.afterSlide();
    if (this.oriParam) this.param = this.oriParam;
    this.oriPos = {left:this.currPos.left, top:this.currPos.top};
  }

  this.useAnimation = function() {
    return this.param.fade  || (this.param.growOrigin.length > 0 && this.param.growOrigin != 'none');
  }

  this.open = function(param) {
    this.saveOriParam(param);

    if (this.param.beforeOpen) this.param.beforeOpen(this);

    var from = {width:this.currSize.width, height:this.currSize.height};
    if (!animTimer) {
      from = {width:this.param.minwidth, height:this.param.minheight};
      if (this.param.growOrigin.indexOf("right") >= 0)
        this.currPos.left = this.oriPos.left + this.param.maxwidth;
      if (this.param.growOrigin.indexOf("bottom") >= 0)
        this.currPos.top = this.oriPos.top + this.param.maxheight;
      if (this.param.growOrigin == "center")
        this.currPos = {left:this.oriPos.left + this.param.maxwidth/2, top:this.oriPos.top + this.param.maxheight/2};
    }

    if (this.param.growOrigin == "top" || this.param.growOrigin == "bottom" || this.param.growOrigin.length == 0 || this.param.growOrigin == "none") from.width = this.param.maxwidth;
    if (this.param.growOrigin == "left" || this.param.growOrigin == "right" || this.param.growOrigin.length == 0 || this.param.growOrigin == "none") from.height = this.param.maxheight;
    var to = {width:this.param.maxwidth, height:this.param.maxheight};
    this.getResizeParams(from, to);
    this.getFadeParams(this.param.opacity, this.param.maxOpacity);

    this.setVisibility(true);
    if (this.param.onOpen) this.param.onOpen(this);

    if (this.param.fade) this.setOpacity(this.param.opacity);
    if (this.param.growOrigin != "none") {
      this.setDimensions(this.currSize);
      this.setPosition(this.currPos);
    }

    this.animCallback = this.afterOpen;

    if (animTimer) clearInterval(animTimer);
    if (this.useAnimation()) animTimer = setInterval(function(){thisref.animate();}, this.param.speed);
    else this.afterOpen();
  }

  this.afterOpen = function() {
    if (this.param.onOpened) this.param.onOpened(this);
    if (this.oriParam) this.param = this.oriParam;
  }

  this.close = function(param) {
//    if (!this.id) return;

    if (!this.param.visible) {
      this.currPos = {left:this.oriPos.left, top:this.oriPos.top};
      this.currSize = {width:this.param.maxwidth, height:this.param.maxheight};
      this.afterClose();
      return;
    }

    this.saveOriParam(param);

    if (!this.param.fade && (this.param.growOrigin.length == 0 || this.param.growOrigin == 'none')) {
      if (animTimer) clearInterval(animTimer);
      animTimer = null;
      this.afterClose();
      return;
    }

    if (this.param.beforeClose) this.param.beforeClose(this);

    var from = {width:this.currSize.width, height:this.currSize.height};

    var to = {width:this.param.minwidth, height:this.param.minheight};
    if (this.param.growOrigin == "top" || this.param.growOrigin == "bottom" || this.param.growOrigin.length == 0 || this.param.growOrigin == "none") to.width = this.param.maxwidth;
    if (this.param.growOrigin == "left" || this.param.growOrigin == "right" || this.param.growOrigin.length == 0 || this.param.growOrigin == "none") to.height = this.param.maxheight;
    this.getResizeParams(from, to);
    this.getFadeParams(this.param.opacity, this.param.minOpacity);

    if (this.param.fade) this.setOpacity(this.param.opacity);
    if (this.param.growOrigin != "none") {
      this.setDimensions(this.currSize);
      this.setPosition(this.currPos);
    }

    this.animCallback = this.afterClose;

    if (animTimer) clearInterval(animTimer);
    if (this.useAnimation()) animTimer = setInterval(function(){thisref.animate();}, this.param.speed);
    else this.afterClose();
  }

  this.afterClose = function() {
    if (this.param.onClosed) this.param.onClosed();
    if (this.param.growOrigin != "none") {
      this.setDimensions({width:this.param.maxwidth,height:this.param.maxheight});
      this.setPosition(this.oriPos);
    }
    this.param.opacity = this.param.minOpacity;
    if (this.oriParam) this.param = this.oriParam;
    if (this.param.toggleVisibility) this.setVisibility(false);
  }

  this.animate = function() {
    this.animating = true;
    var done = true;
    this.setVisibility(true);

    if (this.param.fade && this.fadeDir*this.param.opacity < this.fadeDir*this.fadeEnd) {
      done = false;
      this.param.opacity += this.fadeDir*this.fadeStep;
      if (this.fadeDir*this.param.opacity > this.fadeDir*this.fadeEnd)
        this.param.opacity = this.fadeEnd;
    }

    if (this.param.growOrigin != "none") {
      if (this.growDir.x*this.currSize.width < this.growDir.x*this.endSize.width) {
        done = false;
        this.currSize.width += this.growDir.x*this.growStep.width;
        if (this.growDir.x*this.currSize.width > this.growDir.x*this.endSize.width)
          this.currSize.width = this.endSize.width;
      }

      if (this.growDir.y*this.currSize.height < this.growDir.y*this.endSize.height) {
        done = false;
        this.currSize.height += this.growDir.y*this.growStep.height;
        if (this.growDir.y*this.currSize.height > this.growDir.y*this.endSize.height)
          this.currSize.height = this.endSize.height;
      }

      if (this.moveDir.x*this.currPos.left < this.moveDir.x*this.endPos.left) {
        done = false;
        this.currPos.left += this.moveDir.x*this.moveStep.x;
        if (this.moveDir.x*this.currPos.left > this.moveDir.x*this.endPos.left)
          this.currPos.left = this.endPos.left;
      }

      if (this.moveDir.y*this.currPos.top < this.moveDir.y*this.endPos.top) {
        done = false;
        this.currPos.top += this.moveDir.y*this.moveStep.y;
        if (this.moveDir.y*this.currPos.top > this.moveDir.y*this.endPos.top)
          this.currPos.top = this.endPos.top;
      }

      this.setDimensions(this.currSize);
      this.setPosition(this.currPos);
    }

    if (this.param.fade) this.setOpacity(this.param.opacity);

    if (done) {
      clearInterval(animTimer);
      animTimer = null;
      this.animating = false;
      if (this.animCallback) this.animCallback();
    }
  }

  this.makeMoveable = function() {
    this.param.moveable = true;
    var moveDiv = targetdiv;
    if (this.param.moveDiv != null)
      moveDiv = (typeof (this.param.moveDiv) != "object" ? document.getElementById(this.param.moveDiv) : this.param.moveDiv);
    if (moveDiv) {
      moveDiv.style.cursor = "move";
      if (moveDiv.attachEvent) moveDiv.attachEvent('onmousedown', thisref.startDrag);
      else moveDiv.addEventListener("mousedown", thisref.startDrag, false);
    }
  }

  this.cancelMoveable = function() {
    this.param.moveable = false;
    if (targetdiv) {
      targetdiv.style.cursor = "normal";
      if (document.detachEvent) document.detachEvent('onmousedown', thisref.startDrag);
      else document.removeEventListener("mousedown", thisref.startDrag, false);
    }
  }

  var refPoint = new Object();
  var dragging = false;

  this.startDrag = function(event) {
    if (document.attachEvent) {
      document.attachEvent('onmousemove', thisref.drag);
      document.attachEvent('onmouseup', thisref.endDrag);
    }
    else {
      document.addEventListener("mousemove", thisref.drag, false);
      document.addEventListener("mouseup", thisref.endDrag, false);
    }
    refPoint = getEventPos(event);
    var divpos = thisref.getElemPos(targetdiv);
    refPoint.x -= divpos.left;
    refPoint.y -= divpos.top;
    dragging = true;
  }

  this.drag = function(event) {
    if (dragging) {
      var currMouse = getEventPos(event);
      targetdiv.style.left = (currMouse.x - refPoint.x) + "px";
      targetdiv.style.top = (currMouse.y - refPoint.y) + "px";
    }
  }

  this.endDrag = function(event) {
    if (document.detachEvent) {
      document.detachEvent('onmousemove', thisref.drag);
      document.detachEvent('onmouseup', thisref.endDrag);
    }
    else {
      document.removeEventListener("mousemove", thisref.drag, false);
      document.removeEventListener("mouseup", thisref.endDrag, false);
    }
    var currMouse = getEventPos(event);
    targetdiv.style.left = (currMouse.x - refPoint.x) + "px";
    targetdiv.style.top = (currMouse.y - refPoint.y) + "px";
    this.oriPos = {left:currMouse.x - refPoint.x, top:currMouse.y - refPoint.y};
    if (thisref.param.afterDrag) thisref.param.afterDrag(this.oriPos);
    dragging = false;
  }



  this.wobble = function(dir) {
    var steps;
    if (!dir) {
      steps = this.param.steps;
      this.param.steps = 2;
      this.slideRelative(5, 0, function(pop){ pop.wobble(1); });
    }
    else if (dir == 1) this.slideRelative(-10, 0, function(pop){ pop.wobble(2); });
    else if (dir == 2) this.slideRelative(5, 5, function(pop){ pop.wobble(3); });
    else if (dir == 3) this.slideRelative(0, -10, function(pop){ pop.wobble(4); });
    else if (dir == 4) this.slideRelative(0, 5, function(pop){ pop.wobble(5);});
    else this.param.steps = steps;
  }

  anim_divs.push(this);
}

function getAnimDivById(id) {
  var res = null;
  for (var i = 0; i < anim_divs.length && res == null; i++) {
    alert(anim_divs[i].id);
    if (anim_divs[i].id == id) res = anim_divs[i];
  }
  return res;
}

function animdiv_init() {
  var divs = document.getElementsByTagName("DIV");
  for (var i=0; i < divs.length; i++)
    if (divs[i].className.indexOf("animdiv") >= 0)
      new AnimDiv(divs[i]);

  for (var i=0; i < anim_divs.length; i++)
    if (!anim_divs[i].manualsetup) anim_divs[i].setup();
};

if (window.attachEvent)
  window.attachEvent("onload", animdiv_init);
else
  window.addEventListener("load", animdiv_init, false);
