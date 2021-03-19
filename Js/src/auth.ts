/// <reference path="./main.ts" />

namespace NuScien {

    interface InternalClientContract {
        (): string;
        id(): string;
        clientType(): string;
        login(body: any): Promise<GenericWebResponseContract<TokenResponseContract>>;
        logout(): Promise<GenericWebResponseContract<TokenResponseContract>>;
        url(path: string, query?: any): string;
        path(key: string, value?: string, skipIfExist?: boolean): string;
        pathKeys(): string[];
        fetch<T = any>(path: string, reqInit?: RequestOptions): Promise<GenericWebResponseContract<T>>;
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

    export interface ClientOptionsContact {

        /**
         * The URI host.
         */
        host?: string;

        /**
         * The client identifier.
         */
        clientId: string;

        /**
         * The secret key of the client.
         */
        secretKey?: string;
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
         * @param options  The options containing the client information.
         */
        public constructor(options: ClientOptionsContact) {
            if (!options) options = {} as any;
            let clientId = Assert.toStr(options.clientId, "t") || "webjsclient";
            let secretKey = Assert.toStr(options.secretKey, "t");
            let host = Assert.toStr(options.host, "t");
            let pathes: any = {
            };
            let onreq = this.onreqinit;
            let clientTypeStr = `jssdk; ${ver}; fetch; ${encodeURIComponent(clientId)};`;
            let getLoginInit = (body: any) => {
                let s = Assert.toStr(body, "u") || "";
                if (secretKey) s = "client_secret=" + encodeURIComponent(secretKey) + "&" + s;
                if (clientId) s = "client_id=" + encodeURIComponent(clientId) + "&" + s;
                let init: RequestInit = {
                    method: "POST",
                    body: s,
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded",
                        "X-Ns-Client-Type": clientTypeStr
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
                clientType() {
                    return clientTypeStr;
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
                            "X-Ns-Client-Type": clientTypeStr
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
             * Sets an authentication code for current user.
             * @param serviceProvider  The service provider.
             * @param code  The authentication code.
             * @param insert  true if inserts; otherwise, false.
             */
            setAuthCode(serviceProvider: string, code: string, insert?: boolean): Promise<ChangingWebResponseContract>;

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
            m.setAuthCode = (serviceProvider: string, code: string, insert?: boolean) => {
                let body = "code=" + encodeURIComponent(Assert.toStr(code, "t"));
                if (insert) body += "&insert=true";
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "authcode/" + serviceProvider,
                    body: body,
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    }
                });
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
            /**
             * Gets a specific user by identifier.
             * @param id  The user identifier.
             */
            (id: string): Promise<GenericWebResponseContract<UserEntityContract>>;

            /**
             * Tests if a specific user does exist.
             * @param name  The logname.
             */
            exist(id: string): Promise<Response>;

            /**
             * Searches users joined in a specific group.
             * @param id  The user group identifier.
             */
            searchByGroup(id: string): Promise<CollectionWebResponseContract<UserEntityContract>>;

            /**
             * Adds or updates a user entity.
             * @param entity  The entity to save.
             */
            save(entity: UserEntityContract): Promise<ChangingWebResponseContract>;

            /**
             * Updates a user entity.
             * @param id  The user identifier.
             * @param data  The entity data to delta update.
             */
            update(id: string, data: any): Promise<ChangingWebResponseContract>;

            /**
             * Lists the user group relationship entities for current user.
             * @param q  The optional query arguments to search.
             */
            rela(q?: {
                name?: string;
                state?: ResourceEntityStateValue;
            }): Promise<UserGroupRelaEntityContract>;

            /**
             * Saves the user group relationship.
             * @param value  The entity to save.
             */
            saveRela(value: UserGroupRelaEntityContract): Promise<ChangingWebResponseContract>;
        
            /**
             * Gets a specific contact by identifier.
             * @param id  The contact identifier.
             */
            contact(id: string): Promise<GenericWebResponseContract<ContactEntityContract>>;

            /**
             * Searches contacts joined in a specific group.
             * @param id  The contact identifier.
             */
            contacts(q: QueryArgsContract): Promise<CollectionWebResponseContract<ContactEntityContract>>;
 
            /**
             * Adds or updates a contact entity.
             * @param entity  The entity to save.
             */
            saveContact(entity: ContactEntityContract): Promise<ChangingWebResponseContract>;

            /**
             * Updates a contact entity.
             * @param id  The contact identifier.
             * @param data  The entity data to delta update.
             */
            updateContact(id: string, data: any): Promise<ChangingWebResponseContract>;

        } {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            let m = this.#internalModelServices.user;
            if (m) return m;
            m = function (id: string) {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "users/e/" + (Assert.toStr(id, "t") || "0")
                });
            };
            m.exist = (name: string) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "POST",
                    path: "users/exist",
                    body: "logname=" + encodeURIComponent(Assert.toStr(name, "t")),
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    }
                });
            };
            m.searchByGroup = (id: string) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "users/group/" + (Assert.toStr(id, "t") || "0")
                });
            };
            m.save = (entity: UserEntityContract) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "users/e",
                    body: JSON.stringify(entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.update = (id: string, data: any) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "users/e/" + (Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.rela = (q?: any) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "rela",
                    query: q
                });
            };
            m.saveRela = (data: any) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "rela",
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.contact = (id: string) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "contact/" + (Assert.toStr(id, "t") || "0")
                });
            };
            m.contacts = (q: QueryArgsContract) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "contact",
                    query: q
                });
            };
            m.saveContact = (entity: ContactEntityContract) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "contact",
                    body: JSON.stringify(entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.updateContact = (id: string, data: any) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "contact/" + (Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };

            return this.#internalModelServices.user = m;
        }

        /**
         * User group resources.
         */
        public get group(): {
            /**
             * Gets a specific use groupr by identifier.
             * @param id  The user group identifier.
             */
            (id: string): Promise<GenericWebResponseContract<UserGroupEntityContract>>;

            /**
             * Searches user groups joined in a specific group.
             * @param id  The user group identifier.
             */
            list(q: QueryArgsContract): Promise<CollectionWebResponseContract<UserGroupEntityContract>>;

            /**
             * Adds or updates a user group entity.
             * @param entity  The entity to save.
             */
            save(entity: UserGroupEntityContract): Promise<ChangingWebResponseContract>;

            /**
             * Updates a user group entity.
             * @param id  The user group identifier.
             * @param data  The entity data to delta update.
             */
            update(id: string, data: any): Promise<ChangingWebResponseContract>;

            /**
             * Gets the user group relationship entity.
             * @param group  The user group identifer.
             * @param user  The optional user identifier; or null, if gets for the current user.
             */
            rela(group: string, user?: string): Promise<UserGroupRelaEntityContract>;
        } {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            let m = this.#internalModelServices.group;
            if (m) return m;
            m = function (id: string) {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "groups/e/" + (Assert.toStr(id, "t") || "0")
                });
            };
            m.list = (q: QueryArgsContract) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: "groups",
                    query: q
                });
            };
            m.save = (entity: UserEntityContract) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "groups/e",
                    body: JSON.stringify(entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.update = (id: string, data: any) => {
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "PUT",
                    path: "groups/e/" + (Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.rela = (group: string, user?: string) => {
                let path = "rela/g/" + group;
                if (user) path += "/" + user;
                return appInfo.fetch(appInfo.path("passport") || "nuscien5/passport", {
                    method: "GET",
                    path: path
                });
            };

            return this.#internalModelServices.group = m;
        }

        /**
         * Settings.
         */
         public get settings(): {
            /**
             * Gets the settings.
             * @param key  The settings key.
             * @param site  The optional site identifier; or null, for global.
             */
            (key: string, site?: string): Promise<GenericWebResponseContract<any>>;

            /**
             * Gets the settings for the specific site.
             * @param site  The optional site identifier; or null, for global.
             * @param key  The settings key.
             */
            site(site: string, key: string): Promise<GenericWebResponseContract<any>>;

            /**
             * Saves the settings.
             * @param key  The settings key.
             * @param site  The optional site identifier; or null, for global.
             * @param value  The settings data.
             */
            setGlobal(key: string, value: any): Promise<ChangingWebResponseContract>;

            /**
             * Saves the settings.
             * @param site  The optional site identifier; or null, for global.
             * @param key  The settings key.
             */
            setSite(site: string, key: string, value: any): Promise<ChangingWebResponseContract>;

            /**
             * Saves the permissions.
             * @param site  The site identifier.
             * @param targetType  The type of the target resource.
             * @param targetId  The identifer of the target resource.
             */
            getPerm(site: string, targetType: "user" | "group" | "client", targetId: string): Promise<CollectionWebResponseContract<string>>;

            /**
             * Saves the permissions.
             * @param site  The site identifier.
             * @param targetType  The type of the target resource.
             * @param targetId  The identifer of the target resource.
             */
            setPerm(site: string, targetType: "user" | "group" | "client", targetId: string, value: {
                permissions: string[];
                [property: string]: any;
            }): Promise<ChangingWebResponseContract>;
        } {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            let m = this.#internalModelServices.settings;
            if (m) return m;
            m = function (key: string, site?: string) {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "GET",
                    path: site ? `global/${key}` : `site/${site}/${key}`
                });
            };
            m.site = (site: string, key: string) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "GET",
                    path: site ? `global/${key}` : `site/${site}/${key}`
                });
            };
            m.setGlobal = (key: string, value: any) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "PUT",
                    path: "global/" + key,
                    body: JSON.stringify(value),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.setSite = (site: string, key: string, value: any) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "PUT",
                    path: site ? `global/${key}` : `site/${site}/${key}`,
                    body: JSON.stringify(value),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.getPerm = (site: string, targetType: "user" | "group" | "client", targetId: string) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "GET",
                    path: `perms/${site}/${targetType}/${targetId}`
                });
            };
            m.setPerm = (site: string, targetType: "user" | "group" | "client", targetId: string, value: any) => {
                return appInfo.fetch(appInfo.path("settings") || "nuscien5/settings", {
                    method: "PUT",
                    path: `perms/${site}/${targetType}/${targetId}`,
                    body: JSON.stringify(value),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };

            return this.#internalModelServices.settings = m;
        }

        /**
         * CMS.
         */
         public get cms(): {
            (id: string): Promise<GenericWebResponseContract<ContentEntityContract>>;

            /**
             * Searches publish content.
             * @param id  The publish content identifier.
             */
            list(id: string, q?: QueryArgsContract): Promise<CollectionWebResponseContract<ContentEntityContract>>;

            /**
             * Adds or updates a publish content entity.
             * @param entity  The entity to save.
             */
            save(entity: ContentEntityContract, message?: string): Promise<ChangingWebResponseContract>;
 
            /**
             * Updates a publish content entity.
             * @param id  The publish content identifier.
             * @param data  The entity data to delta update.
             */
            update(id: string, data: any, message?: string): Promise<ChangingWebResponseContract>;

            /**
             * Lists the revisions.
             * @param parent  The parent identifier.
             * @param q  The optional query.
             */
            revs(parent: string, q?: QueryArgsContract): Promise<CollectionWebResponseContract<ContentEntityContract>>;

             /**
              * Gets a specific revisions.
              * @param id  The identifier of the revision.
              */
            rev(id: string): Promise<CollectionWebResponseContract<ContentEntityContract>>;

            /**
             * Gets a specific template of publish content.
             * @param id  The publish content template identifier.
             */
            template(id: string): Promise<CollectionWebResponseContract<ContentTemplateEntityContract>>;

            /**
             * Searches template of publish content.
             * @param q  The optional query.
             */
            templates(q: QueryArgsContract): Promise<CollectionWebResponseContract<ContentTemplateEntityContract>>;
            
            /**
             * Adds or updates a publish content template entity.
             * @param entity  The entity to save.
             */
            saveTemplate(entity: ContentTemplateEntityContract, message?: string): Promise<ChangingWebResponseContract>;

            /**
             * Updates a publish content template entity.
             * @param id  The publish content identifier.
             * @param data  The entity data to delta update.
             */
            updateTemplate(id: string, data: any, message?: string): Promise<ChangingWebResponseContract>;

            /**
             * Lists the revisions.
             * @param parent  The parent identifier.
             * @param q  The optional query.
             */
            templateRevs(parent: string, q?: QueryArgsContract): Promise<CollectionWebResponseContract<ContentTemplateEntityContract>>;
 
            /**
             * Gets a specific revisions.
             * @param id  The identifier of the revision.
             */
            templateRev(id: string): Promise<CollectionWebResponseContract<ContentTemplateEntityContract>>;
 
            /**
             * Gets a specific comment of publish content.
             * @param id  The publish content comment identifier.
             */
            comment(id: string): Promise<CollectionWebResponseContract<CommentEntityContract>>;

             /**
              * Searches comment of publish content.
             * @param parent  The parent content identifier.
              */
            comments(content: string, plain?: boolean): Promise<CollectionWebResponseContract<CommentEntityContract>>;

            /**
             * Searches child comment of publish content.
             * @param parent  The parent comment identifier.
             */
            childComments(parent: string): Promise<CollectionWebResponseContract<CommentEntityContract>>;
            
            /**
             * Adds or updates a publish content comment entity.
             * @param entity  The entity to save.
             */
            saveComment(entity: CommentEntityContract): Promise<ChangingWebResponseContract>;

            /**
             * Updates a publish content comment entity.
             * @param id  The publish content identifier.
             * @param data  The entity data to delta update.
             */
            updateComment(id: string, data: any): Promise<ChangingWebResponseContract>;

            /**
             * Deletes a publish content comment entity.
             * @param id  The publish content identifier.
             */
            deleteComment(id: string): Promise<ChangingWebResponseContract>; 
        } {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            let m = this.#internalModelServices.cms;
            if (m) return m;
            m = function (id: string) {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "c/" + id
                });
            };
            m.list = (id: string, q?: QueryArgsContract) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "c",
                    query: q ? { ...q, parent: id } : { parent: id }
                });
            };
            m.save = (entity: ContentEntityContract, message?: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "c",
                    body: JSON.stringify(message && entity ? { ...entity, message } : entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.update = (id: string, data: any, message?: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "c/" + (Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(message && data ? { ...data, message } : data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.revs = (parent: string, q?: QueryArgsContract) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "c/" + parent + "/rev",
                    query: q
                });
            };
            m.rev = (id: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "cr/" + id
                });
            };
            m.template = (id: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "t/" + id
                });
            };
            m.templates = (q?: QueryArgsContract) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "t",
                    query: q
                });
            };
            m.saveTemplate = (entity: ContentTemplateEntityContract, message?: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "t",
                    body: JSON.stringify(message && entity ? { ...entity, message } : entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.updateTemplate = (id: string, data: any, message?: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "t/" + (Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(message && data ? { ...data, message } : data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.templateRevs = (parent: string, q?: QueryArgsContract) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "t/" + parent + "/rev",
                    query: q
                });
            };
            m.templateRev = (id: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "tr/" + id
                });
            };
            m.comment = (id: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: "cc/" + id
                });
            };
            m.comments = (content: string, plain?: boolean) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: `c/${content}/comments`,
                    query: plain ? { plain: true } : null
                });
            };
            m.childComments = (parent: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "GET",
                    path: `cc/${parent}/children`
                });
            };
            m.saveComment = (entity: CommentEntityContract) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "cc",
                    body: JSON.stringify(entity),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.updateComment = (id: string, data: any) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "PUT",
                    path: "cc/" + (Assert.toStr(id, "t") || "0"),
                    body: JSON.stringify(data),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            };
            m.deleteComment = (id: string) => {
                return appInfo.fetch(appInfo.path("cms") || "nuscien5/cms", {
                    method: "DELETE",
                    path: "cc/" + (Assert.toStr(id, "t") || "0")
                });
            };

            return this.#internalModelServices.cms = m;
        }

        /**
         * User activities.
         */
         public get activities(): {

        } {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            let m = this.#internalModelServices.activities;
            if (m) return m;
            m = function () {
            };

            return this.#internalModelServices.activities = m;
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
        public fetch<T = any>(path: string, reqInit?: RequestOptions) {
            let appInfo: InternalClientContract = this.#internalModelServices.app;
            if (typeof this.onreqinit.fetch === "function") this.onreqinit.fetch(reqInit);
            return appInfo.fetch<T>(path, reqInit);
        }

        /**
         * Gets the resource entity provider.
         * @param path  The relative root path.
         */
        public resProvider<T = any>(path: string) {
            return new ResourceEntityProvider<T>(this.#internalModelServices.app, path);
        }
    }

    /**
     * The resource entity provider.
     */
    export class ResourceEntityProvider<TEntity> {
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
            return this.appInfo.fetch<CollectionResultContract<TEntity>>(this.path, {
                method: "GET",
                query: q
            });
        }

        /**
         * Gets a resource entity.
         * @param id  The entity identifier.
         */
        get(id: string) {
            return this.appInfo.fetch<TEntity>(this.path, {
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
                if (id) return this.appInfo.fetch<ChangingResultContract>(this.path, {
                    method: "PUT",
                    body: JSON.stringify(value),
                    path: "e/" + (Assert.toStr(id, "t") || "0"),
                    headers: {
                        "Content-Type": "application/json"
                    }
                });
            }

            return this.appInfo.fetch<ChangingResultContract>(this.path, {
                method: "PUT",
                body: JSON.stringify(value),
                headers: {
                    "Content-Type": "application/json"
                }
            });
        }

        /**
         * Deletes a specific resource entity.
         * @param id  The entity identifier.
         */
        delete(id: string) {
            return this.appInfo.fetch<ChangingResultContract>(this.path, {
                method: "DELETE",
                path: "e/" + (Assert.toStr(id, "t") || "0")
            });
        }

        /**
         * Sends request to service and gets response.
         * @param path  The relative path.
         * @param reqInit  The options.
         */
        fetch<T>(subPath: string, reqInit: RequestOptions) {
            let url = this.path || subPath;
            if (url && subPath) {
                if (url[url.length - 1] === "/" && subPath[0] === "/") url += subPath.substring(1);
                else if (url[url.length - 1] !== "/" && subPath[0] !== "/") url += "/" + subPath;
                else url += subPath;
            }

            return this.appInfo.fetch<T>(url, reqInit);
        }
    }
}
