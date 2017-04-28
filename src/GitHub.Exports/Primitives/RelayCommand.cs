using GitHub.Extensions;
using System;
using System.Windows.Input;

namespace GitHub.Primitives
{
    public class RelayCommand : ICommand
    {
        readonly Func<object, bool> canExecute;
        readonly Action<object> execute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            Guard.ArgumentNotNull(execute, nameof(execute));
            this.execute = execute;
            this.canExecute = canExecute ?? (_ => true);
        }

        public bool CanExecute(object parameter)
        {
            bool ret = false;
            try
            {
                ret = canExecute(parameter);
            }
            catch {}
            return ret;
        }

        public void Execute(object parameter)
        {
            var handler = CanExecuteChanged;
            handler?.Invoke(this, EventArgs.Empty);
            try
            {
                execute(parameter);
            }
            finally
            {
                handler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
