
$M.insertFujian = function (S) {
    var win = $(document.body).addControl({
        xtype: "Window",
        text: "上传文件",
        isModal: true,
        style: { width: "500px" },
        ico: S.ico,
        onClose: function () {
            cancelFlag = true;
        },
        footer: [
            { xtype: "Button", text: "取 消", size: 2, onClick: function () { win.dialogResult = $M.dialogResult.cancel; win.remove(); return false; } }
        ]
    });
    var cancelFlag = false; //终止上传
    var label = win.append("<h2></h2>");
    var p1 = win.addControl({ xtype: "ProgressBar" });

    var files = [];
    var uploadComplete = function (json) {
        if (json.errNo < 0) {
            $M.confirm(json.errMsg, function (flag) {
                fileIndex--;
                uploadFile();
            }, { onCancel: function () { end(); } });
        } else {
            for (var i1 = 0; i1 < json.userData.length; i1++) {
                files[files.length] = json.userData[i1];
            }
            uploadFile();
        }
    };
    var uploadProgress = function (evt) {
        if (evt.lengthComputable) {
            var fen = 100 / inputFile[0].files.length;
            var percentComplete = Math.round(evt.loaded / evt.total * fen + fen * fileIndex);
            p1.val(percentComplete);
        }
    };
    var end = function () {
        win.remove();
        if (S.back) S.back(files);
        inputFile.remove();
    };
    var uploadFile = function () {
        fileIndex++;
        if (cancelFlag || fileIndex == inputFile[0].files.length) { end(); return; }
        // if (inputFile[0].files[fileIndex].type == "") {uploadFile();return;}
        var fd = new FormData();
        label.html("正在上传" + inputFile[0].files[fileIndex].name);
        fd.append("fileData", inputFile[0].files[fileIndex]);
        coveredFlag = 0;
        var xhr = new XMLHttpRequest();
        xhr.upload.addEventListener("progress", uploadProgress, false);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                var json = null;
                //try {
                eval("json=" + xhr.responseText);
                uploadComplete(json);
                //                } catch (x) {
                //                    json = { errNo: -1, errMsg: xhr.responseText };
                //                    uploadComplete(json);
                //                }
            }
        };

        xhr.open("POST", $M.config.appPath + "article/upload");
        xhr.send(fd);
    }
    var inputFile = $("<input type='file' accept='aplication/*'  " + (S.isMultiple == false ? "" : "multiple='multiple'") + " style='display:none' >").appendTo($(document.body));
    inputFile.on("change", function () {
        fileIndex = -1;
        win.show();
        uploadFile();
    });
    inputFile[0].click();
};

$M.article = {
    //预览接口
    preview: function (S) {
        $M.comm("article.read", { id: S.dataId }, function (json) {
            S.back(json.u_content);
        });
    },
    //后台数据编辑入口函数 命名为 edit 不能更改
    edit: function (S) {
        var tab = mainTab.addItem({ text: "文章编辑", "class": "form-horizontal", closeButton: true });
        var toolBar = tab.addControl({
            xtype: "ToolBar", class: "note-toolbar",
            items: [
                [{ text: "保存", ico: "fa-save", primary:true, onClick: function () { form.submit(); } }, { text: "打开网址", ico: "fa-external-link", onClick: function () {
                    if (S.url) window.open(S.url);
                }
                }, { text: "新增", ico: "fa-plus", onClick: function () { newData(); } }],
                [{ text: "选择模板",
                    onClick: function () {
                        $M.dialog.selectTemplate({ classId: S.classId, skinId: S.skinId,
                            back: function (id) {
                                skinId.val(id);
                            }
                        });
                    }
                }], [
                    { text: "审核", menu: [
                        { text: "通过", onClick: function () { 
                            $M.comm("auditData", { ids: dataId.val(), flag: 0, classId: classId.val() }, function () { $M.alert("操作成功！"); if (S.back) S.back(); });
                        }
                        }, {
                            text: "拒绝", onClick: function () {
                                $M.prompt("拒绝理由", function (msg) {
                                    $M.comm("auditData", { ids: dataId.val(), flag: 1, msg: msg, classId: classId.val() }, function () { $M.alert("操作成功！"); if (S.back) S.back(); });
                                }, { required: true });
                                
                            }
                        }]
                    }]
                ]
        });
        var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: "*" }, { size: 300, visible: false}] });
        //frame.attr("items", [{ size: "*" }, { size: 0}]);
        //frame.items[1].attr("size",0);
        frame.items[0].css({ "overflow-y": "auto", "overflow-x": "hidden" });
        tab.focus();
        var form = frame.items[0].addControl({
            xtype: "Form",
            command: "article.edit",
            onBeginSubmit: function () {
                var xml = new $M.xml();
                var domRoot = xml.addDom("variables");
                var data = grid.val();
                for (var i = 0; i < data.length; i++) {
                    var node = domRoot.addDom("item")
                    node.val(data[i][1]);
                    node.attr("name", data[i][0]);
                }
                custom.val(xml.getXML());
            },
            onSubmit: function (sender, e) {
                if (S.back) S.back();
                $M.confirm("保存成功，是否关闭？", tab.remove, { footer: [{ text: "是" }, { text: "否"}] });
                read(e.returnData);
            }
        });
        var custom = form.append("<textarea  name=\"u_custom\" style=\"display:none\"></textarea>");
        var skinId = form.append("<input name=skinId value='' type='hidden'>");
        var dataId = form.append("<input name=id value='" + S.dataId + "' type='hidden'>");
        var classId = form.append("<input name=classId value='" + S.classId + "' type='hidden'>");
        var dataList = form.addControl([
            { xtype: "TextBox", name: "title", labelText: "文章标题", labelWidth: 2 },
            { xtype: "TextBox", name: "u_keyword", labelText: "关 键 词", labelWidth: 2 },
            /*{
                xtype: "DialogInput", name: "u_fromWeb", labelText: "附 　 件", labelWidth: 2, onButtonClick: function (sender,e) {

                    $M.insertFujian({
                        isMultiple: false,
                        back: function (json) {
                            sender.val(json[0]);
                        }
                    });
                }
            },*/
            { xtype: "UploadFileBox", name: "pic", labelText: "缩 略 图", labelWidth: 2 },
            { xtype: "TextBox", name: "u_info", labelText: "文章摘要", labelWidth: 2, multiLine: true, style: { height: "50px"} },
            { xtype: "Editor", name: "u_content", style: { height: 300 }, labelText: "文章内容", labelWidth: 2 }
        ]);
        var grid = frame.items[1].addControl({
            xtype: "GridView", dock: 2,
            columns: [
                { text: "名称", name: "name", width: 60 },
                { text: "值", xtype: "TextBox", name: "value", width: 200 }
            ]
        });
        var read = function (dataId) {
            if (dataId) {
                $M.comm("article.read", { id: dataId }, function (json) {
                    setForm(json);
                });
            }
        };
        var setForm = function (data) {
            form.val(data);
            S.skinId = data.skinId;
            S.url = data.url;
            var xml = new $M.xml(data.u_custom);
            for (var i = 0; i < grid.rows.length; i++) {
                if (xml.documentElement.childNodes && xml.documentElement.childNodes.length > i) grid.rows[i].cells[1].val(xml.documentElement.childNodes[i].text);
            }
        };
        var newData = function () {
            for (var i = 0; i < grid.rows.length; i++) grid.rows[i].cells[1].val("")
            for (var i = 0; i < dataList.length; i++) dataList[i].val("");
            dataId.val("");
        };
        var comm = [["getCustomField", { classId: S.classId}]];
        if (S.dataId != null) comm[comm.length] = ["article.read", { id: S.dataId}];
        $M.comm(comm, function (data) {
            if (data[0] && data[0].variables) {
                grid.addRow(data[0].variables.item);
                frame.items[1].attr("visible", true);
            }
            if (S.dataId != null) setForm(data[1]);
        });
    },
    
    uploadFile: function (S) {
        var win = $(document.body).addControl({
            xtype: "Window",
            text: "上传文件",
            isModal: true,
            style: { width: "500px" },
            ico: S.ico,
            onClose: function () {
                cancelFlag = true;
            },
            footer: [
                { xtype: "Button", text: "取 消", size: 2, onClick: function () { win.dialogResult = $M.dialogResult.cancel; win.remove(); return false; } }
            ]
        });
        var cancelFlag = false; //终止上传
        var label = win.append("<h2></h2>");
        var p1 = win.addControl({ xtype: "ProgressBar" });
        var succeed = 0;
        var files = [];
        var uploadComplete = function (json) {
            if (json.errNo < 0) {
                //$M.confirm(json.errMsg, function (flag) {
                //    fileIndex--;
                //    uploadFile();
                //}, { onCancel: function () { end(); } });
            } else {
                succeed++;
                //for (var i1 = 0; i1 < json.userData.length; i1++) {
                //    files[files.length] = json.userData[i1];
                //}
            }
            uploadFile();
        };
        var uploadProgress = function (evt) {
            if (evt.lengthComputable) {
                var fen = 100 / inputFile[0].files.length;
                var percentComplete = Math.round(evt.loaded / evt.total * fen + fen * fileIndex);
                p1.val(percentComplete);
            }
        };
        var end = function () {
            win.remove();
            //if (S.back) S.back(files);
            inputFile.remove();
            $M.alert("上传成功" + succeed + "篇");
            S.reload();
        };
        var uploadFile = function () {
            fileIndex++;
            if (cancelFlag || fileIndex == inputFile[0].files.length) { end(); return; }
            // if (inputFile[0].files[fileIndex].type == "") {uploadFile();return;}
            var fd = new FormData();
            label.html("正在上传" + inputFile[0].files[fileIndex].name);
            fd.append("fileData", inputFile[0].files[fileIndex]);
            fd.append("moduleId", S.moduleId);
            fd.append("classId", S.classId);
            coveredFlag = 0;
            var xhr = new XMLHttpRequest();
            xhr.upload.addEventListener("progress", uploadProgress, false);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4) {
                    var json = null;
                    //try {
                    eval("json=" + xhr.responseText);
                    uploadComplete(json);
                    //                } catch (x) {
                    //                    json = { errNo: -1, errMsg: xhr.responseText };
                    //                    uploadComplete(json);
                    //                }
                }
            };

            xhr.open("POST", $M.config.appPath + "article/uploadTxt.ashx");
            xhr.send(fd);
        }
        var inputFile = $("<input type='file' accept='text/plain'  " + (S.isMultiple == false ? "" : "multiple='multiple'") + " style='display:none' >").appendTo($(document.body));
        inputFile.on("change", function () {
            fileIndex = -1;
            win.show();
            uploadFile();
        });
        inputFile[0].click();

    }
};