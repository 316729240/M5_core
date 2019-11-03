$M.serviceManual.list = function (S) {
    var text = S.text, moduleId = S.moduleId, classId = S.classId, back = S.back;
    var extensionMenu = null;
    var _p = ["moduleId", "dataTypeId", "keyword", "searchField", "type"];
    if (S.classId) _p = ["classId", "dataTypeId", "keyword", "searchField", "type"];

    var item = null;
    extensionMenu = $M.loadInterface(_p, S.dataTypeId, function (name) {
        $M.app.call(name, { moduleId: item._moduleId, classId: item._classId, type: item._type, searchField: item._searchField, keyword: item._keyword });
    });
    if ($.dataManage == null) $.dataManage = {};
    if ($.dataManage[S.dataTypeId]) {
        $.dataManage[S.dataTypeId]._moduleId = moduleId;
        $.dataManage[S.dataTypeId]._classId = classId;
        $.dataManage[S.dataTypeId]._dataTypeId = S.dataTypeId;
        $.dataManage[S.dataTypeId].attr("text", text);
    } else {
        $.dataManage[S.dataTypeId] = mainTab.addItem({ text: text, closeButton: true, onClose: function () { $.dataManage[S.dataTypeId] = null; } });
        $.dataManage[S.dataTypeId]._moduleId = moduleId;
        $.dataManage[S.dataTypeId]._classId = classId;
        $.dataManage[S.dataTypeId]._dataTypeId = S.dataTypeId;
        item = $.dataManage[S.dataTypeId];

        var searchMenu = $(document.body).addControl({ xtype: "Menu", onItemClicked:
            function (sender, e) {
                item._searchField = e.attr("value");
                searchF.val("搜索'" + e.attr("text") + "'");
            }, items: [{ text: "id", value: "id" }, { text: "标题", value: "title" }, { text: "发布人", value: "userId" }, { text: "审核人", value: "auditorId" }, { text: "车型名称", value: "u_carName" }]
        });
        var attrIndex = -1;

        var dataTypeMenu = $(document.body).addControl({
            xtype: "Menu", onItemClicked:
                function (sender, e) {
                    e.attr("checked",!e.attr("checked"));
                }, items: [{ text: "已审核数据", value: "0", checked: true }, { text: "待审核数据", value: "1", checked: true }, { text: "未通过数据", value: "2" ,checked:true}]
        });
        var setB = function (index) {
            if(index==0){
                toolBar.controls[0].addClass("active");
                toolBar.controls[1].removeClass("active");
            } else {
                toolBar.controls[1].addClass("active");
                toolBar.controls[0].removeClass("active");
            }
            loadPage(1, item._orderByName, item._sortDirection);
        };
        var showType = "1110";
        var toolBar = item.addControl({
            xtype: "ToolBar",
            items: [
                [
                    {
                        tip: "数据", value: 0, ico: "fa-check-square-o", menu: dataTypeMenu, onMenuClose: function () {
                            showType = "";
                            for (var i = 0; i < dataTypeMenu.items.length; i++) showType += dataTypeMenu.items[i].attr("checked") ? "1" : "0";
                            showType += "0";
                            setB(0);
                        }
                    },
                    { tip: "回收站", value: 2, ico: "fa-recycle", onClick: function (sender, e) { showType = "0001";setB(1); } }],

                [{ xtype: "InputGroup", name: "searchGroup", style: { width: "300px" }, items: [
                    { xtype: "Button", text: "搜索'标题'", menu: searchMenu, name: "searchF" },
                    { xtype: "TextBox", text: "", name: "searchInput" },
                    { xtype: "Button", ico: "fa-search", name: "searchButton"}]
                }],
                [
                    { xtype: "Button", tip: "添加", name: "appendButton", ico: "fa-plus", onClick: function () { addData(); } },
                    { xtype: "Button", tip: "修改", name: "editButton", ico: "fa-edit", onClick: function () { editData(); } },
                    { xtype: "Button", tip: "删除", name: "delButton", ico: "fa-trash-o", onClick: function () { delData(); } },
                    { xtype: "Button", tip: "移动", name: "moveButton", ico: "fa-arrows", onClick: function () { moveData(); } }
                ],
                [{ tip: "扩展工具", ico: "fa-share-alt", menu: extensionMenu}]
            ]
        });
        var searchGroup = toolBar.find("searchGroup");
        var searchF = searchGroup.find("searchF");
        var searchButton = searchGroup.find("searchButton");
        var searchInput = searchGroup.find("searchInput");
        //var attrSetButton = toolBar.find("attrSetButton");
        var editButton = toolBar.find("editButton");
        var delButton = toolBar.find("delButton");
        var moveButton = toolBar.find("moveButton");
        searchInput.attr("onKeyDown", function (sender, e) {
            if (e.which == 13) {
                item._keyword = searchInput.val();
                loadPage(1, item._orderByName, item._sortDirection, item._type);
            }
        });
        searchButton.attr("onClick", function () {
            item._keyword = searchInput.val();
            loadPage(1, item._orderByName, item._sortDirection, item._type);
        });
        var setEditButtonStatus = function (flag) {
            //attrSetButton.enabled(flag);
            editButton.enabled(flag);
            delButton.enabled(flag);
            moveButton.enabled(flag);
        };
        setEditButtonStatus(false);
        item._pageNo = 1;
        item._orderByName = null;
        item._sortDirection = null;
        item._type = 0;
        item._searchField = "title";
        item._keyword = "";
        var grid = null;
        var reload = function () {
            loadPage(item._pageNo, item._orderByName, item._sortDirection, item._type);
        };
        var getId = function () {
            var ids = "";
            for (var i = 0; i < grid.selectedRows.length; i++) {
                if (i > 0) ids += ",";
                ids += grid.selectedRows[i].cells[0].val();
            }
            return ids;
        };
        var moveData = function () {
            if (grid.selectedRows.length == 0) return;
            $M.dialog.selectColumn(item.moduleId, function (json) {
                if (item._classId == json.id) { $M.alert("目标栏目不能为当前栏目"); return; }
                $M.comm("moveData", { ids: getId(), classId: json.id }, function () {
                    reload();
                });
            });
        };
        var setField = function () {
            $M.app.call("$M.system.displayField", { id: item._dataTypeId, back: function () { reload(); } });
        };
        var setTop = function (sender, item) {
            if (grid.selectedRows.length == 0) return;
            var ids = getId();
            $M.comm("setTop", { ids: ids, flag: (item.attr("checked") ? 0 : 1) }, function (json) {
                reload();
            });
        };
        var delData = function () {
            if (grid.selectedRows.length == 0) return;
            var ids = getId();
            if (ids == "") return;
            $M.confirm("您确定要删除所选数据吗？", function () {
                $M.app.interface("del", item._dataTypeId, function (f) {
                    if (f) {
                        f({ moduleId: item._moduleId, classId: item._classId, ids: ids, type: showType.substr(3, 1), back: reload });
                    } else {
                        $M.comm("delData", { moduleId: item._moduleId, classId: item._classId, ids: ids, tag: (showType.substr(3, 1) == 0 ? 0 : 1) }, function () {
                            reload();
                        });
                    }
                });
            });
        };
        var addData = function () {
            $M.app.interface("edit",item._dataTypeId, function(f){f({ dataId: null, classId: item._classId, back: reload })});
        };
        var editData = function () {
            if (grid.selectedRows.length == 0) return;
            $M.app.interface("edit", item._dataTypeId, function (f) { f({ dataId: grid.selectedRows[0].cells[0].val(), classId: item._classId, back: reload }) });
        };
        var setAttr = function (sender, item) {
            if (grid.selectedRows.length == 0) return;
            var ids = getId();
            $M.comm("setAttr", { ids: ids, type: item.attr("value"), flag: (item.attr("checked") ? 0 : 1) }, function (json) {
                reload();
            });
        };
        var setOrderId = function (sender, item) {
            if (grid.selectedRows.length == 0) return;
            var ids = getId();
            $M.comm("setOrderId", { ids: ids, orderId: item.attr("value") }, function (json) {
                reload();
            });
        };
        var reductionData = function () {
            if (grid.selectedRows.length == 0) return;
            var ids = getId();
            if (ids == "") return;
            $M.app.interface("reduction", item._dataTypeId, function (f) {
                    if (f) {
                        f({ moduleId: item._moduleId, classId: item._classId, ids: ids, type: item._type, back: reload });
                    } else {
                        $M.comm("reductionData", { moduleId: item._moduleId, classId: item._classId, ids: ids, tag: (item._type == 0 ? 0 : 1) }, function () {
                            reload();
                        });
                    }
                });
 
        };
        var menu1 = $(document.body).addControl({ xtype: "Menu", items: [
            { text: "编辑", onClick: editData },
            { text: "-" },
            { text: "至顶", onClick: setTop },
            {
                text: "审核状态", items: [
                    { text: "已审核", value: "0", onClick: setOrderId },
                    { text: "未审核", value: "-1", onClick: setOrderId },
                    { text: "已拒绝", value: "-2", onClick: setOrderId }
                ]
            },
            { text: "自定义属性", items: [
                { text: "推荐", value: "0", onClick: setAttr }, { text: "精华", value: "1", onClick: setAttr }, { text: "热点", value: "2", onClick: setAttr }
            ]
            },
            { text: "-" },
		    { text: "移动", onClick: moveData },
		    { text: "删除", onClick: delData },
            { text: "-" },
		    { text: "设置", onClick: setField },
		    { text: "属性" }
        ]
        });

        var menu2 = $(document.body).addControl({
            xtype: "Menu", items: [
                { text: "还原数据", onClick: reductionData },
                { text: "-" },
                { text: "彻底删除", onClick: delData },
                { text: "-" },
                { text: "属性" }
            ]
        });
        var popover = $(document.body).addControl({ xtype: "Popover", style: { width: "800px" }, maxHeight: 200, location: "left" });
        var formGroup = popover.append("<div class=modal-fo1oter ></div>");
        var button1 = formGroup.addControl({
            xtype: "Button", ico: "fa-check", color: 2, text: "通过", onClick: function () {
                $M.comm("auditData", { ids: previewHtml.dataId, flag: 0, moduleId: item._moduleId, classId: item._classId }, function () { $M.alert("操作成功！"); grid.rows[previewHtml._index].cells[statusIndex].val(0); grid.rows[previewHtml._index].cells[fieldCount].val(""); });
            }
        });
        var button2 = formGroup.addControl({
            xtype: "Button", ico: "fa-ban", color: 5, text: "拒绝", onClick: function () {

                //alert(grid.rows[previewHtml._index].cells[fieldCount].val(""));
                //return;
                $M.prompt("拒绝理由", function (msg) {
                    $M.comm("auditData", { ids: previewHtml.dataId, flag: 1, msg: msg, moduleId: item._moduleId, classId: item._classId }, function () { $M.alert("操作成功！"); grid.rows[previewHtml._index].cells[statusIndex].val(-2); grid.rows[previewHtml._index].cells[fieldCount].val(""); });
                }, { vtype: { required: false }});
            }
        });

        var previewHtml = popover.append("<div style='height:500px;overflow:auto;'></div>");
        grid = item.addControl({
            xtype: "GridView",
            dock: $M.Control.Constant.dockStyle.fill,
            allowMultiple: true,
            allowSorting: true,
            contextMenuStrip: menu1,
            onSorting: function (sender, e) {
                loadPage(1, e.name, e.sortDirection, item._type);
            },
            onKeyDown: function (sender, e) {
                if (e.which == 46) delData();
            },
            onRowCommand: function (sender, e) {
                if (e.commandName == "link") {
                    $M.app.interface("edit", item._dataTypeId, function(f){f({ classId: item._classId, dataId: sender.rows[e.y].cells[0].val(),back:reload })});
                }
            },
            onCellMouseClick: function (sender, e) {
                if (e.x == fieldCount) {
                    if (e.target.tagName == "I") {
                        var id = sender.rows[e.y].cells[0].val();
                        previewHtml.dataId = id;
                        previewHtml._index = e.y;
                        
                        $M.app.interface("preview", item._dataTypeId, function (f) {
                            if (f) {
                                f({
                                    dataId: id, back: function (html) {
                                        e.target.previewHtml = html;
                                        if (previewHtml.dataId==id) previewHtml.html(e.target.previewHtml == null ? "正在加载..." : e.target.previewHtml);
                                    }
                                });

                                previewHtml.html(e.target.previewHtml == null ? "正在加载..." : e.target.previewHtml);
                                popover.show($(e.target));
                            }
                        });
                    }
                }
            },
            onMouseDown: function (sender, e) {
                if (e.which == 3) {
                    var flag = [0, 0, 0, 0, 0, 0];
                    for (var i = 0; i < grid.selectedRows.length; i++) {
                        attr = grid.selectedRows[i].cells[attrIndex].val().split(",");
                        for (var i1 = 0; i1 < 6; i1++) {
                            flag[i1] = (flag[i1] | parseInt(attr[i1]));
                        }
                    }
                    menu1.items[2].attr("checked", (flag[0] == 1));
                    for (var i = 0; i < menu1.items[4].items.length; i++) {
                        menu1.items[4].items[i].attr("checked", (flag[i + 1] == 1));
                    }
                }
            },
            onCellFormatting: function (sender, e) {
                for (var i = 0; i < dateField.length; i++) {
                    if (e.columnIndex == dateField[i]) {
                        var value = new Date(parseInt(e.value.replace(/\D/igm, "")));
                        return value.format("yyyy-MM-dd hh:mm:ss");
                    }
                }
                if (e.columnIndex == fieldCount) {
                    
                    var orderId = sender.rows[e.rowIndex].cells[statusIndex].val();
                    if (orderId == -1) { sender.rows[e.rowIndex].addClass("text-warning"); return "<i class='fa fa-question-circle' title='待审核' />"; }
                    if (orderId == -2) {
                        var msg = sender.rows[e.rowIndex].cells[auditMsgIndex].val();
                        if (msg==null) msg= "被拒绝";
                        sender.rows[e.rowIndex].addClass("text-danger"); return "<i class='fa fa-exclamation-circle' title='" + msg + "' />";
                    }
                    return "";
                }
                if (e.columnIndex == attrIndex) {
                    var attr = e.value.split(',');
                    var html = "";
                    if (attr[0] == "1") html += "<i class='fa fa-arrow-circle-up' title='至顶' /> ";
                    if (attr[1] == "1") html += "<i class='fa fa-thumbs-o-up' title='推荐' /> ";
                    if (attr[2] == "1") html += "<i class='fa fa-star-o' title='精华' /> ";
                    return html;
                }
                return e.value;
            },
            onSelectionChanged: function (sender, e) {
                setEditButtonStatus(sender.selectedRows.length > 0);
            }
        });
        var dateField = null;
        var statusIndex = -1, fieldCount = -1, auditMsgIndex=-1;
        var loadPage = function (pageNo, orderByName, sortDirection, type) {

            if (type == 0) grid.attr("contextMenuStrip", menu1);
            else if (type == 2) grid.attr("contextMenuStrip", menu2);

            item._orderByName = orderByName == null ? "" : orderByName;
            item._type = type;
            item._sortDirection = sortDirection == null ? 0 : sortDirection;
            $M.comm("serviceManual.dataList", {
                moduleId: item._moduleId,
                classId: item._classId,
                pageNo: pageNo,
                orderBy: item._orderByName,
                sortDirection: item._sortDirection,
                type: showType,
                searchField: item._searchField,
                keyword: item._keyword
            },
            function (json) {

                grid.clear();
                //if (!grid.attr("columns")) {
                dateField = [];
                json[0][0].isLink = true;
                attrIndex = -1; statusIndex = -1; auditMsgIndex = -1;
                for (var i = 0; i < json[0].length; i++) {
                    json[0][i].isLink = json[0][i].isTitle;
                    if (json[0][i].name == "attribute") { attrIndex = i; json[0][i].name = null; }
                    if (json[0][i].type == "Date") dateField[dateField.length] = i;
                    if (json[0][i].name == "orderId") { json[0][i].visible = false; statusIndex = i; }
                    if (json[0][i].name == "auditMsg") { json[0][i].visible = false; auditMsgIndex = i; }
                    
                    if (json[0][i].name == item._orderByName) json[0][i].sortDirection = item._sortDirection;
                }
                fieldCount = json[0].length;
                json[0][json[0].length] = { name: "orderId", text: "状态", width: 60, sortDirection: (json[0][statusIndex].name == item._orderByName?item._sortDirection:null) };
                grid.attr("columns", json[0]);
                //}
                //item._classId = classId;
                grid.addRow(json[1].data);
                item.pageBar.attr("pageSize", json[1].pageSize);
                item.pageBar.attr("recordCount", json[1].recordCount);
                item.pageBar.attr("pageNo", json[1].pageNo);
                item.resize();
                setEditButtonStatus(false);
            });
        };
        item.pageBar = item.addControl({
            xtype: "PageBar",
            onPageChanged: function (sender, e) {
                item._pageNo = e.pageNo;
                loadPage(item._pageNo, item._orderByName, item._sortDirection, item._type);
            }
        });

    }
    $.dataManage[S.dataTypeId].pageBar.go(1);
    $.dataManage[S.dataTypeId].focus();
};