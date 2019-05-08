using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseWork.Commands
{
    class DelegateCommand : ICommand
    {
        Action<object> execute;
        Func<object, bool> canExecute;
        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> exec, Func<object, bool> canExec = null)
        {
            execute = exec;
            canExecute = canExec;
        }
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        public void Execute(object param) => execute(param);
        public bool CanExecute(object param) => canExecute?.Invoke(param) ?? true;
    }
}
