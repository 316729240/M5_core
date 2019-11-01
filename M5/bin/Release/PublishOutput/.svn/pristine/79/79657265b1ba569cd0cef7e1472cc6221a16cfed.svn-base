$M.fileManage.txtEdit = function (S) {
    var tab = mainTab.addItem({ text: S.fileName, closeButton: true, onClose: function () { $.dataManage = null; } });

    var toolBar = tab.addControl({
        xtype: "ToolBar", items: [[
            { ico: "fa-floppy-o", text: "保存", name: "saveButton",
                onClick: function () {
                    $M.comm("fileManage.saveFile", { path: S.path, fileName: S.fileName, encoding: encoding.val(), content: editBox.val() }, function (json) {
                        $M.alert("保存成功！");
                        if(S.back)S.back();
                    });
                }
            },
            { ico: "fa-external-link", text: "打开地址", enabled: true, name: "openUrl",
                onClick: function () {
                    alert($M.config.webPath + S.path + S.fileName);
                }
            }
        ],
        [
            { xtype: "SelectBox", name: "encoding", items: [{ text: "gb2312", value: "gb2312" }, { text: "ascii", value: "us-ascii" }, { text: "utf-8", value: "utf-8" }, { text: "unicode", value: "unicode"}] }
        ]
    ]
    });
    var encoding = toolBar.find("encoding");
    var editBox = tab.addControl({ xtype: "Editor", name: "content",codemirror: true,sourceMode: true, style: { height: 300 }, dock: 2 });
    tab.focus();
    if (S.newfile != true) {
        $M.comm("fileManage.getFile", { path: S.path, fileName: S.fileName }, function (json) {
            editBox.val(json.content);
            encoding.val(json.encoding);
        });
    }
};
