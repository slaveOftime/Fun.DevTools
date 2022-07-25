var utils;
(function (utils) {
    utils.saveFile = function (filename, data) {
        var blob = new Blob([data], { type: 'text/plain' });
        var a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = filename;
        a.click();
        a.remove();
    };
})(utils || (utils = {}));
//# sourceMappingURL=index.js.map