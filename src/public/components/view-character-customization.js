Slim.tag('view-character-customization', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    onBeforeCreated() {
        this.app = window.app;
        let self = this;
        document.addEventListener('res:customizeChar', (ev) => {
            if (ev.detail.success) {
                self.message = "Change received";
                console.log(ev.detail);

                this.current = ev.detail.CharacterCustomization;
            }
            else {
                self.message = "Change failed";
            }
            setTimeout(() => self.message = '', 1000);
        });
    }

    onCreated() {
        this.message = '';
        this.customization = { Models: [], Faces: [] };
        this.current = {};
        this.selectedModelIndex = 0;
    }

    getModelName(a, b, c) {
        this.selectedModelIndex = this.customization.Models.findIndex(p => p.Hash === this.current.ModelHash);
        if (typeof this.customization.Models === 'object' && this.customization.Models.length && this.selectedModelIndex >= 0 && this.selectedModelIndex < this.customization.Models.length) {
            return this.customization.Models[this.selectedModelIndex].Name;
        }
        return '(' + this.selectedModelIndex + ')';
    }

    selectPreviousModel(e) {
        e.preventDefault();
        let newIndex = this.shiftInBounds(this.customization.Models, this.selectedModelIndex, -1);
        this.message = "Changing ...";
        this.app.customizeCharacter('model', this.customization.Models[newIndex].Hash);
    }

    selectNextModel(e) {
        e.preventDefault();
        let newIndex = this.shiftInBounds(this.customization.Models, this.selectedModelIndex, 1);
        this.message = "Changing ...";
        this.app.customizeCharacter('model', this.customization.Models[newIndex].Hash);
    }

    shiftInBounds(array, index, delta) {
        if (index + delta > array.length) {
            return 0;
        }
        if (index + delta < 0) {
            return array.length - 1;
        }
        return index + delta;
    }

    get template() {
        return `<div style="position:absolute;top:10%;right:0;">
<div class="box">

<div class="field has-addons">
  <p class="control">
    <a class="button" click="selectPreviousModel">
      &lt;
    </a>
  </p>
  <p class="control">
    <input class="input" type="text" placeholder="Gender" disabled="disabled" value="[[getModelName(customization.Models, current, selectedModelIndex)]]">
  </p>
  <p class="control">
    <a class="button" click="selectNextModel">
      &gt;
    </a>
  </p>
</div>

<p bind>[[message]]</p>

<div class="field is-grouped">
  <p class="control">
    <button class="button is-primary" click="ok">OK</button>
  </p>
  <p class="control">
    <button class="button" click="cancel">Abbrechen</button>
  </p>
</div>

</div>
</div>`;
    }

    ok(e) {
        e.preventDefault();
        this.app.confirmCharacterCustomization('Disconnected from Login');
    }

    cancel(e) {
        e.preventDefault();
        this.app.cancelCharacterCustomization('Disconnected from Login');
    }
});