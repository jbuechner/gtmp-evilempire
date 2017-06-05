using gtmp.evilempire.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.server.mapping
{
    class MapPed
    {
        public string TemplateName { get; }
        public int Hash { get; }
        public Vector3f Position { get; }
        public float Rotation { get; }
        public bool IsInvincible { get; }
        public bool IsPositionFrozen { get; set; }
        public bool IsCollisionless { get; set; }

        public MapDialogue Dialogue { get; set; }

        public string Title { get; set; }

        public bool IsTemplate
        {
            get
            {
                return !string.IsNullOrEmpty(TemplateName);
            }
        }

        public MapPed(string templateName, int hash, Vector3f position, float rotation, bool isInvincible)
        {
            TemplateName = templateName;
            Hash = hash;
            Position = position;
            Rotation = rotation;
            IsInvincible = isInvincible;
        }
    }
}
