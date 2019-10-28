//权限
$M.system.permissions = function (S) {
    var grid = null;
    var custom = null;
    var obj = new $M.dialog({
        title: "权限",
        style: { width: "500px" },
        command: "setPermissions",
        onBeginSubmit: function () {
            var text = "";
            for (var i = 0; i < grid.rows.length; i++) {
                text += grid.rows[i].cells[1].val() + "," + grid.rows[i].cells[0].val() + ",";
                for (var i1 = 3; i1 < grid.rows[i].cells.length; i1++) {
                    var v = grid.rows[i].cells[i1].val();
                    text += v ? "1" : "0";
                    text += ",";
                }
                text += "\n";
            }
            permissions.val(text);
        },
        onClose: function (sender, e) {
        }
    });
    var classId = obj.addControl({ value: S.classId, name: "classId", visible: false });
    var permissions = obj.addControl({ multiLine: true, name: "permissions", visible: false });
    obj.addControl({ xtype: "ToolBar", items: [[
        { text: "添加", ico: "fa-plus", onClick: function () {
            var filter = [1, 2, 3, 4, 5];
            for (var i = 0; i < grid.rows.length; i++) {
                filter[filter.length] = grid.rows[i].cells[0].val();
            }
            $M.dialog.selectUsers({ type: 0, filter: filter }, function (json) {
                grid.addRow(json);
            });
            return false;
        }
        }, { text: "删除", ico: "fa-trash-o", onClick: function () {
            if (grid.selectedRows.length > 0) {
                grid.selectedRows[0].remove();
            }
            return false;
        }
        }]]
    });
    grid = obj.addControl({
        xtype: "GridView",
        style: { height: "300px" },
        columns: [{ text: "dataId", width: 50, visible: false }, { text: "类型", width: 50, visible: false }, { text: "名称", width: 100 }, { text: "编辑", xtype: "CheckBox", width: 50 }, { text: "删除", xtype: "CheckBox", width: 50 }, { text: "审核", xtype: "CheckBox", width: 50 }, { text: "管理", xtype: "CheckBox", width: 50 }, { text: "-", xtype: "CheckBox", width: 50 } ],
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 2) {
                var type = sender.rows[e.rowIndex].cells[1].val();
                return "<i class='fa " + (type == 2 ? "fa-user" : "fa-users") + "' /> " + e.value;
            }
        }
    });
    $M.comm("getPermissions", { classId: S.classId }, function (json) { grid.addRow(json); });

    obj.show();
};
