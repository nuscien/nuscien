namespace NuScien {

    interface InternalClientContract {
        (): string;
        id(): string;
        login(body: any): Promise<Response>;
        logout(): Promise<Response>;
        url(path: string, query?: any): string;
        path(key: string, value?: string, skipIfExist?: boolean): string;
        pathKeys(): string[];
        fetch(path: string, reqInit?: RequestOptions): Promise<Response>;
    }

    export interface LoginOptionsContract {
        scope?: string | string[] | null;
    }

    export interface RequestOptions extends RequestInit {
        query?: any;
        path?: string;
        reason?: string;
    }

    function defineObjectProperty(obj: any, key: string, value: any) {
        Object.defineProperty(obj, key, { value, enumerable: false });
    }

    function defineAccessorProperty<T>(obj: any, key: string, getter: () => T, setter: (value: T) => void) {
        Object.defineProperty(obj, key, { get: getter, set: setter, enumerable: false });
    }

    function appendUrl(host: string, path: string, path2: string, query: any) {
        host = Assert.toStr(host, "t");
        path = Assert.toStr(path, "t");
        if (!host) host = path && (path.indexOf("//") === 0 || path.indexOf("://") > 0) ? path : (typeof location === "undefined" ? "http://localhost" : "/");
        let q = Assert.toStr(query, "q");
        if (path) {
            let last = host[host.length - 1];
            let first = path[0];
            if (last === "/" && first === "/") host += path.substring(1);
            else if (last !== "/" && first !== "/") host += "/" + path;
            else host += path;
        }

        if (path2) {
            let last = host[host.length - 1];
            let first = path2[0];
            if (last === "/" && first === "/") host += path2.substring(1);
            else if (last !== "/" && first !== "/") host += "/" + path2;
            else host += path2;
        }

        if (q) host += (host.indexOf("?") ? "?" : "&") + (q[0] == "?" || q[0] == "&" ? q.substring(1) : q);
        return host;
    }

    function fetchImpl(path: string, reqInit: RequestInit) {
        return fetch(path, reqInit);
    }

    /**
     * The resource access client.
     */
    export class Client {
        #internalModelServices: any = {
            reqInit: {}
        };

        /**
         * Initializes a new instance of the Client class.
         * @param host  The URI host.
         * @param appId  The client identifier.
         * @param secretKey  The secret key of the client.
         */
        public constructor(host: string, clientId: string, secretKey?: string) {
            clientId = Assert.toStr(clientId, "t");
            secretKey = Assert.toStr(secretKey, "t");
            host = Assert.toStr(host, "t");
            let pathes: any = {
            };
            let onreq = this.onreqinit;
            let getLoginInit = (body: any) => {
                let s = Assert.toStr(body, "u") || "";
                if (secretKey) s = "client_secret=" + encodeURIComponent(secretKey) + s;
                if (clientId) s = "client_id=" + encodeURIComponent(clientId) + s;
                let init: RequestInit = {
                    method: "POST",
                    body: s,
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "X-Ns-Client-Type": "js"
                    }
                };
                if (typeof onreq.login === "function") onreq.login(init);
                let resp = fetchImpl(appendUrl(host, pathes.passport || "nuscien5/passport", "login", null), init);
                return resp;
            };
            defineObjectProperty(this.#internalModelServices, "app", {
                id() {
                    return clientId
                },
                login(body: any) {
                    return getLoginInit(body);
                },
                logout() {
                    let init: RequestInit = {
                        method: "POST",
                        body: "",
                        headers: {
                            "Content-Type": "application/x-www-form-urlencoded",
                            "X-Ns-Client-Type": "js"
                        }
                    };
                    if (typeof onreq.logout === "function") onreq.logout(init);
                    let resp = fetchImpl(appendUrl(host, pathes.passport || "nuscien5/passport", "logout", null), init);
                    return resp;
                },
                url(path: string, query?: any) {
                    return appendUrl(path.indexOf("//") === 0 || path.indexOf("://") > 0 ? null : host, path, null, query);
                },
                path(key: string, value?: string, skipIfExist?: boolean) {
                    if (!key) return undefined;
                    if (arguments.length > 1) {
                        if (!skipIfExist || !pathes[key]) {
                            if (typeof value === "undefined") delete pathes[key];
                            else pathes[key] = Assert.toStr(value);
                        }
                    }

                    return pathes[key];
                },
                pathKeys() {
                    return Object.keys(pathes);
                },
                fetch(path: string, reqInit?: RequestOptions) {
                    let onreq = this.onreqinit;
                    if (typeof onreq.all === "function") onreq.all(reqInit);
                    let query: any = null;
                    let subFolder: string = null;
                    if (reqInit) {
                        query = reqInit.query;
                        subFolder = reqInit.path;
                        //if (query || subFolder) {
                        //    let reqInit2 = { ...reqInit };
                        //    if (reqInit.query === query && reqInit.path === subFolder) {
                        //        reqInit = reqInit2;
                        //        delete reqInit.query;
                        //        delete reqInit.path;
                        //    }
                        //}
                    }

                    let url = appendUrl(path.indexOf("//") === 0 || path.indexOf("://") > 0 ? null : host, path, subFolder, query);
                    return fetchImpl(url, reqInit);
                }
            } as InternalClientContract);
        }

        public get onreqinit(): {
            login: (init: RequestInit) => void;
            logout: (init: RequestInit) => void;
            fetch: (init: RequestOptions) => void;
            all: (init: RequestOptions) => void;
        } {
            return this.#internalModelServices.reqInit;
        }

        /**
         * Signs in.
         */
        public get login(): {


            /**
             * Signs in.
             * @param req  The request token body.
             */
            (req: any): Promise<Response>;

            /**
             * Sends an authentication request for current user.
             */
            alive(): Promise<Response>;

            /**
             * Signs in by username and password.
             * @param username  The username.
             * @param password  The password.
             * @param options  The additional options for login.
             */
            password(username: string, password: string, options?: LoginOptionsContract & {
                /**
                 * The LDAP.
                 */
                ldap?: string
            }): Promise<Response>;

            /**
             * Signs in by refresh token.
             * @param refreshToken  The refresh token.
             * @param options  The additional options for login.
             */
            refreshToken(refreshToken: string, options?: LoginOptionsContract): Promise<Response>;

            /**
             * Signs in by authentication code.
             * @param code  The authentication code.
             * @param options  The additional options for login.
             */
            authCode(code: string, options?: LoginOptionsContract & {
                /**
                 * The redirect URI.
                 */
                redirect_uri?: string;
                redir?: string;

                /**
                 * The code verifier.
                 */
                code_verifier?: string;
                verifier?: string;

                /**
                 * The service provider.
                 */
                provider?: string;
            }): Promise<Response>;

            /**
             * Signs in by client credentials.
             * @param options  The additional options for login.
             */
            client(options?: LoginOptionsContract): Promise<Response>;

            /**
             * Signs out.
             */
            logout(): Promise<Response>;
        } {
            let m = this.#internalModelServices.login;
            if (m) return m;
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            m = ((req: any) => appInfo.login(req)) as any;
            m.alive = () => appInfo.login(undefined);
            m.password = (username: string, password: string, options?: LoginOptionsContract) => appInfo.login({
                grant_type: "password",
                username,
                password,
                ...options
            });
            m.refreshToken = (refreshToken: string, options?: LoginOptionsContract) => appInfo.login({
                grant_type: "refresh_token",
                refresh_token: refreshToken,
                ...options
            });
            m.authCode = (code: string, options?: any) => {
                let data = { grant_type: "authorization_code", code, ...options };
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
            m.client = (options?: LoginOptionsContract) => {
                return appInfo.login({ grant_type: "client_credentials", ...options });
            };
            m.logout = () => {
                return appInfo.logout();
            };
            return this.#internalModelServices.login = m;
        }

        /**
         * Gets a specific path.
         */
        public get path(): {

            /**
             * Gets or sets a specific path.
             * @param key  The path key.
             * @param value  The optional value to set.
             * @param skipIfExist  true if only set the value when it does not exist; otherwise, false.
             */
            (key: string | "passport" | "settings" | "cms" | "sns", value?: string, skipIfExist?: boolean): string;

            /**
             * Gets a specific path.
             * @param key  The path key.
             */
            get(key: string | "passport" | "settings" | "cms" | "sns"): string;

            /**
             * Sets a specific path.
             * @param key  The path key.
             * @param value  The optional value to set.
             * @param skipIfExist  true if only set the value when it does not exist; otherwise, false.
             */
            set(key: string | "passport" | "settings" | "cms" | "sns", value: string, skipIfExist?: boolean): void;

            /**
             * Sets a number of path.
             * @param obj  The dictionary of path key and value.
             * @param skipIfExist  true if only set the value when it does not exist; otherwise, false.
             */
            set(obj: any, skipIfExist?: boolean): void;

            /**
             * Removes a specific path.
             * @param key  The path key or a set of keys.
             */
            remove(key: string | string[]): void;

            /**
             * Gets key list of path registered.
             */
            keys(): string[];
        } {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            let p = function (key: string, value?: string, skipIfExist?: boolean) {
                if (arguments.length > 1) return appInfo.path(key, value, skipIfExist);
                return appInfo.path(key);
            } as any;
            p.get = (key: string) => appInfo.path(key);
            p.set = function (key: string | any, value: string, skipIfExist?: boolean) {
                if (typeof key === "object") {
                    let keys = Object.keys(key);
                    if (typeof skipIfExist === "undefined" || skipIfExist === null) skipIfExist = !!value;
                    for (let k in keys) {
                        appInfo.path(k, keys[k], skipIfExist);
                    }

                    return;
                }

                appInfo.path(key, value, skipIfExist);
            };
            p.remove = function (key: string | string[]) {
                if (key && key instanceof Array) {
                    for (let item in key) {
                        appInfo.path(item, undefined);
                    }

                    return;
                }

                appInfo.path(key as string, undefined);
            };
            p.keys = () => appInfo.pathKeys();
            return p;
        }

        /**
         * User resources.
         */
        public get user(): {
            (id: string): Promise<Response>;
            exist(id: string): Promise<Response>;
        } {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            let m = this.#internalModelServices.user;
            if (m) return m;
            m = function (id: string) {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "user/" + (Assert.toStr(id, "t") || "0")
                });
            };
            m.exist = (id: string) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "user/exist" + (Assert.toStr(id, "t") || "0")
                });
            }
            return this.#internalModelServices.user = m;
        }

        /**
         * Signs out.
         */
        public logout() {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            return appInfo.logout();
        }

        /**
         * Gets a URL.
         * @param path  The relative path.
         * @param query  The query data.
         */
        public url(path: string, query?: any) {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            return appInfo.url(path, query);
        }

        /**
         * Sends request to service and gets response.
         * @param path  The relative path.
         * @param reqInit  The options.
         */
        public fetch(path: string, reqInit?: RequestOptions) {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            if (typeof this.onreqinit.fetch === "function") this.onreqinit.fetch(reqInit);
            return appInfo.fetch(path, reqInit);
        }

        /**
         * Gets the resource entity provider.
         * @param path  The relative root path.
         */
        public resProvider(path: string) {
            return new ResourceEntityProvider(this.#internalModelServices.app, path);
        }
    }

    /**
     * The resource entity provider.
     */
    export class ResourceEntityProvider {
        /**
         * Initializes a new instance of the ResourceEntityProvider class.
         * @param appInfo  The client proxy.
         * @param path  The relative root path.
         */
        constructor(readonly appInfo: InternalClientContract, readonly path: string) {}

        /**
         * Searches.
         * @param q  The query.
         */
        search(q: any) {
            return this.appInfo.fetch(this.path, {
                method: "GET",
                query: q
            });
        }

        /**
         * Gets a resource entity.
         * @param id  The entity identifier.
         */
        get(id: string) {
            return this.appInfo.fetch(this.path, {
                method: "GET",
                path: "e/" + (Assert.toStr(id, "t") || "0")
            });
        }

        /**
         * Creates or updates a specific resource entity.
         * @param value  The entity to save or the content to delta update.
         * @param id  The optional entity identifier. Only set this parameter when need delta update.
         */
        save(value: any, id?: string) {
            Assert.isNoNull(value, true);
            if (id) {
                id = Assert.toStr(id, "t");
                if (id) return this.appInfo.fetch(this.path, {
                    method: "PUT",
                    body: JSON.stringify(value),
                    path: "e/" + (Assert.toStr(id, "t") || "0"),
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

        /**
         * Deletes a specific resource entity.
         * @param id  The entity identifier.
         */
        delete(id: string) {
            return this.appInfo.fetch(this.path, {
                method: "DELETE",
                path: "e/" + (Assert.toStr(id, "t") || "0")
            });
        }

        /**
         * Sends request to service and gets response.
         * @param path  The relative path.
         * @param reqInit  The options.
         */
        fetch(subPath: string, reqInit: RequestOptions) {
            let url = this.path || subPath;
            if (url && subPath) {
                if (url[url.length - 1] === "/" && subPath[0] === "/") url += subPath.substring(1);
                else if (url[url.length - 1] !== "/" && subPath[0] !== "/") url += "/" + subPath;
                else url += subPath;
            }

            return this.appInfo.fetch(url, reqInit);
        }
    }
}
