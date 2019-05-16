/* $Date: 2010-10-06 14:43:51 +0200 (on, 06 okt 2010) $    $Revision: 7015 $ */

(function($){

  $.fn.scroller = function(options, callback) {

    var nofsetup = 0;
    var thisref = this;

    var settings = $.extend({}, $.fn.scroller.defaults, options);

    function setupDone() {
      nofsetup++;
      if (callback && nofsetup == thisref.length)
        callback(thisref);
    }


    return this.each(function() {

      var obj = this;
      var loadingImages = 0;
      var scrollTimer = null;

      var horizThumb;
      var vertThumb;
      var sframe;
      var sdiv;

      obj.scroller = new Object();
      obj.scrollInfo = new Object();
      obj.thumbObj = new Object();

      obj.init = function() {
        if (settings.width == null) {
          var w = obj.getCss("width", true);
          settings.width = (w == 0 ? 200 : w);
        }
        if (settings.height == null || settings.height.length == 0) {
          var h = obj.getCss("height", true);
          settings.height = (h == 0 ? 200 : h);
        }

        if (settings.vertScroller) loadingImages += 4;
        if (settings.horizScroller) loadingImages += 4;

        if (settings.vertScroller) {
          if (!settings.upArrHeight) obj.setImgParam(settings.imagepath + "/" + settings.upimg, "upArrHeight", "height");
          if (!settings.downArrHeight) obj.setImgParam(settings.imagepath + "/" + settings.downimg, "downArrHeight", "height");
          if (!settings.vertThumbLength) obj.setImgParam(settings.imagepath + "/" + settings.vsliderimg, "vertThumbLength", "height");
          if (!settings.vertTrackWidth) obj.setImgParam(settings.imagepath + "/" + settings.vtrackimg, "vertTrackWidth", "width");
        }
        if (settings.horizScroller) {
          if (!settings.leftArrWidth) obj.setImgParam(settings.imagepath + "/" + settings.leftimg, "leftArrWidth", "width");
          if (!settings.rightArrWidth) obj.setImgParam(settings.imagepath + "/" + settings.rightimg, "rightArrWidth", "width");
          if (!settings.horizThumbLength) obj.setImgParam(settings.imagepath + "/" + settings.hsliderimg, "horizThumbLength", "width");
          if (!settings.horizTrackHeight) obj.setImgParam(settings.imagepath + "/" + settings.htrackimg, "horizTrackHeight", "height");
        }
      }


      obj.imgLoaded = function(img, paramname, imgparam) {
        settings[paramname] = eval("img." + imgparam);
        loadingImages--;
        if (loadingImages == 0) obj.setup();
      };

      obj.setImgParam = function(imgsrc, paramname, imgparam) {
        var img = new Image();
        img.onload = function() {
          obj.imgLoaded(img, paramname, imgparam);
        }
        img.src = imgsrc;

        //$("body").append($("<img />").attr("src", imgsrc).load(function(){obj.imgLoaded(this, paramname, imgparam);}));
      };


      obj.isVertScrollVisible = function() {
        var res = false;
        if (obj.vertTrack != null)
          res = (obj.vertTrack.css("display") == 'block');
        return res;
      };

      obj.isHorizScrollVisible = function() {
        var res = false;
        if (obj.horizTrack != null)
          res = (obj.horizTrack.css("display") == 'block');
        return res;
      };

      obj.setContent = function(html, keepscrollpos) {
        obj.contentdiv.html(html);
        obj.contentChanged(keepscrollpos);
      };

      obj.getContent = function() {
        return obj.contentdiv.html();
      };

      obj.contentChanged = function(keepscrollpos) {
        try {
          sframe.css("width", "auto");
          sframe.css("height", "auto");
          sframe.css("overflow", "visible");
          sdiv.css("position", "relative");

          obj.initScrollers();
          if (!keepscrollpos && settings.horizScroller) obj.jumpTo("horizontal", 0);
          if (!keepscrollpos && settings.vertScroller) obj.jumpTo("vertical", 0);
        }
        catch(e) {}
      };

      obj.getCss = function(csselem, asNumber) {
        var res = "";
        var elem = $(this).get()[0];
        if (elem.style[csselem] && elem.style[csselem].length > 0)
          res = elem.style[csselem];
        else {
          var classes = elem.className.split(" ");
          var cssRules = (document.all ? "rules" : "cssRules");
          for (var i=0; i < document.styleSheets.length && res.length == 0; i++){
            if (document.styleSheets[i].href.indexOf(_url) == 0) {
              for (var j=0; j < document.styleSheets[i][cssRules].length && res.length == 0; j++) {
                var sel = document.styleSheets[i][cssRules][j].selectorText;
                if (sel == "#" + elem.id)
                  res = document.styleSheets[i][cssRules][j].style[csselem];
                else {
                  for (var k=0; k < classes.length; k++)
                    if (sel == "." + classes[k]) res = document.styleSheets[i][cssRules][j].style[csselem];
                }
              }
            }
          }
        }
        if (asNumber) {
          if (res.length == 0) return 0;
          return parseInt(res.substring(0, res.length-2));
        }
        else
          return res;
      };

      obj.getEventPos = function(event) {
        if (obj.scrollInfo.orientation == null) return 0;
        if ($.browser.msie || $.browser.opera) {
          if (obj.scrollInfo.orientation == 'vert')
            return window.event.clientY + document.documentElement.scrollTop + document.body.scrollTop;
          else
            return window.event.clientX + document.documentElement.scrollLeft + document.body.scrollLeft;
        }
        else {
          if (obj.scrollInfo.orientation == 'vert')
            return event.clientY + window.scrollY;
          else
            return event.clientX + window.scrollX;
        }
      };

      obj.adjustMousePos = function(apos) {
        if (obj.scrollInfo.orientation != null) {
          var extraVert = 0;
          var extraHoriz = 0
          if (settings.horizScrollPosition == "top")
            extraVert = settings.horizTrackHeight;
          if (settings.vertScrollPosition == "left")
            extraHoriz = settings.vertTrackWidth;
          if (obj.scrollInfo.orientation == 'vert')
            apos -= (obj.scroller.top + obj.scroller.arrHeight + extraVert);
          else
            apos -= (obj.scroller.left + obj.scroller.arrWidth + extraHoriz);
        }
        return apos;
      };

      obj.thumbPosFromScrollPos = function(apos) {
        if (obj.scrollInfo.orientation == 'vert')
          return parseInt(apos/(obj.scroller.height-obj.scrollInfo.height)*(obj.scroller.trackHeight-obj.thumbObj.size_vert));
        else
          return parseInt(apos/(obj.scroller.width-obj.scrollInfo.width)*(obj.scroller.trackWidth-obj.thumbObj.size_horiz));
      };

      obj.scrollPosFromThumbPos = function(apos) {
        if (obj.scrollInfo.orientation == 'vert')
          return parseInt(apos/(obj.scroller.trackHeight-obj.thumbObj.size_vert)*(obj.scroller.height-obj.scrollInfo.height));
        else
          return parseInt(apos/(obj.scroller.trackWidth-obj.thumbObj.size_horiz)*(obj.scroller.width-obj.scrollInfo.width));
      };

      obj.resetSpeed = function() {
        obj.scrollInfo.incVert = settings.vertScrollSpeed;
        if (obj.scrollInfo.incVert == 0) obj.scrollInfo.incVert = 1;
        obj.scrollInfo.incHoriz = settings.horizScrollSpeed;
        if (obj.scrollInfo.incHoriz == 0) obj.scrollInfo.incHoriz = 1;
      };

      obj.jumpTo = function(dir,endPos) {
        if (dir == 'vertical') {
          obj.scrollInfo.orientation = 'vert';
          obj.thumbObj.elNode = vertThumb;
        }
        else {
          obj.scrollInfo.orientation = 'horiz';
          obj.thumbObj.elNode = horizThumb;
        }

        obj.initScrolling(0);
        if (endPos < obj.scrollInfo.scrollMax && (dir == 'horiz' && obj.horizTrack.css("display") == 'block' || dir == 'vert' && obj.vertTrack.css("display") == 'block'))
          endPos = obj.scrollInfo.scrollMax;

        obj.thumbObj.pos = obj.thumbPosFromScrollPos(endPos);
        obj.scrollInfo.pos = endPos;
        obj.scrollTo();
      };

      obj.scrollStart = function(event,dir) {
        if (scrollTimer == null) {
          obj.scrollInfo.orientation = dir;
          var thumb = (obj.scrollInfo.orientation == 'vert' ? vertThumb : horizThumb);
          obj.thumbObj.elNode = thumb;

          var mousePos = obj.adjustMousePos(obj.getEventPos(event));
          obj.initScrolling(mousePos);
          obj.thumbObj.pos = obj.thumbObj.elStart;

          var asize = (dir == 'vert' ? obj.thumbObj.size_vert : obj.thumbObj.size_horiz);
          obj.scrollInfo.direction = 'none';

          if (mousePos > obj.thumbObj.pos + asize) obj.scrollInfo.direction = 'positive';
          else if (mousePos < obj.thumbObj.pos) obj.scrollInfo.direction = 'negative';

          var endoffset = (obj.scrollInfo.orientation == 'vert' ? sframe.offsetTop : sframe.offsetLeft);

          obj.thumbObj.endPos = mousePos - parseInt(asize/2) - endoffset;
          obj.scrollInfo.endPos = obj.scrollPosFromThumbPos(obj.thumbObj.endPos);

          obj.doStartScroll();
        }
        else if (obj.scrollInfo.orientation == 'vert')
          obj.scrollInfo.incVert *= 2;
        else
          obj.scrollInfo.incHoriz *= 2;
      };

      obj.doStartScroll = function() {
        if (obj.scrollInfo.direction != 'none')
          scrollTimer = setTimeout(function(){obj.doScroll();}, obj.scrollInfo.speed);
      };

      obj.doScroll = function() {
        if (obj.scrollInfo.direction == 'negative')
          obj.scrollInfo.pos += (obj.scrollInfo.orientation == 'vert' ? obj.scrollInfo.incVert : obj.scrollInfo.incHoriz);
        else
          obj.scrollInfo.pos -= (obj.scrollInfo.orientation == 'vert' ? obj.scrollInfo.incVert : obj.scrollInfo.incHoriz);
        obj.thumbObj.pos = obj.thumbPosFromScrollPos(obj.scrollInfo.pos);

        var doStop = false;
        if (obj.scrollInfo.pos > 0) {
          obj.scrollInfo.pos = 0;
          doStop = true;
        }
        if (obj.scrollInfo.pos < obj.scrollInfo.scrollMax) {
          obj.scrollInfo.pos = obj.scrollInfo.scrollMax;
          doStop = true;
        }
        if (obj.scrollInfo.direction == 'negative' && obj.scrollInfo.pos > obj.scrollInfo.endPos) {
          obj.thumbObj.pos = obj.thumbObj.endPos;
          obj.scrollInfo.pos = obj.scrollInfo.endPos;
          doStop = true;
        }
        if (obj.scrollInfo.direction == 'positive' && obj.scrollInfo.pos < obj.scrollInfo.endPos) {
          obj.thumbObj.pos = obj.thumbObj.endPos;
          obj.scrollInfo.pos = obj.scrollInfo.endPos;
          doStop = true;
        }

        obj.thumbObj.pos = obj.thumbPosFromScrollPos(obj.scrollInfo.pos);
        obj.scrollTo();
        if (doStop) obj.scrollStop();
        else scrollTimer = setTimeout(function(){obj.doScroll();}, obj.scrollInfo.speed);
      };

      obj.scrollTo = function() {
        if (obj.scrollInfo.orientation == 'vert') {
          obj.thumbObj.elNode.css("top", obj.thumbObj.pos);
          sdiv.css("top", obj.scrollInfo.pos);
        }
        else {
          obj.thumbObj.elNode.css("left", obj.thumbObj.pos);
          sdiv.css("left", obj.scrollInfo.pos);
        }
      };

      obj.scrollStop = function() {
        clearTimeout(scrollTimer);
        scrollTimer = null;
        obj.resetSpeed();
        if (obj.scrollInfo.orientation == 'vert')
          obj.scrollInfo.vertPos = obj.scrollInfo.pos;
        else
          obj.scrollInfo.horizPos = obj.scrollInfo.pos;
      };

      obj.doStep = function(dir) {
        var scrollTo = null;
        var step = (dir == 'left' || dir == 'right' ? parseInt(obj.scroller.width*obj.scroller.horizStepSize/100) : parseInt(obj.scroller.height*obj.scroller.vertStepSize/100));
        var apos = (dir == 'left' || dir == 'right' ? obj.scrollInfo.horizPos : obj.scrollInfo.vertPos);
        if (dir == 'left' || dir == 'up') scrollTo = apos + step;
        else scrollTo = apos - step;
        if (scrollTo != null) obj.stepTo(dir, scrollTo);
      };

      obj.stepTo = function(dir,endPos) {
        if (dir == 'up' || dir == 'down') {
          obj.scrollInfo.orientation = 'vert';
          obj.thumbObj.elNode = vertThumb;
        }
        else {
          obj.scrollInfo.orientation = 'horiz';
          obj.thumbObj.elNode = horizThumb;
        }

        obj.scrollInfo.pos = obj.scrollInfo.direction = (dir == 'up' || dir == 'down' ? obj.scrollInfo.vertPos : obj.scrollInfo.horizPos);
        obj.scrollInfo.direction = (dir == 'up' || dir == 'left' ? 'negative' : 'positive');

        obj.initScrolling(0);
        obj.thumbObj.endPos = obj.thumbPosFromScrollPos(endPos);
        obj.scrollInfo.endPos = endPos;

        obj.scrollInfo.incVert = obj.scroller.stepIncVert;
        obj.scrollInfo.incHoriz = obj.scroller.stepIncHoriz;
        obj.doStartScroll();
      };

      obj.changeScrollEnd = function(event) {
        var mousePos = obj.adjustMousePos(obj.getEventPos(event));

        var endoffset = (obj.scrollInfo.orientation == 'vert' ? sframe.offset().top : sframe.offset().left);

        if (obj.scrollInfo.orientation == 'vert')
          obj.thumbObj.endPos = mousePos - parseInt(obj.thumbObj.size_vert/2) - endoffset;
        else
          obj.thumbObj.endPos = mousePos - parseInt(obj.thumbObj.size_horiz/2) - endoffset;
        obj.scrollInfo.endPos = obj.scrollPosFromThumbPos(obj.thumbObj.endPos);
      };

      obj.dragStart = function(event,thumb) {
        obj.thumbObj.elNode = thumb;
        obj.scrollInfo.orientation = (thumb == vertThumb ? 'vert' : 'horiz');

        var mousePos = obj.getEventPos(event);
        obj.initScrolling(mousePos);

        obj.dragGoRef = function(e){obj.dragGo(e);};
        obj.dragStopRef = function(e){obj.dragStop(e);};

        obj.thumbObj.elNode.css("z-index", ++obj.thumbObj.zIndex);
        if ($.browser.msie) {
          document.attachEvent('onmousemove', obj.dragGoRef);
          document.attachEvent('onmouseup',   obj.dragStopRef);
          window.event.cancelBubble = true;
          window.event.returnValue = false;
        }
        else {
          document.addEventListener('mousemove', obj.dragGoRef,   true);
          document.addEventListener('mouseup',   obj.dragStopRef, true);
          event.preventDefault();
        }
      };

      obj.dragGo = function(event) {
        var mousePos = obj.getEventPos(event);
        obj.thumbObj.pos = obj.thumbObj.elStart + mousePos - obj.thumbObj.cursorStart;

        if (obj.thumbObj.pos < 0) obj.thumbObj.pos = 0;
        if (obj.thumbObj.pos > obj.thumbObj.scrollMax) obj.thumbObj.pos = obj.thumbObj.scrollMax;

        obj.scrollInfo.pos = obj.scrollPosFromThumbPos(obj.thumbObj.pos);
        obj.scrollTo();

        if ($.browser.msie) {
          window.event.cancelBubble = true;
          window.event.returnValue = false;
        }
        else {
          event.preventDefault();
        }
      };

      obj.dragStop = function(event) {
        if ($.browser.msie) {
          document.detachEvent('onmousemove', obj.dragGoRef);
          document.detachEvent('onmouseup',   obj.dragStopRef);
        }
        else {
          document.removeEventListener('mousemove', obj.dragGoRef,   true);
          document.removeEventListener('mouseup',   obj.dragStopRef, true);
        }
      };


      obj.initScrolling = function(mousePos) {
        obj.thumbObj.cursorStart = mousePos;
        if (obj.scrollInfo.orientation == 'vert') {
          obj.thumbObj.elStart = parseInt(obj.thumbObj.elNode.css("top"), 10);
          obj.thumbObj.scrollMax = obj.scroller.trackHeight - obj.thumbObj.size_vert;
          obj.scrollInfo.scrollMax = obj.scroller.height - obj.scrollInfo.height;
        }
        else {
          obj.thumbObj.elStart  = parseInt(obj.thumbObj.elNode.css("left"), 10);
          obj.thumbObj.scrollMax = obj.scroller.trackWidth - obj.thumbObj.size_horiz;
          obj.scrollInfo.scrollMax = obj.scroller.width - obj.scrollInfo.width;
        }
        if (isNaN(obj.thumbObj.elStart)) obj.thumbObj.elStart = 0;
      };

      obj.setupObjects = function() {
        obj.scroller.width = settings.width;
        obj.scroller.height = settings.height;
        obj.scroller.trackWidth = settings.width - settings.leftArrWidth - settings.rightArrWidth;
        obj.scroller.trackHeight = settings.height - settings.upArrHeight - settings.downArrHeight;
        obj.scroller.arrWidth = settings.leftArrWidth;
        obj.scroller.arrHeight = settings.upArrHeight;
        obj.scroller.vertStepSize = settings.vertStepSize;
        obj.scroller.horizStepSize = settings.horizStepSize;
        obj.scroller.vertStepSpeed = settings.vertStepSpeed;
        obj.scroller.horizStepSpeed = settings.horizStepSpeed;
        obj.scrollInfo.speed = 1;
        obj.scrollInfo.pos = 0;
        obj.scrollInfo.vertPos = 0;
        obj.scrollInfo.horizPos = 0;

        obj.thumbObj.zIndex = 0;
        obj.thumbObj.pos = 0;
        obj.thumbObj.size_vert = settings.vertThumbLength;
        obj.thumbObj.size_horiz = settings.horizThumbLength;
      };


      obj.setupHtml = function() {
        $(obj).wrapInner($("<div />"));
        obj.contentdiv = $(obj).children(":first");
        obj.contentdiv.remove();

        obj.contentdiv.css({
          "position": "relative",
//          "width":(settings.horizScroller && $.browser.msie ? obj.getCss("width") : "auto"),
          "width":"auto",
          "height" : "auto"
        });
        $(obj).css({
          width: settings.width,
          height: settings.height
        });

        sframe = $("<div />").css({"overflow":"hidden", "position":"relative"});
        $(obj).append(sframe);
        sdiv = $("<div />").css("position", "absolute");
        sframe.append(sdiv);
        sdiv.append(obj.contentdiv);

        if (settings.horizScroller) {
          obj.horizTrack = $("<div />").css({
            "position": "absolute",
            "background": "url(" + settings.imagepath + "/" + settings.htrackimg + ")",
            "display": "block",
            "margin-top": (settings.horizScrollPosition == "top" ? "0" : String(settings.height-settings.horizTrackHeight) + "px"),
            "width": String(settings.width) + "px",
            "height": String(settings.horizTrackHeight) + "px"
          });

           var leftarr = $("<div />").css("position", "absolute").append(
            $("<a />").focus(function(){this.blur();}).attr("href", "javascript:void(0)").click(function(){obj.doStep("left");}).append(
              $("<img />").attr({
                "src": settings.imagepath + "/" + settings.leftimg,
                "alt": ""
              }).css({"border": "0", "display": "block" })
            )
          );
          obj.horizTrack.append(leftarr);

          var w = settings.width - settings.leftArrWidth - settings.rightArrWidth;
          var track = $("<div />").css({
            "position": "absolute",
            "margin-left": String(settings.leftArrWidth) + "px",
            "height": String(settings.horizTrackHeight) + "px",
            "width": String(w < 0 ? 0 : w) + "px"
          }).mouseout(function(){
            obj.scrollStop();
          }).mousemove(function(e){
            obj.changeScrollEnd(e);
          }).click(function(e){
            obj.scrollStart(e, "horiz");
          });
          obj.horizTrack.append(track);

          horizThumb = $("<div />").css({
            "position": "absolute",
            "z-index": "1"
          }).mouseover(function() {
            obj.scrollStop();
          });
          var hthumb = horizThumb;
          horizThumb.mousedown(function(e){
            obj.dragStart(e, hthumb);
          }).append(
            $("<a />").focus(function(){this.blur();}).attr("href", "javascript:void(0)").append(
              $("<img />").attr({
                "src": settings.imagepath + "/" + settings.hsliderimg,
                "alt": ""
              }).css({"border": "0", "display": "block"})
            )
          );
          track.append(horizThumb);

          var rightarr = $("<div />").css({
            "position": "absolute",
            "margin-left": String(settings.width - settings.leftArrWidth) + "px"
          }).append(
            $("<a />").focus(function(){this.blur();}).attr("href", "javascript:void(0)").click(function(){obj.doStep("right");}).append(
              $("<img />").attr({
                "src": settings.imagepath + "/" + settings.rightimg,
                "alt": ""
              }).css({"border": "0", "display": "block" })
            )
          );
          obj.horizTrack.append(rightarr);

          $(obj).append(obj.horizTrack);
        }

        if (settings.vertScroller) {
          obj.vertTrack = $("<div />").css({
            "position": "absolute",
            "background": "url(" + settings.imagepath + "/" + settings.vtrackimg + ")",
            "display": "block",
            "margin-left": (settings.vertScrollPosition == "left" ? "0" : String(settings.width - settings.vertTrackWidth) + "px"),
            "width": String(settings.vertTrackWidth) + "px",
            "height": String(settings.height) + "px"
          });

          var uparr = $("<div />").css("position", "absolute").append(
            $("<a />").focus(function(){this.blur();}).attr("href", "javascript:void(0)").click(function(){obj.doStep("up");}).append(
              $("<img />").attr({
                "src": settings.imagepath + "/" + settings.upimg,
                "alt": ""
              }).css({"border": "0", "display": "block" })
            )
          );
          obj.vertTrack.append(uparr);

          var h = settings.height - settings.upArrHeight - settings.downArrHeight;
          var track = $("<div />").css({
            "position": "absolute",
            "margin-top": String(settings.upArrHeight) + "px",
            "height": String(h < 0 ? 0 : h) + "px",
            "width": String(settings.vertTrackWidth) + "px"
          }).mouseout(function(){
            obj.scrollStop();
          }).mousemove(function(e){
            obj.changeScrollEnd(e);
          }).click(function(e){
            obj.scrollStart(e, "vert");
          });
          obj.vertTrack.append(track);

          vertThumb = $("<div />").css({
            "position": "absolute",
            "z-index": "1"
          }).mouseover(function() {
            obj.scrollStop();
          });
          var vthumb = vertThumb;
          vertThumb.mousedown(function(e){
            obj.dragStart(e, vthumb);
          }).append(
            $("<a />").focus(function(){this.blur();}).attr("href", "javascript:void(0)").append(
              $("<img />").attr({
                "src": settings.imagepath + "/" + settings.vsliderimg,
                "alt": ""
              }).css({"border": "0", "display": "block"})
            )
          );
          track.append(vertThumb);

          var downarr = $("<div />").css({
            "position": "absolute",
            "margin-top": String(settings.height - settings.downArrHeight) + "px"
          }).append(
            $("<a />").focus(function(){this.blur();}).attr("href", "javascript:void(0)").click(function(){obj.doStep("down");}).append(
              $("<img />").attr({
                "src": settings.imagepath + "/" + settings.downimg,
                "alt": ""
              }).css({"border": "0", "display": "block" })
            )
          );
          obj.vertTrack.append(downarr);

          $(obj).append(obj.vertTrack);
        }
      };

      obj.initScrollers = function() {
        var swidth = sdiv.width();

        obj.scroller.top = sframe.offset().top;
        obj.scroller.left = sframe.offset().left;

        obj.scrollInfo.width = swidth;
        sframe.css({
          "overflow": "hidden",
          "position": "absolute",
          "width": obj.scroller.width,
          "height": obj.scroller.height
        });
        obj.scroller.width = settings.width;
        sdiv.css("position", "absolute");

        obj.scrollInfo.height = sdiv.height();
        obj.scroller.stepIncVert = obj.scroller.vertStepSpeed;
        if (obj.scroller.stepIncVert == 0) obj.scroller.stepIncVert = 1;
        obj.scroller.stepIncHoriz = obj.scroller.horizStepSpeed;
        if (obj.scroller.stepIncHoriz == 0) obj.scroller.stepIncHoriz = 1;

        obj.resetSpeed();
        if (settings.vertScroller) {
          if (obj.vertTrack != null) {
            if(settings.autoHide) obj.vertTrack.css("display", (obj.scrollInfo.height <= settings.height ? 'none' : 'block'));
            else obj.vertTrack.css("display", "block");
          }

          var cw = obj.getCss("width");
          if (obj.vertTrack.css("display") == 'block') {
            var w = parseInt(cw) - settings.vertTrackWidth;
            obj.contentdiv.css("width", String(w < 0 ? 0 : w) + "px");
            obj.scrollInfo.height = sdiv.height();
          }
          else {
            obj.contentdiv.css("width", cw);
          }

        }
        if (settings.horizScroller) {
          if (obj.horizTrack != null) {
            if(settings.autoHide) obj.horizTrack.css("display", (obj.scrollInfo.width <= settings.width ? 'none' : 'block'));
            else obj.horizTrack.css("display", "block");
          }

          var ch = obj.getCss("height");
          if (obj.horizTrack.css("display") == 'block') {
            var h = parseInt(ch) - settings.horizTrackHeight;
            obj.contentdiv.css("height", String(h < 0 ? 0 : h) + "px");
            obj.scrollInfo.width = sdiv.width();
          }
          else {
            obj.contentdiv.css("height", ch);
          }

        }
        $(obj).css("visibility", "visible");
      };

      obj.setup = function() {
        obj.setupObjects();
        obj.setupHtml();
        obj.initScrollers();

        obj.thumbObj.elStart = 0;
        if (settings.vertStartPos != 0) {
          var vertpos = -parseInt(obj.scroller.height*settings.vertStartPos/100);
          obj.jumpTo("vertical", vertpos);
          obj.scrollInfo.pos = vertpos;
        }
        if (settings.horizStartPos != 0) {
          var horizpos = -parseInt(obj.scroller.width*settings.horizStartPos/100);
          obj.jumpTo("horizontal", horizpos);
          obj.scrollInfo.pos = horizpos;
        }
        setupDone();
      };

      obj.init();
    });

  }



  $.fn.scroller_setVertPos = function(perc) {
    return this.each(function(){
      var pos = (this.scroller.height - this.scrollInfo.height)*perc/100;
      this.jumpTo("vertical", pos);
    });
  }

  $.fn.scroller_setHorizPos = function(perc) {
    return this.each(function(){
      var pos = (this.scroller.width - this.scrollInfo.width)*perc/100;
      this.jumpTo("horizontal", pos);
    });
  }

  $.fn.scroller_setVertPosPx = function(pos) {
    return this.each(function(){
      this.jumpTo("vertical", -pos);
    });
  }

  $.fn.scroller_setHorizPosPx = function(pos) {
    return this.each(function(){
      this.jumpTo("horizontal", pos);
    });
  }

  $.fn.scroller_isVertScrollVisible = function() {
    var isvis = true;
    this.each(function(){
      if (!this.isVertScrollVisible()) isvis = false;
    });
    return isvis;
  }

  $.fn.scroller_isHorizScrollVisible = function() {
    var isvis = true;
    this.each(function(){
      if (!this.isHorizScrollVisible()) isvis = false;
    });
    return isvis;
  }

  $.fn.scroller_setContent = function(html, keepscrollpos) {
    return this.each(function(){
      this.setContent(html, keepscrollpos);
    });
  }

  $.fn.scroller_getContent = function() {
    var html = "";
    this.each(function(){
      html += this.getContent();
    });
    return html;
  }

  $.fn.scroller_contentChanged = function() {
    return this.each(function(){
      this.contentChanged();
    });
  }


  $.fn.scroller.defaults = {
    vertScroller: true,
    vertStartPos: 0,
    vertStepSize: 90,
    vertStepSpeed: 30,
    vertScrollPosition: 'right',
    vertScrollSpeed: 2,
    horizScroller: false,
    horizStartPos: 0,
    horizStepSize: 90,
    horizStepSpeed: 30,
    horizScrollPosition: 'bottom',
    horizScrollSpeed: 2,
    autoHide: true,
    imagepath: "/admin/jstools/jquery/plugins/scroller",
    upimg: "scroll_up.gif",
    downimg: "scroll_down.gif",
    leftimg: "scroll_left.gif",
    rightimg: "scroll_right.gif",
    vsliderimg: "vert_slider.gif",
    vtrackimg: "vert_track.gif",
    hsliderimg: "horiz_slider.gif",
    htrackimg: "horiz_track.gif"
  };

})(jQuery);

