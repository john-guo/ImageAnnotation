using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Demo
{
    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action<object> execute)
                       : base(execute)
        {
        }

        public DelegateCommand(Action<object> execute,
                       Predicate<object> canExecute) : base(execute, canExecute)
        {
        }
    }

    public interface IDelegateCommand
    {
        Predicate<object> CanExecuteHandler { get; set; }
        Action<object> PrepareHandler { get; set; }
        Action<object> CompletionHandler { get; set; }

        void RaiseCanExecuteChanged();
    }

    public class DelegateCommand<T> : ICommand, IDelegateCommand
    {
        private Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> execute)
                       : this(execute, null)
        {
        }

        public DelegateCommand(Action<T> execute,
                       Predicate<T> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public Predicate<object> CanExecuteHandler
        {
            get
            {
                return _canExecute as Predicate<object>;
            }
            set
            {
                _canExecute = value as Predicate<T>;
            }
        }

        public Action<object> PrepareHandler { get; set; }
        public Action<object> CompletionHandler { get; set; }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return true;
            }

            return _canExecute(parameter == null ? default(T) : (T)parameter);
        }

        public void Execute(object parameter)
        {
            PrepareHandler?.Invoke(parameter);
            _execute((T)parameter);
            CompletionHandler?.Invoke(parameter);
            RaiseCanExecuteChanged();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
