'use strict';
const ClientLifecycleState = {
    None: 0,
    Connected: 1,
    LoggedIn: 2
};

class Proxy {
    invoke(target, args) {
        resourceCall('cef_invoke', JSON.stringify({target, args}));
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

function addView(args) {
    if (typeof args !== 'object') {
        args = JSON.parse(args);
    }
    console.log('addView', args);
    let el = document.createElement(args.selector);
    document.body.appendChild(el);
}

function removeView(args) {
    args = JSON.parse(args);
    let elements = document.querySelectorAll(args.selector);
    for (let i = 0; i < elements.length; i++) {
         let el = elements[i];
         if (el && el.parentNode) {
             el.parentNode.removeChild(el);
         }
    }
}

window.app = new App();