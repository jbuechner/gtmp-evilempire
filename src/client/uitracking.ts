type Entity = GTA.Entity;

interface IUiTrackingInfo {
    id: number;
    position: Vector3;
    positionAbove: Vector3;
    netHandle: any;
    entity: LocalHandle;
}

class UiTracking {
    tracked: Map<number, IUiTrackingInfo> = new Map<number, IUiTrackingInfo>();
    maximumRange: number = 2.5;

    resolveNetHandleFromEntityId(entityId) {
        if (entityId) {
            entityId = Number.parseInt(entityId);
            let uiTrackedEntity = this.tracked.get(entityId);
            if (uiTrackedEntity && uiTrackedEntity.netHandle) {
                return uiTrackedEntity.netHandle;
            }
        }
        return entityId;
    }

    resolveEntityIdFromNetHandle(entityId) {
        if (entityId) {
            for (let [key, value] of this.tracked) {
                if (value.netHandle && value.netHandle === entityId) {
                    return key;
                }
            }
        }
        return entityId;
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
            value.position = API.getEntityPosition(value.entity);
            value.positionAbove = value.position.Add(new Vector3(0, 0, 1));

            let viewPoint = API.worldToScreenMaintainRatio(value.positionAbove);
            browser.raiseEventInBrowser('updateview', { what: 'entitytargetpos', value: { entityId: '' + value.id, x: viewPoint.X, y: viewPoint.Y} });
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

    addTrackingFor(item: LocalHandle): void {
        let id = item.Value;
        if (this.tracked.has(id)) {
            return;
        }

        let position = API.getEntityPosition(item);
        let positionAbove = position.Add(new Vector3(0, 0, 1));
        let viewPoint = API.worldToScreenMaintainRatio(positionAbove);
        let netHandle = API.getEntitySyncedData(item, 'ENTITY:NET');
        let entityKey = API.getEntitySyncedData(item, 'ENTITY:KEY');

        let options: any = UiTracking.getDefaultOptions(item);
        if (options) {
            this.tracked.set(id, { id, netHandle, entity: item, position, positionAbove });

            options.entityId = '' + id;
            options.entityKey = entityKey;
            options.pos = {x: viewPoint.X, y: viewPoint.Y};

            browser.addView('view-entityinteractionmenu', options);
        }
    }

    static getDefaultOptions(item) {
        if (API.isVehicle(item)) {
            let model = API.returnNative('0x9F47B058362C84B5', 0, item);
            let name = API.getVehicleDisplayName(model);
            let plate = API.getVehicleNumberPlate(item);
            return { title: '' + name + ' <span class="monospace">' + plate + '</span>', actions: ['lock', 'engine'], entityType: 'VEHICLE' };
        }
        if (API.isPed(item)) {
            let actions = [];
            let title = API.getEntitySyncedData(item, 'ENTITY:TITLE');
            if (title && typeof title === 'string') {
                if (API.hasEntitySyncedData(item, 'DIALOGUE:NAME')) {
                    actions.push('speak');
                }
                return {title: title, actions, entityType: 'PED'};
            }
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
        this.removeExistingEntities((key: number, value: IUiTrackingInfo) => {
            let distance = (Vector3 as any).Distance(center, value.position);
            return distance > this.maximumRange;
        });
    }

    removeExistingEntities(predicate: (key: number, value: IUiTrackingInfo) => boolean): boolean {
        if (this.tracked.size < 1) {
            return;
        }

        predicate = predicate || ((value, key) => true);
        let removable = [];
        this.tracked.forEach((value, key) => {
            if (predicate(key, value)) {
                removable.push(key);
                browser.removeView('view-entityinteractionmenu[data-entityId="' + value.id + '"]');
            }
        });
        removable.forEach(key => {
            this.tracked.delete(key);
        });
    }
}