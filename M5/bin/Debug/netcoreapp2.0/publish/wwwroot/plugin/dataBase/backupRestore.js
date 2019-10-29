$M.dataBase.backupRestore = function (S) {
    var win = new $M.dialog({
        title: "还原数据库",
        ico: "fa-history",
        style: { width: "400px" },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                if (grid.selectedRows.length == 0) return;
                var name = grid.selectedRows[0].cells[0].val() + ".zip";
                $M.confirm("您确定要还原所选数据库文件？", function () {
                    $M.app.call("$M.dataBase.reduction", { name: name });
                });
            }
        }
    });
    var grid = win.addControl({ xtype: "GridView", style: { height: "300px" },
        columns: [{ text: "名称", name: "name", width: 180 }, { text: "备份时间", name: "createDate", width: 170}],
        onCellMouseDoubleClick: function (sender, e) {
            win.enter();
        }
    });
    $M.comm("dataBase.readBackupList", null, function (json) {
        grid.addRow(json);
    });
    win.show();
};