using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Research.MaskVerification
{
	public sealed class ActionCommand : ICommand
	{
		#region Instance Fields

		private readonly Action<object> _actionToExecute;
		private bool _canActionExecute;

		#endregion

		#region Properties

		internal bool CanActionExecute
		{
			get { return _canActionExecute; }
			set
			{
				_canActionExecute = value;
				this.OnCanExecuteChanged();
			}
		}

		#endregion

		#region Constructors

		internal ActionCommand(Action<object> actionToExecute, bool canExecute)
		{
			// Checks
			if (actionToExecute == null)
			{
				throw new ArgumentNullException("actionToExecute");
			}

			_actionToExecute = actionToExecute;
			_canActionExecute = canExecute;
		}

		#endregion

		#region ICommand Implementation

		public bool CanExecute(object parameter)
		{
			return this.CanActionExecute;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			_actionToExecute(parameter);
		}

		#endregion

		#region Instance Methods

		private void OnCanExecuteChanged()
		{
			var handler = CanExecuteChanged;
			if (handler != null)
			{
				handler(this, new EventArgs());
			}
		}

		#endregion
	}
}
