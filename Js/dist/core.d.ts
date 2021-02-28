/**
 * NuScien core library for front-end (web client).
 * https://github.com/nuscien/trivial
 * Copyright (c) 2020 Kingcean Tuan. All rights reserved.
 */
declare namespace NuScien {
    class Assert {
        /**
         * Converts to string. Or returns null for unsupported type.
         * @param obj  The source object.
         * @param level  The options.
         *  - "default" or null, to convert the object to string in normal way;
         *  - "text" to convert string, number or symbol only;
         *  - "compatible" to convert basic types and stringify object and array;
         *  - "json" to stringify in JSON format;
         *  - "query" to stringify in URI query component format;
         *  - "url" to stringify in URI query format;
         *  - "string" to pass only for string.
         */
        static toStr(obj: any, level?: undefined | "d" | "default" | "DEFAULT" | "t" | "text" | "TEXT" | "c" | "compatible" | "COMPATIBLE" | "j" | "json" | "JSON" | "q" | "query" | "QUERY" | "u" | "url" | "URL" | "s" | "string" | "STRING"): string | null;
        static isNoNull(input: any, throwIfFailure?: boolean | string): boolean;
        static isString(input: any, throwIfFailure?: boolean | string): input is string;
        static isStrOrNull(input: any, throwIfFailure?: boolean | string): input is string | null | undefined;
        static isValidNumber(input: any, throwIfFailure?: boolean | string): input is number;
        static isSafeInteger(input: any, throwIfFailure?: boolean | string): input is number;
    }
}
declare namespace NuScien {
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
    /**
     * The resource access client.
     */
    export class Client {
        #private;
        /**
         * Initializes a new instance of the Client class.
         * @param host  The URI host.
         * @param appId  The client identifier.
         * @param secretKey  The secret key of the client.
         */
        constructor(host: string, clientId: string, secretKey?: string);
        get onreqinit(): {
            login: (init: RequestInit) => void;
            logout: (init: RequestInit) => void;
            fetch: (init: RequestOptions) => void;
            all: (init: RequestOptions) => void;
        };
        /**
         * Signs in.
         */
        get login(): {
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
                ldap?: string;
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
        };
        /**
         * Gets a specific path.
         */
        get path(): {
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
        };
        /**
         * User resources.
         */
        get user(): {
            (id: string): Promise<Response>;
            exist(id: string): Promise<Response>;
        };
        /**
         * Signs out.
         */
        logout(): Promise<Response>;
        /**
         * Gets a URL.
         * @param path  The relative path.
         * @param query  The query data.
         */
        url(path: string, query?: any): string;
        /**
         * Sends request to service and gets response.
         * @param path  The relative path.
         * @param reqInit  The options.
         */
        fetch(path: string, reqInit?: RequestOptions): Promise<Response>;
        /**
         * Gets the resource entity provider.
         * @param path  The relative root path.
         */
        resProvider(path: string): ResourceEntityProvider;
    }
    /**
     * The resource entity provider.
     */
    export class ResourceEntityProvider {
        readonly appInfo: InternalClientContract;
        readonly path: string;
        /**
         * Initializes a new instance of the ResourceEntityProvider class.
         * @param appInfo  The client proxy.
         * @param path  The relative root path.
         */
        constructor(appInfo: InternalClientContract, path: string);
        /**
         * Searches.
         * @param q  The query.
         */
        search(q: any): Promise<Response>;
        /**
         * Gets a resource entity.
         * @param id  The entity identifier.
         */
        get(id: string): Promise<Response>;
        /**
         * Creates or updates a specific resource entity.
         * @param value  The entity to save or the content to delta update.
         * @param id  The optional entity identifier. Only set this parameter when need delta update.
         */
        save(value: any, id?: string): Promise<Response>;
        /**
         * Deletes a specific resource entity.
         * @param id  The entity identifier.
         */
        delete(id: string): Promise<Response>;
        /**
         * Sends request to service and gets response.
         * @param path  The relative path.
         * @param reqInit  The options.
         */
        fetch(subPath: string, reqInit: RequestOptions): Promise<Response>;
    }
    export {};
}
