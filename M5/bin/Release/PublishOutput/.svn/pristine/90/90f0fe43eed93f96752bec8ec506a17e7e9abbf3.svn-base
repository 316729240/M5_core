$M.announcement.main = function (S) {
    var win = new $M.dialog({
        title: "修改公告",
        ico: "fa-edit",
        isModal: true,
        style: { width: "700px",height:"480px" },
        command: "announcement.save",
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                if (S.back) S.back(text.val());
            }
        }
    });
    win.show();
    var text = win.addControl({ xtype: "Editor", style: { height: "300px" }, name: "text" });
    $M.comm("announcement.read", {}, function (json) { text.val(json); });

};