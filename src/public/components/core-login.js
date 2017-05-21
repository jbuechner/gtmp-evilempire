Slim.tag('core-login', class extends Slim {
    constructor() {
        super();
    }

    get isInteractive() {
        return true;
    }

    onBeforeCreated() {
        this.app = window.app;
        let self = this;
        document.addEventListener('relay', (ev) => {
            console.log(ev);
            if (ev.detail.target === 'login:response') {
                if (ev.detail.args.state !== ClientLifecycleState.Success) {
                    self.message = ev.detail.args.data;
                }
            }
        });
    }

    onCreated() {
        this.message = '';
    }

    get template() {
        return `<form>
    <div class="form-group">
        <label for="User name">User Name</label>
        <input type="text" class="form-control" slim-id="username" placeholder="User Name"
    </div>
    <div class="form-group">
        <label for="password">Password</label>
        <input type="password" class="form-control" slim-id="password" placeholder="Password">
    </div>
    <button type="submit" click="login" class="btn btn-default">Login</button>
    <button type="button" click="disconnect" class="btn btn-default">Disconnect</button>
    <p>&nbsp;</p>
    <p bind>[[message]]</p>
</form>`;
    }

    login(e) {
        e.preventDefault();
        if (this.username.value && this.password.value) {
            this.app.login({ username: this.username.value, password: this.password.value });
            this.message = 'Logging in ...';
            this.username.focus();
        }
        this.username.value = this.password.value = null;
    }

    disconnect() {
        this.app.disconnect();
    }
});