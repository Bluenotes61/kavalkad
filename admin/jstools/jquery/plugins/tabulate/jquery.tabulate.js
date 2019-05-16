(function($){

  $.fn.tabulate = function(options) {

    var settings = $.extend({}, $.fn.tabulate.defaults, options);

    return this.each(function() {

      var colwidth = $(this).width()/settings.nofcols;
      $(this).css("height","auto");
      var colheight = $(this).height()/settings.nofcols;
      var top = $(this).offset().top;
      var maxheight = 0;

      this.tabulate = function() {
        var cols = new Array(settings.nofcols);
        for (var i=0; i < settings.nofcols; i++)
          cols[i] = this.nextCol(i == settings.nofcols-1);
        for (var i=0; i < settings.nofcols; i++) {
          $(this).append(cols[i]);
          if (cols[i].height() > maxheight)
            maxheight = cols[i].height();
        }
      }

      this.nextCol = function(islast) {
        var childarr = [];
        $(this).children().each(function(){
          if ($(this).offset().top - top < colheight || islast) {
            if (childarr.length == 0) $(this).addClass(settings.topclass);
            childarr.push($(this));
          }
        });
        while (!islast && settings.avoidbottom.length > 0 && childarr.length > 0 && childarr[childarr.length-1].hasClass(settings.avoidbottom))
          childarr.splice(childarr.length-1, 1);
        var acol = $("<div />").css({"float":"left", "width":colwidth});
        for (var i=0; i < childarr.length; i++) {
          childarr[i].remove();
          acol.append(childarr[i]);
        }
        return acol;
      }

      this.tabulate();
      $(this).css("height",maxheight);
    });

  }

  $.fn.tabulate.defaults = {
    nofcols : 2,
    topclass : "",
    avoidbottom : ""
  }

})(jQuery);
