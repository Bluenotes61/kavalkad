//------ Extensions of Date object ------

Date.prototype.add = function(addtype, amount) {
  var aDate = this.clone();
  if (addtype == "y") aDate.setFullYear(aDate.getFullYear() + amount);
  else if (addtype == "M") aDate.setMonth(aDate.getMonth() + amount);
  else if (addtype == "d") aDate.setDate(aDate.getDate() + amount);
  else if (addtype == "H") aDate.setHours(aDate.getHours() + amount);
  else if (addtype == "m") aDate.setMinutes(aDate.getMinutes() + amount);
  else if (addtype == "s") aDate.setSeconds(aDate.getSeconds() + amount);
  return aDate;
}
Date.prototype.addYears = function(amount) { return this.add("y", amount); }
Date.prototype.addMonths = function(amount) { return this.add("M", amount); }
Date.prototype.addDays = function(amount) { return this.add("d", amount); }
Date.prototype.addHours = function(amount) { return this.add("H", amount); }
Date.prototype.addMinutes = function(amount) { return this.add("m", amount); }
Date.prototype.addSeconds = function(amount) { return this.add("s", amount); }
Date.prototype.clone = function() {
  var aDate = new Date(this.getFullYear(), this.getMonth(), this.getDate(), this.getHours(), this.getMinutes(), this.getSeconds());
  return aDate;
}
Date.prototype.monthName = function() {
  var amonth = this.getMonth();
  if (amonth == 0) return translate("Januari");
  else if (amonth == 1) return translate("Februari");
  else if (amonth == 2) return translate("Mars");
  else if (amonth == 3) return translate("April");
  else if (amonth == 4) return translate("Maj");
  else if (amonth == 5) return translate("Juni");
  else if (amonth == 6) return translate("Juli");
  else if (amonth == 7) return translate("Augusti");
  else if (amonth == 8) return translate("September");
  else if (amonth == 9) return translate("Oktober");
  else if (amonth == 10) return translate("November");
  else if (amonth == 11) return translate("December");
  else return "";
}
Date.prototype.dayName = function() {
  var aday = this.getDay();
  if (aday == 0) return translate("Söndag");
  else if (aday == 1) return translate("Måndag");
  else if (aday == 2) return translate("Tisdag");
  else if (aday == 3) return translate("Onsdag");
  else if (aday == 4) return translate("Torsdag");
  else if (aday == 5) return translate("Fredag");
  else if (aday == 6) return translate("Lördag");
  else return "";
}
Date.prototype.dayEquals = function(adate) {
  return this.getFullYear() == adate.getFullYear() && this.getMonth() == adate.getMonth() && this.getDate() == adate.getDate();
}
Date.prototype.dateString = function() {
  var m = (this.getMonth() < 9 ? "0" : "") + String(this.getMonth() + 1);
  var d = (this.getDate() < 10 ? "0" : "") + String(this.getDate());
  return this.getFullYear() + "-" + m + "-" + d;
}
Date.prototype.dateTimeString = function() {
  var h = (this.getHours() < 10 ? "0" : "") + String(this.getHours());
  var m = (this.getMinutes() < 10 ? "0" : "") + String(this.getMinutes());
  var s = (this.getSeconds() < 10 ? "0" : "") + String(this.getSeconds());
  return this.dateString + " " + h + ":" + m + ":" + s;
}
Date.parse = function(datestr) {
  try {
    var adate = new Date();
    var dt = datestr.split(' ');
    var date = dt[0].split('-');
    adate.setFullYear(parseInt(date[0]));
    adate.setMonth(parseInt(date[1])-1);
    adate.setDate(parseInt(date[2]));
    if (adate.getFullYear() != parseInt(date[0]) || adate.getMonth() != parseInt(date[1])-1 || adate.getDate() != parseInt(date[2]))
      return null;
    if (dt.length > 1 && dt[1].length > 0) {
      var time = dt[1].split(':');
      adate.setHours(parseInt(time[0]));
      if (adate.getHours() != parseInt(time[0])) return null;
      if (time.length > 1) {
        adate.setMinutes(parseInt(time[1]));
        if (adate.getMinutes() != parseInt(time[1])) return null;
      }
      if (time.length > 2) {
        adate.setSeconds(parseInt(time[2]));
        if (adate.getSeconds() != parseInt(time[2])) return null;
      }
    }
    if (adate == "Invalid Date") return null;
    return adate;
  }
  catch(e) { return null; }
}


