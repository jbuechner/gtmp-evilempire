'use strict';
const ClientLifecycleState = {
    None: 0,
    Connected: 1,
    LoggedIn: 2
};

class Proxy {
    invoke(target, args) {
        resourceCall('invoke', JSON.stringify({target, args}));
    }

    relay(args) {
        document.dispatchEvent(new CustomEvent('relay', { detail: args }));
    }
}

class App {
    constructor() {
        this.proxy = new Proxy();
    }

    login(credentials) {
        this.proxy.invoke('app.login', { username: credentials.username, password: credentials.password });
    }

    disconnect() {
        this.proxy.invoke('app.disconnect', { reason: 'client disconnected.' });
    }
}

function relay(raw) {
    if (window.app) {
        let args = {};
        if (typeof raw === 'string') {
            args = JSON.parse(raw);
        }
        window.app.proxy.relay(args);
    }
}

window.app = new App();