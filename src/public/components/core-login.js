Slim.tag('core-login', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.app = window.app;
        let self = this;
        document.addEventListener('relay', (ev) => {
            if (ev.detail.event === 'res:login') {
                if (ev.detail.status !== ClientLifecycleState.Success) {
                    self.message = ev.detail.data;
                }
            }
        });
    }

    onCreated() {
        this.message = '';
    }

    get template() {
        return `<form>
<div class="field">
  <label class="label">Name</label>
  <p class="control">
    <input class="input" type="text" slim-id="username" placeholder="User Name">
  </p>
  <label class="label">Password</label>
  <p class="control">
    <input class="input" type="password" slim-id="password" placeholder="Password">
  </p>
  </div>
<div class="field is-grouped">
  <p class="control">
    <button class="button is-primary" click="login">Login</button>
  </p>
  <p class="control">
    <button class="button" click="disconnect">Disconnect</button>
  </p>
</div>
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

    disconnect(e) {
        e.preventDefault();
        this.app.disconnect();
    }
});