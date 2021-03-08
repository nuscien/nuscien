/**
 * NuScien core library for front-end (web client).
 * https://github.com/nuscien/trivial
 * Copyright (c) 2020 Kingcean Tuan. All rights reserved.
 */
namespace NuScien {

    export interface ResourceEntityContract {
        id: string;
        name: string;
        state: number | "deleted" | "draft" | "request" | "normal";
        creation: number | Date;
        lastupdate: number | Date;
        rev?: string;
        slim?: boolean;
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
        nickname: string;
        avatar: string;
    }

    export interface UserEntityContract extends SecurityResourceEntityContract {
        gender: number | "unknown" | "male" | "female" | "asexual" | "machine" | "other";
        avatar: string;
        birthday?: Date | number;
        market?: string;
    }

    export interface UserGroupEntityContract extends SecurityResourceEntityContract {
        site: string;
        membership: number | "forbidden" | "application" | "allow";
        visible: number | "hidden" | "memberwise" | "public";
    }

    export interface GenericWebResponseContract<T> extends Response {
        /**
         * Gets the response data.
         */
        json(): Promise<T>;
    }

    /**
     * The main version.
     */
    export const ver = "5.0";

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