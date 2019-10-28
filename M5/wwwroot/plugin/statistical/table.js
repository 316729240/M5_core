$M.statistical.table = function (S) {
    var tab = mainTab.addItem({ text: "用户数据发布报表", closeButton: true, onClose: function () { $.dataManage = null; } });
    var _pageNo = 1;
    var _status = 1;
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 200, text: "用户组", ico: "fa-users" }, { size: "*"}] });
    var userList = null, roleGrid = null;

    var reload = function (name, sortDirection) {
        if (roleGrid.selectedRows.length == 0) return;
        var id = roleGrid.selectedRows[0].cells[0].val();
        $M.comm("statistical.readUserReport", { day:S.day,roleId: id, name: name, sortDirection: sortDirection }, function (json) {
            userList.clear();
            for (var i = 0; i < json.length; i++) {
                if (json[i][0] == null) json[i][0] = "已删除";
                userList.addRow(json[i]);
            }
        });
    };
    roleGrid = frame.items[0].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0, condensed: 1,
        columns: [{ text: "id", name: "id", visible: false }, { text: "角色名称", name: "text", width: 200}],
        onSelectionChanged: function (sender, e) {
            reload();
        }
    });

//    var toolBar = frame.items[1].addControl({
//        xtype: "ToolBar", items: [
//        [{ xtype: "Button", text: "刷新"}],
//        [{ xtype: "InputGroup", name: "searchBoxInput", style: { width: "300px" }, items: [{ name: "searchBox", xtype: "TextBox", text: "" }, { xtype: "Button", text: "搜索", onClick: function () { reload(1); } }]}]]
//    });

//    var searchBoxInput = toolBar.find("searchBoxInput");
//    var searchBox = searchBoxInput.find("searchBox");
//    searchBox.attr("onKeyDown", function (sender, e) {
//        if (e.which == 13) reload(1);
//    });
    userList = frame.items[1].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1,
        columns: [{ text: "用户名", name: "uname", width: 300 }, { text: "发布量", name: "count", width: 150}],
        allowSorting: true,
        onSorting: function (sender, e) {
            reload(e.name, e.sortDirection);
        },
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                var _id = sender.rows[e.y].cells[0].val();
            }
        }
    });


    $M.comm("userManage.readRole", null, function (json) { roleGrid.addRow({ id: 0, text: "全部" }); roleGrid.addRow(json); roleGrid.rows[0].focus(); });
    tab.focus();
};
