using System;
using MEFedMVVM.Common;

namespace Lemon
{
    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action<object> executeMethod)
            : base(executeMethod)
        {

        }

        public DelegateCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod) 
            : base(executeMethod, canExecuteMethod)
        {
            
        }
    }
}
