Slim.tag('core-login', class extends Slim {
    username: any;
    password: any;
    message: any;

    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        let self = this;
        document.addEventListener('res:login', (ev: any) => {
            if (ev.detail.success) {
                self.message = "Login successful";
            }
            else {
                self.message = "Login failed";
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
            let password = this.password.value;
            if (password) {
                let a = '__' + password + '::0';
                for (let i = 0; i < 10; i++) {
                    a = sha512(a);
                }
                password = '::' + a;
            }
            App.login(this.username.value, password);
            this.message = 'Logging in ...';
            this.username.focus();
        }
        this.username.value = this.password.value = null;
    }

    disconnect(e) {
        e.preventDefault();
        App.disconnect('Disconnected from Login');
    }
});