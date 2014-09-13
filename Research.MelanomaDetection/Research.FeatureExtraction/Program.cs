using Research.Algorithms;
using Research.Algorithms.ImageFilters;
using Research.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.FeatureExtraction
{
	class Program
	{
		static void Main(string[] args)
		{

			foreach (DirectoryInfo directory in new DirectoryInfo(@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected").GetDirectories().SelectMany(o => o.GetDirectories()))
			{
				using (Bitmap dermotologyImage = new Bitmap(Path.Combine(directory.FullName, "base.jpg")))
				using (Bitmap skinImage = new Bitmap(Path.Combine(directory.FullName, "skin.jpg")))
				using (Bitmap lesionMaskImage = new Bitmap(Path.Combine(directory.FullName, "lesion.bmp")))
				{
					Color black = Color.FromArgb(0, 0, 0);
					MaskFromFunctionFilter lesionMaskkFromFunctionFilter = new MaskFromFunctionFilter(o => black == o);
					IMask lesionMask = lesionMaskkFromFunctionFilter.Apply(lesionMaskImage);

					Color white = Color.FromArgb(255, 255, 255);
					MaskFromFunctionFilter skinMaskFromFunctionFilter = new MaskFromFunctionFilter(o => white != o);
					IMask skinMask = skinMaskFromFunctionFilter.Apply(skinImage);

					Line primaryAxis = lesionMask.GetPrimaryAxisOfSymmetry();
					Line secondaryAxis = lesionMask.GetSecondaryAxisOfSymmetry();

					int primaryOverlap = lesionMask.GetNonOverlappingArea(primaryAxis);
					int secondaryOverlap = lesionMask.GetNonOverlappingArea(secondaryAxis);

					int area = lesionMask.Sum();
					int perimeter = lesionMask.Perimeter();

					double irregularityIndex = Math.Pow(perimeter, 2) / (4 * Math.PI * area);

					double edgeLightnessChange = lesionMask.EdgeAverageLightnessChange(dermotologyImage, skinMask.Average(dermotologyImage, (color) => color.GetLightness()));
					//double lesionAverageChroma = lesionMask.Average(dermotologyImage, (color) => color.GetChroma());
					//double skinAverageChroma = skinMask.Average(dermotologyImage, (color) => color.GetChroma());

					double varianceR = lesionMask.Variance(dermotologyImage, (color) => color.R);
					double varianceG = lesionMask.Variance(dermotologyImage, (color) => color.G);
					double varianceB = lesionMask.Variance(dermotologyImage, (color) => color.B);


					double varianceHue = lesionMask.Variance(dermotologyImage, (color) => color.GetHue());
					double varianceChroma = lesionMask.Variance(dermotologyImage, (color) => color.GetChroma());
					double varianceLightness = lesionMask.Variance(dermotologyImage, (color) => color.GetLightness());

					double averageR = lesionMask.Average(dermotologyImage, (color) => color.R);
					double averageG = lesionMask.Average(dermotologyImage, (color) => color.G);
					double averageB = lesionMask.Average(dermotologyImage, (color) => color.B);

					double averageHue = lesionMask.Average(dermotologyImage, (color) => color.GetHue());
					double averageLightness = lesionMask.Average(dermotologyImage, (color) => color.GetLightness());
					double averageChroma = lesionMask.Average(dermotologyImage, (color) => color.GetChroma());


					Color skinColor = Color.FromArgb(223, 207, 197);
					Color whitishColor = Color.FromArgb(241, 239, 231);
					Color blackishColor = Color.FromArgb(45, 0, 0);
					Color lightBrownOneColor = Color.FromArgb(184, 137, 138);
					Color blueOneColor = Color.FromArgb(115, 87, 115);
					Color blueTwoColor = Color.FromArgb(78, 15, 33);
					Color blueThreeColor = Color.FromArgb(140, 132, 174);
					Color lightBrownTwoColor = Color.FromArgb(191, 115, 86);
					Color darkBrownOneColor = Color.FromArgb(68, 1, 2);
					Color darkBrownTwoColor = Color.FromArgb(136, 41, 37);


					List<double> colorBuckets10 = lesionMask.PercentageCloseTo(dermotologyImage, new List<Color>
					{
						skinColor,
						whitishColor,
						blackishColor,
						lightBrownOneColor,
						blueOneColor,
						blueTwoColor,
						blueThreeColor,
						lightBrownTwoColor,
						darkBrownOneColor,
						darkBrownTwoColor,
					}, 0.1).ToList();

					List<double> colorBuckets20 = lesionMask.PercentageCloseTo(dermotologyImage, new List<Color>
					{
						skinColor,
						whitishColor,
						blackishColor,
						lightBrownOneColor,
						blueOneColor,
						blueTwoColor,
						blueThreeColor,
						lightBrownTwoColor,
						darkBrownOneColor,
						darkBrownTwoColor,
					}, 0.2).ToList();
					List<double> colorBuckets30 = lesionMask.PercentageCloseTo(dermotologyImage, new List<Color>
					{
						skinColor,
						whitishColor,
						blackishColor,
						lightBrownOneColor,
						blueOneColor,
						blueTwoColor,
						blueThreeColor,
						lightBrownTwoColor,
						darkBrownOneColor,
						darkBrownTwoColor,
					}, 0.3).ToList();



					using (MelanomaResearchEntities melanomaResearchEntities = new MelanomaResearchEntities())
					{
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "IrregularityIndex", Value = irregularityIndex, Path = directory.FullName });

						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "AssymetryPrimaryAxis", Value = primaryOverlap / (double)area, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "AssymetrySecondaryAxis", Value = secondaryOverlap / (double)area, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "EdgeLightnessChange", Value = edgeLightnessChange, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "VarianceR", Value = varianceR, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "VarianceG", Value = varianceG, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "VarianceB", Value = varianceB, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "VarianceHue", Value = varianceHue, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "VarianceChroma", Value = varianceChroma, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "VarianceLightness", Value = varianceLightness, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "AverageR", Value = averageR, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "AverageG", Value = averageG, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "AverageB", Value = averageB, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "AverageHue", Value = averageHue, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "AverageLightness", Value = averageLightness, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "AverageChroma", Value = averageChroma, Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "SkinColorBucket10", Value = colorBuckets10[0], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "WhitishColorBucket10", Value = colorBuckets10[1], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlackishColorBucket10", Value = colorBuckets10[2], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "LightBrownOneColorBucket10", Value = colorBuckets10[3], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueOneColorBucket10", Value = colorBuckets10[4], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueTwoColorBucket10", Value = colorBuckets10[5], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueThreeColorBucket10", Value = colorBuckets10[6], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "LightBrownTwoColorBucket10", Value = colorBuckets10[7], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "DarkBrownOneColorBucket10", Value = colorBuckets10[8], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "DarkBrownTwoColorBucket10", Value = colorBuckets10[9], Path = directory.FullName });

						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "SkinColorBucket20", Value = colorBuckets20[0], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "WhitishColorBucket20", Value = colorBuckets20[1], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlackishColorBucket20", Value = colorBuckets20[2], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "LightBrownOneColorBucket20", Value = colorBuckets20[3], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueOneColorBucket20", Value = colorBuckets20[4], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueTwoColorBucket20", Value = colorBuckets20[5], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueThreeColorBucket20", Value = colorBuckets20[6], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "LightBrownTwoColorBucket20", Value = colorBuckets20[7], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "DarkBrownOneColorBucket20", Value = colorBuckets20[8], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "DarkBrownTwoColorBucket20", Value = colorBuckets20[9], Path = directory.FullName });

						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "SkinColorBucket30", Value = colorBuckets30[0], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "WhitishColorBucket30", Value = colorBuckets30[1], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlackishColorBucket30", Value = colorBuckets30[2], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "LightBrownOneColorBucket30", Value = colorBuckets30[3], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueOneColorBucket30", Value = colorBuckets30[4], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueTwoColorBucket30", Value = colorBuckets30[5], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "BlueThreeColorBucket30", Value = colorBuckets30[6], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "LightBrownTwoColorBucket30", Value = colorBuckets30[7], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "DarkBrownOneColorBucket30", Value = colorBuckets30[8], Path = directory.FullName });
						melanomaResearchEntities.Features.Add(new Feature { FeatureName = "DarkBrownTwoColorBucket30", Value = colorBuckets30[9], Path = directory.FullName });
						melanomaResearchEntities.SaveChanges();
					}
				}
				Console.WriteLine(directory.FullName);
			}
			//using (Bitmap dermotologyImage = new Bitmap(@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll069\base.jpg"))
			//using (Bitmap skinImage = new Bitmap(@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll069\skin.jpg"))
			//using (Bitmap lesionMaskImage = new Bitmap(@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll069\lesion.bmp"))
			//{
			//	Color black = Color.FromArgb(0, 0, 0);
			//	MaskFromFunctionFilter lesionMaskkFromFunctionFilter = new MaskFromFunctionFilter(o => black == o);
			//	IMask lesionMask = lesionMaskkFromFunctionFilter.Apply(lesionMaskImage);

			//	Color white = Color.FromArgb(255, 255, 255);
			//	MaskFromFunctionFilter skinMaskFromFunctionFilter = new MaskFromFunctionFilter(o => white != o);
			//	IMask skinMask = skinMaskFromFunctionFilter.Apply(skinImage);

			//	Line primaryAxis = lesionMask.GetPrimaryAxisOfSymmetry();
			//	Line secondaryAxis = lesionMask.GetSecondaryAxisOfSymmetry();

			//	int primaryOverlap = lesionMask.GetNonOverlappingArea(primaryAxis);
			//	int secondaryOverlap = lesionMask.GetNonOverlappingArea(secondaryAxis);

			//	int area = lesionMask.Sum();
			//	int perimeter = lesionMask.Perimeter();

			//	double irregularityIndex = Math.Pow(perimeter, 2) / (4 * Math.PI * area);

			//	double edgeLightnessChange = lesionMask.EdgeAverageLightnessChange(dermotologyImage, skinMask.Average(dermotologyImage, (color) => color.GetLightness()));
			//	//double lesionAverageChroma = lesionMask.Average(dermotologyImage, (color) => color.GetChroma());
			//	//double skinAverageChroma = skinMask.Average(dermotologyImage, (color) => color.GetChroma());

			//	double varianceR = lesionMask.Variance(dermotologyImage, (color) => color.R);
			//	double varianceG = lesionMask.Variance(dermotologyImage, (color) => color.G);
			//	double varianceB = lesionMask.Variance(dermotologyImage, (color) => color.B);


			//	double varianceHue = lesionMask.Variance(dermotologyImage, (color) => color.GetHue());
			//	double varianceChroma = lesionMask.Variance(dermotologyImage, (color) => color.GetChroma());
			//	double varianceLightness = lesionMask.Variance(dermotologyImage, (color) => color.GetLightness());

			//	double averageHue = lesionMask.Average(dermotologyImage, (color) => color.GetHue());
			//	double averageLightness = lesionMask.Average(dermotologyImage, (color) => color.GetLightness());
			//	double averageChroma = lesionMask.Average(dermotologyImage, (color) => color.GetChroma());


			//	Color skinColor = Color.FromArgb(223, 207, 197);
			//	Color whitishColor = Color.FromArgb(241, 239, 231);
			//	Color blackishColor = Color.FromArgb(45, 0, 0);
			//	Color lightBrownOneColor = Color.FromArgb(184, 137, 138);
			//	Color blueOneColor = Color.FromArgb(115, 87, 115);
			//	Color blueTwoColor = Color.FromArgb(78, 15, 33);
			//	Color blueThreeColor = Color.FromArgb(140, 132, 174);
			//	Color lightBrownTwoColor = Color.FromArgb(191, 115, 86);
			//	Color darkBrownOneColor = Color.FromArgb(68, 1, 2);
			//	Color darkBrownTwoColor = Color.FromArgb(136, 41, 37);


			//	IEnumerable<double> colorBuckets = lesionMask.PercentageCloseTo(dermotologyImage, new List<Color>
			//	{
			//	skinColor,
			//	whitishColor,
			//	blackishColor,
			//	lightBrownOneColor,
			//	blueOneColor,
			//	blueTwoColor,
			//	blueThreeColor,
			//	lightBrownTwoColor,
			//	darkBrownOneColor,
			//	darkBrownTwoColor,
			//	}, 0.1);

			//	//IEnumerable<double> colorBuckets = lesionMask.PercentageBuckets(dermotologyImage, new List<Func<Color, bool>>
			//	//{
			//	//	(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 0 <= hue && hue < 20;
			//	//	},
			//	//	(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 20 <= hue && hue < 40;
			//	//	},
			//	//	(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 40 <= hue && hue < 60;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 60 <= hue && hue < 80;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 80 <= hue && hue < 100;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 100 <= hue && hue < 120;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 120 <= hue && hue < 140;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 140 <= hue && hue < 160;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 160 <= hue && hue < 180;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 180 <= hue && hue < 200;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 200 <= hue && hue < 220;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 220 <= hue && hue < 240;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 240 <= hue && hue < 260;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 260 <= hue && hue < 280;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 280 <= hue && hue < 300;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 300 <= hue && hue < 320;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 320 <= hue && hue < 340;
			//	//	},(color)=>
			//	//	{
			//	//		float hue  = color.GetHue();
			//	//		return 340 <= hue && hue <= 360;
			//	//	},(color)=>
			//	//	{
			//	//		float lightness  = color.GetLightness();
			//	//		return 0.80 <= lightness && lightness <= 1;
			//	//	}
			//	//	,(color)=>
			//	//	{
			//	//		float lightness  = color.GetLightness();
			//	//		return 0.0 <= lightness && lightness <= 0.20;
			//	//	}
			//	//});

			//	//using (Graphics graphics = Graphics.FromImage(bitmap))
			//	//{
			//	//	graphics.DrawLine(new Pen(Color.Red), primaryAxis.Point, new Point((int)newX, (int)newY));
			//	//}



			//	////bitmap.SetPixel(centerOfMass.X, centerOfMass.Y, Color.Red);
			//	//bitmap.Save(@"C:\LesionTest\temp111.bmp");
			//}
		}
	}
}
