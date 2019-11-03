$M.system.insertPic = function (S) {
    var win = $(document.body).addControl({
        xtype: "Window",
        text: "上传文件",
        isModal: true,
        style: { width: "500px" },
        ico: S.ico,
        onClose: function () {
            cancelFlag = true;
        },
        footer: [
            { xtype: "Button", text: "取 消", size: 2, onClick: function () { win.dialogResult = $M.dialogResult.cancel; win.remove(); return false; } }
        ]
    });
    var cancelFlag = false; //终止上传
    var label = win.append("<h2></h2>");
    var p1 = win.addControl({ xtype: "ProgressBar" });

    var files = [];
    var uploadComplete = function (json) {
        if (json.errNo < 0) {
            $M.confirm(json.errMsg, function (flag) {
                fileIndex--;
                uploadFile();
            }, { onCancel: function () { end(); } });
        } else {
            for (var i1 = 0; i1 < json.userData.length; i1++) {
                files[files.length] = json.userData[i1];
            }
            uploadFile();
        }
    };
    var uploadProgress = function (evt) {
        if (evt.lengthComputable) {
            var fen = 100 / inputFile[0].files.length;
            var percentComplete = Math.round(evt.loaded / evt.total * fen + fen * fileIndex);
            p1.val(percentComplete);
        }
    };
    var end = function () {
        win.remove();
        if (S.back) S.back(files);
        inputFile.remove();
    };
    var uploadFile = function () {
        fileIndex++;
        if (cancelFlag || fileIndex == inputFile[0].files.length) { end(); return; }
        // if (inputFile[0].files[fileIndex].type == "") {uploadFile();return;}
        var fd = new FormData();
        label.html("正在上传" + inputFile[0].files[fileIndex].name);
        fd.append("fileData", inputFile[0].files[fileIndex]);
        coveredFlag = 0;
        var xhr = new XMLHttpRequest();
        xhr.upload.addEventListener("progress", uploadProgress, false);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                var json = null;
                //try {
                eval("json=" + xhr.responseText);
                uploadComplete(json);
                //                } catch (x) {
                //                    json = { errNo: -1, errMsg: xhr.responseText };
                //                    uploadComplete(json);
                //                }
            }
        };

        xhr.open("POST", $M.config.appPath + "system/upload");
        xhr.send(fd);
    }
    var accept = "image/*";
    if (S.accept) accept = S.accept;
    var inputFile = $("<input type='file' accept='" + accept + "'  " + (S.isMultiple == false ? "" : "multiple='multiple'") + " style='display:none' >").appendTo($(document.body));
    inputFile.on("change", function () {
        fileIndex = -1;
        win.show();
        uploadFile();
    });
    inputFile[0].click();
};
