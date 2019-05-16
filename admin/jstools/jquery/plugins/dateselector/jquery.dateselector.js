(function($){

  $.fn.dateselector = function(options) {

    return this.each(function() {

      var obj = this;
      obj.settings = jQuery.extend({}, jQuery.fn.dateselector.defaults, options);
      obj.currdate = obj.settings.initialdate;

      obj.init = function() {
        $(obj).append(
          $("<input />").attr(
            "type","hidden"
          ).val(
            obj.currdate
          )
        ).append(
          $("<a />").attr(
            "href","javascript:void(0)"
          ).html(
            obj.currdate.length > 0 ? obj.currdate : obj.settings.defaulttext
          ).focus(function(){
            this.blur();
          }).click(function(){
            showCalendar(this.previousSibling, this, obj.settings.showtime, obj.onchange);
          })
        );
      }

      obj.onchange = function(cal, adate) {
        $(cal.sel).val(adate);
        $(cal.sel).next().html(adate);
        obj.currdate = adate;
        if (obj.settings.onselect) obj.settings.onselect(obj.id, adate);
      }

      obj.setDate = function(adate) {
        $(obj).find("input").val(adate);
        $(obj).find("a").html(adate);
        obj.currdate = adate;
      }

      obj.getDate = function() {
        return obj.currdate;
      }

      obj.clear = function() {
        $(obj).find("input").val("");
        $(obj).find("a").html(obj.settings.defaulttext);
        obj.currdate = "";
      }

      obj.init();

    });

    return this;
  }

  $.fn.dateselector_getDate = function() {
    var res = "";
    this.each(function() {
      res = this.getDate();
    });
    return res;
  }

  $.fn.dateselector_setDate = function(adate) {
    return this.each(function() {
      this.setDate(adate);
    });
  }

  $.fn.dateselector_clear = function() {
    return this.each(function() {
      this.clear();
    });
  }

  $.fn.dateselector.defaults = {
    initialdate:"",
    defaulttext:"Välj datum",
    showtime:false,
    onselect:null
  };

})(jQuery);

