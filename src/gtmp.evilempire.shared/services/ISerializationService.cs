using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.services
{
    public interface ISerializationService
    {
        string SerializeAsDesignatedJson(object o);
        object DeserializeFromDesignatedJson(string s);

        string DecorateAsJson(string value);
    }
}
