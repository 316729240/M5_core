$M.dataBase.backup = function (S) {
    var isClose = false;
    var win = $(document.body).addControl({
        xtype: "Window",
        text: "备份数据库",
        ico: "fa-database",
        style: { width: "500px" },
        isModal: true,
        onClose: function (sender, e) {
            if (isClose) return;
            stop = true;
            $M.confirm("您确定要终止备份吗", function () {
                isClose = true;
                win.remove();
            }, { onCancel: function () { stop = false; saveData(); } });
            return false;
        }
    });
    var label1 = win.append("<h2></h2>");
    var p1 = win.addControl({ xtype: "ProgressBar" });
    var label2 = win.append("<h5></h5>");
    var p2 = win.addControl({ xtype: "ProgressBar" });
    win.show();

    var index = 0, pageNo = 1, stop = false;
    var data = null;
    var saveData = function () {
        if (pageNo > data[index].pageCount) {
            index++;
            p1.val(index);
            if (index == data.length) {
                zipData();
                return;
            }
            pageNo = data[index].start == null ? 1 : data[index].start + 1;
            p2.val(pageNo);
            p2.attr("maximum", data[index].pageCount);
            if (pageNo > data[index].pageCount) {
                label1.html("正在备份表[" + data[index].tableName + "]");
                setTimeout(saveData, $M.config.operationTimeDelay);
                return;
            }
        }
        label1.html("正在备份表[" + data[index].tableName + "]");
        label2.html("已处理" + (pageNo * data[index].pageSize) + "条记录");
        $M.comm("dataBase.saveTableData", {backName:S.name, tableName: data[index].tableName, pageSize: data[index].pageSize, pageNo: pageNo }, function (json) {
            p2.val(pageNo);
            if (!stop) setTimeout(saveData, $M.config.operationTimeDelay);
        });
        pageNo++;
    };
    var zipData = function () {
        label1.html("正在打包数据...");
        $M.comm("dataBase.zipData", { backName: S.name }, function (json) {
            isClose = true;
            win.remove();
        });
    };
    $M.comm("dataBase.saveTableStructure", { backName: S.name }, function (json) {
        data = json;
        p1.attr("maximum", data.length+1);
        pageNo = data[0].start == null ? 1 : data[0].start + 1;
        saveData();
    });
};
