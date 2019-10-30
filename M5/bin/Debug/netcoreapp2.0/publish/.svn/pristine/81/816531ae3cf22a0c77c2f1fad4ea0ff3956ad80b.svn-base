$M.userManage.manage = function (S) {
    var tab = mainTab.find("userManage");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "角色", name: "userManage", closeButton: true, onClose: function () { } });
    var _pageNo = 1;
    var _status = 1;
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 200, text: "用户组", ico: "fa-users" }, { size: "*"}] });
    var userList = null, roleGrid = null;
    var reloadClass = function () {
        $M.comm("userManage.readRole", null, function (json) {
            roleGrid.clear();
            roleGrid.addRow({ id: 0, text: "全部" }); roleGrid.addRow(json); roleGrid.rows[0].focus();
        });
    };
    var addUser = function () {
        $M.app.call("$M.userManage.edit", {});
    };
    var reload = function (pageno) {
        if (roleGrid.selectedRows.length == 0) return;
        var id = roleGrid.selectedRows[0].cells[0].val();
        if (pageno == null) pageno = 1;
        $M.comm("userManage.readUserList", { roleId: id, pageNo: pageno, status: _status, keyword: searchBox.val() }, function (json) {
            userList.clear();
            userList.addRow(json.data);
            pageBar.attr("pageSize", json.pageSize);
            pageBar.attr("recordCount", json.recordCount);
            pageBar.attr("pageNo", json.pageNo);
            setEditButtonStatus();
        });
    };
    var delUser = function () {
        if (roleGrid.selectedRows.length == 0) return;
        var id = roleGrid.selectedRows[0].cells[0].val();
        $M.confirm("您确定要删除所选用户吗？", function () {
            $M.comm("userManage.delUser", { id: id }, reloadClass);
        });
    };
    var delGroup = function () {
        if (roleGrid.selectedRows.length == 0) return;
        var id = roleGrid.selectedRows[0].cells[0].val();
        $M.confirm("您确定要删除所选角色吗（删除角色不会删除相关用户）？", function () {
            $M.comm("userManage.delRole", { id: id }, reloadClass);
        });
    };
    var addGroup = function () {
        $M.prompt("角色名称", function (value) {
            $M.comm("userManage.editRole", { id: -1, name: value }, reloadClass);
        });
    };
    var editGroup = function () {
        if (roleGrid.selectedRows.length == 0) return;
        var id = roleGrid.selectedRows[0].cells[0].val();
        $M.prompt("角色名称", function (value) {
            $M.comm("userManage.editRole", { id: id, name: value }, function () {
                roleGrid.selectedRows[0].cells[1].val(value);
            });
        }, { value: roleGrid.selectedRows[0].cells[1].val() });
    };
    var menu1 = $(document.body).addControl({xtype: "Menu", items: [
            { text: "新增", onClick: addGroup },
            { text: "修改", onClick: editGroup },
		    { text: "-" },
		    { text: "删除", onClick: delGroup },
		    { text: "-" },
		    { text: "刷新", onClick: reloadClass }
        ]
    });
    roleGrid = frame.items[0].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0, condensed: 1,
        columns: [{ text: "id", name: "id", visible: false }, { text: "角色名称", name: "text", width: 200}],
        contextMenuStrip: menu1,
        onSelectionChanged: function (sender, e) {
            reload(1);
        },
        onKeyDown: function (sender, e) {
            //if (e.which == 46) delRole();
        }
    });

    var getId = function () {
        var ids = "";
        for (var i = 0; i < userList.selectedRows.length; i++) {
            if (i > 0) ids += ",";
            ids += userList.selectedRows[i].cells[0].val();
        }
        return ids;
    };
    var addData = function () {
        $M.app.call("$M.userManage.edit", {});
    };
    var editData = function () {
        if (userList.selectedRows.length == 0) return;
        $M.app.openFunction("userManage", "edit", { id: userList.selectedRows[0].cells[0].val() });
    };
    var delData = function () {
        if (userList.selectedRows.length == 0) return;
        $M.confirm("您确定要删除所选用户吗？", function () {
            var ids = getId();
            if (ids != "") {
                $M.comm("userManage.delUser", { ids: ids }, reload);
            }
        });
    };
    var setStatus = function () {
        if (userList.selectedRows.length == 0) return;
        var ids = getId();
        if (ids != "") {
            $M.comm("userManage.setStatus", { ids: ids, status: _status == 1 ? 0 : 1 }, reload);
        }
    };

    var toolBar = frame.items[1].addControl({
        xtype: "ToolBar", items: [
        {
            xtype: "ButtonCheckGroup", items: [{ text: "有效帐号", value: 1, ico: "fa-check-square-o" }, { text: "已停用", value: 0, ico: "fa-pause"}], value: 1,
            onClick: function (sender, e) {
                _status = e.value; reload(1);
                stopButton.val(_status == 1 ? "停用" : "启用");
            }
        },
        [{ xtype: "InputGroup", name: "searchBoxInput", style: { width: "300px" }, items: [{ name: "searchBox", xtype: "TextBox", text: "" }, { xtype: "Button", text: "搜索", onClick: function () { reload(1); } }]}],
        [
            { text: "添加", name: "appendButton", ico: "fa-plus", onClick: addData },
            { text: "修改", name: "editButton", ico: "fa-edit", onClick: editData, enabled: false },
            { text: "删除", name: "delButton", ico: "fa-trash-o", onClick: delData, enabled: false }
       ],
       [{ text: "停用", name: "stopButton", onClick: setStatus}]]
    });
    var stopButton = toolBar.find("stopButton");
    var appendButton = toolBar.find("appendButton");
    var editButton = toolBar.find("editButton");
    var delButton = toolBar.find("delButton");
    var setEditButtonStatus = function (flag) {
        editButton.enabled(flag);
        delButton.enabled(flag);
        stopButton.enabled(flag);
    };
    var searchBoxInput = toolBar.find("searchBoxInput");
    var searchBox = searchBoxInput.find("searchBox");
    searchBox.attr("onKeyDown", function (sender, e) {
        if (e.which == 13) reload(1);
    });
    userList = frame.items[1].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1,
        allowMultiple: true,
        columns: [{ text: "id", name: "id", visible: false, width: 100 }, { text: "用户名", name: "uname", width: 300, isLink: true }, { text: "上次登陆时间", name: "loginDateTime", width: 150}],
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                var _id = sender.rows[e.y].cells[0].val();
                $M.app.openFunction("userManage", "edit", { id: _id });
            }
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delData();
        },
        onSelectionChanged: function (sender, e) {
            setEditButtonStatus(sender.selectedRows.length > 0);
        },
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 2) {
                var value = new Date(parseInt(e.value.replace(/\D/igm, "")));
                return value.format("yyyy-MM-dd hh:mm:ss");
            }
        }
    });
    var pageBar = frame.items[1].addControl({
        xtype: "PageBar",
        onPageChanged: function (sender, e) {
            _pageNo = e.pageNo;
            reload(_pageNo);
            //loadPage(item._pageNo, item._orderByName, item._sortDirection, item._type);
        }
    });


    reloadClass();
    tab.focus();
};
