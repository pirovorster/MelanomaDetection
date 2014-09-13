using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Research.MaskSelection
{
	public sealed class MaskSelectionViewModel : ViewModel
	{
		private readonly IEnumerator<DirectoryInfo> _melanomasToProcess;
		private Visibility _complete;
		private BitmapImage _image;
		private IEnumerable<MaskLookupItem> _maskLookupItems;
		private readonly ICommand _saveCommand;
		private readonly ICommand _cancelCommand;
		internal MaskSelectionViewModel()
		{
			string destination = ConfigurationManager.AppSettings["DataDestination"];
			IEnumerable<DirectoryInfo> melanomasToProcess = new DirectoryInfo(ConfigurationManager.AppSettings["DataLocation"]).GetDirectories().SelectMany(o => o.GetDirectories());

			IEnumerable<DirectoryInfo> melanomasProcessed = new DirectoryInfo(ConfigurationManager.AppSettings["DataDestination"]).GetDirectories().SelectMany(o => o.GetDirectories());

			_melanomasToProcess = melanomasToProcess.Where(o => !melanomasProcessed.Any(i => i.Name.ToUpperInvariant() == o.Name.ToUpperInvariant())).GetEnumerator();

			Complete = Visibility.Hidden;
			_saveCommand = new ActionCommand((o) =>
			{
				if (Complete == Visibility.Hidden)
				{
					string chosen = (string)o;
					string type = _melanomasToProcess.Current.Parent.Name;
					string name = _melanomasToProcess.Current.Name;
					string maskSelectionDirectory = Path.Combine(destination, type, name);
					Directory.CreateDirectory(maskSelectionDirectory);
					File.Copy(Path.Combine(_melanomasToProcess.Current.FullName, "_base.jpg"), Path.Combine(maskSelectionDirectory, "base.jpg"));
					File.Copy(chosen, Path.Combine(maskSelectionDirectory, "mask.bmp"));
					Next();
				}

			}, true);

			_cancelCommand = new ActionCommand((o) =>
			{
				if (Complete == Visibility.Hidden)
				{
					string type = _melanomasToProcess.Current.Parent.Name;
					string name = _melanomasToProcess.Current.Name;
					Directory.CreateDirectory(Path.Combine(destination, type, name));
					Next();
				}

			}, true);

			Next();


		}
		internal void Next()
		{
			if (_melanomasToProcess.MoveNext())
			{
				MaskLookupItems = _melanomasToProcess.Current.GetFiles().Where(o => o.Name != "_base.jpg").Select(o => new MaskLookupItem(new BitmapImage(new Uri(o.FullName, UriKind.Absolute)), o.FullName)).ToList();
				Image = new BitmapImage(new Uri(Path.Combine(_melanomasToProcess.Current.FullName, "_base.jpg"), UriKind.Absolute));
			}
			else
			{
				Complete = Visibility.Visible;
			}
		}

		public BitmapImage Image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
				OnPropertyChanged("Image");

			}
		}



		public IEnumerable<MaskLookupItem> MaskLookupItems
		{
			get
			{
				return _maskLookupItems;
			}
			set
			{
				_maskLookupItems = value;
				OnPropertyChanged("MaskLookupItems");

			}
		}
		public ICommand SaveCommand { get { return _saveCommand; } }
		public ICommand CancelCommand { get { return _cancelCommand; } }
		public Visibility Complete
		{
			get
			{
				return _complete;
			}
			set
			{
				_complete = value;
				OnPropertyChanged("Complete");

			}
		}

	}
}
