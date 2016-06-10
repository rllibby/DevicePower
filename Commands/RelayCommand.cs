/*
 *  Copyright © 2016, Russell Libby 
 */

using System;
using System.Windows.Input;
using Template10.Mvvm;

namespace DevicePower.Commands
{
    /// <summary>
    /// Relay command for bindings.
    /// </summary>
    public class RelayCommand : BindableBase, ICommand
    {
        #region Private fields

        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        public RelayCommand(Action execute) : this(execute, null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="canExecute">The function to check to see if execution is allowed.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Determines if the action can be executed.
        /// </summary>
        /// <param name="parameter">The object parameter.</param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return (_canExecute == null) ? true : _canExecute();
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        /// <param name="parameter">The object parameter.</param>
        public void Execute(object parameter)
        {
            _execute();
        }

        /// <summary>
        /// Raises the can execute change event to notify bindings of state change.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Event handler for state change notification.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        #endregion
    }
}
