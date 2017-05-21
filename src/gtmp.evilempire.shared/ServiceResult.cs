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
        public ServiceResultState State { get; protected set; }
        public Exception Exception { get; protected set; }
        public object Data { get; protected set; }

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
                base.Data = value;
            }
        }

        ServiceResult()
        {
        }

        void SetBaseData(object data)
        {
            base.Data = data;
        }

        public ServiceResult(ServiceResultState state, T data, Exception exception)
            : base(state, data, exception)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public new static IServiceResult<T> AsError(string errorMessage)
        {
            var result = new ServiceResult<T> { State = ServiceResultState.Error };
            result.SetBaseData(errorMessage);
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static IServiceResult<T> AsSuccess(T data)
        {
            return new ServiceResult<T> { State = ServiceResultState.Success, Data = data };
        }
    }
}
