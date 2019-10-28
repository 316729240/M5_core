$M.fileManage.fileExists = function (S) {
    var win = $(document.body).addControl({
        xtype: "Window",
        isModal: true,
        style: { width: "400px" },
        onClose: function () {
            //if (S.onClose) S.onClose(win, rdata);
        },
        footer: [
            { xtype: "Button", size: 2, text: "覆盖", onClick: function () { return clickButton(1); } },
            { xtype: "Button", size: 2, text: "全部覆盖", onClick: function () { return clickButton(2); } },
            { xtype: "Button", size: 2, text: "跳过", onClick: function () { return clickButton(3); } },
            { xtype: "Button", size: 2, text: "全部跳过", onClick: function () { return clickButton(4); } }
            ],
        onKeyDown: function (sender, e) {
            if (e.which == 27) clickButton(3);
        }
    });
    var clickButton = function (value) {
        if (S.onClose) S.onClose(value);
        win.remove();
        return false;
    };
    var html = "<h5 >此位置已包含同名文件</h5>";
    html += "<h5>现有文件<h5/>";
    html += "<code>" + S.oldFile[0] + " (" + S.oldFile[1] + " " + S.oldFile[2] + ")</code>";
    html += "<h5>替换为<h5/>";
    html += "<code>" + S.oldFile[0] + " (" + S.oldFile[1] + " " + S.oldFile[2] + ")</code>";

    win.append(html);
    win.show();
    win.footer[2].focus();
};
$M.fileManage.uploadFile = function (S) {
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
    var coveredFlag = 0; //0 询问 1覆盖  2跳过
    var allCovered = 0; //0默认  1全部 覆盖 2全部取消
    var cancelFlag = false; //终止上传
    var label = win.append("<h2></h2>");
    var p1 = win.addControl({ xtype: "ProgressBar" });


    var uploadComplete = function (json) {

        if (json.errNo == -2) {
            if (allCovered == 1) {
                fileIndex--;
                coveredFlag = 1;
                uploadFile();
                return;
            }
            if (allCovered == 2) {
                coveredFlag = 2;
                uploadFile();
                return;
            }
            $M.fileManage.fileExists({
                oldFile: json.userData.oldFile,
                newFile: json.userData.newFile,
                onClose: function (value) {
                    if (value < 3) {//覆盖
                        if (value == 2) allCovered = 1;
                        coveredFlag = 1;
                        fileIndex--;
                        uploadFile();
                    } else {
                        if (value == 4) allCovered = 2;
                        //coveredFlag = 2;
                        uploadFile();
                    }
                }
            });
        } else if (json.errNo < 0) {
            $M.confirm(json.errMsg, function (flag) {
                fileIndex--;
                uploadFile();
            }, { onCancel: function () { end(); } });
        } else {
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
        if (S.back) S.back();
    };
    var uploadFile = function () {
        fileIndex++;
        if (cancelFlag || fileIndex == inputFile[0].files.length) { end(); return; }
        // if (inputFile[0].files[fileIndex].type == "") {uploadFile();return;}
        var fd = new FormData();
        fd.append("editDate", inputFile[0].files[fileIndex].lastModifiedDate.format("yyyy-MM-dd HH:mm:ss"));
        label.html("正在上传" + inputFile[0].files[fileIndex].name);
        fd.append("fileData", inputFile[0].files[fileIndex]);
        fd.append('path', S.path);
        fd.append('covered', coveredFlag);
        coveredFlag = 0;
        fd.append('filePath', encodeURIComponent(inputFile[0].files[fileIndex].webkitRelativePath));
        var xhr = new XMLHttpRequest();
        xhr.upload.addEventListener("progress", uploadProgress, false);
        //xhr.upload.addEventListener("loaded", function () { return; alert(xhr.responseText); }, false);
        //xhr.addEventListener("readystatechange", function () { alert(xhr.responseText); }, false);
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

        xhr.open("POST", $M.config.appPath + "fileManage/upload");
        xhr.send(fd);
    }
    var inputFile = win.append("<input type='file' " + (S.type == 2 ? "webkitdirectory" : "") + " multiple='multiple' style='display:none' >");
    inputFile.on("change", function () {

        fileIndex = -1;
        win.show();
        uploadFile();
    });
    inputFile[0].click();

};
