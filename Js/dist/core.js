var __classPrivateFieldGet = (this && this.__classPrivateFieldGet) || function (receiver, privateMap) {
    if (!privateMap.has(receiver)) {
        throw new TypeError("attempted to get private field on non-instance");
    }
    return privateMap.get(receiver);
};
var NuScien;
(function (NuScien) {
    class Assert {
        static toStr(obj, level) {
            if (typeof obj === "undefined")
                return null;
            if (!level)
                level = "d";
            else
                level = level.toLowerCase();
            if (level === "json" || level === "j")
                return JSON.stringify(obj) || null;
            if (typeof obj === "string")
                return obj;
            if (obj === null || level === "string" || level === "s")
                return null;
            if (typeof obj === "number")
                return isNaN(obj) ? null : obj.toString(10);
            if (typeof obj === "symbol")
                return obj.toString();
            if (level === "text" || level === "t")
                return null;
            if (typeof obj === "boolean")
                return obj.toString();
            if (level === "compatible" || level === "c")
                return JSON.stringify(obj) || null;
            if (level === "query" || level === "q") {
                if (obj instanceof Array)
                    return obj.filter(ele => typeof ele !== "undefined").map(ele => encodeURIComponent(Assert.toStr(obj[ele], "c") || "")).join(",");
                return Object.keys(obj).map(ele => `${encodeURIComponent(ele)}=${encodeURIComponent(Assert.toStr(obj[ele], "c") || "")}`).join("&");
            }
            if (level === "url" || level === "u") {
                if (obj instanceof Array)
                    return obj.filter(ele => typeof ele !== "undefined").map(ele => encodeURIComponent(Assert.toStr(obj[ele], "c") || "")).join(",");
                return Object.keys(obj).map(ele => `${encodeURIComponent(ele)}=${encodeURIComponent(Assert.toStr(obj[ele], "q") || "")}`).join("&");
            }
            return obj instanceof Array && obj.length === 1 ? Assert.toStr(obj[0]) : null;
        }
        static isNoNull(input, throwIfFailure = false) {
            var isNull = typeof input === "undefined" || input === null || (typeof input === "number" && isNaN(input));
            if (!isNull)
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new Error("input is null or undefined.") : throwIfFailure;
            return false;
        }
        static isString(input, throwIfFailure = false) {
            var isStr = typeof input === "string";
            if (isStr)
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new Error("input is not a string.") : throwIfFailure;
            return false;
        }
        static isStrOrNull(input, throwIfFailure = false) {
            var isStr = typeof input === "string" || typeof input === "undefined" || input === null;
            if (isStr)
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new TypeError("input is not a string.") : throwIfFailure;
            return false;
        }
        static isValidNumber(input, throwIfFailure = false) {
            var isNum = typeof input === "number" && !isNaN(input);
            if (isNum)
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new TypeError("input is not a valid number.") : throwIfFailure;
            return false;
        }
        static isSafeInteger(input, throwIfFailure = false) {
            if (Number.isSafeInteger(input))
                return true;
            if (throwIfFailure)
                throw throwIfFailure === true ? new TypeError("input is not a safe integer.") : throwIfFailure;
            return false;
        }
    }
    NuScien.Assert = Assert;
})(NuScien || (NuScien = {}));
(function () {
    if (typeof define === 'function') {
        if (define.amd || typeof __webpack_require__ !== "undefined") {
            define(["exports"], function (exports) {
                return NuScien;
            });
        }
    }
    else if (typeof require === "function" && typeof exports === "object" && typeof module === "object") {
        module["exports"] = NuScien;
    }
})();
var NuScien;
(function (NuScien) {
    var _internalModelServices;
    function defineObjectProperty(obj, key, value) {
        Object.defineProperty(obj, key, { value, enumerable: false });
    }
    function defineAccessorProperty(obj, key, getter, setter) {
        Object.defineProperty(obj, key, { get: getter, set: setter, enumerable: false });
    }
    function appendUrl(host, path, path2, query) {
        host = NuScien.Assert.toStr(host, "t");
        path = NuScien.Assert.toStr(path, "t");
        if (!host)
            host = path && (path.indexOf("//") === 0 || path.indexOf("://") > 0) ? path : (typeof location === "undefined" ? "http://localhost" : "/");
        let q = NuScien.Assert.toStr(query, "q");
        if (path) {
            let last = host[host.length - 1];
            let first = path[0];
            if (last === "/" && first === "/")
                host += path.substring(1);
            else if (last !== "/" && first !== "/")
                host += "/" + path;
            else
                host += path;
        }
        if (path2) {
            let last = host[host.length - 1];
            let first = path2[0];
            if (last === "/" && first === "/")
                host += path2.substring(1);
            else if (last !== "/" && first !== "/")
                host += "/" + path2;
            else
                host += path2;
        }
        if (q)
            host += (host.indexOf("?") ? "?" : "&") + (q[0] == "?" || q[0] == "&" ? q.substring(1) : q);
        return host;
    }
    function fetchImpl(path, reqInit) {
        return fetch(path, reqInit);
    }
    class Client {
        constructor(options) {
            _internalModelServices.set(this, {
                reqInit: {}
            });
            if (!options)
                options = {};
            let clientId = NuScien.Assert.toStr(options.clientId, "t");
            let secretKey = NuScien.Assert.toStr(options.secretKey, "t");
            let host = NuScien.Assert.toStr(options.host, "t");
            let pathes = {};
            let onreq = this.onreqinit;
            let getLoginInit = (body) => {
                let s = NuScien.Assert.toStr(body, "u") || "";
                if (secretKey)
                    s = "client_secret=" + encodeURIComponent(secretKey) + "&" + s;
                if (clientId)
                    s = "client_id=" + encodeURIComponent(clientId) + "&" + s;
                let init = {
                    method: "POST",
                    body: s,
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "X-Ns-Client-Type": "js"
                    }
                };
                if (typeof onreq.login === "function")
                    onreq.login(init);
                let resp = fetchImpl(appendUrl(host, pathes.passport || "nuscien5/passport", "login", null), init);
                return resp;
            };
            defineObjectProperty(__classPrivateFieldGet(this, _internalModelServices), "app", {
                id() {
                    return clientId;
                },
                login(body) {
                    return getLoginInit(body);
                },
                logout() {
                    let init = {
                        method: "POST",
                        body: "",
                        headers: {
                            "Content-Type": "application/x-www-form-urlencoded",
                            "X-Ns-Client-Type": "js"
                        }
                    };
                    if (typeof onreq.logout === "function")
                        onreq.logout(init);
                    let resp = fetchImpl(appendUrl(host, pathes.passport || "nuscien5/passport", "logout", null), init);
                    return resp;
                },
                url(path, query) {
                    return appendUrl(path.indexOf("//") === 0 || path.indexOf("://") > 0 ? null : host, path, null, query);
                },
                path(key, value, skipIfExist) {
                    if (!key)
                        return undefined;
                    if (arguments.length > 1) {
                        if (!skipIfExist || !pathes[key]) {
                            if (typeof value === "undefined")
                                delete pathes[key];
                            else
                                pathes[key] = NuScien.Assert.toStr(value);
                        }
                    }
                    return pathes[key];
                },
                pathKeys() {
                    return Object.keys(pathes);
                },
                fetch(path, reqInit) {
                    let onreq = this.onreqinit;
                    if (typeof onreq.all === "function")
                        onreq.all(reqInit);
                    let query = null;
                    let subFolder = null;
                    if (reqInit) {
                        query = reqInit.query;
                        subFolder = reqInit.path;
                    }
                    let url = appendUrl(path.indexOf("//") === 0 || path.indexOf("://") > 0 ? null : host, path, subFolder, query);
                    return fetchImpl(url, reqInit);
                }
            });
        }
        get onreqinit() {
            return __classPrivateFieldGet(this, _internalModelServices).reqInit;
        }
        get login() {
            let m = __classPrivateFieldGet(this, _internalModelServices).login;
            if (m)
                return m;
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            m = ((req) => appInfo.login(req));
            m.alive = () => appInfo.login(undefined);
            m.password = (username, password, options) => appInfo.login(Object.assign({ grant_type: "password", username,
                password }, options));
            m.refreshToken = (refreshToken, options) => appInfo.login(Object.assign({ grant_type: "refresh_token", refresh_token: refreshToken }, options));
            m.authCode = (code, options) => {
                let data = Object.assign({ grant_type: "authorization_code", code }, options);
                if (data.redir && !data.redirect_uri) {
                    data.redirect_uri = data.redir;
                    delete data.redir;
                }
                if (data.verifier && !data.code_verifier) {
                    data.code_verifier = data.verifier;
                    delete data.verifier;
                }
                return appInfo.login(data);
            };
            m.client = (options) => {
                return appInfo.login(Object.assign({ grant_type: "client_credentials" }, options));
            };
            m.logout = () => {
                return appInfo.logout();
            };
            return __classPrivateFieldGet(this, _internalModelServices).login = m;
        }
        get path() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            let p = function (key, value, skipIfExist) {
                if (arguments.length > 1)
                    return appInfo.path(key, value, skipIfExist);
                return appInfo.path(key);
            };
            p.get = (key) => appInfo.path(key);
            p.set = function (key, value, skipIfExist) {
                if (typeof key === "object") {
                    let keys = Object.keys(key);
                    if (typeof skipIfExist === "undefined" || skipIfExist === null)
                        skipIfExist = !!value;
                    for (let k in keys) {
                        appInfo.path(k, keys[k], skipIfExist);
                    }
                    return;
                }
                appInfo.path(key, value, skipIfExist);
            };
            p.remove = function (key) {
                if (key && key instanceof Array) {
                    for (let item in key) {
                        appInfo.path(item, undefined);
                    }
                    return;
                }
                appInfo.path(key, undefined);
            };
            p.keys = () => appInfo.pathKeys();
            return p;
        }
        get user() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            let m = __classPrivateFieldGet(this, _internalModelServices).user;
            if (m)
                return m;
            m = function (id) {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "user/" + (NuScien.Assert.toStr(id, "t") || "0")
                });
            };
            m.exist = (id) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "user/exist" + (NuScien.Assert.toStr(id, "t") || "0")
                });
            };
            return __classPrivateFieldGet(this, _internalModelServices).user = m;
        }
        logout() {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            return appInfo.logout();
        }
        url(path, query) {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            return appInfo.url(path, query);
        }
        fetch(path, reqInit) {
            let appInfo = __classPrivateFieldGet(this, _internalModelServices).app;
            if (typeof this.onreqinit.fetch === "function")
                this.onreqinit.fetch(reqInit);
            return appInfo.fetch(path, reqInit);
        }
        resProvider(path) {
            return new ResourceEntityProvider(__classPrivateFieldGet(this, _internalModelServices).app, path);
        }
    }
    _internalModelServices = new WeakMap();
    NuScien.Client = Client;
    class ResourceEntityProvider {
        constructor(appInfo, path) {
            this.appInfo = appInfo;
            this.path = path;
        }
        search(q) {
            return this.appInfo.fetch(this.path, {
                method: "GET",
                query: q
            });
        }
        get(id) {
            return this.appInfo.fetch(this.path, {
                method: "GET",
                path: "e/" + (NuScien.Assert.toStr(id, "t") || "0")
            });
        }
        save(value, id) {
            NuScien.Assert.isNoNull(value, true);
            if (id) {
                id = NuScien.Assert.toStr(id, "t");
                if (id)
                    return this.appInfo.fetch(this.path, {
                        method: "PUT",
                        body: JSON.stringify(value),
                        path: "e/" + (NuScien.Assert.toStr(id, "t") || "0"),
                        headers: {
                            "Content-Type": "application/json",
                            "X-Ns-Client-Type": "js"
                        }
                    });
            }
            return this.appInfo.fetch(this.path, {
                method: "PUT",
                body: JSON.stringify(value),
                headers: {
                    "Content-Type": "application/json",
                    "X-Ns-Client-Type": "js"
                }
            });
        }
        delete(id) {
            return this.appInfo.fetch(this.path, {
                method: "DELETE",
                path: "e/" + (NuScien.Assert.toStr(id, "t") || "0")
            });
        }
        fetch(subPath, reqInit) {
            let url = this.path || subPath;
            if (url && subPath) {
                if (url[url.length - 1] === "/" && subPath[0] === "/")
                    url += subPath.substring(1);
                else if (url[url.length - 1] !== "/" && subPath[0] !== "/")
                    url += "/" + subPath;
                else
                    url += subPath;
            }
            return this.appInfo.fetch(url, reqInit);
        }
    }
    NuScien.ResourceEntityProvider = ResourceEntityProvider;
})(NuScien || (NuScien = {}));
//# sourceMappingURL=core.js.map