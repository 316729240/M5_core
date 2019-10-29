$M.appShop.main = function (S) {
    var win = $(document.body).addControl({
        xtype: "Window",
        text: "应用商城",
        ico: "fa-shopping-cart",
        isModal: true,
        style: { width: "450px" },
        onClose: function () {
            //if (S.onClose) S.onClose(win, rdata);
        }
    });
    //var tab = win.addControl({ xtype: "Tab", items: [{ text: "应用下载", "class": "form-horizontal" }, { text: "已安装"}] });
    var grid = win.addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0,
        style:{height:"360px"},
        columns: [{ text: "ico", name: "ico", width: 40 }, { text: "title", name: "title", visible: false }, { text: "info", name: "info" }, { name: "installFlag", visible: false }, { name: "updateFlag", width: 80, isLink: true }, { name: "name", width: 80, isLink: true }, { name: "datetime", visible: false }],
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 0) {
                return "<img src='" + e.value + "' style='width:40px;height:40px;'>";
            } else if (e.columnIndex == 2) {
                return "<h4>" + grid.rows[e.rowIndex].cells[1].val() + "</h4><span>" + grid.rows[e.rowIndex].cells[2].val() + "</span>";
            } else if (e.columnIndex == 5) {
                var installFlag = grid.rows[e.rowIndex].cells[3].val();
                var updateFlag = grid.rows[e.rowIndex].cells[4].val();
                if (updateFlag) return "更新";
                if (installFlag) return "";
                return "安装";
            } else if (e.columnIndex == 4) {
                var installFlag = grid.rows[e.rowIndex].cells[3].val();
                return installFlag?"卸载":"";
            }
        },
        onRowCommand: function (sender, e) {
            if (e.x == 5) {
                if (grid.rows[e.y].cells[5].attr("foreColor") ==null) {
                    grid.rows[e.y].cells[5].html("安装中...");
                    grid.rows[e.y].cells[5].attr("foreColor", "#808080");
                    $M.comm("appShop.setup", { appName: grid.rows[e.y].cells[5].val(), datetime: grid.rows[e.y].cells[6].val() }, function () {
                        grid.rows[e.y].cells[5].html("");
                        grid.rows[e.y].cells[4].html("卸载");
                    });
                }
            } else if (e.x == 4) {
                $M.confirm("您确定要删除该应用吗？", function () {
                    $M.comm("appShop.uninstall", { appName: grid.rows[e.y].cells[5].val() }, function () {
                        grid.rows[e.y].cells[5].html("安装");
                        grid.rows[e.y].cells[4].html("");
                    });
                });
            }
        }
    });
    win.show();
    $M.comm("appShop.readAppList", null, function (json) {
        grid.addRow(json);
    });

};