/* $Date: 2010-10-07 12:13:34 +0200 (to, 07 okt 2010) $    $Revision: 7023 $ */
var swfu;
var progress;

function initUpload() {
  progress = new UploadProgress();

  swfu = new SWFUpload({
    upload_url: "/admin/DocumentBank/upload/upload.aspx", // Relative to the SWF file
    flash_url : "/admin/DocumentBank/upload/swfupload.swf",  // Relative to this file

    file_size_limit : maxUploadSize,
    file_types : "*.*",
    file_types_description : allafiler,
    file_upload_limit : "0", // Zero means unlimited
    file_queue_limit : 0,

    button_placeholder_id:"uploadbtn_inner",
    button_image_url : "/admin/DocumentBank/gfx/upload.gif",
    button_width : 22,
    button_height : 20,
    button_window_mode : SWFUpload.WINDOW_MODE.TRANSPARENT,


    file_queued_handler : function(file){ progress.addItem(file);},
    file_queue_error_handler : function(file, errorCode, message){ progress.queueError(file, errorCode, message);},
    file_dialog_complete_handler : function(numFilesSelected, numFilesQueued){ progress.start(numFilesSelected, numFilesQueued); },
    upload_progress_handler : function(file, bytesLoaded, bytesTotal){progress.updateItem(file, bytesLoaded, bytesTotal);},
    upload_error_handler : function(file, errorCode, message){ progress.uploadError(file, errorCode, message);},
    upload_complete_handler : function(file){ progress.itemComplete(file); },

    debug: false
  });
  setTimeout("swfu.loadFlash();swfu.displayDebugInfo();", 500);

}


function UploadProgress() {
  var barlength = 200;

  this.maindiv = document.getElementById("uploadProgress");
  this.progressItem = new Object();

  this.start = function(numFilesSelected, numFilesQueued) {
    if (numFilesSelected > 0) {
      this.maindiv.style.display = "block";
      this.nOfPendingFiles = numFilesSelected;
      swfu.startUpload();
    }
  }

  this.addItem = function(file) {
    this.progressItem[file.id] = new Object();
    this.progressItem[file.id].div = document.createElement("div");
    this.maindiv.childNodes[1].appendChild(this.progressItem[file.id].div);
    this.updateItem(file, 0, 1);
  }

  this.updateItem = function(file, bytesLoaded, bytesTotal) {
    var percent = Math.ceil((bytesLoaded / bytesTotal) * barlength);
    this.progressItem[file.id].div.innerHTML = "<div>" + file.name + "</div><div class='uploadbar'><div class='uploadprogressbar' style='width:" + percent + "px'></div></div>";
  }

  this.clear = function() {
    this.progressItem = new Object();
    this.maindiv.childNodes[1].innerHTML = "";
    this.maindiv.style.display = 'none';
  }

  this.itemComplete = function(file) {
    if (this.progressItem[file.id] && this.progressItem[file.id].div.innerHTML.indexOf('uploaddonebarerr') < 0) this.progressItem[file.id].div.innerHTML = "<div>" + file.name + "</div><div class='uploadbar'><div class='uploaddonebar' style='width:" + barlength + "px'></div></div>";
    this.nOfPendingFiles--;
    if (this.nOfPendingFiles > 0)
      swfu.startUpload();
    else {
      setTimeout('progress.clear()', 1000);
      db.uploadDone();
    }
  }

  this.queueError = function(file, errorCode, message) {
    //this.progressItem[file.id].div.innerHTML = "<div>" + file.name + "</div><div class='uploadbar'><div class='uploaddonebarerr' style='width:" + barlength + "px'></div></div>";
    if (errorCode === SWFUpload.QUEUE_ERROR.QUEUE_LIMIT_EXCEEDED) {
      alert("You have attempted to queue too many files.\n" + (message === 0 ? "You have reached the upload limit." : "You may select " + (message > 1 ? "up to " + message + " files." : "one file.")));
      return;
    }
    switch (errorCode) {
      case SWFUpload.QUEUE_ERROR.FILE_EXCEEDS_SIZE_LIMIT:
        alert("File too big, File name: " + file.name + ", File size: " + file.size + " byte, Max size: " + maxUploadSize + " kb");
        break;
      case SWFUpload.QUEUE_ERROR.ZERO_BYTE_FILE:
        alert("Zero byte file, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
        break;
      case SWFUpload.QUEUE_ERROR.INVALID_FILETYPE:
        alert("Invalid File Type, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
        break;
      default:
        if (file == null) alert("Unhandled Error");
        else alert("Error Code: " + errorCode + ", File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
        break;
    }
    this.itemComplete(file);
  }

  this.uploadError = function(file, errorCode, message) {
    this.progressItem[file.id].div.innerHTML = "<div>" + file.name + "</div><div class='uploadbar'><div class='uploaddonebarerr' style='width:" + barlength + "px'></div></div>";
    switch (errorCode) {
      case SWFUpload.UPLOAD_ERROR.HTTP_ERROR:
        alert("HTTP Error, File name: " + file.name + ", Message: " + message);
        break;
      case SWFUpload.UPLOAD_ERROR.UPLOAD_FAILED:
        alert("Upload Failed, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
        break;
      case SWFUpload.UPLOAD_ERROR.IO_ERROR:
        alert("Error Code: IO Error, File name: " + file.name + ", Message: " + message);
        break;
      case SWFUpload.UPLOAD_ERROR.SECURITY_ERROR:
        alert("Error Code: Security Error, File name: " + file.name + ", Message: " + message);
        break;
      case SWFUpload.UPLOAD_ERROR.UPLOAD_LIMIT_EXCEEDED:
        alert("Error Code: Upload Limit Exceeded, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
        break;
      case SWFUpload.UPLOAD_ERROR.FILE_VALIDATION_FAILED:
        alert("Error Code: File Validation Failed, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
        break;
      case SWFUpload.UPLOAD_ERROR.FILE_CANCELLED:
        alert("Cancelled");
        break;
      case SWFUpload.UPLOAD_ERROR.UPLOAD_STOPPED:
        alert("Stopped");
        break;
      default:
        if (file == null) alert("Unhandled Error: " + errorCode);
        else alert("Error Code: " + errorCode + ", File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
        break
    };
    this.itemComplete(file);
  }
}

