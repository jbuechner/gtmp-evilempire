function argumentsToArray(args: any): any[] {
    if (args.length) {
        return Array.prototype.slice.call(args, 0);
    } else {
        return args;
    }
}

function serializeToDesignatedJson(value: any): string {
    return '$json' + JSON.stringify(value);
}