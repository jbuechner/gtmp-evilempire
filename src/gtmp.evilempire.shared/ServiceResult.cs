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
        public ServiceResultState State { get; private set; }
        public Exception Exception { get; private set; }
        public object Data { get; private set; }

        protected ServiceResult()
        {
        }

        public ServiceResult(ServiceResultState state, object data, Exception exception)
        {
            this.State = state;
            this.Data = data;
            this.Exception = exception;
        }

        public static IServiceResult AsSuccess()
        {
            return new ServiceResult { State = ServiceResultState.Success };
        }

        public static IServiceResult AsError(Exception ex)
        {
            return new ServiceResult { State = ServiceResultState.Error, Exception = ex };
        }

        public static IServiceResult AsError(string errorMessage)
        {
            return new ServiceResult { State = ServiceResultState.Error, Data = errorMessage };
        }
    }

    public class ServiceResult<T> : ServiceResult, IServiceResult<T>
    {
        public new T Data
        {
            get
            {
                return (T)base.Data;
            }
            private set
            {
                this.Data = value;
            }
        }

        ServiceResult()
        {
        }

        public ServiceResult(ServiceResultState state, T data, Exception exception)
            : base(state, data, exception)
        {
        }
    }
}
