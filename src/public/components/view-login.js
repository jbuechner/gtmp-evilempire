Slim.tag('view-login', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    get template() {
        return `<div><img src="gfx/bg-login.png" style="position: absolute; left: 25%; top: 45px;" /></div>
	    <div style="position: absolute; width: 932px; left: calc(50% - 466px); top: 350px; border-radius: 0 0 8px 8px; border: solid 1px #e9e9e9; border-top: 0;">
	        <div class="bg-teal lighten-2 fg-white" style="padding: 0.5em;"><h3>Login</h3></div>
	        <div class="bg-grey lighten-5" style="padding: 2em">
                <core-login></core-login>	        
            </div>
		</div>
	</div>`;
    }
});