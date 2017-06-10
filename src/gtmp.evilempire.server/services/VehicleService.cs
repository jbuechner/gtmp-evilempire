using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gtmp.evilempire.entities;

namespace gtmp.evilempire.server.services
{
    class VehicleService : IVehicleService
    {
        IDbService db;

        public VehicleService(IDbService db)
        {
            if (db == null)
            {
                throw new ArgumentNullException(nameof(db));
            }
            this.db = db;
        }

        public Vehicle CreateVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            var copy = new Vehicle(vehicle);
            copy.Id = db.NextInt64ValueFor(Constants.Database.Sequences.VehicleIdSequence);
            return copy;
        }
    }
}
