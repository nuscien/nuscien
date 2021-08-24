/**
 * NuScien core library for front-end (web client).
 * https://github.com/nuscien/trivial
 * Copyright (c) 2020 Kingcean Tuan. All rights reserved.
 */
namespace NuScien {

    /**
     * The main version.
     */
    export const ver = "5.1";

    /**
     * The state of resource entity.
     */
    export type ResourceEntityStateValue = number | "deleted" | "draft" | "request" | "normal" | "Deleted" | "Draft" | "Request" | "Normal";

    /**
     * The query arguments.
     */
    export interface QueryArgsContract {
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
    export interface ResourceEntityContract {
        id: string;
        name: string;
        state: ResourceEntityStateValue;
        creation: number | Date;
        lastupdate: number | Date;
        rev?: string;
        slim?: boolean;
        [property: string]: any;
    }

    export interface ConfigurableResourceEntityContract extends ResourceEntityContract {
        config: any;
    }

    export interface SiteOwnerResourceEntityContract extends ConfigurableResourceEntityContract {
        site: string;
    }

    export interface BaseOwnerResourceEntityContract extends ConfigurableResourceEntityContract {
        owner: string;
    }

    export interface RelationshipResourceEntityContract extends BaseOwnerResourceEntityContract {
        target: string;
    }

    export interface SecurityResourceEntityContract extends ResourceEntityContract {
        identype: string;
        nickname: string | null;
        avatar: string | null;
    }

    export interface UserEntityContract extends SecurityResourceEntityContract {
        gender: number | "unknown" | "male" | "female" | "asexual" | "machine" | "other" | "Unknown" | "Male" | "Female" | "Asexual" | "Machine" | "Other";
        avatar: string;
        birthday?: Date | number | undefined | null;
        market?: string;
    }

    export interface UserGroupEntityContract extends SecurityResourceEntityContract {
        site: string;
        membership: number | "forbidden" | "application" | "allow" | "Forbidden" | "Application" | "Allow";
        visible: number | "hidden" | "memberwise" | "public" | "Hidden" | "Memberwise" | "Public";
    }

    export interface UserGroupRelaEntityContract extends ResourceEntityContract {
        owner: string;
        res: string;
        config: any;
        role: number | "member" | "poweruser" | "master" | "owner" | "Member" | "PowerUser" | "Master" | "Owner" | null;
    }

    export interface RevisionAdditionalInfoContract {
        owner: string;
        message?: string;
    }

    export interface ContentEntityContract extends SiteOwnerResourceEntityContract {
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

    export interface ContentTemplateEntityContract extends SiteOwnerResourceEntityContract {
        intro?: string;
        publisher?: string;
        thumb?: string;
        content: string;
        creator?: string;
    }

    export interface CommentEntityContract extends BaseOwnerResourceEntityContract {
        publisher: string;
        parent?: string;
        ancestor?: string;
        content: string;
    }

    export interface ContactEntityContract extends BaseOwnerResourceEntityContract {
    }

    export interface TokenResponseContract {
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

    export interface CollectionResultContract<T = any> {
        col: T[];
        offset?: number | null | undefined;
        count?: number | null | undefined;
    }

    export interface ChangingResultContract {
        state: number | "unknown" | "unchanged" | "same" | "update" | "membermodify" | "remove" | "invalid" | "Unknown" | "Unchanged" | "Same" | "Update" | "MemberModify" | "Remove" | "Invalid";
        code?: number | string | undefined;
        message?: string | undefined | null;
        data?: any | undefined | null;
    }

    export interface GenericWebResponseContract<T> extends Response {
        /**
         * Gets the response data.
         */
        json(): Promise<T>;
    }

    export interface CollectionWebResponseContract<T> extends GenericWebResponseContract<CollectionResultContract<T>> {
    }

    export interface ChangingWebResponseContract extends GenericWebResponseContract<ChangingResultContract> {
    }

    /**
     * Assert helper.
     */
    export class Assert {

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
        public static toStr(obj: any, level?: undefined | "d" | "default" | "DEFAULT" | "t" | "text" | "TEXT" | "c" | "compatible" | "COMPATIBLE" | "j" | "json" | "JSON" | "q" | "query" | "QUERY" | "u" | "url" | "URL" | "s" | "string" | "STRING"): string | null {
            if (typeof obj === "undefined") return null;
            if (!level) level = "d";
            else level = level.toLowerCase() as any;
            if (level === "json" || level === "j")
                return JSON.stringify(obj) || null;
            if (typeof obj === "string") return obj;
            if (obj === null || level === "string" || level === "s") return null;
            if (typeof obj === "number")
                return isNaN(obj) ? null : obj.toString(10);
            if (typeof obj === "symbol")
                return obj.toString();
            if (level === "text" || level === "t")
                return null;
            if (typeof obj === "boolean") return obj.toString();
            if (level === "compatible" || level === "c")
                return JSON.stringify(obj) || null;
            if (level === "query" || level === "q") {
                if (obj instanceof Array)
                    return (obj as any[]).filter(ele => typeof ele !== "undefined").map(ele => encodeURIComponent(Assert.toStr(obj[ele], "c") || "")).join(",");
                return Object.keys(obj).map(ele => `${encodeURIComponent(ele)}=${encodeURIComponent(Assert.toStr(obj[ele], "c") || "")}`).join("&");
            }

            if (level === "url" || level === "u") {
                if (obj instanceof Array)
                    return (obj as any[]).filter(ele => typeof ele !== "undefined").map(ele => encodeURIComponent(Assert.toStr(obj[ele], "c") || "")).join(",");
                return Object.keys(obj).map(ele => `${encodeURIComponent(ele)}=${encodeURIComponent(Assert.toStr(obj[ele], "q") || "")}`).join("&");
            }

            return obj instanceof Array && obj.length === 1 ? Assert.toStr(obj[0]) : null;
        }

        public static isNoNull(input: any, throwIfFailure: boolean | string = false) {
            var isNull = typeof input === "undefined" || input === null || (typeof input === "number" && isNaN(input));
            if (!isNull) return true;
            if (throwIfFailure) throw throwIfFailure === true ? new Error("input is null or undefined.") : throwIfFailure;
            return false;
        }

        public static isString(input: any, throwIfFailure: boolean | string = false): input is string {
            var isStr = typeof input === "string";
            if (isStr) return true;
            if (throwIfFailure) throw throwIfFailure === true ? new Error("input is not a string.") : throwIfFailure;
            return false;
        }

        public static isStrOrNull(input: any, throwIfFailure: boolean | string = false): input is string | null | undefined {
            var isStr = typeof input === "string" || typeof input === "undefined" || input === null;
            if (isStr) return true;
            if (throwIfFailure) throw throwIfFailure === true ? new TypeError("input is not a string.") : throwIfFailure;
            return false;
        }

        public static isValidNumber(input: any, throwIfFailure: boolean | string = false): input is number {
            var isNum = typeof input === "number" && !isNaN(input);
            if (isNum) return true;
            if (throwIfFailure) throw throwIfFailure === true ? new TypeError("input is not a valid number.") : throwIfFailure;
            return false;
        }

        public static isSafeInteger(input: any, throwIfFailure: boolean | string = false): input is number {
            if (Number.isSafeInteger(input)) return true;
            if (throwIfFailure) throw throwIfFailure === true ? new TypeError("input is not a safe integer.") : throwIfFailure;
            return false;
        }
    }
}