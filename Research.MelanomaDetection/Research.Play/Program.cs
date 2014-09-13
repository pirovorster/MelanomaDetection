using Accord.MachineLearning;
using AForge.Imaging.Filters;
using Research.Algorithms;
using Research.Algorithms.ImageFilters;
using Research.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Play
{
	class Program
	{

		static void Main(string[] args)
		{


			using (MelanomaResearchEntities melanomaResearchEntities = new MelanomaResearchEntities())
			{
				var featuresPerImages = melanomaResearchEntities.Features.Where(o => !o.FeatureName.Contains("Bucket")).GroupBy(o => o.Path).ToList();
				var imagesPerFeatures = melanomaResearchEntities.Features.Where(o => !o.FeatureName.Contains("Bucket")).GroupBy(o => o.FeatureName).ToList();

				List<double[]> imagesForClustering = new List<double[]>();

				foreach (var melanoma in featuresPerImages)
				{
					List<double> normalizedFeatures = new List<double>();
					foreach (var imagesPerFeature in imagesPerFeatures)
					{
						double normalizationValue = imagesPerFeature.Max(o => o.Value);
						normalizedFeatures.Add(melanoma.First(o => o.FeatureName.Equals(imagesPerFeature.Key, StringComparison.OrdinalIgnoreCase)).Value / normalizationValue);
					}

					imagesForClustering.Add(normalizedFeatures.ToArray());
					normalizedFeatures.Add(1);

				}
				bool shouldContinue = true;
				while (shouldContinue)
				{
					KMeans kmeans = new KMeans(10);

					int[] labels = kmeans.Compute(imagesForClustering.ToArray());
					Dictionary<string, int> clusters = new Dictionary<string, int>();
					for (int index = 0; index < featuresPerImages.Count; index++)
					{
						clusters.Add(featuresPerImages[index].Key, labels[index]);
					}

					var a = clusters.Where(o => !o.Key.Contains("Melanoma")).Select(o=>o.Key).Distinct().Count();

					var fakeMelanoma = clusters.Where(o => o.Key.Contains("Melanoma")).GroupBy(o => o.Value).OrderByDescending(o => o.Count()).Take(4);
					var fakeBenign = clusters.Where(o => !o.Key.Contains("Melanoma")).GroupBy(o => o.Value).OrderByDescending(o => o.Count()).Take(5);
					var fake = clusters.Where(i => !fakeBenign.Select(o => o.Key).Contains(i.Value) && !fakeMelanoma.Select(o => o.Key).Contains(i.Value));
					
					if(fakeMelanoma.Select(o=>o.Key).Intersect(fakeBenign.Select(o=>o.Key)).Count()==0)
					{
						shouldContinue = false;

						string mel = string.Join("\n",fakeMelanoma.SelectMany(o=>o).Select(o=>o.Key));

						string ben = string.Join("\n", fakeBenign.SelectMany(o => o).Select(o => o.Key));
					}
				}
			}



			// Create a new K-Means algorithm with 3 clusters 

			// Compute the algorithm, retrieving an integer array
			//  containing the labels for each of the observations

			//DirectoryInfo directory = new DirectoryInfo(@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal062");
			//using (Bitmap dermotologyImage = new Bitmap(Path.Combine(directory.FullName, "base.jpg")))
			//using (Bitmap lesionMaskImage = new Bitmap(Path.Combine(directory.FullName, "lesion.bmp")))
			//{
			//	Color black = Color.FromArgb(0, 0, 0);
			//	MaskFromFunctionFilter lesionMaskkFromFunctionFilter = new MaskFromFunctionFilter(o => black == o);
			//	IMask lesionMask = lesionMaskkFromFunctionFilter.Apply(lesionMaskImage);

			//	Grayscale gray = new Grayscale(1.0 / 3, 1.0 / 3, 1.0 / 3);

			//	using (Bitmap grayImage = gray.Apply(dermotologyImage))
			//	{

			//		SobelEdgeDetector filter = new SobelEdgeDetector();
			//		filter.ApplyInPlace(grayImage);
			//		grayImage.Save(@"c:\temp\test.jpg");

			//		Threshold threshold = new Threshold(50);
			//		threshold.ApplyInPlace(grayImage);

			//		grayImage.Save(@"c:\temp\test.jpg");
			//	}
			//}
		}
	}
}
