/* $Date: 2010-01-29 17:42:55 +0100 (fr, 29 jan 2010) $    $Revision: 5897 $ */

(function($){

  $.fn.dropdownmenu = function(options) {

    var settings = $.extend({}, $.fn.dropdownmenu.defaults, options);

    return this.each(function() {

      $(this).find(".nfn_sm_ul").each(function(){
        $(this).css("position","absolute");
      });
      $(this).find(".nfn_mm_haschildren").mouseenter(function(){
        var ul = $(this).find("ul");
        var left = $(this).offset().left;
        var top = $(this).offset().top + $(this).height();
        if ($.browser.opera) {
          left += 32769;
          top -= 23;
        }
        ul.css({"left":left, "top":top});
        ul.stop(true, true);
        if (settings.animation == "slide")
          ul.slideDown(settings.animationspeed);
        else if (settings.animation == "fade")
          ul.show(settings.animationspeed);
        else
          ul.show();
      }).mouseleave(function(){
        var ul = $(this).find("ul");
        ul.stop(true, true);
        if (settings.animation == "slide")
          ul.slideUp(settings.animationspeed);
        else if (settings.animation == "fade")
          ul.hide(settings.animationspeed);
        else
          ul.hide();
      });

    });
  }

  $.fn.dropdownmenu.defaults = {
    animation:"slide", // slide/fade/none
    animationspeed:"normal" //slow, normal, fast
  };

})(jQuery);
