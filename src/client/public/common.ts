const enum Currency {
    None = 0,
    Dollar = 1
}

interface IVector {
    x: number;
    y: number;
}

interface IItemDescription {
    Id: number;
    Name: string;
    Weight: number;
    Volume: number;


    IsStackable: boolean;
    MaximumStack: number;


    AssociatedCurrency: Currency;
    Denomination: number;

    Description: string;
}

interface IItem {
    Id: string;
    ItemDescriptionId: number;
    Amount: number;
    Name?: string;
    KeyForEntityId?: string;
}

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