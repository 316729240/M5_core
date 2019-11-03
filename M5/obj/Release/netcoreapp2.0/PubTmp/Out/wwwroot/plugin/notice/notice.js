$M.notice.edit = function (S) {
    var tab = mainTab.addItem({ text: "发布公告", "class": "form-horizontal", closeButton: true });
    tab.focus();

    var toolBar = tab.addControl({
        xtype: "ToolBar", class: "note-toolbar",
        items: [
            [{ text: "保存", ico: "fa-save", color: 2, onClick: function () { form.submit(); } }],
            
        ]
    });
    var form = tab.addControl({
        xtype: "Form",
        command: "notice.edit",
        onSubmit: function (sender, e) {
            if (S.back) S.back();
            $M.confirm("保存成功，是否关闭？", tab.remove, { footer: [{ text: "是" }, { text: "否" }] });
            read(e.returnData);
        }
    });
    var dataId = form.append("<input name=id value='" + S.dataId + "' type='hidden'>");
    var classId = form.append("<input name=classId value='" + S.classId + "' type='hidden'>");
    //form.addControl({ xtype: "SelectBox", name: "objectType", labelText: "对象", labelWidth: 1, items: [{ text: "全部用户", value: 0 }, { text: "角色", value: 1 }, {text:"用户",value:2}] });
    form.addControl({ xtype: "TextBox", name: "u_userList", labelText: "用户名", labelWidth: 1 });
    form.addControl({ xtype: "CheckBox", name: "u_groupList", labelText: "用户组", labelWidth: 1, items: [{ text: "企业会员", value: 9896847028 }, { text: "个人会员", value: 9896848409 }] });
    form.addControl({ xtype: "TextBox", name: "title", labelText: "标题", labelWidth: 1, vtype: { required: true } });
    form.addControl({ xtype: "Editor", name: "u_content", style: { height: 300 }, labelText: "内容", labelWidth: 1 });


    var read = function (dataId) {
        if (dataId) {
            $M.comm("notice.read", { id: dataId }, function (json) {
                form.val(json);
            });
        }
    };
    read(S.dataId);
};
$M.notice.messageList = function (S) {
    var tab = mainTab.find("messageList");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ ico: "fa-envelope-o", text: "我的消息", name: "messageList", closeButton: true, onClose: function () { } });
    var _pageNo = 1;
    var _status = 0;
    var userList = null, roleGrid = null;

    var reload = function (pageno) {
        if (pageno == null) pageno = 1;
        $M.comm("notice.messageList", { pageNo: pageno ,"status":_status}, function (json) {
            userList.clear();
            userList.addRow(json.data);
            pageBar.attr("pageSize", json.pageSize);
            pageBar.attr("recordCount", json.recordCount);
            pageBar.attr("pageNo", json.pageNo);
            setEditButtonStatus();
        });
    };

    var getId = function () {
        var ids = "";
        for (var i = 0; i < userList.selectedRows.length; i++) {
            if (i > 0) ids += ",";
            ids += userList.selectedRows[i].cells[0].val();
        }
        return ids;
    };
    var toolBar = tab.addControl({
        xtype: "ToolBar", items: [
        {
            xtype: "ButtonCheckGroup", items: [{ text: "未读", value: 0}, { text: "已读", value: 1}], value: 0,
            onClick: function (sender, e) {
                _status = e.value; reload(1);
                //stopButton.val(_status == 1 ? "未读" : "已读");
            }
        }, {
            text: "删除", onClick: function () {
                if (userList.selectedRows.length == 0) return;
                $M.confirm("您确定要删除所选消息吗？", function () {
                    var ids = getId();
                    if (ids != "") {
                        $M.comm("notice.delMessage", { ids: ids }, reload);
                    }
                });
            }
        }]
    });

    userList = tab.addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1,
        allowMultiple: true,
        columns: [
            { text: "id", name: "id", visible: false, width: 100 },
            { text: "消息标题", name: "title", width: 300,isLink:true },
            { text: "时间", name: "createDate", width: 150 },
            { text: "用户", name: "uname", width: 150 }],
        onRowCommand: function (sender, e) {
            if (e.commandName == "link") {
                var _id = sender.rows[e.y].cells[0].val();
                $M.app.call("$M.notice.send", { dataId: _id, back: reload });
            }
        },
        onKeyDown: function (sender, e) {
            if (e.which == 46) delData();
        },
        onSelectionChanged: function (sender, e) {
            //setEditButtonStatus(sender.selectedRows.length > 0);
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
$M.notice.send = function (S) {
    var obj = new $M.dialog({
        title: "回信",
        style: { width: "600px",height:"500px" },
        command: "notice.edit",
        onBeginSubmit: function () {

        },
        onClose: function () {
            S.back();
        }
    });
    var u_userList=obj.append("<input name=u_userList value='' type='hidden'>");
    var classId = obj.append("<input name=classId  type='hidden'>");
    var box = obj.append("<div class='bs-example' style='height:150px;overflow:auto;'></div>");
    
    var title=obj.addControl({ xtype: "TextBox", name: "title", labelText: "标题", labelWidth: 2, vtype: { required: true } });

    obj.addControl({ xtype: "Editor", name: "u_content", style: { height: 150 }, labelText: "内容", labelWidth: 2, size: 0, customItem: ["font", "figure", "align", "list", "other"] });
    obj.append("<div style=\"clear:both\"></div>");

    var read = function (dataId) {
        if (dataId) {
            $M.comm([["notice.read", { id: dataId }], ["notice.setMessageStatus", { id: dataId,status:1 }]], function (json) {
                box.html("<blockquote>" + json[0].title + "<br>" + json[0].u_content + "</blockquote>");
                u_userList.val(json[0].uname);
                classId.val(json[0].classId);
                title.val("回复：" + json[0].title);
            });
        }
    };
    read(S.dataId);
    obj.show();
};
