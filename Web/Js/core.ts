namespace NuScien {
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
            if (obj === null || isNaN(obj) || level === "string" || level === "s") return null;
            if (typeof obj === "number")
                return obj.toString(10);
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
            var isNull = typeof input === "undefined" || input === null || isNaN(input);
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