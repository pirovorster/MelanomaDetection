
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Algorithms
{
	/// <summary>
	/// A class that implements a particle swarm
	/// </summary>
	public sealed class ParticleSwarm : IOptimizer
	{

		private readonly int _numberOfDimensions;
		private readonly int _numberOfParticles;
		private readonly int _iterations;
		private readonly double _dimensionMin;
		private readonly double _dimensionMax;
		private readonly double _inertia;
		private readonly double _localWeight;
		private readonly double _socialWeight;

		private readonly double _particleExecuionProbability;
		private readonly double _exitError;
		private readonly Random _random = new Random(DateTime.Now.Millisecond);
		private readonly List<Particle> _particles = new List<Particle>();


		/// <summary>
		/// Constructs a particle swarm
		/// </summary>
		/// <param name="numberOfParticles">The number of particles</param>
		/// <param name="numberOfDimensions">The number of dimesions of each particle</param>
		/// <param name="iterations">The amount of iterations before the best position returned</param>
		/// <param name="dimensionMin">The search space minimum dimension value</param>
		/// <param name="dimensionMax">The search space maximum dimension value</param>
		/// <param name="inertia">Rate of change</param>
		/// <param name="localWeight">The amount that a particle moves towards his own best per iteration</param>
		/// <param name="socialWeight">The amount that a particle moves towards the swarm's best per iteration</param>
		
		public ParticleSwarm(
			int numberOfDimensions,
			int numberOfParticles,
			int iterations,
			double dimensionMin,
			double dimensionMax,
			double inertia,
			double localWeight,
			double socialWeight,
			double particleExecuionProbability,
			double exitError
			)
		{
			
			if (numberOfDimensions <1)
				throw new ArgumentOutOfRangeException("numberOfDimensions","numberOfDimensions must be more or equal to 1");
			if (numberOfParticles < 1)
				throw new ArgumentOutOfRangeException("numberOfParticles", "numberOfParticles must be more or equal to 1");
			if (inertia < 0)
				throw new ArgumentOutOfRangeException("inertia", "inertia must be more or equal to 0");
			if (inertia < 0)
				throw new ArgumentOutOfRangeException("localWeight", "localWeight must be more or equal to 0");
			if (inertia < 0)
				throw new ArgumentOutOfRangeException("localWeight", "localWeight must be more or equal to 0");
			if (socialWeight < 0)
				throw new ArgumentOutOfRangeException("socialWeight", "socialWeight must be more or equal to 0");
			if (iterations < 1)
				throw new ArgumentOutOfRangeException("iterations", "iterations must be more or equal to 1");
			if (particleExecuionProbability < 0)
				throw new ArgumentOutOfRangeException("particleExecuionProbability", "particleExecuionProbability must be more or equal to 0");
			if (particleExecuionProbability >1)
				throw new ArgumentOutOfRangeException("particleExecuionProbability", "particleExecuionProbability must be less or equal to 1");
			if (particleExecuionProbability > 1)
				throw new ArgumentOutOfRangeException("particleExecuionProbability", "particleExecuionProbability must be less or equal to 1");
			if (dimensionMin >= dimensionMax)
				throw new ArgumentException("dimensionMin must be smaller than dimensionMax");
		
			_numberOfDimensions = numberOfDimensions;
			_numberOfParticles = numberOfParticles;
			_inertia = inertia;
			_localWeight = localWeight;
			_socialWeight = socialWeight;
			_iterations = iterations;
			_exitError = exitError;
			_dimensionMin = dimensionMin;
			_dimensionMax = dimensionMax;
			_particleExecuionProbability = particleExecuionProbability;


		}

		/// <summary>
		/// Searches for a solution 
		/// </summary>
		/// <param name="errorFunction">The error function to minimize</param>
		/// <returns>Returns the answer of n dimensions</returns>
		public double[] FindSolution(OptimizationErrorFunction errorFunction)
		{
			if (errorFunction == null)
				throw new ArgumentNullException("errorFunction");

			int iterations = _iterations;
			double[] _bestParticle;
			double _bestError;

			//if (bestGlobalError < exitError) 
			for (int particleIndex = 0; particleIndex < _numberOfParticles; particleIndex++)
			{
				Particle newParticle = CreateParticle(errorFunction);
				_particles.Add(newParticle);
			}

			Particle bestParticle = _particles.OrderBy(o => o.BestError).First();
			_bestParticle = bestParticle.BestDimensionValues;
			_bestError = bestParticle.BestError;


			while (iterations > 0 && _bestError>_exitError)
			{
				
				for (int particleIndex = 0; particleIndex < _numberOfParticles; particleIndex++)
				{
					Particle particle = _particles[particleIndex];

					for (int dimensionIndex = 0; dimensionIndex < _numberOfDimensions; dimensionIndex++)
					{
						double randomGlobal = _random.NextDouble();
						double randomLocal = _random.NextDouble();
						particle.DimensionVelocities[dimensionIndex] = particle.DimensionVelocities[dimensionIndex] * _inertia + _localWeight * randomLocal * (particle.BestDimensionValues[dimensionIndex] - particle.DimensionValues[dimensionIndex]) + _socialWeight * randomGlobal * (_bestParticle[dimensionIndex] - particle.DimensionValues[dimensionIndex]);
						double dimensionValue = particle.DimensionValues[dimensionIndex] + particle.DimensionVelocities[dimensionIndex];

						if (dimensionValue > _dimensionMax)
							particle.DimensionValues[dimensionIndex] = _dimensionMax;
						else if (dimensionValue < _dimensionMin)
							particle.DimensionValues[dimensionIndex] = _dimensionMin;
						else
							particle.DimensionValues[dimensionIndex] = dimensionValue;
					}

					double error = errorFunction(particle.DimensionValues);

					particle.SetError(error);

					if (error < _bestError)
					{
						_bestParticle = particle.BestDimensionValues;
						_bestError = error;
						
					}

					double die = _random.NextDouble();
					if (die < _particleExecuionProbability)
					{
						Particle newParticle = CreateParticle(errorFunction);
						_particles.Remove(particle);
						_particles.Add(newParticle);

						double newParticleError = errorFunction(particle.DimensionValues);

						newParticle.SetError(newParticleError);

						if (newParticleError < _bestError)
						{
							_bestParticle = newParticle.BestDimensionValues;
							_bestError = newParticleError;

						}
					}


				}

				iterations--;
			}
			Console.WriteLine(iterations);
			Console.WriteLine(_bestError);
			return _bestParticle;
		}

		private Particle CreateParticle(OptimizationErrorFunction errorFunction)
		{
			Particle newParticle = new Particle(_numberOfDimensions);

			for (int dimensionIndex = 0; dimensionIndex < _numberOfDimensions; dimensionIndex++)
			{
				newParticle.DimensionValues[dimensionIndex] = (_random.NextDouble() * (_dimensionMax - _dimensionMin) + _dimensionMin);

				double lo = -1.0 * Math.Abs(_dimensionMax - _dimensionMin);
				double hi = Math.Abs(_dimensionMax - _dimensionMin);

				newParticle.DimensionVelocities[dimensionIndex] = (hi - lo) * _random.NextDouble() + lo;
			}
			double error = errorFunction(newParticle.DimensionValues);
			newParticle.SetError(error);
			return newParticle;
		}



		private class Particle
		{
			private readonly double[] _dimensionValues;
			private readonly double[] _dimensionVelocities;
			private double _particleError;

			private readonly double[] _bestDimensionValues;
			private double _bestParticleError;
			internal Particle(int numberOfdimensions)
			{
				_dimensionValues = new double[numberOfdimensions];
				_dimensionVelocities = new double[numberOfdimensions];
				_bestDimensionValues = new double[numberOfdimensions];
				_bestParticleError = double.MaxValue;
			}

			internal double[] DimensionValues
			{
				get { return _dimensionValues; }
			}

			internal double[] DimensionVelocities
			{
				get { return _dimensionVelocities; }
			}

			internal double[] BestDimensionValues
			{
				get { return _bestDimensionValues; }
			}

			internal double CurrentError
			{
				get { return _particleError; }
			}

			internal double BestError
			{
				get { return _bestParticleError; }
			}

			internal void SetError(double error)
			{
				if (error < _bestParticleError)
				{
					_dimensionValues.CopyTo(_bestDimensionValues, 0);
					_bestParticleError = error;
				}
				_particleError = error;
			}
		}
	}
}
