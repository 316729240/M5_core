$M.account.list = function (S) {
    if ($._account) {
        $._account._moduleId = S.moduleId;
        $._account._classId = S.classId;
        $._account.pageBar.go(1);
        $._account.focus();
        return;
    }
    $._account = mainTab.addItem({ text: S.text, closeButton: true, onClose: function () { $._account = null; } });
    $._account._moduleId = S.moduleId;
    $._account._classId = S.classId;
    var T = $._account;

    var toolBar = T.addControl({
        xtype: "ToolBar",
        items: [
            [{ xtype: "Button", tip: "添加", name: "appendButton", ico: "fa-plus", onClick: function () { addData(); } } ,
            {
                text: "删除所选会员", onClick: function () { delData(); }
            }], [{
                xtype: "InputGroup", name: "searchGroup", style: { width: "300px" }, items: [
                        { xtype: "Button", text: "搜索'用户名'", name: "searchF" },
                        { xtype: "TextBox", text: "", name: "searchInput" },
                        { xtype: "Button", ico: "fa-search", name: "searchButton" }]
            }]
        ]
    });

    var addData = function () {
        $M.app.call("$M.account.edit", { classId: $._account._classId, back: reload });
    };
    var searchGroup = toolBar.find("searchGroup");
    var searchF = searchGroup.find("searchF");
    var searchButton = searchGroup.find("searchButton");
    var searchInput = searchGroup.find("searchInput");
    searchInput.attr("onKeyDown", function (sender, e) {
        if (e.which == 13) {
            $._account._keyword = searchInput.val();
            loadPage(1, T._orderByName, T._sortDirection, T._type);
        }
    });
    searchButton.attr("onClick", function () {
        $._account._keyword = searchInput.val();
        loadPage(1, T._orderByName, T._sortDirection, T._type);
    });
    var getId = function () {
        var ids = "";
        for (var i = 0; i < grid.selectedRows.length; i++) {
            if (i > 0) ids += ",";
            ids += grid.selectedRows[i].cells[0].val();
        }
        return ids;
    };
    var delData = function () {
        if (grid.selectedRows.length == 0) return;
        var ids = getId();
        if (ids == "") return;
        $M.confirm("您确定要删除所选数据吗？", function () {
            $M.comm("account.delData", { moduleId: $._account._moduleId, classId: $._account._classId, ids: ids, tag: ($._account._type == 0 ? 0 : 1) }, function () {
                reload();
            });
        });
    };
    var grid = T.addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill,
        allowMultiple: true,
        allowSorting: true,
        onSelectionChanged: function (sender, e) {
            //setEditButtonStatus(sender.selectedRows.length > 0);
        },
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                $M.app.call("$M.account.edit", { classId: $._account._classId, dataId: sender.rows[e.y].cells[0].val(),back:reload });
            }
        },
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 6) {
                if (sender.rows[e.rowIndex].cells[6].val() == 0) return "待审核";
                else if (sender.rows[e.rowIndex].cells[6].val() == -1) return "被拒绝";
                return "";
            }
        }
    });
    T._pageNo = 1;
    T._orderByName = null;
    T._sortDirection = null;
    T._type = 0;
    T._searchField = "title";
    T._keyword = "";
    var loadPage = function (pageNo, orderByName, sortDirection, type) {

        //if (type == 0) grid.attr("contextMenuStrip", menu1);
        //else if (type == 2) grid.attr("contextMenuStrip", menu2);

        T._orderByName = orderByName == null ? "" : orderByName;
        T._type = type;
        T._sortDirection = sortDirection == null ? 0 : sortDirection;
        $M.comm("account.list", {
            moduleId: T._moduleId,
            classId: T._classId,
            pageNo: pageNo,
            orderBy: T._orderByName,
            sortDirection: T._sortDirection,
            type: T._type,
            searchField: T._searchField,
            keyword: T._keyword
        },
        function (json) {
            grid.clear();
            dateField = [];
            json[0][0].isLink = true;
            for (var i = 0; i < json[0].length; i++) {
                json[0][i].isLink = json[0][i].isTitle;
                if (json[0][i].name == "attribute") attrIndex = i;
                if (json[0][i].type == "Date") dateField[dateField.length] = i;
                if (json[0][i].name == T._orderByName) json[0][i].sortDirection = T._sortDirection;
            }
            grid.attr("columns", json[0]);
            grid.addRow(json[1].data);
            T.pageBar.attr("pageSize", json[1].pageSize);
            T.pageBar.attr("recordCount", json[1].recordCount);
            T.pageBar.attr("pageNo", json[1].pageNo);
            T.resize();
            setEditButtonStatus(false);
        });
    };
    var reload = function () {
        loadPage(T._pageNo, T._orderByName, T._sortDirection, T._type);
    };
    T.pageBar = T.addControl({
        xtype: "PageBar",
        onPageChanged: function (sender, e) {
            T._pageNo = e.pageNo;
            loadPage(T._pageNo, T._orderByName, T._sortDirection, T._type);
        }
    });
    T.pageBar.go(1);
    T.focus();
};