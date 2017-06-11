///<reference path="tsd/index.d.ts"/>
'use strict';

const ServerClientMessage = {
    RequestLogin: 'req:login',
    CustomizeCharacter: 'req:customizeChar',
    ConfirmCustomizeCharacter: 'req:customizeChar:ok',
    CancelCustomizeCharacter: 'req:customizeChar:cancel',
    InteractWithEntity: 'req:interactWithEntity',
    TriggerEntityAction: 'req:triggerEntityAction',
    RequestCharacterInventory: 'req:charInventory'
};

const UserGroups = {
    Guest: 0,
    Player: 1,
    GameMaster: 100,
    Admin: 200
};

const ClientEvents = {
    'enterFreeroam': function __enterFreeroam() {
        client.cursor = false;
        Client.resetCamera();
        browser.removeView('view-login');
        browser.removeView('view-character-customization');
        browser.addView('view-status', { displayCoordinates: Client.hasRequiredUserGroup(UserGroups.GameMaster) });

        for (let [key, value] of moneyMap) {
            browser.raiseEventInBrowser('moneyChanged', { currency: key, amount: value});
        }
    },
    'moneyChanged': function __moneyUpdated(currency, amount) {
        moneyMap.set(currency, amount);
        browser.raiseEventInBrowser('moneyChanged', { currency, amount });
    },
    '::display:login': function __display_login() {
        client.cursor = true;
        Client.setCamera(200, 200, 150, 1, 1, 1);
        browser.addView('view-login');
    },
    '::display:characterCustomization': function __display_characterCustomization(data) {
        client.cursor = true;
        Client.setCameraToViewPlayer();
        let freeroamCustomizationData = deserializeFromDesignatedJson(data);
        browser.addView('view-character-customization', { current: characterCustomization, customization: freeroamCustomizationData });
    },
    'res:login': function __login_response(success, data) {
        data = deserializeFromDesignatedJson(data);
        if (success) {
            client.cursor = false;
            Client.resetCamera();
            browser.removeView('view-login');
            user = data.User;
            character = data.Character;
            characterCustomization = data.CharacterCustomization;
        } else {
            data = {};
        }
        data.success = success;
        browser.raiseEventInBrowser('res:login', data);
    },
    'res:customizeChar': function __customizeChar_response(success, data) {
        data = deserializeFromDesignatedJson(data);
        if (!success && typeof data !== 'object') {
            data = {};
        }
        data.success = success;
        browser.raiseEventInBrowser('res:customizeChar', data);
    },
    'res:interactWithEntity': function __interactWithEntity_response(success, data) {
        ClientEvents.entityContentResponse(success, data);
    },
    'res:triggerEntityAction': function __triggerEntityAction_response(success, data) {
        ClientEvents.entityContentResponse(success, data);
    },
    'res:charInventory': function __characterInventory_response(success, data) {
        data = deserializeFromDesignatedJson(data);
        if (!success) {
            data = data || {};
            data.Items = data.Items || [];
            data.Money = data.Money || [];
        }
        browser.raiseEventInBrowser('res:charInventory', data);
    },

    'entityContentResponse': function __triggerEntityContentResponse(success, data) {
        data = deserializeFromDesignatedJson(data);
        if (!success) {
            data = data || {};
            data.EntityId = data.EntityId || 0;
        }

        if (data) {
            data.EntityId = resolveEntityIdFromNetHandle(data.EntityId);
            if (data.Content) {
                data.Content = deserializeFromDesignatedJson(data.Content);
            }
        }
        browser.raiseEventInBrowser('updateview', {
            what: 'content',
            value: {entityId: '' + data.EntityId, content: data.Content}
        });
    }
};

const BrowserEvents = {
    'disconnect': function __disconnect(reason) {
        API.disconnect(reason);
    },
    'login': function __login(username, password) {
        sendToServer(ServerClientMessage.RequestLogin, argumentsToArray(arguments));
    },
    'customizeCharacter': function __customizeChar(what, value) {
        sendToServer(ServerClientMessage.CustomizeCharacter, argumentsToArray(arguments));
    },
    'confirmCharacterCustomization': function __confirmCustomizeChar() {
        sendToServer(ServerClientMessage.ConfirmCustomizeCharacter);
    },
    'cancelCharacterCustomization': function __cancelCustomizeChar() {
        sendToServer(ServerClientMessage.CancelCustomizeCharacter);
    },
    'interactWithEntity': function __interactWithEntity(entityId, entityType, entityKey, action) {
        entityId = resolveNetHandleFromEntityId(entityId);
        sendToServer(ServerClientMessage.InteractWithEntity, [entityId, entityType, entityKey, action]);
    },
    'triggerEntityAction': function __triggerEntityAction(entityId, action) {
        entityId = resolveNetHandleFromEntityId(entityId);
        sendToServer(ServerClientMessage.TriggerEntityAction, [entityId, action]);
    },
    'closeInventory': function __closeInventory() {
        client.displayInventory = false;
    },
    'requestCharacterInventory': function __requestCharacterInventory() {
        sendToServer(ServerClientMessage.RequestCharacterInventory);
    }
};

function debugOut(ex) {
    let msg = ':: ' + ex.toString();
    if (ex.stack) {
        msg += ex.stack;
    }
    API.sendNotification(msg);
}

function deserializeFromDesignatedJson(value) {
    if (typeof value === 'string' && value.startsWith('$json')) {
        return JSON.parse(value.substring(5));
    }
    return value;
}

function serializeToDesignatedJson(value) {
    return '$json' + JSON.stringify(value);
}

function argumentsToArray(args) {
    return Array.prototype.slice.call(args, 0);
}

let serverEventTriggerQueue = [];
function onServerEventTrigger(eventName, argsArray) {
    let newArgs = [];
    for(let arg in argsArray) {
        let v = argsArray[arg];
        if (typeof v !== 'function') {
            newArgs.push(v);
        }
    }
    serverEventTriggerQueue.push([eventName, newArgs]);
    if (!isReadyToProcessServerEventTriggers) {
        return;
    }
    processServerEventTriggers();
}

let updateCount = 0;
let testRange = 3;
let uiTrackedEntities = new Map();
function addUiTrackedEntity(entity) {
    let entityId = entity.Value;
    if (uiTrackedEntities.has(entityId)) {
        return;
    }

    let position = API.getEntityPosition(entity);
    let positionAbove = position.Add(new Vector3(0, 0, 1));
    let viewPoint = API.worldToScreenMaintainRatio(positionAbove);

    if (client.canDisplayUiTrackedElements) { // check netHandle
        let netHandle = API.getEntitySyncedData(entity, 'ENTITY:NET');
        uiTrackedEntities.set(entityId, {id: entityId, netHandle, entity, position, positionAbove});

        let options: any = getUiTrackingOptions(entity);
        if (options) {
            let entityKey = API.getEntitySyncedData(entity, 'ENTITY:KEY');

            options.entityId = '' + entityId;
            options.entityKey = entityKey;
            options.pos = {x: viewPoint.X, y: viewPoint.Y};

            browser.addView('view-entityinteractionmenu', options);
        }
    }
}
function updateUiTrackedEntities() {
    uiTrackedEntities.forEach(value => {
        value.position = API.getEntityPosition(value.entity);
        value.positionAbove = value.position.Add(new Vector3(0, 0, 1));

        let viewPoint = API.worldToScreenMaintainRatio(value.positionAbove);
        browser.raiseEventInBrowser('updateview', { what: 'entitytargetpos', value: { entityId: '' + value.id, x: viewPoint.X, y: viewPoint.Y} });
    });
}
function removeAllUiTrackedEntities(predicate) {
    if (uiTrackedEntities.size < 1) {
        return;
    }

    predicate = predicate || ((value, key) => true);
    let removable = [];
    uiTrackedEntities.forEach((value, key) => {
        if (predicate(value, key)) {
            removable.push(key);
            browser.removeView('view-entityinteractionmenu[data-entityId="' + value.id + '"]');
        }
    });
    removable.forEach(key => {
        uiTrackedEntities.delete(key);
    });
}
function removeUiTrackedEntitiesThatAreOutOfRange() {
    let player = API.getLocalPlayer();
    let playerPosition = API.getEntityPosition(player);
    removeAllUiTrackedEntities((value, key) => {
        let distance = (Vector3 as any).Distance(playerPosition, value.position);
        return distance > testRange;
    });
}
function getUiTrackingOptions(entity) {
    if (API.isVehicle(entity)) {
        let model = API.returnNative('0x9F47B058362C84B5', 0, entity);
        let name = API.getVehicleDisplayName(model);
        let plate = API.getVehicleNumberPlate(entity);
        return { title: '' + name + ' <span class="monospace">' + plate + '</span>', actions: ['lock', 'engine'], entityType: 'VEHICLE' };
    }
    if (API.isPed(entity)) {
        let actions = [];
        let title = API.getEntitySyncedData(entity, 'ENTITY:TITLE');
        if (title && typeof title === 'string') {
            if (API.hasEntitySyncedData(entity, 'DIALOGUE:NAME')) {
                actions.push('speak');
            }

            return {title: title, actions, entityType: 'PED'};
        }
    }

    return null;
}
function resolveNetHandleFromEntityId(entityId) {
    if (entityId) {
        entityId = Number.parseInt(entityId);
        let uiTrackedEntity = uiTrackedEntities.get(entityId);
        if (uiTrackedEntity && uiTrackedEntity.netHandle) {
            return uiTrackedEntity.netHandle;
        }
    }
    return entityId;
}
function resolveEntityIdFromNetHandle(entityId) {
    if (entityId) {
        for (let [key, value] of uiTrackedEntities) {
            if (value.netHandle && value.netHandle === entityId) {
                return key;
            }
        }
    }
    return entityId;
}

function onUpdate() {
    if (updateCount++ > 30) {
        updateCount = 0;

        if (client.cursor || client.cursorToggle) {
            let player = API.getLocalPlayer();
            let playerPos = API.getEntityPosition(player);
            let tar = API.getOffsetInWorldCoords(player, new Vector3(0, testRange, 0));
            let raycast = API.createRaycast(playerPos, tar, 10 | 12, player);
            if (raycast && raycast.didHitEntity) {
                if (!API.isPed(raycast.hitEntity) || !(raycast.hitEntity as any).IsHuman) {
                    addUiTrackedEntity(raycast.hitEntity);
                }
            }
            if (client.isInVehicle) {

                if (client.canDisplayUiTrackedElements) {
                    let vehicle = API.getPlayerVehicle(player);
                    if (vehicle) {
                        addUiTrackedEntity(vehicle);
                    }
                } else {
                    removeAllUiTrackedEntities(null);
                }
            }
            removeUiTrackedEntitiesThatAreOutOfRange();
        } else {
            removeAllUiTrackedEntities(null);
        }
    }
    if (updateCount % 10 === 0) {
        updateUiTrackedEntities();
    }
}

function onPlayerEnterVehicle(localPlayerHandle) {
    let player = API.getLocalPlayer();
    if ((player as any).value === localPlayerHandle.value) {
        client.isInVehicle = true;
        if (!client.canDisplayUiTrackedElements) {
            removeAllUiTrackedEntities(null);
        }
    }
}

function onPlayerExitVehicle(localPlayerHandle) {
    let player = API.getLocalPlayer();
    if ((player as any) === localPlayerHandle.value) {
        client.isInVehicle = false;
    }
}

function onEntityStreamIn(entity, entityType) {
    updateCharacterCustomization(entity);
}

function onEntityDataChange(entity, key, oldValue) {
    updateCharacterCustomization(entity);
}

function updateCharacterCustomization(entity) {
    try {
        let entityType = API.getEntitySyncedData(entity, 'ENTITY_TYPE');
        if (typeof entityType !== 'undefined') {
            if (entityType === 6 || entityType === 8) {
                let shapeFirst = API.getEntitySyncedData(entity, 'FACE::SHAPEFIRST');
                let shapeSecond = API.getEntitySyncedData(entity, 'FACE::SHAPESECOND');
                let skinFirst = API.getEntitySyncedData(entity, 'FACE::SKINFIRST');
                let skinSecond = API.getEntitySyncedData(entity, 'FACE::SKINSECOND');
                let shapeMix = API.getEntitySyncedData(entity, 'FACE::SHAPEMIX');
                let skinMix = API.getEntitySyncedData(entity, 'FACE::SKINMIX');

                let hairStyle = API.getEntitySyncedData(entity, 'HAIR::STYLE');
                let hairColor = API.getEntitySyncedData(entity, 'HAIR::COLOR');

                if (typeof shapeFirst !== 'undefined' && typeof shapeSecond !== 'undefined' && typeof skinFirst !== 'undefined' && skinSecond !== 'undefined' && typeof shapeMix !== 'undefined' && skinMix !== 'undefined') {
                    API.callNative('0x9414E18B9434C2FE', entity, shapeFirst, shapeSecond, 0, skinFirst, skinSecond, 0, shapeMix, skinMix, 0, false);
                }
                if (typeof hairStyle !== 'undefined') {
                    API.callNative('0x262B14F48D29DE80', entity, 2, hairStyle, 0, 0);
                }
                if (typeof hairColor !== 'undefined') {
                    API.callNative('0x4CFFC65454C93A49', entity, hairColor, 0);
                }
            }
        }
    } catch(ex) {
        debugOut(ex);
    }
}

function processServerEventTriggers() {
    while (serverEventTriggerQueue.length > 0) {
        let args = serverEventTriggerQueue.shift();
        let handler = ClientEvents[args[0]];
        if (typeof handler === 'function') {
            try {
                handler.apply(null, args[1]);
            } catch(ex) {
                API.sendNotification('process server event trigger ' + args[0]);
                API.sendNotification('' + ex);
                if (ex.stack) {
                    API.sendNotification('' + ex);
                }
            }
        }
    }
}

function sendToServer(eventName, args = null) {
    args = serializeToDesignatedJson(args);
    API.triggerServerEvent(eventName, args);
}

function pushViewData() {
    if (pushViewDataTickCount++ > 360) {
        pushViewDataTickCount = 0;
        let coordinates = Client.coordinates;
        let rotation = Client.rotation;

        let value = {
            coord: { x: coordinates.X, y: coordinates.Y, z: coordinates.Z },
            rot: { x: rotation.X, y: rotation.Y, z: rotation.Z }
        };

        browser.raiseEventInBrowser('updateview', { what: 'coordinates', value });
    }
}

let pushViewDataTickCount = 0;
let user = null;
let character = null;
let characterCustomization = null;
let moneyMap = new Map();
let displayInfo = { minimap: { margin: { left: 0, bottom: 0 }, width: 0, height: 0 } };

let browser = null;
let client: Client = null;
let inputs: InputController = null;
let isReadyToProcessServerEventTriggers = false;
let onServerEventTriggerSubscription = null;
let onResourceStartSubscription = API.onResourceStart.connect(() => {
    onResourceStartSubscription.disconnect();
    onResourceStartSubscription = null;

    onServerEventTriggerSubscription = API.onServerEventTrigger.connect(onServerEventTrigger);
    API.onEntityStreamIn.connect(onEntityStreamIn);
    API.onEntityDataChange.connect(onEntityDataChange);
    API.onUpdate.connect(onUpdate);

    API.onPlayerEnterVehicle.connect(onPlayerEnterVehicle);
    API.onPlayerExitVehicle.connect(onPlayerExitVehicle);

    let res = API.getScreenResolution();
    displayInfo.minimap.margin.left = res.Width / 64;
    displayInfo.minimap.margin.bottom = res.Height / 60;
    displayInfo.minimap.width = res.Width / 7.11;
    displayInfo.minimap.height = res.Height / 5.71;

    client = new Client();
    inputs = new InputController();
    Browser.create().then(newBrowser => {
        browser = newBrowser;
        browser.navigate('index.html').then(() => {
            browser.show();
        });
    });
});

function browser_ready() {
    isReadyToProcessServerEventTriggers = true;
    processServerEventTriggers();

    API.onUpdate.connect(() => pushViewData());

    browser.raiseEventInBrowser('displayInfoChanged', displayInfo);

    inputs.addMapping(KEY.CTRL, { onDown: () => client.cursorToggle = true, onUp: () => client.cursorToggle = false });
    inputs.addMapping(KEY.I, () => client.displayInventory = !client.displayInventory);
    inputs.addMapping(KEY.F12, () => client.cursor = !client.cursor);
}

function browser_backend(args) {
    args = deserializeFromDesignatedJson(args);
    let handler = BrowserEvents[args.eventName];
    if (typeof handler === 'function') {
        handler.apply(null, args.args);
    }
}
