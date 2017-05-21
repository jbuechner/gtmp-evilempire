namespace gtmp.evilempire.services
{
    public interface IJsonSerializer
    {
        string Stringify(object value);
        dynamic Parse(string json);
    }
}
