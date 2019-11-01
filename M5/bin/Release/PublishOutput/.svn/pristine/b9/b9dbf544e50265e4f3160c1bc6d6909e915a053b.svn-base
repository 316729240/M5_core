//升级
$M.system.upgrade = function (S) {
    var p = null;
    var win = $(document.body).addControl({
        xtype: "Window",
        isModal: true,
        style: { width: "400px" },
        onClose: function () {
            //if (S.onClose) S.onClose(win, rdata);
        },
        footer: [
            { xtype: "Button", size: 2, text: "在线升级", color: 3, onClick: function () { p = $M.progressDialog(); p.show(); updateFile(); return false; } },
            { xtype: "Button", size: 2, text: "取 消", onClick: function () { win.remove(); return false; } }
            ],
        onKeyDown: function (sender, e) {
            if (e.which == 27) clickButton(3);
        }
    });
    win.show();
    var box = win.append("<div></div>");
    var logList = [], fileList = [];
    $M.comm([["getUpdateLog", { dateTime: S.dateTime}], ["getUpdateFile", { dateTime: S.dateTime}]], function (json) {
        if (json[0].data != null) {
            if ($.isArray(json[0].data.value)) {
                logList = json[0].data.value;
            } else {
                logList = [json[0].data.value];
            }
        }
        if ($.isArray(json[1].data.value)) {
            fileList = json[1].data.value;
        } else {
            fileList = [json[1].data.value];
        }
        var html = "";
        for (var i = 0; i < logList.length; i++) {
            html += "<blockquote>" +
            "<p>" + logList[i].value + "</p>" +
            "<footer class=\"text-right\">" + logList[i].dateTime + "</footer>" +
            "</blockquote>";
        }
        box.html(html);
        if (box.height() > 200) box.css({ "overflow": "auto", "height": "100px" });
    });
    var index = 0;
    var files = "", fileIds = "",types="", dateTime = "";
    var updateFile = function () {
        if (index < fileList.length) {
            $M.comm("downUpdateFile", { fileName: fileList[index].fileId }, function () {
                files += fileList[index].path + fileList[index].fileName + ",";
                fileIds += fileList[index].fileId + ",";
                types += fileList[index].type + ",";
                dateTime = fileList[index].dateTime;
                setTimeout(updateFile, $M.config.operationTimeDelay); index++;
                p.val(index / fileList.length * 100);
            });
        } else {
            $M.comm("updateSystem", { files: files, fileIds: fileIds, dateTime: dateTime, types: types }, function () {
                p.remove();
                $M.alert("升级完成", function () { location.reload(); });
            });
        }
    };
};
