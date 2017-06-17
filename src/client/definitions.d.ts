//<reference path="tsd/index.d.ts"/>

declare interface IVector {
    x: number;
    y: number;
}

declare interface IUiTrackingInfo {
    id: number;
    position: { X: number, Y: number; Z: number };
    positionAbove: { X: number, Y: number; Z: number };
    position2d: IVector

    entityType: EntityType;
    displayName: string;

    actions: string[];
}

declare interface IPedUiTrackingInfo extends IUiTrackingInfo {
    dialogueName: string;
}
