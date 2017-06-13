using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.services
{
    public interface IWeatherService
    {

        Weather GetCurrentWeather();

        void SetWeather(Weather weather);


    }
}
