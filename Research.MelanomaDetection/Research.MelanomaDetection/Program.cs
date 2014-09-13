using Research.Algorithms.ImageFilters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.MelanomaDetection
{
	class Program
	{
		static void Main(string[] args)
		{



			foreach (DirectoryInfo lesionType in new DirectoryInfo(@"C:\Users\Piro\Desktop\wget\~sumbaug\Borders Complete").GetDirectories("Dermal Nevus"))
			{
				foreach (FileInfo lesion in lesionType.GetFiles("Nml051.jpg"))
				{
					using (Bitmap image = new Bitmap(lesion.FullName))
					{

						string destinationDirectory = Path.Combine(@"D:\Studies 2014\Research\Code\Images\ForReport", lesionType.Name, Path.GetFileNameWithoutExtension(lesion.FullName));
						Directory.CreateDirectory(destinationDirectory);

						image.Save(Path.Combine(destinationDirectory, string.Format("{0}.jpg", "_base")));

						for (int threshold = 100; threshold <= 210; threshold += 10)
						{
							

							using (Bitmap newImage = image.ThresholdSegmentation(threshold))
							{

								newImage.Save(Path.Combine(destinationDirectory, string.Format("{0}.bmp", threshold.ToString())), ImageFormat.Bmp);
							}

							Console.WriteLine("{0}: {1}", threshold, lesion.FullName);
						}
					}
				}
			}

		}
	}
}
