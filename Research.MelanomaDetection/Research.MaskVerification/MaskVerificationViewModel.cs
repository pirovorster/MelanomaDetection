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

namespace Research.MaskVerification
{
	public sealed class MaskVerificationViewModel : ViewModel
	{
		private readonly IEnumerator<DirectoryInfo> _melanomasToProcess;
		private Visibility _complete;
		private BitmapImage _image;
		private BitmapImage _mask;
		private BitmapImage _overlay;
		private BitmapImage _skin;
		private string _path;
		private readonly ICommand _selectedCommand;
		private readonly ICommand _cancelledCommand;
		private readonly ICommand _repairableCommand;
		internal MaskVerificationViewModel()
		{

			string selectionLocation = ConfigurationManager.AppSettings["SelectionLocation"];
			string cancelledLocation = ConfigurationManager.AppSettings["CancelledLocation"];
			string repairableLocation = ConfigurationManager.AppSettings["RepairableLocation"];
			Directory.CreateDirectory(selectionLocation);
			Directory.CreateDirectory(cancelledLocation);
			Directory.CreateDirectory(repairableLocation);

			IEnumerable<DirectoryInfo> melanomasToProcess = new DirectoryInfo(ConfigurationManager.AppSettings["DataLocation"]).GetDirectories().SelectMany(o => o.GetDirectories());

			IEnumerable<DirectoryInfo> melanomasProcessed = new DirectoryInfo(selectionLocation).GetDirectories()
				.Concat(new DirectoryInfo(cancelledLocation).GetDirectories())
				.Concat(new DirectoryInfo(repairableLocation).GetDirectories())
				.SelectMany(o => o.GetDirectories());

			_melanomasToProcess = melanomasToProcess.Where(o => !melanomasProcessed.Any(i => i.Name.ToUpperInvariant() == o.Name.ToUpperInvariant())).GetEnumerator();


			Complete = Visibility.Hidden;

			Action<string> copyFiles = (destination) =>
				{
					destination = Path.Combine(destination, _melanomasToProcess.Current.Parent.Name, _melanomasToProcess.Current.Name);
					Directory.CreateDirectory(destination);
					//Copy all the files & Replaces any files with the same name
					foreach (string file in Directory.GetFiles(_melanomasToProcess.Current.FullName, "*.*", SearchOption.TopDirectoryOnly))
						File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
				};

			_selectedCommand = new ActionCommand((o) =>
			{
				if (Complete == Visibility.Hidden)
				{
					copyFiles(selectionLocation);
					Next();
				}

			}, true);
			_cancelledCommand = new ActionCommand((o) =>
			{
				if (Complete == Visibility.Hidden)
				{
					copyFiles(cancelledLocation);
					Next();
				}

			}, true);
			_repairableCommand = new ActionCommand((o) =>
			{
				if (Complete == Visibility.Hidden)
				{
					copyFiles(repairableLocation);
					Next();
				}

			}, true);

			Next();


		}
		internal void Next()
		{
			if (_melanomasToProcess.MoveNext())
			{
				Mask = new BitmapImage(new Uri(Path.Combine(_melanomasToProcess.Current.FullName, "lesion.bmp"), UriKind.Absolute));
				Image = new BitmapImage(new Uri(Path.Combine(_melanomasToProcess.Current.FullName, "base.jpg"), UriKind.Absolute));
				Overlay = new BitmapImage(new Uri(Path.Combine(_melanomasToProcess.Current.FullName, "overlay.jpg"), UriKind.Absolute));
				Skin = new BitmapImage(new Uri(Path.Combine(_melanomasToProcess.Current.FullName, "skin.jpg"), UriKind.Absolute));

				FullPath = _melanomasToProcess.Current.FullName;
			}
			else
			{
				Complete = Visibility.Visible;
			}
		}

		public BitmapImage Overlay
		{
			get
			{
				return _overlay;
			}
			set
			{
				_overlay = value;
				OnPropertyChanged("Overlay");

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
		public BitmapImage Mask
		{
			get
			{
				return _mask;
			}
			set
			{
				_mask = value;
				OnPropertyChanged("Mask");

			}
		}

		public BitmapImage Skin
		{
			get
			{
				return _skin;
			}
			set
			{
				_skin = value;
				OnPropertyChanged("Skin");

			}
		}

		public string FullPath
		{
			get
			{
				return _path;
			}
			set
			{
				_path = value;
				OnPropertyChanged("FullPath");

			}
		}

		public ICommand SelectedCommand { get { return _selectedCommand; } }
		public ICommand CancelledCommand { get { return _cancelledCommand; } }
		public ICommand RepairableCommand { get { return _repairableCommand; } }
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
