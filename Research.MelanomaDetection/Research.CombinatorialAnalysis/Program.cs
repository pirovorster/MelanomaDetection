using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Research.Algorithms;

namespace Research.CombinatorialAnalysis
{
	class Program
	{
		private class Measurement
		{
			public string FileName { get; set; }

			public List<string> Features { get; set; }
			public List<string> Groups { get; set; }
			public int Unknowns { get; set; }

			public int TrueNegatives { get; set; }

			public int FalseNegatives { get; set; }

			public int TruePositives { get; set; }

			public int FalsePositives { get; set; }

			public double Accurancy { get { return (TrueNegatives + TruePositives) / (double)(FalsePositives + TruePositives + FalseNegatives + TrueNegatives + Unknowns); } }

		}

		static void Main(string[] args)
		{
			string path = @"D:\Studies 2014\Research\Code\Data\FeatureSelection2";
			List<FileInfo> fileInfos = new DirectoryInfo(path).GetFiles("*.txt").ToList();
			
			fileInfos.Remove(fileInfos.Last());

			List<Measurement> measurements = new List<Measurement>();


			foreach (FileInfo fileInfo in fileInfos)
			{
				Measurement measurement = new Measurement();

				List<string> lines = File.ReadAllLines(fileInfo.FullName).Where(o=>!string.IsNullOrWhiteSpace(o)).ToList();

				measurement.Features = lines.First().Split(',').ToList();

				lines.Reverse();

				measurement.Groups = new List<string>();
				if (measurement.Features.Contains("AssymetryPrimaryAxis"))
					measurement.Groups.Add("Symmetry");
				if (measurement.Features.Contains("IrregularityIndex"))
					measurement.Groups.Add("Border");
				if (measurement.Features.Contains("VarianceHue"))
					measurement.Groups.Add("ColourVariance");
				if (measurement.Features.Contains("AverageR"))
					measurement.Groups.Add("ColourAverage");
				if (measurement.Features.Contains("SkinColorBucket10"))
					measurement.Groups.Add("Buckets");
				

				measurement.Unknowns = int.Parse(lines.Skip(0).First().Split(':').Last().Trim());

				measurement.TrueNegatives = int.Parse(lines.Skip(1).First().Split(':').Last().Trim());

				measurement.FalseNegatives= int.Parse(lines.Skip(2).First().Split(':').Last().Trim());

				measurement.TruePositives = int.Parse(lines.Skip(3).First().Split(':').Last().Trim());

				measurement.FalsePositives = int.Parse(lines.Skip(4).First().Split(':').Last().Trim());

				measurement.FileName = fileInfo.FullName;

				measurements.Add(measurement);
			}

			//int a = measurements.Select(o => string.Join(",", o.Features)).Distinct().Count();
			//int b = measurements.Select(o => string.Join(",", o.Features)).Count();
			List<IEnumerable<string>> combosList = FeatureCombo().Where(o => o.Any() && !string.IsNullOrEmpty(o.First())).ToList();
			using (StreamWriter textWriter = new StreamWriter("OrderedCombinations2.txt", true))
			{
				foreach (Measurement measurement in measurements.OrderByDescending(o => o.Accurancy))
				{
					List<string> featureFilter = combosList.Select(o => string.Join(",", measurement.Features).ToUpper()).ToList();
					string features = string.Join(",", measurement.Features).ToUpper();
					if (featureFilter.Contains(features))
						textWriter.WriteLine("{7}\t{8}\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", string.Join(",", measurement.Features), measurement.TruePositives, measurement.TrueNegatives, measurement.FalsePositives, measurement.FalseNegatives, measurement.Unknowns, measurement.Accurancy, measurement.FileName, string.Join(",", measurement.Groups) );

				}
				textWriter.Flush();
			}

		}

		private static IEnumerable<IEnumerable<string>> FeatureCombo()
		{
			var lookups1 = new List<List<string>>
			{
			
				new List<string>
				{
					 "AssymetryPrimaryAxis",      
					"AssymetrySecondaryAxis"    
				},
				new List<string>
				{
					"IrregularityIndex",
					 "EdgeLightnessChange"  
				},
			
				new List<string>
				{
					 "VarianceHue",             
					 "VarianceChroma",
					 "VarianceLightness",
					 "VarianceR",             
					 "VarianceG",
					"VarianceB",  
				},
				new List<string>
				{
					"AverageR",            
					"AverageG",
					"AverageB",
					"AverageHue",                
					"AverageLightness",
					"AverageChroma" 
				},
			
				new List<string>
				{
					
					 "SkinColorBucket10",         
					 "WhitishColorBucket10",      
					 "BlackishColorBucket10",     
					 "LightBrownOneColorBucket10",
					 "BlueOneColorBucket10",      
					 "BlueTwoColorBucket10",      
					 "BlueThreeColorBucket10",    
					 "LightBrownTwoColorBucket10",
					 "DarkBrownOneColorBucket10", 
					 "DarkBrownTwoColorBucket10" 
				},
				
			
			};

			var lookups2 = new List<List<string>>
			{
				new List<string>
				{
					"IrregularityIndex"
				},
				new List<string>
				{
					 "AssymetryPrimaryAxis",      
					"AssymetrySecondaryAxis"    
				},
				new List<string>
				{
					 "EdgeLightnessChange"  
				},
			
				new List<string>
				{
					 "VarianceHue",             
					 "VarianceChroma",
					 "VarianceLightness",
					 "VarianceR",             
					 "VarianceG",
					"VarianceB",  
				},
				new List<string>
				{
					"AverageR",            
					"AverageG",
					"AverageB",
					"AverageHue",                
					"AverageLightness",
					"AverageChroma" 
				},
			
				new List<string>
				{
					"SkinColorBucket20",       
					 "WhitishColorBucket20",      
					 "BlackishColorBucket20",     
					 "LightBrownOneColorBucket20",
					 "BlueOneColorBucket20",      
					 "BlueTwoColorBucket20",      
					 "BlueThreeColorBucket20",    
					 "LightBrownTwoColorBucket20",
					 "DarkBrownOneColorBucket20", 
					 "DarkBrownTwoColorBucket20" 
				},
				
			
			};

			var lookups3 = new List<List<string>>
			{
				new List<string>
				{
					"IrregularityIndex"
				},
				new List<string>
				{
					 "AssymetryPrimaryAxis",      
					"AssymetrySecondaryAxis"    
				},
				new List<string>
				{
					 "EdgeLightnessChange"  
				},
			
				new List<string>
				{
					 "VarianceHue",             
					 "VarianceChroma",
					 "VarianceLightness",
					 "VarianceR",             
					 "VarianceG",
					"VarianceB",  
				},
				new List<string>
				{
					"AverageR",            
					"AverageG",
					"AverageB",
					"AverageHue",                
					"AverageLightness",
					"AverageChroma" 
				},
			
				new List<string>
				{
					"SkinColorBucket30",        
					 "WhitishColorBucket30",      
					 "BlackishColorBucket30",     
					 "LightBrownOneColorBucket30",
					 "BlueOneColorBucket30",      
					 "BlueTwoColorBucket30",      
					 "BlueThreeColorBucket30",    
					 "LightBrownTwoColorBucket30",
					 "DarkBrownOneColorBucket30", 
					 "DarkBrownTwoColorBucket30" 

				},
			
			};

			//string.Join(",", combos.OrderBy(o => o)

			var allCombos = lookups1.GetAllCombos().Select(o => o.SelectMany(i => i)).Concat(lookups2.GetAllCombos().Select(o => o.SelectMany(i => i))).Concat(lookups3.GetAllCombos().Select(o => o.SelectMany(i => i)));

			var allCombos2 = allCombos.Select(o => string.Join(",", o.OrderBy(i => i))).Distinct().Select(o => o.Split(',').ToList());

			return allCombos2;
		}
	}
}
