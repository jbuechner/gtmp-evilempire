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
        return `<div style="position:absolute;top:10%;right:0;">
<div class="box">

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