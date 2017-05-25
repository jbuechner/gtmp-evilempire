using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.mapping
{
    public class MapVehicle
    {
        public string TemplateName { get; set; }
        public int Hash { get; }
        public Vector3f Position { get; }
        public Vector3f Rotation { get; }

        public int Color1 { get; }
        public int Color2 { get; }

        public bool IsTemplate
        {
            get
            {
                return !string.IsNullOrEmpty(TemplateName);
            }
        }

        public MapVehicle(string templateName, int hash, Vector3f position, Vector3f rotation, int color1, int color2)
        {
            TemplateName = templateName;
            Hash = hash;
            Position = position;
            Rotation = rotation;
            Color1 = color1;
            Color2 = color2;
        }
    }
}
