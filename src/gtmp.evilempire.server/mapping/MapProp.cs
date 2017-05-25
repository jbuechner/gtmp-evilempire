using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.mapping
{
    public class MapProp
    {
        public string TemplateName { get; }
        public int Hash { get; }
        public Vector3f Position { get; }
        public Vector3f Rotation { get; }

        public bool IsTemplate
        {
            get
            {
                return !string.IsNullOrEmpty(TemplateName);
            }
        }

        public MapProp(string templateName, int hash, Vector3f position, Vector3f rotation)
        {
            TemplateName = templateName;
            Hash = hash;
            Position = position;
            Rotation = rotation;
        }
    }
}
