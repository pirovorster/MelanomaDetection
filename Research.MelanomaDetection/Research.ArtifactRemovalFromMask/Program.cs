using Research.Algorithms;
using Research.Algorithms.ImageFilters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.ArtifactRemovalFromMask
{
	class Program
	{
		static void Main(string[] args)
		{
			MaskFromFunctionFilter maskFromFunctionFilter = new MaskFromFunctionFilter(o => o.R == 0 && o.G == 0 && o.B == 0);
			foreach (FileInfo maskFile in new DirectoryInfo(@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected").GetDirectories().SelectMany(o => o.GetDirectories()).SelectMany(o => o.GetFiles("*.bmp")))
			{
				Mask mask;
				using (Bitmap bitmap = new Bitmap(maskFile.FullName))
				{
					mask = maskFromFunctionFilter.Apply(bitmap).SplitBlobMask(200).OrderByDescending(o => o.Sum()).First().ToMask();

					int width = mask.Width;
					int height = mask.Height;

					for (int x = 0; x < width; x++)
					{
						mask.SetCell(x, 0, false);

						mask.SetCell(x, height-1,false);
					}

					for (int y = 0; y < height; y++)
					{

						mask.SetCell(0, y, false);

						mask.SetCell(width-1,y, false);
					}
				}
				using (Bitmap destinationBitmap = BitmapOperations.CreateFilledBitmap(mask.Width, mask.Height, Color.White))
				{
					FillMaskFilter fillLesionFilter = new FillMaskFilter(mask, Color.Black);
					fillLesionFilter.ApplyOnSource(destinationBitmap);
					destinationBitmap.Save(maskFile.FullName);
				}
				Console.WriteLine(maskFile.FullName);

			}


		}

		private static void RemoveArtifacts()
		{
			MaskFromFunctionFilter maskFromFunctionFilter = new MaskFromFunctionFilter(o => o.R == 0 && o.G == 0 && o.B == 0);

			foreach (FileInfo maskFile in new DirectoryInfo(ConfigurationManager.AppSettings["Source"]).GetDirectories().SelectMany(o => o.GetDirectories()).SelectMany(o => o.GetFiles("*.bmp")))
			{
				using (Bitmap bitmap = new Bitmap(maskFile.FullName))
				{

					using (Bitmap destinationBitmap = BitmapOperations.CreateFilledBitmap(bitmap.Width, bitmap.Height, Color.White))
					{

						string destiantion = Path.Combine(ConfigurationManager.AppSettings["Destination"], maskFile.Directory.Parent.Name, maskFile.Directory.Name);
						Directory.CreateDirectory(destiantion);

						IMask mask = maskFromFunctionFilter.Apply(bitmap);
						IMask legionMask = mask.MostCenterMask();

						legionMask.FillBlobMask(20000);

						FillMaskFilter fillLesionFilter = new FillMaskFilter(legionMask, Color.Black);
						using (Bitmap lesionCutout = fillLesionFilter.Apply(destinationBitmap))
						{
							lesionCutout.Save(Path.Combine(destiantion, "lesion.bmp"), ImageFormat.Bmp);
						}

						using (Bitmap lesion = new Bitmap(Path.Combine(maskFile.Directory.FullName, "base.jpg")))
						{
							OverlayMaskFilter overlayFilter = new OverlayMaskFilter(legionMask, Color.Green);
							using (Bitmap lesionOverlay = overlayFilter.Apply(lesion))
							{
								lesionOverlay.Save(Path.Combine(destiantion, "overlay.jpg"));
							}

							MaskFromFunctionFilter darkColorFilter = new MaskFromFunctionFilter(o => o.R < 50 && o.G < 50 && o.B < 50);
							IMask lightColors = darkColorFilter.Apply(lesion);

							FillMaskFilter fillNonSkinFilter = new FillMaskFilter(mask.Add(legionMask).Add(lightColors), Color.White);
							using (Bitmap blankoutNonSkin = fillNonSkinFilter.Apply(lesion))
							{
								blankoutNonSkin.Save(Path.Combine(destiantion, "skin.jpg"));
							}
						}


						File.Copy(Path.Combine(maskFile.Directory.FullName, "base.jpg"), Path.Combine(destiantion, "base.jpg"), true);
					}


				}
			}
		}
	}
}
