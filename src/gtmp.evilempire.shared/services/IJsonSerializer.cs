namespace gtmp.evilempire.services
{
    public interface IJsonSerializer
    {
        string Stringify(object o);
        dynamic Parse(string json);
    }
}
