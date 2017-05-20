Slim.tag('core-login', class extends Slim {
    constructor() {
        super();
    }

    get isInteractive() {
        return true;
    }

    onBeforeCreated() {
        this.app = window.app;
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
    <button type="button" click="login" class="btn btn-default">Login</button>
    <button type="button" click="disconnect" class="btn btn-default">Disconnect</button>
</form>`;
    }

    login() {
        this.app.login({ username: this.username.value, password: this.password.value });
    }

    disconnect() {
        this.app.disconnect();
    }
});