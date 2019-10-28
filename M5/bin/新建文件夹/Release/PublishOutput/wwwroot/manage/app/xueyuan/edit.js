(function ($) {
    var printAreaCount = 0;
    $.fn.printArea = function () {
        var ele = $(this);
        var idPrefix = "printArea_";
        removePrintArea(idPrefix + printAreaCount);
        printAreaCount++;
        var iframeId = idPrefix + printAreaCount;
        var iframeStyle = 'position:absolute;width:0px;height:0px;left:-500px;top:-500px;';
        iframe = document.createElement('IFRAME');
        $(iframe).attr({
            style: iframeStyle,
            id: iframeId
        });
        document.body.appendChild(iframe);
        var doc = iframe.contentWindow.document;
        $(document).find("link").filter(function () {
            return $(this).attr("rel").toLowerCase() == "stylesheet";
        }).each(
                function () {
                    doc.write('<link type="text/css" rel="stylesheet" href="'
                            + $(this).attr("href") + '" >');
                });
        doc.write('<div class="' + $(ele).attr("class") + '">' + $(ele).html()
                + '</div>');
        doc.close();
        var frameWindow = iframe.contentWindow;
        frameWindow.close();
        frameWindow.focus();
        frameWindow.print();
    }
    var removePrintArea = function (id) {
        $("iframe#" + id).remove();
    };
})(jQuery);
$M.xueyuan.edit = function (S) {
    var tab = mainTab.addItem({ text: "学员信息", "class": "form-horizontal", closeButton: true });
    tab.focus();
    var toolBar = tab.addControl({
        xtype: "ToolBar", class: "note-toolbar",
        items: [
            [
                { text: "保存", ico: "fa-save", primary: true, onClick: function () { form.submit(); } },
                { text: "新增", ico: "fa-plus", onClick: function () { form.reset(); } },
                { text: "打印", ico: "fa-plus", onClick: function () { var f = window.open(); f.document.write(printbox.html()); } }
            ]
        ]
    });
    var printbox = tab.append("<div style='display:none1'></div>");
    printbox.load($M.config.appPath + "xueyuan/print.html");
    var form=tab.addControl({
        xtype: "Form",
        command: "xueyuan.ajax.edit",
        templateUrl:$M.config.appPath + "xueyuan/edit.html",
        onSubmit: function () {
            if (S.back) S.back();
            $M.alert("保存成功", tab.remove);
        },
        onLoad: function (sender) {
            sender.find("id").attr({ value: S.dataId });
            sender.find("classId").attr({ value: S.classId, defaultValue: S.classId });
            sender.find("u_pic").attr("onChange", function (sender) {
                form.container.find(".u_pic_img").attr("src", sender.val());
            });
            sender.read("xueyuan.ajax.read", S.dataId, function (json) {
                form.container.find(".u_pic_img").attr("src", json.u_pic);
                for (var o in json) {
                    printbox.find("span[name='"+o+"']").html(json[o]);
                }
                printbox.find(".u_pic_img").attr("src", json.u_pic);
            });
        }
    });
};
$M.xueyuan.uploadFile= function (S) {
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
        var succeed = 0;
        var files = [];
        var uploadComplete = function (json) {
            if (json.errNo < 0) {
                //$M.confirm(json.errMsg, function (flag) {
                //    fileIndex--;
                //    uploadFile();
                //}, { onCancel: function () { end(); } });
            } else {
                succeed++;
                //for (var i1 = 0; i1 < json.userData.length; i1++) {
                //    files[files.length] = json.userData[i1];
                //}
            }
            uploadFile();
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
            //if (S.back) S.back(files);
            inputFile.remove();
            $M.alert("上传成功" + succeed + "篇");
            S.reload();
        };
        var uploadFile = function () {
            fileIndex++;
            if (cancelFlag || fileIndex == inputFile[0].files.length) { end(); return; }
            // if (inputFile[0].files[fileIndex].type == "") {uploadFile();return;}
            var fd = new FormData();
            label.html("正在上传" + inputFile[0].files[fileIndex].name);
            fd.append("fileData", inputFile[0].files[fileIndex]);
            fd.append("moduleId", S.moduleId);
            fd.append("classId", S.classId);
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

            xhr.open("POST", $M.config.appPath + "xueyuan/upload.ashx");
            xhr.send(fd);
        }
        var inputFile = $("<input type='file' accept='text/csv' style='display:none' >").appendTo($(document.body));
        inputFile.on("change", function () {
            fileIndex = -1;
            win.show();
            uploadFile();
        });
        inputFile[0].click();

    }