class App {
    static login(username, password) {
        App.callBackend('login', argumentsToArray(arguments));
    }

    static disconnect(reason) {
        App.callBackend('disconnect', argumentsToArray(arguments));
    }

    static customizeCharacter(what, newValue) {
        App.callBackend('customizeCharacter', argumentsToArray(arguments));
    }

    static confirmCharacterCustomization() {
        App.callBackend('confirmCharacterCustomization');
    }

    static cancelCharacterCustomization() {
        App.callBackend('cancelCharacterCustomization');
    }

    static entityinteraction(entityId: string, entityType: string, entityKey: string, action: string): void {
        App.callBackend('interactWithEntity', argumentsToArray(arguments));
    }

    static triggerEntityAction(entityId, action) {
        App.callBackend('triggerEntityAction', argumentsToArray(arguments));
    }

    static requestCharacterInventory() {
        App.callBackend('requestCharacterInventory');
    }

    static requestDeleteItem(itemId: string, quantity: number) {
        App.callBackend('requestDeleteItem', argumentsToArray(arguments));
    }

    static callBackend(eventName, args?: any[]) {
        resourceCall('browser_backend', serializeToDesignatedJson({ eventName: eventName, args }));
    }
}