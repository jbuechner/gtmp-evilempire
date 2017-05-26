'use strict';
const debug = false;
function define(module)  {
    const $client = module.require('client');

    const ClientLifecycleState = {
        None: 0,
        Connected: 1,
        LoggedIn: 2,
        CharacterCustomization: 3
    };

    class ClientLifecycle {
        constructor() {
            this._transitions = new Map([
                [ClientLifecycle.getStateTransitionKey(ClientLifecycleState.None, ClientLifecycleState.Connected), ClientLifecycle.onClientConnected],
                [ClientLifecycle.getStateTransitionKey(ClientLifecycleState.Connected, ClientLifecycleState.LoggedIn), ClientLifecycle.onClientLoggedIn],
                [ClientLifecycle.getStateTransitionKey(ClientLifecycleState.LoggedIn, ClientLifecycleState.CharacterCustomization), ClientLifecycle.onStartCharacterCustomization]
            ]);
            this._state = ClientLifecycleState.None;
        }

        get state() {
            return this._state;
        }

        transit(state, context) {
            const changed = this._state !== state;
            if (changed) {
                const old = this._state;
                this._state = state;
                this.invokeStateTransition(old, state, context);
            }
        }

        invokeStateTransition(from, to, context) {
            if (debug) {
                API.sendNotification(`from ${from} to ${to}`);
            }

            const key = ClientLifecycle.getStateTransitionKey(from, to);
            const transition = this._transitions.get(key);
            if (transition !== undefined && typeof transition === 'function') {
                transition.call(null, context);
            } else {
                API.sendNotification(`missing transition from ${from} to ${to}.`);
            }
        }

        static getStateTransitionKey(from, to) {
            return ((from & 0xffff) << 16) + (to & 0xfffff);
        }

        static onClientConnected(app) {
            app.client.setCamera(200, 200, 150, 1, 1, 1);
            app.browser.addView('view-login');
            app.browser.show();
            app.client.cursor = true;
            app.client.isRadarVisible = false;
        }

        static onClientLoggedIn(app) {
            app.client.cursor = false;
            app.client.resetCamera();
            app.browser.removeView('view-login');
            // serialize again because we are crossing the V8 again and objects arent marshalled by gtmp
            app.browser.addView('view-status', JSON.stringify({ displayCoordinates: app.client.hasRequiredUserGroup($client.UserGroups.GameMaster) }) );
        }

        static onStartCharacterCustomization(appAndData) {
            if (debug) {
                API.sendNotification('onStartCharacterCustomization:' + appAndData.data);
            }
            let app = appAndData.app;
            let data = appAndData.data;
            app.client.cursor = true;
            app.browser.removeView('view-status');
            app.browser.addView('view-character-customization', data);

            app.client.setCameraToViewPlayer();
        }
    }

    module.exports = {
        ClientLifecycleState,
        ClientLifecycle
    }
}