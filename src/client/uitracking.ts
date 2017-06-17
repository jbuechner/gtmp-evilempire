interface EntityOptionProvider {
    (entity: LocalHandle): IUiTrackingInfo
}

interface IUiTrackedEntity {
    entity: LocalHandle;
    info: IUiTrackingInfo;
}

class UiTracking {
    tracked: Map<number, IUiTrackedEntity> = new Map<number, IUiTrackedEntity>();
    maximumRange: number = 2.5;

    private _entityOptionProvider: Map<EntityType, EntityOptionProvider>;

    constructor() {
        this._entityOptionProvider = new Map<EntityType, EntityOptionProvider>([
            [ EntityType.Player, UiTracking.CreatePlayerOption ],
            [ EntityType.Ped, UiTracking.CreatePedOption ],
            [ EntityType.Vehicle, UiTracking.CreateVehicleOption ]
        ]);
    }

    update(trackEntities: boolean): void {
        if (!trackEntities) {
            this.removeAllExistingEntities();
            return;
        }

        let player = API.getLocalPlayer();
        let playerPos = API.getEntityPosition(player);

        let items = API.getStreamedPeds();
        UiTracking.__forEach(items, item => this.updateTrackingForItem(playerPos, item));
        items = API.getStreamedVehicles();
        UiTracking.__forEach(items, item => this.updateTrackingForItem(playerPos, item));

        this.removeEntitiesOutOfRange(playerPos);
    }

    updateTrackedElements() {
        this.tracked.forEach(value => {
            value.info.position = API.getEntityPosition(value.entity);
            value.info.positionAbove = (<Vector3>value.info.position).Add(new Vector3(0, 0, 1));

            let viewPoint = API.worldToScreenMaintainRatio(<Vector3>value.info.positionAbove);
            value.info.position2d = { x: viewPoint.X, y: viewPoint.Y };
            browser.raiseEventInBrowser('updateview', { what: 'entityinfo', value: value.info });
        });
    }

    updateTrackingForItem(playerPosition: Vector3, item: LocalHandle) {
        let pos = API.getEntityPosition(item);
        if (API.isInRangeOf(playerPosition, pos, this.maximumRange)) {
            this.addTrackingFor(item);
        }
    }

    static __forEach(array: System.Array<LocalHandle>, fn: (value: LocalHandle, index?: number) => void): void {
        for (let i = 0; i < array.Length; i++) {
            let item = array[i];
            fn(item, i);
        }
    }

    addTrackingFor(entity: LocalHandle): void {
        let id = API.getEntitySyncedData(entity, <string>SynchronizationProperties.EntityId);
        if (this.tracked.has(id)) {
            return;
        }

        let trackingInfo: any = this.getTrackingInfo(entity);
        if (trackingInfo) {
            this.tracked.set(id, { entity: entity, info: trackingInfo });
            browser.addView('view-entityinteractionmenu', { info: trackingInfo });
        }
    }

    getTrackingInfo(entity: LocalHandle): IUiTrackingInfo {
        let entityType = getEntityType(entity);
        let provider = this._entityOptionProvider.get(entityType);
        if (provider !== undefined) {
            let trackingInfo = provider(entity);
            trackingInfo.id = API.getEntitySyncedData(entity, <string>SynchronizationProperties.EntityId);

            trackingInfo.position = API.getEntityPosition(entity);
            trackingInfo.positionAbove = (<Vector3>trackingInfo.position).Add(new Vector3(0, 0, 1));
            trackingInfo.entityType = entityType;

            let viewPoint = API.worldToScreenMaintainRatio(<Vector3>trackingInfo.positionAbove);
            trackingInfo.position2d = {x: viewPoint.X, y: viewPoint.Y};
            return trackingInfo;
        } else {
            API.sendNotification(`There is no entity option provider for entity type "${entityType}".`);
        }
        return null;
    }

    removeEntitiesOutOfPlayerRange() {
        let player = API.getLocalPlayer();
        let playerPos = API.getEntityPosition(player);
        this.removeEntitiesOutOfRange(playerPos);
    }

    removeAllExistingEntities() {
        this.removeExistingEntities(null);
    }

    removeEntitiesOutOfRange(center: Vector3) {
        this.removeExistingEntities((key: number, value: IUiTrackedEntity) => {
            let distance = (Vector3 as any).Distance(center, value.info.position);
            return distance > this.maximumRange;
        });
    }

    removeExistingEntities(predicate: (key: number, value: IUiTrackedEntity) => boolean): boolean {
        if (this.tracked.size < 1) {
            return;
        }

        predicate = predicate || ((value, key) => true);
        let removable = [];
        this.tracked.forEach((value, key) => {
            if (predicate(key, value)) {
                removable.push(key);
                browser.removeView('view-entityinteractionmenu[data-entityId="' + value.info.id + '"]');
            }
        });
        removable.forEach(key => {
            this.tracked.delete(key);
        });
    }

    private static CreatePlayerOption(entity: LocalHandle): IUiTrackingInfo {
        let trackingInfo = UiTracking.createEmptyTrackingInfo();
        trackingInfo.displayName = 'a player';
        trackingInfo.actions = [];
        return trackingInfo;
    }

    private static CreatePedOption(entity: LocalHandle): IPedUiTrackingInfo {
        let trackingInfo = <IPedUiTrackingInfo>UiTracking.createEmptyTrackingInfo();
        trackingInfo.displayName = API.getEntitySyncedData(entity, <string>SynchronizationProperties.EntityDisplayName);
        trackingInfo.dialogueName = API.getEntitySyncedData(entity, <string>SynchronizationProperties.EntityDialogueName);

        if (trackingInfo.dialogueName) {
            trackingInfo.actions = [ 'speak' ];
        } else {
            trackingInfo.actions = [];
        }

        return trackingInfo;
    }

    private static CreateVehicleOption(entity: LocalHandle): IUiTrackingInfo {
        let trackingInfo = UiTracking.createEmptyTrackingInfo();

        let model = API.returnNative('0x9F47B058362C84B5', 0, entity);
        let name = API.getVehicleDisplayName(model);
        let plate = API.getVehicleNumberPlate(entity);

        trackingInfo.displayName = `${name} <span class="monospace">${plate}</span>span>`;
        trackingInfo.actions = [ 'lock', 'engine' ];

        return trackingInfo;
    }


    private static createEmptyTrackingInfo(): IUiTrackingInfo {
        return {
            id: null,
            position: null,
            positionAbove: null,
            position2d: null,
            displayName: null,
            actions: null,
            entityType: null
        }
    }
}