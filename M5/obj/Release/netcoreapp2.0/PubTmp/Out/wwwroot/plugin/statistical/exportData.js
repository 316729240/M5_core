$M.statistical.exportData = function (S) {
    var win = new $M.dialog({
        title: "导出数据",
        ico: "fa-file-excel-o",
        style: { width: "500px" },
        onClose: function (sender, e) {
            if (sender.dialogResult == $M.dialogResult.ok) {
                window.open($M.config.appPath + "statistical/export.ashx?moduleId="+S.moduleId+"&classId=" + S.classId + "&type=" + S.type + "&searchField=" + S.searchField + "&keyword=" + escape(S.keyword) + "&day=" + cList.val());
            }
        }
    });
    var cList = win.addControl({ labelText: "范围", xtype: "DateRangePickerInput", size: 1});
    win.show();
};
