$M.templateManage.backupRestore = function (S) {
    var win = new $M.dialog({
        title: "还原模板",
        ico: "fa-history",
        style: { width: "400px"},
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                if (grid.selectedRows.length == 0) return;
                var id = grid.selectedRows[0].cells[0].val();
                $M.comm("templateManage.readBackup", { id: id }, function (json) {
                    if (S.back) S.back(json);
                });
            }
        }
    });
    var grid = win.addControl({ xtype: "GridView", style: { height: "300px" },
        columns: [{ text: "id", name: "id", visible: false }, { text: "备份时间", name: "updateDate", width: 260 }, { text: "操作人", name: "userName", width: 150}],
        onCellMouseDoubleClick: function (sender, e) {
            win.enter();
        },
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 1) {
                var value = new Date(parseInt(e.value.replace(/\D/igm, "")));
                return value.format("yyyy-MM-dd hh:mm:ss");
            }
        }
    });
    $M.comm("templateManage.readBackupList", {dataId:S.dataId, u_webFAid: S.u_webFAid, classId: S.classId, u_type: S.u_type, u_defaultFlag: S.u_defaultFlag, u_datatypeId: S.u_datatypeId, title: S.title }, function (json) {
        grid.addRow(json);
    });
    win.show();
};
$M.templateManage.backupViewRestore = function (S) {
    var win = new $M.dialog({
        title: "还原模板",
        ico: "fa-history",
        style: { width: "400px" },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                if (grid.selectedRows.length == 0) return;
                var id = grid.selectedRows[0].cells[0].val();
                $M.comm("templateManage.readBackup", { id: id }, function (json) {
                    if (S.back) S.back(json);
                });
            }
        }
    });
    var grid = win.addControl({
        xtype: "GridView", style: { height: "300px" },
        columns: [{ text: "id", name: "id", visible: false }, { text: "备份时间", name: "updateDate", width: 260 }, { text: "操作人", name: "userName", width: 150 }],
        onCellMouseDoubleClick: function (sender, e) {
            win.enter();
        },
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 1) {
                var value = new Date(parseInt(e.value.replace(/\D/igm, "")));
                return value.format("yyyy-MM-dd hh:mm:ss");
            }
        }
    });
    $M.comm("templateManage.readViewBackupList", { dataId: S.dataId, u_webFAid: S.u_webFAid, classId: S.classId, u_type: S.u_type, u_defaultFlag: S.u_defaultFlag, u_datatypeId: S.u_datatypeId, title: S.title }, function (json) {
        grid.addRow(json);
    });
    win.show();
};