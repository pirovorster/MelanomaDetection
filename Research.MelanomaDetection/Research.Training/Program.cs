using Research.Algorithms;
using Research.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Research.Training
{
	class Program
	{
		private class Experiment
		{
			public List<string> BenignTrainingSet { get; set; }
			public List<string> MelanomaTrainingSet { get; set; }
			public List<string> BenignTestSet { get; set; }
			public List<string> MelanomaTestSet { get; set; }
		}
		private static Func<double[], NeuralNetwork> GetNetworkFactory(int inputNumber, int hiddenNumber, int outputNumber)
		{
			Func<double, double> sigmoid = (number) =>
				   1 / (1 + Math.Pow(Math.E, -number));

			return (weights) =>
		   {
			   double[,] hiddenWeightsMatrix = new double[inputNumber, hiddenNumber];
			   double[,] outputWeightsMatrix = new double[hiddenNumber, outputNumber];
			   double[] hiddenBiasWeightsMatrix = new double[hiddenNumber];
			   double[] outputBiasWeightsMatrix = new double[outputNumber];

			   for (int weightIndex = 0; weightIndex < weights.Length; weightIndex++)
			   {
				   if (weightIndex < inputNumber * hiddenNumber)
				   {
					   int column = weightIndex / hiddenNumber;
					   int row = weightIndex % hiddenNumber;
					   hiddenWeightsMatrix[column, row] = weights[weightIndex];
				   }
				   else if (weightIndex < inputNumber * hiddenNumber + (hiddenNumber * outputNumber))
				   {
					   int realIndex = weightIndex - inputNumber * hiddenNumber;
					   int column = realIndex / outputNumber;
					   int row = realIndex % outputNumber;
					   outputWeightsMatrix[column, row] = weights[weightIndex];
				   }
				   else if (weightIndex < inputNumber * hiddenNumber + (hiddenNumber * outputNumber) + hiddenNumber)
				   {
					   int realIndex = weightIndex - (inputNumber * hiddenNumber + (hiddenNumber * outputNumber));
					   hiddenBiasWeightsMatrix[realIndex] = weights[weightIndex];
				   }
				   else
				   {
					   int realIndex = weightIndex - (inputNumber * hiddenNumber + (hiddenNumber * outputNumber) + hiddenNumber);
					   outputBiasWeightsMatrix[realIndex] = weights[weightIndex];
				   }

			   }

			   return new NeuralNetwork(inputNumber, new List<NeuralNetworkLayer> { new NeuralNetworkLayer(hiddenWeightsMatrix, hiddenBiasWeightsMatrix, sigmoid) }, new NeuralNetworkLayer(outputWeightsMatrix, outputBiasWeightsMatrix, sigmoid));
		   };
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

			
			
	
			//string.Join(",", combos.OrderBy(o => o)

			var allCombos = lookups1.GetAllCombos().Select(o => o.SelectMany(i => i));

			var allCombos2 = allCombos.Select(o => string.Join(",", o.OrderBy(i => i))).Distinct().Select(o => o.Split(',').ToList());

			return allCombos2;
		}


		static void Main(string[] args)
		{
			int startInc = int.Parse(args[0]);

			int endExclude = int.Parse(args[1]);

			Random ran = new Random(DateTime.Now.Millisecond);


			IFormatter formatter = new BinaryFormatter();
			Stream stream = new FileStream("Features.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
			List<Feature> allFeatures = (List<Feature>)formatter.Deserialize(stream);
			stream.Close();

			List<string> features = allFeatures.Select(o => o.FeatureName).Distinct().ToList();
			List<IEnumerable<string>> combosList = FeatureCombo().Where(o => o.Any() && !string.IsNullOrEmpty(o.First())).ToList();

			//List<string> alreadyProcessed = new List<string>();
			//foreach(FileInfo fileInfo in new DirectoryInfo("Data").GetDirectories().SelectMany(o=>o.GetFiles("*.txt")))
			//{
			//	alreadyProcessed.Add(string.Join(",", File.ReadAllLines(fileInfo.FullName).First().Split(',').OrderBy(o => o)).ToUpperInvariant());
			//}

			//Stream stream3 = new FileStream("Cache.bin", FileMode.Open, FileAccess.Write, FileShare.Write);
			//formatter.Serialize(stream3, alreadyProcessed);
			//stream3.Close();

			//Stream stream2 = new FileStream("Cache.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
			//List<string> cache = (List<string>)formatter.Deserialize(stream2);
			//stream2.Close();
			List<string> cache = new List<string>();
			IEnumerable<string> allImages = allFeatures.Select(o => o.Path).Distinct();
			IEnumerable<string>  allMelanomas = allImages.Where(o => melanomaInclusions.Any(i => o == i)).ToList();
			IEnumerable<string>  allBenigns = allImages.Where(o => benignInclusions.Any(i => o == i)).ToList();

			List<Experiment> experiments = new List<Experiment>();
			for (int testIndex = 0; testIndex < 10; testIndex++)
			{
				IEnumerable<string> randomizedMelanomas = allMelanomas.Select(o => o).Shuffle();
				IEnumerable<string> randomizedBenigns = allBenigns.Select(o=>o).Shuffle();
				Experiment experiment = new Experiment();
				experiment.BenignTrainingSet = randomizedBenigns.Take(40).ToList();
				experiment.MelanomaTrainingSet = randomizedMelanomas.Take(30).ToList();
				experiment.BenignTestSet = randomizedBenigns.Skip(40).Take(20).ToList();
				experiment.MelanomaTestSet = randomizedMelanomas.Skip(30).Take(10).ToList();
				experiments.Add(experiment);

			}

			for (int index = startInc; index < endExclude; index++)
			{
				var combos = combosList[index];// "AssymetryPrimaryAxis,AssymetrySecondaryAxis,EdgeLightnessChange,IrregularityIndex,VarianceB,VarianceChroma,VarianceG,VarianceHue,VarianceLightness,VarianceR".Split(',');


				if (File.Exists(string.Format("{0}.txt", index)) || cache.Contains(string.Join(",", combos.OrderBy(o => o)).ToUpperInvariant()))
				{
					Console.WriteLine("{0} Skipped", index);
					continue;
				}

				File.Create(string.Format("{0}.txt", index)).Close();

				using (StreamWriter textWriter = new StreamWriter(string.Format("{0}.txt", index), true))
				{

					textWriter.WriteLine(string.Join(",", combos));
					//Console.WriteLine(string.Join(",", combos));

					int totalFalsePositives = 0;
					int totalTruePositives = 0;
					int totalFalseNegatives = 0;
					int totalTrueNegatives = 0;
					int totalunknowns = 0;

					int inputNodes = combos.Count();
					int hiddenNodes = 6;
					int outputNodes = 2;


					var featuresPerImages = allFeatures.Where(o => combos.Contains(o.FeatureName)).GroupBy(o => o.Path).ToList();
					var imagesPerFeatures = allFeatures.Where(o => combos.Contains(o.FeatureName)).GroupBy(o => o.FeatureName).ToList();

					for (int testIndex = 0; testIndex < 10; testIndex++)
					{
						Experiment experiment = experiments[testIndex];
						
						List<ClassifierTrainingItem> classifierTrainingItems = new List<ClassifierTrainingItem>();

						foreach (var melanoma in featuresPerImages.Where(o => experiment.MelanomaTrainingSet.Contains(o.Key)))
						{
							List<double> normalizedFeatures = new List<double>();
							foreach (var imagesPerFeature in imagesPerFeatures)
							{
								double normalizationValue = imagesPerFeature.Max(o => o.Value);
								normalizedFeatures.Add(melanoma.First(o => o.FeatureName.Equals(imagesPerFeature.Key, StringComparison.OrdinalIgnoreCase)).Value / normalizationValue);
							}


							classifierTrainingItems.Add(new ClassifierTrainingItem(normalizedFeatures.ToArray(), new double[] { 1, 0 }));
						}

						foreach (var benign in  featuresPerImages.Where(o => experiment.BenignTrainingSet.Contains(o.Key)))
						{
							List<double> normalizedFeatures = new List<double>();
							foreach (var imagesPerFeature in imagesPerFeatures)
							{
								double normalizationValue = imagesPerFeature.Max(o => o.Value);
								normalizedFeatures.Add(benign.First(o => o.FeatureName.Equals(imagesPerFeature.Key, StringComparison.OrdinalIgnoreCase)).Value / normalizationValue);
							}
							classifierTrainingItems.Add(new ClassifierTrainingItem(normalizedFeatures.ToArray(), new double[] { 0, 1 }));
						}



						ClassifierTrainer trainer = new ClassifierTrainer();
						double[] trainedWeights = trainer.TrainWeights(
							new ParticleSwarm(inputNodes * hiddenNodes + hiddenNodes * outputNodes + hiddenNodes + outputNodes, 1000, 1000, -10, 10, 0.729, 1.49445, 1.49445, 0.05, 0.01), GetNetworkFactory(inputNodes, hiddenNodes, outputNodes), classifierTrainingItems);


						NeuralNetwork neuralNetwork = GetNetworkFactory(inputNodes, hiddenNodes, outputNodes)(trainedWeights);

						{

							List<ClassifierTrainingItem> classifierTestItems = new List<ClassifierTrainingItem>();
							int c = 0;
							foreach (var melanoma in featuresPerImages.Where(o => experiment.MelanomaTestSet.Contains(o.Key)))
							{
								c++;
								List<double> normalizedFeatures = new List<double>();
								foreach (var imagesPerFeature in imagesPerFeatures)
								{
									double normalizationValue = imagesPerFeature.Max(o => o.Value);
									normalizedFeatures.Add(melanoma.First(o => o.FeatureName.Equals(imagesPerFeature.Key, StringComparison.OrdinalIgnoreCase)).Value / normalizationValue);
								}

								classifierTestItems.Add(new ClassifierTrainingItem(normalizedFeatures.ToArray(), new double[] { 1, 0 }));

							}


							foreach (var benign in featuresPerImages.Where(o => experiment.BenignTestSet.Contains(o.Key)))
							{
								c++;
								List<double> normalizedFeatures = new List<double>();
								foreach (var imagesPerFeature in imagesPerFeatures)
								{
									double normalizationValue = imagesPerFeature.Max(o => o.Value);
									normalizedFeatures.Add(benign.First(o => o.FeatureName.Equals(imagesPerFeature.Key, StringComparison.OrdinalIgnoreCase)).Value / normalizationValue);
								}

								classifierTestItems.Add(new ClassifierTrainingItem(normalizedFeatures.ToArray(), new double[] { 0, 1 }));
							}

							int falsePositives = 0;
							int truePositives = 0;
							int falseNegatives = 0;
							int trueNegatives = 0;
							int unknowns = 0;

							foreach (ClassifierTrainingItem test in classifierTestItems)
							{
								double[] classifiedOutputs = neuralNetwork.Classify(test.Features);



								if (classifiedOutputs[0] >= 0.5 && classifiedOutputs[1] <= 0.5)
								{
									if (1 == test.Outputs[0])
										truePositives++;
									else
										falsePositives++;
								}
								else if (classifiedOutputs[0] <= 0.5 && classifiedOutputs[1] >= 0.5)
								{
									if (0 == test.Outputs[0])
										trueNegatives++;
									else
										falseNegatives++;
								}
								else
								{
									unknowns++;
								}


							}


							totalFalsePositives += falsePositives;
							totalTruePositives += truePositives;
							totalFalseNegatives += falseNegatives;
							totalTrueNegatives += trueNegatives;
							totalunknowns += unknowns;


							textWriter.WriteLine("##########################################");
							textWriter.WriteLine("False Positives: {0}", falsePositives);
							textWriter.WriteLine("True Positives: {0}", truePositives);
							textWriter.WriteLine("False Negatives: {0}", falseNegatives);
							textWriter.WriteLine("True Negatives: {0}", trueNegatives);
							textWriter.WriteLine("Unknowns: {0}", unknowns);
							textWriter.WriteLine("Accuracy: {0}", (truePositives + trueNegatives) / (double)(falsePositives + truePositives + falseNegatives + trueNegatives));
							textWriter.WriteLine();
						}
					}

					textWriter.WriteLine("##########################################");
					textWriter.WriteLine("##########################################");
					textWriter.WriteLine("Total False Positives: {0}", totalFalsePositives);
					textWriter.WriteLine("Total True Positives: {0}", totalTruePositives);
					textWriter.WriteLine("Total False Negatives: {0}", totalFalseNegatives);
					textWriter.WriteLine("Total True Negatives: {0}", totalTrueNegatives);
					textWriter.WriteLine("Total Unknowns: {0}", totalunknowns);
					textWriter.WriteLine();
				}


				Console.WriteLine(index);
			}

			Console.ReadLine();




		}
		private static List<string> benignInclusions = new List<string>() { 
			@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Fal096",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Fcl062",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dermal Nevus\Nll038",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dermal Nevus\Nll050",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dermal Nevus\Nll058",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal004",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal006",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal008",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal066",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl112",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl145",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl153",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl163",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl217",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl223",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl245",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl321",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl329",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl346",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael413",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael471",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael477",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\FAL042",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fal050",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fal064",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fal072",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl006",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl024",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl036",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl080",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fcl014",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fcl036",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fcl050",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fcl076",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fcl098",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fdl028",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fel018",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fel052",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fel086",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ffl016",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\ffl032",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fgl002",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fgl051",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl034",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl042",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl066",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl070",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl072",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl074",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl076",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl086",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Gal010",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Gal058",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Gal232",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nbl076",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nbl081",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nbl095",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ncl006",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ncl030",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ndl021",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ndl022",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ngl094",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nhl013",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nil006",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nil027",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nil060",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll046",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll060",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll062",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll072",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll102",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nml002",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nml006",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nml034",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nml035",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nml067",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\Ndl100",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\Nml039",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Seborrheic Keratosis\Ael488",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Seborrheic Keratosis\Fal048",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Seborrheic Keratosis\Fdl060",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Seborrheic Keratosis\Fdl074",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Ncl020",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Nil004",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Nil029",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Nel039",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Nhl011",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dermal Nevus\Nfl042",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal046",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal062",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl157",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl215",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl249",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl371",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl387",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl391",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael453",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael517",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael523",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fal020",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\FAL038",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fal090",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fal100",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl002",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\FBL012",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl064",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl068",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl074",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fcl012",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\fcl018",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fcl030",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\fcl066",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fdl002",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fel002",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fel068",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fel070",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fel082",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fgl010",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fgl012",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fgl042",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl038",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl100",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fll023",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fll037",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fll087",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ncl048",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ncl050",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ndl031",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ndl033",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\NDL035",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ndl040",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ngl053",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ngl074",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll018",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll104",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\Abl193",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Seborrheic Keratosis\Fhl048",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Seborrheic Keratosis\Fll104",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Nbl100",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Ndl075",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Ndl078",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Ngl078",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Nil041",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Nil058",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Nml049",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Adl379",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Fgl088",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Fhl050",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Nml106",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Blue Nevus\Ael446",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dermal Nevus\NLL052",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dermatofibroma\Aal038",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal016",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal044",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal048",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal094",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal099bis",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl101",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl116",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl123",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl171",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl185",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl201",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl219",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl259",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl279",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl299",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl311",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl333",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl375",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl381",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael431",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael435",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael467",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl076",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl098",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fdl030",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fhl078",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Gal045",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Gal047",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ncl014",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ncl051",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ncl053",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ndl009",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\ndl013",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ngl076",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nil010",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nil035",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nil043",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nil045",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nil052",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll098",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nml014",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nml030",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\Ael415",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\gzl48",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\newl006",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\newl018",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\newl020",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\newl022",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\newl024",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\newl026",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Acl225",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Ndl069",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Basal_Cell_Carcinoma\Nel100",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal052",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal056",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal064",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal086",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal088",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal090",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Aal096",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl118",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl121",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl135",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl139",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl151",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl155",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl161",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl165",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl167",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl168",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl175",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Abl179",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl221",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl243",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl247",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl264",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Acl271",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl319",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl335",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl344",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Adl366",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ael481",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl022",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl048",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fbl094",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fdl020",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Fgl008",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Gal006",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Gal025",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Gal043",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nbl086",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ndl012",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ndl039",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ngl092",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Ngl096",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\Nll024",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\nll027",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Dysplastic Nevus\nll032",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Lentigo\newl010",
@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Spitz-Reed\Ngl029",

		};
		private static List<string> melanomaInclusions = new List<string>() 
		{ 
			@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fbl050",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\FBL058",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fcl026",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\FCL056",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fel098",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\FFL020",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fil006",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fil032",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll071",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll108",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll118",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gal209",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gal211",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gdl025",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nal035",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl004",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl008",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl013",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl100",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nil078",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nil084",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nml079",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\FBL026",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Fll122",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Gal039",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Aal002",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\FAL006",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\FBL020",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\FCL068",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\FCL084",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fcl100",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fgl061",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll069",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll128",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fll136",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\gzl04",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\gzl16",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nal025",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nbl030",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Ggl036",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\gzl20",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\gzl28",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Metastatic Melanoma\Fcl016",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Adl305",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\FAL066",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gal220",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gal257",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gal262",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gcl039",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nbl006",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nbl074",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl015",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl017",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl027",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl066",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Ael496",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Ffl080",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Fgl057",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Gal240",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Nbl015",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Fil040",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gdl014",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Gdl089",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nal047",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nbl017",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nbl063",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl021",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl039",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Ngl055",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nil066",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nil082",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nil103",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma (mel in situ not included)\Nml073",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Nal022",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Nbl022",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Ngl002",
		@"D:\Studies 2014\Research\Code\Images\FinalSelection\Selected\Melanoma in situ\Nml094"
 
		
		};
	}
}
