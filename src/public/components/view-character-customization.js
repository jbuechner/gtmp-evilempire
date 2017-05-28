Slim.tag('view-character-customization', class extends Slim {
    get isVirtual() { return false; }
    get isInteractive() { return true; }

    addIndexBasedSelection(methodStem, indexPropertyName, getItems, getCurrent, compareWithCurrentPredicate, makeDisplayName, makeChange) {
        this[indexPropertyName] = 0;
        try { this[indexPropertyName] = getCurrent() } catch(ex) { console.warn(ex) }

        let self = this;
        this['get' + methodStem] = function __getMethod() {
            let items = [];
            try { items = getItems() } catch(ex) { console.warn(ex) }
            try { self[indexPropertyName] = items.findIndex(compareWithCurrentPredicate) } catch(ex) { console.warn(ex) }
            self[indexPropertyName] = self[indexPropertyName] < 0 ? 0 : self[indexPropertyName];

            let index = self[indexPropertyName];
            if (typeof items === 'object' && index >= 0 && index < items.length) {
                return makeDisplayName(items[index], index);
            }
            return makeDisplayName(null, index);
        };

        let createChangeMethod = function __createChangeMethod(step) {
            return function __changeMethod(e) {
                e.preventDefault();
                let index = self[indexPropertyName];
                let items = [];
                try { items = getItems() } catch(ex) { console.warn(ex) }
                let newIndex = self.shiftInBounds(items, index, step);

                if (items.length > index) {
                    self.message = 'Contacting server to change data ...';
                    let item = items[newIndex];
                    makeChange(item);
                }
            };
        };

        this['previous' + methodStem] = createChangeMethod(-1);
        this['next' + methodStem] = createChangeMethod(1);
    }

    onBeforeCreated() {
        this.app = window.app;
        let self = this;

        this.addIndexBasedSelection('model', 'selectedModelIndex',
            () => this.customization.Models,
            () => this.current.ModelHash,
            p => p.Hash === this.current.ModelHash,
            (item, index) => item ? item.Name : '(n/a ' + index + ')',
            item => this.app.customizeCharacter('model', item.Hash)
        );
        this.addIndexBasedSelection('faceshapefirst', 'selectedFaceShapeFirst',
            () => this.customization.Faces,
            () => this.current.Face.ShapeFirst,
            p => p.Id === this.current.Face.ShapeFirst,
            (item, index) => {
                return item ? 'Mother ' + item.Id : '(n/a ' + index + ')'
            },
            item => this.app.customizeCharacter('face::shapeFirst', item.Id)
        );
        this.addIndexBasedSelection('faceshapesecond', 'selectedFaceShapeSecond',
            () => this.customization.Faces,
            () => this.current.Face.ShapeSecond,
            p => p.Id === this.current.Face.ShapeSecond,
            (item, index) => {
                return item ? 'Father ' + item.Id : '(n/a ' + index + ')'
            },
            item => this.app.customizeCharacter('face::shapeSecond', item.Id)
        );
        this.addIndexBasedSelection('faceskinfirst', 'selectedFaceskinFirst',
            () => this.customization.Faces,
            () => this.current.Face.SkinFirst,
            p => p.Id === this.current.Face.SkinFirst,
            (item, index) => {
                return item ? 'Mother ' + item.Id : '(n/a ' + index + ')'
            },
            item => this.app.customizeCharacter('face::skinFirst', item.Id)
        );
        this.addIndexBasedSelection('faceskinsecond', 'selectedFaceskinSecond',
            () => this.customization.Faces,
            () => this.current.Face.SkinSecond,
            p => p.Id === this.current.Face.SkinSecond,
            (item, index) => {
                return item ? 'Father ' + item.Id : '(n/a ' + index + ')'
            },
            item => this.app.customizeCharacter('face::skinSecond', item.Id)
        );


        document.addEventListener('res:customizeChar', (ev) => {
            if (ev.detail.success) {
                self.message = "Change received";
                console.log(ev.detail);
            }
            else {
                self.message = "Change failed";
            }
            if (ev.detail.CharacterCustomization) {
                this.current = ev.detail.CharacterCustomization;

                this.shapeMixInput.value = Math.round(this.current.Face.ShapeMix * 100);
                this.skinMixInput.value = Math.round(this.current.Face.SkinMix * 100);
            }
            setTimeout(() => self.message = '', 1000);
        });
    }

    onCreated() {
        this.message = '';
        this.customization = { Models: [], Faces: [] };
        this.current = {};
    }

    getShapeMix(a) {
        try {
            return Math.round(this.current.Face.ShapeMix * 100);
        } catch(ex) {
            console.warn(ex);
        }
        return 0;
    }

    getSkinMix(a) {
        try {
            return Math.round(this.current.Face.SkinMix * 100);
        } catch(ex) {
            console.warn(ex);
        }
        return 0;
    }

    shapeMixChanged(e) {
        e.preventDefault();
        self.message = 'Contacting server to change data ...';
        this.app.customizeCharacter('face::shapeMix', this.shapeMixInput.value);

    }

    skinMixChanged(e) {
        e.preventDefault();
        self.message = 'Contacting server to change data ...';
        this.app.customizeCharacter('face::skinMix', this.skinMixInput.value);
    }

    shiftInBounds(array, index, delta) {
        if (index + delta > array.length - 1) {
            return 0;
        }
        if (index + delta < 0) {
            return array.length - 1;
        }
        return index + delta;
    }

    get template() {
        return `<div style="position:absolute;top:10%;right:0;max-width:300px;width:300px;">
<div class="box">
<div class="fg-grey darken-3">
    <h5 class="title is-5">Character Customization</h5>
</div>

<div>
Gender (Model)
</div>
<div class="field has-addons">
  <p class="control">
    <a class="button" click="previousmodel">
      &lt;
    </a>
  </p>
  <p class="control">
    <input class="input" type="text" placeholder="Gender" disabled="disabled" value="[[getmodel(customization.Models, current, selectedModelIndex)]]">
  </p>
  <p class="control">
    <a class="button" click="nextmodel">
      &gt;
    </a>
  </p>
</div>
<div style="font-size: 12px;padding-bottom:9px;">
    Please bear in mind that this is only your visual representation. It does not change your In-Character gender at all.
</div>

<div>
Shape
</div>
<div class="field has-addons">
  <p class="control">
    <a class="button" click="previousfaceshapefirst">
      &lt;
    </a>
  </p>
  <p class="control">
    <input class="input" type="text" placeholder="Gender" disabled="disabled" value="[[getfaceshapefirst(customization.Faces, current, selectedFaceShapeFirst)]]">
  </p>
  <p class="control">
    <a class="button" click="nextfaceshapefirst">
      >
    </a>
  </p>
</div>

<div class="field has-addons">
  <p class="control">
    <a class="button" click="previousfaceshapesecond">
      &lt;
    </a>
  </p>
  <p class="control">
    <input class="input" type="text" placeholder="Gender" disabled="disabled" value="[[getfaceshapesecond(customization.Faces, current, selectedFaceShapeSecond)]]">
  </p>
  <p class="control">
    <a class="button" click="nextfaceshapesecond">
      >
    </a>
  </p>
</div>

<div class="field">
  <p class="control">
    <input slim-id="shapeMixInput" class="input" type="text" placeholder="Shape Mix" value="[[getShapeMix(current.Face.ShapeMix)]]" change="shapeMixChanged">
  </p>
</div>
<div style="font-size: 12px;padding-bottom:9px;">
    You can mix the values between mother and father using a number between 0 and 100 (0 all traits of the mother, 100 all trais of the father).
</div>


<div>
Skin
</div>
<div class="field has-addons">
  <p class="control">
    <a class="button" click="previousfaceskinfirst">
      &lt;
    </a>
  </p>
  <p class="control">
    <input class="input" type="text" placeholder="Gender" disabled="disabled" value="[[getfaceskinfirst(customization.Faces, current, selectedFaceskinFirst)]]">
  </p>
  <p class="control">
    <a class="button" click="nextfaceskinfirst">
      >
    </a>
  </p>
</div>

<div class="field has-addons">
  <p class="control">
    <a class="button" click="previousfaceskinsecond">
      &lt;
    </a>
  </p>
  <p class="control">
    <input class="input" type="text" placeholder="Gender" disabled="disabled" value="[[getfaceskinsecond(customization.Faces, current, selectedFaceskinSecond)]]">
  </p>
  <p class="control">
    <a class="button" click="nextfaceskinsecond">
      >
    </a>
  </p>
</div>

<div class="field">
  <p class="control">
    <input slim-id="skinMixInput" class="input" type="text" placeholder="Skin Mix" value="[[getSkinMix(current.Face.SkinMix)]]" change="skinMixChanged">
  </p>
</div>
<div style="font-size: 12px;padding-bottom:9px;">
    You can mix the values between mother and father using a number between 0 and 100 (0 all traits of the mother, 100 all trais of the father).
</div>

<p bind>[[message]]</p>

<div class="field is-grouped">
  <p class="control">
    <button class="button is-primary" click="ok">OK</button>
  </p>
  <p class="control">
    <button class="button" click="cancel">Cancel</button>
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