(function($){

  $.fn.gifpopup = function(options) {

    return this.each(function() {
      var obj = this;
      obj.settings = jQuery.extend({}, jQuery.fn.gifpopup.defaults, options);
      obj.stay = false;
      obj.gif = null;
      var isAnimating = false;
      var id = (obj.settings.id ? obj.settings.id : $(obj).attr("id") );

      obj.init = function() {
        var img = new Image();
        img.onload = function() {
          obj.init2(img);
        }
        img.src = obj.settings.gifurl;
      }

      obj.init2 = function(img) {
        if (obj.settings.gifwidth.length > 0) img.width = obj.settings.gifwidth;
        if (obj.settings.gifheight.length > 0) img.height = obj.settings.gifheight;

        if (obj.settings.initialHeight == 0) obj.settings.initialHeight = img.height;
        if (obj.settings.previewHeight == 0) obj.settings.previewHeight = img.height;
        obj.settings.previewTop = 0;
        if (obj.settings.previewHeight != 0 && obj.settings.growUpwards) obj.settings.previewTop = obj.settings.initialHeight - obj.settings.previewHeight;
        obj.gif = $("<div />").addClass("gifpopup").css({
          "opacity":obj.settings.initialOpacity,
          "height":obj.settings.initialHeight,
          "overflow":"hidden",
          "position":"absolute",
          "z-index":"10000",
          "margin-left":obj.settings.x,
          "margin-top":obj.settings.y
        }).append($(img));
        $(obj).before(obj.gif);

        var pos = obj.getCookiePos();
        nfndebug(id + " - " + pos);
        if (pos) {
          $(obj).click(function(e){obj.stayGif();});
          obj.stay = true;
          obj.gif.css({
            "margin-left":pos.left,
            "margin-top":pos.top,
            "opacity":"1",
            "height":obj.settings.previewHeight
          }).click(function(e){
            obj.stayGif();
          });
        }
        else {
          $(obj).mouseenter(function(e){obj.startAnimate();obj.positionGif(e);});
          $(obj).mousemove(function(e){obj.startAnimate();obj.positionGif(e);});
          $(obj).mouseleave(function(){obj.stopAnimate();});
          $(obj).click(function(e){obj.stayGif();});
          obj.gif.mouseenter(function(e){obj.startAnimate();obj.positionGif(e);});
          obj.gif.mousemove(function(e){obj.startAnimate();obj.positionGif(e);});
          obj.gif.mouseleave(function(){obj.stopAnimate();});
          obj.gif.click(function(e){obj.stayGif();});
        }
      },

      obj.startAnimate = function() {
        if (!obj.stay && !isAnimating) {
          isAnimating = true;
          setTimeout(obj.animShow, obj.settings.previewHiddenTime);
        }
      }

      obj.stopAnimate = function() {
        if (!obj.stay) {
          isAnimating = false;
          obj.gif.stop(true);
          obj.gif.animate({
            "height":obj.settings.initialHeight,
            "opacity":obj.settings.initialOpacity,
            "margin-top":0
          });
        }
      }

      obj.positionGif = function(e) {
        if (!obj.stay && obj.settings.followMouse) {
          obj.gif.css({
            "margin-left":e.pageX - $(obj).offset().left,
            "margin-top":e.pageY - $(obj).offset().top + 1
          });
        }
      }

      obj.animShow = function() {
        if (isAnimating) {
          obj.gif.css({
            "height":obj.settings.initialHeight,
            "opacity":obj.settings.initialOpacity
          }).animate(
            {
              "height":obj.settings.previewHeight,
              "opacity":obj.settings.previewOpacity,
              "margin-top":obj.settings.previewTop
            },
            obj.settings.previewAnimTime,
            function(){
              setTimeout(obj.animHide, obj.settings.previewShownTime);
            }
          );
        }
      }

      obj.animHide = function() {
        if (isAnimating) {
          obj.gif.animate(
            {
              "height":obj.settings.initialHeight,
              "opacity":obj.settings.initialOpacity,
              "margin-top":0
            },
            obj.settings.previewAnimTime,
            function(){
              setTimeout(obj.animShow, obj.settings.previewHiddenTime);
            }
          );
        }
      }

      obj.stayGif = function(e) {
        if (obj.stay) {
          var nof = obj.getNofClicks();
          if (obj.settings.ongifclickedagain) obj.settings.ongifclickedagain(obj, nof, nof >= obj.settings.nofGifs);
        }
        else {
          isAnimating = false;
          obj.gif.stop(true);
          obj.stay = true;
          obj.gif.css({
            "height":obj.settings.previewHeight,
            "opacity":"1",
            "margin-top":obj.settings.previewTop
          });

          obj.setCookiePos();
          obj.incNofClicks();
          var nof = obj.getNofClicks();
          if (obj.settings.ongifclicked) obj.settings.ongifclicked(obj, nof, nof >= obj.settings.nofGifs);
        }
      }


      obj.getCookie = function() {
        var cookie = {};
        if (document.cookie.length > 0) {
          var vals = document.cookie.split(';');
          var val = "";
          for (var i=0; i < vals.length; i++) {
            if (vals[i].indexOf('gifpop') == 0 || vals[i].indexOf('gifpop') == 1)
              val = vals[i].split('=')[1];
          }
          if (val.length > 0) {
            var avals = val.split('|');
            for (var i=0; i < avals.length; i++) {
              var bvals = avals[i].split(':');
              cookie[bvals[0]] = bvals[1];
            }
          }
        }
        return cookie;
      }

      obj.setCookie = function(item, val) {
        var c = obj.getCookie();
        c[item] = val;
        var cstring = "";
        for (var item in c) {
          if (cstring.length > 0) cstring += "|";
          cstring += item + ":" + c[item];
        }
        var exdate = new Date();
        exdate.setDate(exdate.getDate() + 100);
        document.cookie = "gifpop=" + cstring + ";path=/;expires="+exdate.toUTCString();
      }

      obj.getCookiePos = function() {
        var pos = null;
        var c = obj.getCookie();
        var p = c[id];
        if (p) {
          var cval = p.split(',');
          pos = {left:parseInt(cval[0]), top:parseInt(cval[1])};
        }
        return pos;
      }

      obj.setCookiePos = function() {
        var c_val = parseInt(obj.gif.css("margin-left")) + "," + parseInt(obj.gif.css("margin-top"));
        obj.setCookie(id, c_val);
      }


      obj.incNofClicks = function() {
        var nof = obj.getNofClicks() + 1;
        obj.setCookie("nofclicks", nof);
      }

      obj.getNofClicks = function() {
        var c = obj.getCookie();
        var nof = c["nofclicks"];
        if (nof == null) return 0;
        else return parseInt(nof);
      }

      obj.init();

    });

    return this;
  }

  $.fn.gifpopup_nofClicks = function() {
    nof = 0;
    this.each(function(){
      nof = this.getNofClicks();
    });
    return nof;
  }

  $.fn.gifpopup.defaults = {
    gifurl : "path/img1.gif",
    gifwidth : "",
    gifheight : "",
    growUpwards:false,
    previewShownTime : 100,
    previewHiddenTime : 100,
    previewAnimTime:200,
    initialOpacity:1,
    previewOpacity:1,
    initialHeight:0,
    previewHeight:0,
    followMouse : true,
    x : 0,
    y: 0,
    nofGifs:5,
    ongifclicked: null,
    ongifclickedagain: null
  };

})(jQuery);

