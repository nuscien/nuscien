declare namespace NuScien {
    class Assert {
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
    export interface ClientOptionsContact {
        host?: string;
        clientId: string;
        secretKey?: string;
    }
    export class Client {
        #private;
        constructor(options: ClientOptionsContact);
        get onreqinit(): {
            login: (init: RequestInit) => void;
            logout: (init: RequestInit) => void;
            fetch: (init: RequestOptions) => void;
            all: (init: RequestOptions) => void;
        };
        get login(): {
            (req: any): Promise<Response>;
            alive(): Promise<Response>;
            password(username: string, password: string, options?: LoginOptionsContract & {
                ldap?: string;
            }): Promise<Response>;
            refreshToken(refreshToken: string, options?: LoginOptionsContract): Promise<Response>;
            authCode(code: string, options?: LoginOptionsContract & {
                redirect_uri?: string;
                redir?: string;
                code_verifier?: string;
                verifier?: string;
                provider?: string;
            }): Promise<Response>;
            client(options?: LoginOptionsContract): Promise<Response>;
            logout(): Promise<Response>;
        };
        get path(): {
            (key: string | "passport" | "settings" | "cms" | "sns", value?: string, skipIfExist?: boolean): string;
            get(key: string | "passport" | "settings" | "cms" | "sns"): string;
            set(key: string | "passport" | "settings" | "cms" | "sns", value: string, skipIfExist?: boolean): void;
            set(obj: any, skipIfExist?: boolean): void;
            remove(key: string | string[]): void;
            keys(): string[];
        };
        get user(): {
            (id: string): Promise<Response>;
            exist(id: string): Promise<Response>;
        };
        logout(): Promise<Response>;
        url(path: string, query?: any): string;
        fetch(path: string, reqInit?: RequestOptions): Promise<Response>;
        resProvider(path: string): ResourceEntityProvider;
    }
    export class ResourceEntityProvider {
        readonly appInfo: InternalClientContract;
        readonly path: string;
        constructor(appInfo: InternalClientContract, path: string);
        search(q: any): Promise<Response>;
        get(id: string): Promise<Response>;
        save(value: any, id?: string): Promise<Response>;
        delete(id: string): Promise<Response>;
        fetch(subPath: string, reqInit: RequestOptions): Promise<Response>;
    }
    export {};
}
