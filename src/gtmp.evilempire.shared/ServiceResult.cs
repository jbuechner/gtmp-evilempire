using gtmp.evilempire.services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public class ServiceResult : IServiceResult
    {
        public ServiceResultState State { get; }
        public string Error { get; }

        public ServiceResult(ServiceResultState state, string error)
        {
            this.State = state;
            this.Error = error;
        }

        public static IServiceResult AsSuccess()
        {
            return new ServiceResult(ServiceResultState.Success, null);
        }

        public static IServiceResult AsError(Exception ex)
        {
            return new ServiceResult(ServiceResultState.Error, ex.ToString());
        }
    }
}
