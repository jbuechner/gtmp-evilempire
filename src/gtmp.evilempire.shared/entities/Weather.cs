using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.entities
{
    public class Weather
    {
        public override string ToString()
        {
            return Name;
        }

        public string Name { get; set; }

        public int WeatherId { get; set; }

        public Weather(string name, int weatherid)
        {
            Name = name;
            WeatherId = weatherid;
        } 


    }
}
