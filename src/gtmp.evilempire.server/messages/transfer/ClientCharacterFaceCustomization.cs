using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.messages.transfer
{
    class ClientCharacterFaceCustomization
    {
        CharacterFaceCustomization characterFaceCustomization;

        public int ShapeFirst => characterFaceCustomization.ShapeFirst;
        public int ShapeSecond => characterFaceCustomization.ShapeSecond;
        public int SkinFirst => characterFaceCustomization.SkinFirst;
        public int SkinSecond => characterFaceCustomization.SkinSecond;
        public float ShapeMix => characterFaceCustomization.ShapeMix;
        public float SkinMix => characterFaceCustomization.SkinMix;

        public ClientCharacterFaceCustomization(CharacterFaceCustomization characterFaceCustomization)
        {
            this.characterFaceCustomization = characterFaceCustomization;
        }
    }
}
