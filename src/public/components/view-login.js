Slim.tag('view-login', class extends Slim {
    get isInteractive() {
        return true;
    }

    onBeforeCreated() {
        this.app = window.app;
    }

    get template() {
        return `	<div>
		<img src="../gfx/bg-login.png" style="position: absolute; left: 25%; top: 45px;" />
	</div>
	<div class="container" style="position: absolute; width: calc(100% - 55px); left: 27px; top: 350px;">
		<div class="row">
			<div class="col-lg-6 col-lg-offset-3 bg-orange accent-4 fg-white">
				<h3>Login</h3>
			</div>
		</div>
		<div class="row">
			<div class="col-lg-6 col-lg-offset-3 bg-grey darken-4 fg-white">
				<span>&nbsp;</span>
				<p>
					GTA V Evil Empire Role Playing Server
				</p>
				<core-login></core-login>
			</div>
		</div>
	</div>`;
    }
});