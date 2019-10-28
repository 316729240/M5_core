$M.onlinePayment.payment = function (S) {
    var tab = mainTab.find("payment");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "订单管理", name: "payment", closeButton: true, onClose: function () { } });
    var _pageNo = 1;
    var _status = 1;
    var userList = null, roleGrid = null;

    var reload = function (pageno) {
        if (pageno == null) pageno = 1;
        $M.comm("onlinePayment.ajax.list", { pageNo: pageno }, function (json) {
            userList.clear();
            userList.addRow(json.data);
            pageBar.attr("pageSize", json.pageSize);
            pageBar.attr("recordCount", json.recordCount);
            pageBar.attr("pageNo", json.pageNo);
            setEditButtonStatus();
        });
    };
    var setStatus = function () {
        if (userList.selectedRows.length == 0) return;
        var ids = getId();
        if (ids != "") {
            $M.comm("userManage.ajax.setStatus", { ids: ids, status: _status == 1 ? 0 : 1 }, reload);
        }
    };
    /*
    var toolBar = tab.addControl({
        xtype: "ToolBar", items: [
        {
            xtype: "ButtonCheckGroup", items: [{ text: "有效帐号", value: 1, ico: "fa-check-square-o" }, { text: "已停用", value: 0, ico: "fa-pause"}], value: 1,
            onClick: function (sender, e) {
                _status = e.value; reload(1);
                stopButton.val(_status == 1 ? "停用" : "启用");
            }
        },
        [{ xtype: "InputGroup", name: "searchBoxInput", style: { width: "300px" }, items: [{ name: "searchBox", xtype: "TextBox", text: "" }, { xtype: "Button", text: "搜索", onClick: function () { reload(1); } }] }]
        ]
    });*/

    userList = tab.addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1,
        allowMultiple: true,
        columns: [
            { text: "id", name: "id", visible: false, width: 100 },
            { text: "金额", name: "money", width: 300 },
            { text: "购买时间", name: "createdate", width: 150 },
            { text: "用户", name: "uname", width: 150 }],
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
    var pageBar = tab.addControl({
        xtype: "PageBar",
        onPageChanged: function (sender, e) {
            _pageNo = e.pageNo;
            reload(_pageNo);
            //loadPage(item._pageNo, item._orderByName, item._sortDirection, item._type);
        }
    });
    reload();

    tab.focus();
};
