const enum Currency {
    None = 0,
    Dollar = 1
}

const enum EntityType {
    Vehicle = 1,
    Prop = 2,
    Blip = 3,
    Marker = 4,
    Pickup = 5,
    Player = 6,
    TextLabel = 7,
    Ped = 8,
    Particle = 9,
    World = 255
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
    HasBeenDeleted: boolean;
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