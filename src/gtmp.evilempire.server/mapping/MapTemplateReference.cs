namespace gtmp.evilempire.server.mapping
{
    public class MapTemplateReference
    {
        public MapObjectType MapObjectType { get; }
        public string TemplateName { get; }

        public MapTemplateReference(MapObjectType mapObjectType, string templateName)
        {
            MapObjectType = mapObjectType;
            TemplateName = templateName;
        }
    }
}
