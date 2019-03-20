using System;
using System.Windows.Input;

namespace GlitchedPolygons.GlitchedEpistle.Client.Windows.Commands
{
    /// <summary>
    /// <see cref="ICommand"/> that takes an <see cref="Action"/> for the <see cref="ICommand.Execute"/> method
    /// and a <see cref="Func{T, TResult}"/> (where <c>T</c> is an <see cref="object"/> and <c>TResult</c> a <see cref="bool"/>)
    /// for the <see cref="ICommand.CanExecute"/> check.<para> </para>
    /// Implements the <see cref="System.Windows.Input.ICommand"/> interface.
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> execution;
        private readonly Func<object, bool> executionCheck;

        /// <summary>
        /// Is raised when changes occur that affect whether or not the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="execution">The <see cref="ICommand.Execute"/> action.</param>
        /// <param name="executionCheck">The <see cref="ICommand.CanExecute"/> check delegate (can be <see langword="null"/>; in that case <see cref="ICommand.CanExecute"/> is always <see langword="true"/>).</param>
        public DelegateCommand(Action<object> execution, Func<object, bool> executionCheck = null)
        {
            this.execution = execution;
            this.executionCheck = executionCheck;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null"/>.</param>
        public void Execute(object parameter)
        {
            execution?.Invoke(parameter);
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        /// <returns><see langword="true" /> if this command can be executed; otherwise, <see langword="false"/>.</returns>
        public bool CanExecute(object parameter)
        {
            return executionCheck?.Invoke(parameter) ?? true;
        }

        /// <summary>
        /// Invokes the <see cref="CanExecuteChanged"/> <see langword="event"/> (should be done when the command execution conditions have changed).
        /// </summary>
        public void InvokeCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}