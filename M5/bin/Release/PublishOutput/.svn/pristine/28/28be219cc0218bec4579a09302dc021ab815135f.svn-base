$M.accessData.list = function (S) {
    var tab = mainTab.find("payment");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "访问记录", name: "payment", closeButton: true, onClose: function () { } });
    var _pageNo = 1;
    var _status = 1;
    var userList = null, roleGrid = null;

    var reload = function (pageno) {
        if (pageno == null) pageno = 1;
        $M.comm("accessData.list", { pageNo: pageno, datefw: datefw.val, userName: searchInput.val() }, function (json) {
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
            $M.comm("userManage.setStatus", { ids: ids, status: _status == 1 ? 0 : 1 }, reload);
        }
    };

    var toolBar=tab.addControl({
        xtype: "ToolBar", items: [[{ xtype: "DateRangePickerInput", style: { width: "300px" } ,name:"datefw"}],
            [{
                    xtype: "InputGroup", name: "searchGroup", style: { width: "300px" }, items: [
                    { xtype: "TextBox", text: "",placeholder:"用户名", name: "searchInput" },
                    {
                        xtype: "Button", ico: "fa-search", name: "searchButton"
                    }]
            }]]
    });
    var datefw = toolBar.find("datefw");
    var searchGroup = toolBar.find("searchGroup");
    var searchButton = searchGroup.find("searchButton");
    var searchInput = searchGroup.find("searchInput");
    
    searchButton.attr("onKeyDown", function (sender, e) {
        if (e.which == 13) {
            reload(1);
        }
    });
    searchButton.attr("onClick", function (sender, e) {
        reload(1);
    });
    userList = tab.addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1,
        allowMultiple: true,
        columns: [
            { text: "id", name: "id", visible: false, width: 100 },
            { text: "访问页面", name: "u_title", width: 300 },
            { text: "访问地址", name: "u_url", width: 300,isLink:true },
            { text: "用户", name: "u_userName", width: 150 },
            { text: "访问时间", name: "u_createDate"} ],
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                window.open(sender.rows[e.y].cells[e.x].val());
            }
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delData();
        },
        onSelectionChanged: function (sender, e) {
            setEditButtonStatus(sender.selectedRows.length > 0);
        },
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 4) {
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
