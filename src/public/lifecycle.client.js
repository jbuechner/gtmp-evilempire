'use strict';
function define(module)  {
    const ClientLifecycleState = {
        None: 0,
        Connected: 1,
        LoggedIn: 2
    };

    class ClientLifecycle {
        constructor() {
            this._transitions = new Map([
                [ClientLifecycle.getStateTransitionKey(ClientLifecycleState.None, ClientLifecycleState.Connected), ClientLifecycle.onClientConnected],
                [ClientLifecycle.getStateTransitionKey(ClientLifecycleState.Connected, ClientLifecycleState.LoggedIn), ClientLifecycle.onClientLoggedIn]
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
            app.browser.show();
            app.client.cursor = true;
        }

        static onClientLoggedIn(app) {
            app.client.cursor = false;
            app.client.resetCamera();
            app.browser.hide();
        }
    }

    module.exports = {
        ClientLifecycleState,
        ClientLifecycle
    }
}