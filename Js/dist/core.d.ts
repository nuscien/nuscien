/**
 * NuScien core library for front-end (web client).
 * https://github.com/nuscien/trivial
 * Copyright (c) 2020 Kingcean Tuan. All rights reserved.
 */
declare namespace NuScien {
    /**
     * The main version.
     */
    const ver = "5.0";
    /**
     * The state of resource entity.
     */
    type ResourceEntityStateValue = number | "deleted" | "draft" | "request" | "normal" | "Deleted" | "Draft" | "Request" | "Normal";
    /**
     * The query arguments.
     */
    interface QueryArgsContract {
        q?: string;
        eqname?: boolean;
        count?: number;
        offset?: number;
        state?: ResourceEntityStateValue;
        order?: number | "default" | "latest" | "time" | "name" | "z2a" | "Default" | "Latest" | "Time" | "Name" | "Z2A";
        [property: string]: any;
        site?: string | undefined | null;
    }
    /**
     * The base resource entity contract.
     */
    interface ResourceEntityContract {
        id: string;
        name: string;
        state: ResourceEntityStateValue;
        creation: number | Date;
        lastupdate: number | Date;
        rev?: string;
        slim?: boolean;
        [property: string]: any;
    }
    interface ConfigurableResourceEntityContract extends ResourceEntityContract {
        config: any;
    }
    interface SiteOwnerResourceEntityContract extends ConfigurableResourceEntityContract {
        site: string;
    }
    interface BaseOwnerResourceEntityContract extends ConfigurableResourceEntityContract {
        owner: string;
    }
    interface RelationshipResourceEntityContract extends BaseOwnerResourceEntityContract {
        target: string;
    }
    interface SecurityResourceEntityContract extends ResourceEntityContract {
        identype: string;
        nickname: string | null;
        avatar: string | null;
    }
    interface UserEntityContract extends SecurityResourceEntityContract {
        gender: number | "unknown" | "male" | "female" | "asexual" | "machine" | "other" | "Unknown" | "Male" | "Female" | "Asexual" | "Machine" | "Other";
        avatar: string;
        birthday?: Date | number | undefined | null;
        market?: string;
    }
    interface UserGroupEntityContract extends SecurityResourceEntityContract {
        site: string;
        membership: number | "forbidden" | "application" | "allow" | "Forbidden" | "Application" | "Allow";
        visible: number | "hidden" | "memberwise" | "public" | "Hidden" | "Memberwise" | "Public";
    }
    interface UserGroupRelaEntityContract extends ResourceEntityContract {
        owner: string;
        res: string;
        config: any;
        role: number | "member" | "poweruser" | "master" | "owner" | "Member" | "PowerUser" | "Master" | "Owner" | null;
    }
    interface RevisionAdditionalInfoContract {
        owner: string;
        message?: string;
    }
    interface ContentEntityContract extends SiteOwnerResourceEntityContract {
        intro?: string;
        parent?: string;
        publisher?: string;
        thumb?: string;
        templ?: string;
        keywords?: string;
        content: string;
        templc?: string;
        creator?: string;
    }
    interface ContentTemplateEntityContract extends SiteOwnerResourceEntityContract {
        intro?: string;
        publisher?: string;
        thumb?: string;
        content: string;
        creator?: string;
    }
    interface CommentEntityContract extends BaseOwnerResourceEntityContract {
        publisher: string;
        parent?: string;
        ancestor?: string;
        content: string;
    }
    interface ContactEntityContract extends BaseOwnerResourceEntityContract {
    }
    interface TokenResponseContract {
        state?: string | undefined | null;
        token_type?: string | undefined | null;
        access_token?: string | undefined | null;
        refresh_token?: string | undefined | null;
        scope?: string | null;
        user_id?: string | undefined | null;
        user?: UserEntityContract | undefined | null;
        client_id?: string | undefined | null;
        error?: "unknown" | "invalid_request" | "invalid_client" | "invalid_grant" | "UnauthorizedClient" | "access_denied" | "unsupported_response_type" | "unsupported_grant_type" | "invalid_scope" | "server_error" | "temporarily_unavailable" | string | undefined | null;
        error_uri?: string | undefined | null;
        error_description?: string | undefined | null;
    }
    interface CollectionResultContract<T = any> {
        col: T[];
        offset?: number | null | undefined;
        count?: number | null | undefined;
    }
    interface ChangingResultContract {
        state: number | "unknown" | "unchanged" | "same" | "update" | "membermodify" | "remove" | "invalid" | "Unknown" | "Unchanged" | "Same" | "Update" | "MemberModify" | "Remove" | "Invalid";
        code?: number | string | undefined;
        message?: string | undefined | null;
        data?: any | undefined | null;
    }
    interface GenericWebResponseContract<T> extends Response {
        /**
         * Gets the response data.
         */
        json(): Promise<T>;
    }
    interface CollectionWebResponseContract<T> extends GenericWebResponseContract<CollectionResultContract<T>> {
    }
    interface ChangingWebResponseContract extends GenericWebResponseContract<ChangingResultContract> {
    }
    /**
     * Assert helper.
     */
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
        #private;
        /**
         * Initializes a new instance of the Client class.
         * @param options  The options containing the client information.
         */
        constructor(options: ClientOptionsContact);
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
        };
        /**
         * User group resources.
         */
        get group(): {
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
        };
        /**
         * Settings.
         */
        get settings(): {
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
        };
        /**
         * CMS.
         */
        get cms(): {
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
        };
        /**
         * User activities.
         */
        get activities(): {};
        /**
         * Signs out.
         */
        logout(): Promise<GenericWebResponseContract<TokenResponseContract>>;
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
        fetch<T = any>(path: string, reqInit?: RequestOptions): Promise<GenericWebResponseContract<T>>;
        /**
         * Gets the resource entity provider.
         * @param path  The relative root path.
         */
        resProvider<T = any>(path: string): ResourceEntityProvider<T>;
    }
    /**
     * The resource entity provider.
     */
    export class ResourceEntityProvider<TEntity> {
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
        search(q: any): Promise<GenericWebResponseContract<CollectionResultContract<TEntity>>>;
        /**
         * Gets a resource entity.
         * @param id  The entity identifier.
         */
        get(id: string): Promise<GenericWebResponseContract<TEntity>>;
        /**
         * Creates or updates a specific resource entity.
         * @param value  The entity to save or the content to delta update.
         * @param id  The optional entity identifier. Only set this parameter when need delta update.
         */
        save(value: any, id?: string): Promise<GenericWebResponseContract<ChangingResultContract>>;
        /**
         * Deletes a specific resource entity.
         * @param id  The entity identifier.
         */
        delete(id: string): Promise<GenericWebResponseContract<ChangingResultContract>>;
        /**
         * Sends request to service and gets response.
         * @param path  The relative path.
         * @param reqInit  The options.
         */
        fetch<T>(subPath: string, reqInit: RequestOptions): Promise<GenericWebResponseContract<T>>;
    }
    export {};
}
