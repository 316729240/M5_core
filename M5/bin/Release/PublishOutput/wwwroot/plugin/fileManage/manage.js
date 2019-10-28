$M.fileManage.manage = function (S) {
    var tab = mainTab.find("fileManage");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "文件管理",name:"fileManage", closeButton: true });
    var _pageNo = 1;
    var _status = 1;
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 200, text: "目录", ico: "fa-cloud" }, { size: "*"}] });
    var fileList = null, dirTree = null;
    var reloadDir = function () {
        var item = dirTree.selectedItem;
        item.attr("loadTag", "2");
        $M.comm([["fileManage.readDir", { path: item.attr("path")}], ["fileManage.readFiles", { path: item.attr("path")}]], function (json) {
            item.attr("loadTag", "1");
            item.clear();
            item.addItem(json[0]);
            fileList.clear();
            fileList.addRow(json[1].data);
            pageBar.attr("pageSize", json[1].pageSize);
            pageBar.attr("recordCount", json[1].recordCount);
            pageBar.attr("pageNo", json[1].pageNo);
        });
    };
    var createDir = function () {
        if (dirTree.selectedItem == null) return;
        $M.prompt("目录名", function (value) {
            if (value == "") return;
            $M.comm("fileManage.createDir", { path: dirTree.selectedItem.attr("path"), name: value }, function (json) {
                var item = dirTree.selectedItem.addItem(json);
                item.focus();
            });
        }, { vtype: { required: true} });
    };
    var editDir = function () {
        if (dirTree.selectedItem == null) return;
        var name = dirTree.selectedItem.attr("text");
        $M.prompt("目录名", function (value) {
            if (name == value) return;
            $M.comm("fileManage.editDir", { path: dirTree.selectedItem.attr("path"), name: value }, function (json) {
                dirTree.selectedItem.val(value);
                dirTree.selectedItem.attr("path", json.path);
                reloadDir();
            });
        }, { value: name });
    };
    var delDir = function () {
        if (dirTree.selectedItem != null) {
            var type = dirTree.selectedItem.attr("type");
            var path = dirTree.selectedItem.attr("path");
            if (type == null) {
                $M.confirm("您确定要删除[" + dirTree.selectedItem.attr("text") + "]吗？", function () {
                    $M.comm("fileManage.delDir", { path: path }, function () { dirTree.selectedItem.remove(); dirTree.root.items[0].focus(); });
                });
            }
        }
    };
    var menu1 = $(document.body).addControl({ xtype: "Menu", items: [
            { text: "新建文件夹", onClick: createDir },
            { text: "重命名", onClick: editDir },
		    { text: "删除", onClick: delDir },
		    { text: "-" },
		    { text: "刷新", onClick: reloadDir }
        ]
    });

    dirTree = frame.items[0].addControl({
        xtype: "TreeView", dock: 2,
        contextMenuStrip: menu1,
        onKeyDown: function (sender, e) {
            if (e.which == 46) delDir();
        },
        onAfterSelect: function (sender, e) {
            if (e.item.attr("loadTag") == null && e.item.child.items.length == 0) {
                reloadDir();
            } else {
                _pageNo = 1;
                reload();
            }
        }
    });
    var reload = function () {
        $M.comm("fileManage.readFiles", { path: dirTree.selectedItem.attr("path"), pageNo: _pageNo }, function (json) {
            fileList.clear();
            fileList.addRow(json.data);
            pageBar.attr("pageSize", json.pageSize);
            pageBar.attr("recordCount", json.recordCount);
            pageBar.attr("pageNo", json.pageNo);
        });
    };
    var delFile = function () {
        if (fileList.selectedRows.length == 0) return;
        var files = "";
        for (var i = 0; i < fileList.selectedRows.length; i++) {
            if (i > 0) files += ",";
            files += fileList.selectedRows[i].cells[0].val();
        }
        $M.confirm("您确定要删除所选文件吗？", function () {
            $M.comm("fileManage.delFile", { path: dirTree.selectedItem.attr("path"), files: files }, reload);
        });
    };

    var editFileName = function () {
        if (fileList.selectedRows.length == 0) return;
        var row = fileList.selectedRows[0];
        var name = row.cells[0].val();
        $M.prompt("文件名", function (value) {
            if (name == value) return;
            $M.comm("fileManage.editFileName", { path: dirTree.selectedItem.attr("path"),oldName:name, name: value }, function (json) {
                row.cells[0].val(value);
            });
        }, { value: name });
    };
    var menu2 = $(document.body).addControl({
        xtype: "Menu", items: [
                {
                    ico: "fa-file-o", text: "新建文件", onClick: function () {
                        $M.prompt("文件名", function (value) {
                            $M.app.call("$M.fileManage.txtEdit", { path: dirTree.selectedItem.attr("path"), fileName: value, newfile: true, back: reload });
                        }, { vtype: { required: true } });
                    }
                },
                { text: "重命名", onClick: editFileName },
                { text: "-" },
                { text: "上传文件", onClick: function () {
                    $M.app.call("$M.fileManage.uploadFile", { type: 1, path: dirTree.selectedItem.attr("path"), back: reload });
                }
                }, {
                    text: "上传文件夹", onClick: function () {
                        $M.app.call("$M.fileManage.uploadFile", { type: 2, path: dirTree.selectedItem.attr("path"), back: reloadDir });
                    }
                },
                { text: "-" },
                { ico: "fa-trash-o", text: "删除", onClick: delFile },
                { text: "-" },
                { text: "刷新", onClick: reload }
        ]
    });
    var toolBar = frame.items[1].addControl({
        xtype: "ToolBar", items: [
            [{ text: "新建文件", ico: "fa-file-o", onClick: function () {
                $M.prompt("文件名", function (value) {
                    $M.app.call("$M.fileManage.txtEdit", { path: dirTree.selectedItem.attr("path"), fileName: value,newfile:true,back:reload});
                }, { vtype: { required: true} });
            } 
            }], [
            { text: "上传", name: "editButton", ico: "fa-upload", menu: [
            { text: "上传文件", onClick: function () {
                $M.app.call("$M.fileManage.uploadFile", { type: 1, path: dirTree.selectedItem.attr("path"), back: reload });
            }
            }, { text: "上传文件夹", onClick: function () {
                $M.app.call("$M.fileManage.uploadFile", { type: 2, path: dirTree.selectedItem.attr("path"), back: reloadDir });
            }
            }]
            },
            { text: "删除", name: "delButton", ico: "fa-trash-o", enabled: false, onClick: delFile }
       ]]
    });

    var delButton = toolBar.find("delButton");
    var setEditButtonStatus = function (flag) {
        delButton.enabled(flag);
    };
    fileList = frame.items[1].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1,
        allowMultiple: true,
        contextMenuStrip: menu2,
        columns: [{ text: "文件名", name: "text", width: 300, isLink: true }, { text: "文件大小", name: "size", width: 100 }, { text: "修改时间", name: "updateTime", width: 200}],
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                $M.app.call("$M.fileManage.txtEdit", { path: dirTree.selectedItem.attr("path"), fileName: sender.rows[e.y].cells[0].val() });
            }
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delFile();
        },
        onSelectionChanged: function (sender, e) {
            setEditButtonStatus(sender.selectedRows.length > 0);
        }
    });
    var pageBar = frame.items[1].addControl({
        xtype: "PageBar",
        onPageChanged: function (sender, e) {
            _pageNo = e.pageNo;
            reload();
        }
    });
    dirTree.root.addItem([{ text: "根目录", type: 0, path: "\\", ico: "fa-share-alt-square" }, { text: "收藏夹", ico: "fa-star"}]);
    dirTree.root.items[1].addItem([{ text: "配制文件", type: 0, path: "\\config\\"}]);
    dirTree.root.items[1].addItem([{ text: "日志文件", type: 0, path: "\\manage\\log\\"}]);
    tab.focus();
    dirTree.root.items[0].focus();
};
