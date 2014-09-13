using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.MaskSelection
{
	public abstract class ViewModel : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged Implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Instance Methods

		protected virtual void OnPropertyChanged(string propertyName)
		{
			// Checks
			if (propertyName == null)
			{
				throw new ArgumentNullException("propertyName");
			}
			if (string.IsNullOrWhiteSpace(propertyName))
			{
				throw new ArgumentException("The name of the property may not be an empty or whitespace string", "propertyName");
			}

			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}




		#endregion
	}
}
