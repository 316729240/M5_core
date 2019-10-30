$M.templateManage.find = function (S) {
    var tab = mainTab.addItem({ text: (S.u_webFAid==0?"PC":"手机")+"查找替换", closeButton: true, onClose: function () { $.dataManage = null; } });
    var input = tab.addControl([
        { labelText: "查找内容", xtype: "TextBox", name: "content1", multiLine: true, rows: 3, width: 6, onChange: function () { replaceButton.enabled(false); } },
        { labelText: "替换内容", xtype: "TextBox", name: "content2", multiLine: true, rows: 3, width: 6 }
        ]);
    var content1 = input[0];
    var content2 = input[1];
    var box = tab.append("<p class=\"text-right btn-toolbar\"></p>");
    box.addControl({ xtype: "Button", text: "查找", onClick: function () { find(); } });
    var replaceButton = box.addControl({ xtype: "Button", text: "替换", enabled: false, onClick: function () {
        $M.confirm("您确定要执行替换操作吗？", function () {
            replace();
        }); 
    } });

    tab.append("<div style='clear:both;'></div>");

    var listGrid = tab.addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill,
        columns: [
                    { text: "id", name: "id", visible: false },
                    { text: "classId", name: "classId", visible: false },
                    { text: "u_datatypeid", name: "u_datatypeid", visible: false },
                    { text: "u_type", name: "u_type", visible: false },
                    { text: "类型", name: "u_type", width: 80 },
                    { text: "标题", name: "title", width: 300, isLink: true },
                    { text: "操作", name: "v", width: 80, foreColor: "#33CC33" }
                ],
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 4) {
                return e.value == 0 ? "模板" : "视图";
            }
        },
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                var _id = sender.rows[e.y].cells[0].val();
                var _u_type = sender.rows[e.y].cells[4].val();
                var _datatypeId = sender.rows[e.y].cells[2].val();
                if (_u_type == 0) {
                    $M.app.call("$M.templateManage.edit", { id: _id, datatypeId: _datatypeId, u_webFAid: S.u_webFAid });
                } else {
                    $M.app.call("$M.templateManage.viewEdit", { id: _id, u_webFAid: S.u_webFAid });
                }
            }
            $M.call("$M.templateManage.edit", {u_webFAid: S.u_webFAid});
        },
        onSelectionChanged: function (sender, e) {
        },
        onKeyDown: function (sender, e) {

        }
    });
    var find = function () {
        $M.comm("templateManage.find", { keyword: content1.val(), u_webFAid: S.u_webFAid }, function (json) {
            listGrid.clear();
            listGrid.addRow(json);
            if (json.length > 0) replaceButton.enabled(true);
        });
    };
    var replace = function () {
        var p = null;
        if (listGrid.rows.length == 0) {
            $M.alert("请选查找要出要替换的内容，再点击替换按钮");
            return;
        }
        var index = 0;
        p = $M.progressDialog();
        p.show();
        var _rep = function () {
            if (index == listGrid.rows.length) {
                p.remove();
                $M.alert("替换完成");
                return;
            }
            listGrid.rows[index].cells[6].val("处理中");
            $M.comm("templateManage.replace", { type: 0, mbType: listGrid.rows[index].cells[4].val(), id: listGrid.rows[index].cells[0].val(), keyword: content1.val(), keyword2: content2.val(), u_webFAid: S.u_webFAid }, function (json) {
                listGrid.rows[index].focus();
                listGrid.rows[index].cells[6].val("处理成功");
                p.val((index + 0.0) / listGrid.rows.length * 100);
                index++;
                setTimeout(_rep, $M.config.operationTimeDelay);
            });
        };
        _rep();
    };

    tab.focus();
};