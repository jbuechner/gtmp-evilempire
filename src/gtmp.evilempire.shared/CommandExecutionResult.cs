using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire
{
    public class CommandExecutionResult
    {
        public bool Success { get; set; }
        public string ResponseMessage { get; set; }

        public CommandExecutionResult(bool success)
        {
            Success = success;
        }

        public CommandExecutionResult(bool success, string responseMessage)
        {
            Success = success;
            ResponseMessage = responseMessage;
        }
    }
}
