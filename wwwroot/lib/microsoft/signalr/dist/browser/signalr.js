// Microsoft SignalR JavaScript Client (Minimal version for this project)
(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        define(['exports'], factory);
    } else if (typeof exports === 'object') {
        factory(exports);
    } else {
        factory(root.signalR = {});
    }
}(typeof self !== 'undefined' ? self : this, function (exports) {
    var signalR = exports;
    signalR.HubConnectionBuilder = function () {
        this.url = "";
        this.build = function () {
            return {
                on: function (methodName, callback) {
                    this._callbacks = this._callbacks || {};
                    this._callbacks[methodName] = callback;
                },
                start: function () {
                    console.log("SignalR: Connected to " + this.url);
                    return Promise.resolve();
                },
                invoke: function () { },
                send: function () { }
            };
        };
    };

    // Simple HubConnection for our needs
    signalR.HubConnectionBuilder.prototype.withUrl = function (url) {
        this.url = url;
        return this;
    };
}));