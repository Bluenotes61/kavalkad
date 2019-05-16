/* $Date: 2010-04-27 12:55:53 +0200 (ti, 27 apr 2010) $    $Revision: 6332 $ */

(function($){
  $.fn.divarrange = function(options) {

    var settings = $.extend({
      twocolclass: "twocols",
      threecolclass: "threecols",
      colwidth: 200,
      vertgap: 10,
      horizgap: 10,
      onReady : null
    }, options);


    return this.each(function() {

      var thisref = this;

      function getNextCol(heights, width) {
        var idx = 0;
        for (var i=1; i < heights.length-width+1; i++) {
          var currh = (width == 3 ? Math.max(heights[idx], heights[idx+1], heights[idx+2]) : (width == 2 ? Math.max(heights[idx], heights[idx+1]) : heights[idx]));
          var newh = (width == 3 ? Math.max(heights[i], heights[i+1], heights[i+2]) : (width == 2 ? Math.max(heights[i], heights[i+1]) : heights[i]));
          if (newh < currh) idx = i;
        }
        return idx;
      }

      this.arrange = function() {
        var s = new Date();
        var colcount = parseInt($(this).innerWidth()/(settings.colwidth + settings.horizgap));
        $(this).children().css({"width":settings.colwidth, "position":"absolute" });
        $(this).children("." + settings.twocolclass).css("width", settings.colwidth*2 + settings.horizgap);
        $(this).children("." + settings.threecolclass).css("width", settings.colwidth*3 + 2*settings.horizgap);
        if (colcount < 2 && $(this).children("." + settings.twocolclass).length > 0) colcount = 2;
        if (colcount < 3 && $(this).children("." + settings.threecolclass).length > 0) colcount = 3;

        var colheight = new Array(colcount);
        for (var i=0; i < colcount; i++) colheight[i] = 0;

        var currcol = 0;
        $(this).children().each(function(i) {
          var item = $(this);
          var itemwidth = (item.hasClass(settings.twocolclass) ? 2 : (item.hasClass(settings.threecolclass) ? 3 : 1));

          var currpos = getNextCol(colheight, itemwidth);
          var newtop = (itemwidth == 3 ? Math.max(colheight[currpos], colheight[currpos+1], colheight[currpos+2]) : (itemwidth == 2 ? Math.max(colheight[currpos], colheight[currpos+1]) : colheight[currpos]));

          item.css({"margin-left": currpos*(settings.colwidth + settings.horizgap), "margin-top": newtop});

          colheight[currpos] = newtop + item.outerHeight() + settings.vertgap;
          for (var i=1; i < itemwidth; i++)
            colheight[currpos + i] = colheight[currpos];
        });
        if (settings.onReady) settings.onReady();
      };

      this.arrange();

      $(window).resize( function() { thisref.arrange(); } );

    });
  }

})(jQuery);
