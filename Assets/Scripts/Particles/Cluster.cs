using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace NaughtyAttributes
{
    public class Cluster : MonoBehaviour
    {
        public List<Particle> Swarm { get; set; } = new List<Particle>();

        public float[,] InternalForces;
        public float[,] ExternalForces;
        public float[,] InternalMins;
        public float[,] ExternalMins;
        public float[,] InternalRadii;
        public float[,] ExternalRadii;

        public int Energy;

        private Vector3[] positions;
        int numParticles = 40;
        int radius; // average distance from center
        Vector3 center;

        private GameObject _particlePrefab;
        private int _numTypes;

        private void Awake()
        {
            _numTypes = ParticleManager.Instance.NumberOfTypes;
            _particlePrefab = ParticleManager.Instance.ParticlePrefab;

            Energy = ParticleManager.Instance.StartingEnergy;
        }

        public void Initialize(float x, float y)
        {
            InternalForces = new float[_numTypes, _numTypes];
            ExternalForces = new float[_numTypes, _numTypes];
            InternalMins = new float[_numTypes, _numTypes];
            ExternalMins = new float[_numTypes, _numTypes];
            InternalRadii = new float[_numTypes, _numTypes];
            ExternalRadii = new float[_numTypes, _numTypes];
            // Positions are the inital relative positions of all of the particles.
            // This is critcal to cells starting in a 'good' configuration.
            positions = new Vector3[numParticles];
            GenerateNew(x, y);
        }

        // generate the parameters for a new cell
        // note: all of the random ranges could be tweaked
        private void GenerateNew(float x, float y)
        {
            for (int i = 0; i < _numTypes; i++)
            {
                for (int j = 0; j < _numTypes; j++)
                {
                    InternalForces[i, j] = Random.Range(0.1f, 1f); // internal forces are initially attractive, but can mutate
                    InternalMins[i, j] = Random.Range(40f, 70f);
                    InternalRadii[i, j] = Random.Range(InternalMins[i, j] * 2f, 300f); // minimum 'primary' force range must be twice repulsive range
                    ExternalForces[i, j] = Random.Range(-1f, 1f); // external forces could be attractive or repulsive
                    ExternalMins[i, j] = Random.Range(40f, 70f);
                    ExternalRadii[i, j] = Random.Range(ExternalMins[i, j] * 2f, 300f);
                }
            }
            for (int i = 0; i < numParticles; i++)
            {
                positions[i] = new Vector3(x + Random.Range(-0.5f, 0.5f), y + Random.Range(-0.5f, 0.5f));

                Particle newParticle = Instantiate(_particlePrefab, positions[i], Quaternion.identity, transform).GetComponent<Particle>();
                newParticle.Position = positions[i];
                newParticle.Type = 1 + Random.Range(0, _numTypes); // Type 0 is food
                newParticle.GetComponent<SpriteRenderer>().color = Color.HSVToRGB((float)newParticle.Type / _numTypes, 1, 1); // color based on type
            }
        }

        // Used to copy the values from a parent cell to a daughter cell.
        // (I don't trust deep copy when data structures get complex :)
        public void CopyCell(Cluster colony)
        {
            for (int i = 0; i < _numTypes; i++)
            {
                for (int j = 0; j < _numTypes; j++)
                {
                    InternalForces[i, j] = colony.InternalForces[i, j];
                    InternalMins[i, j] = colony.InternalMins[i, j];
                    InternalRadii[i, j] = colony.InternalRadii[i, j];
                    ExternalForces[i, j] = colony.ExternalForces[i, j];
                    ExternalMins[i, j] = colony.ExternalMins[i, j];
                    ExternalRadii[i, j] = colony.ExternalRadii[i, j];
                }
            }
            float x = Random.Range(0, ParticleManager.Instance.ScreenSpace.x);
            float y = Random.Range(0, ParticleManager.Instance.ScreenSpace.y);
            for (int i = 0; i < numParticles; i++)
            {
                positions[i] = new Vector3(x + colony.positions[i].x, y + colony.positions[i].y);
                Particle p = Swarm[i];
                Particle temp = Instantiate(_particlePrefab, positions[i], Quaternion.identity).GetComponent<Particle>();
                if (temp != null)
                {
                    temp.Type = p.Type;
                    Swarm.Add(temp); // add to the new cell
                }
            }
        }

        // When a new cell is created from a 'parent' cell the new cell's values are mutated
        // This mutates all values a 'little' bit. Mutating a few values by a larger amount could work better
        public void MutateCell()
        {
            for (int i = 0; i < _numTypes; i++)
            {
                for (int j = 0; j < _numTypes; j++)
                {
                    InternalForces[i, j] += Random.Range(-0.1f, 0.1f);
                    InternalMins[i, j] += Random.Range(-5f, 5f);
                    InternalRadii[i, j] += Random.Range(-10f, 10f);
                    ExternalForces[i, j] += Random.Range(-0.1f, 0.1f);
                    ExternalMins[i, j] += Random.Range(-5f, 5f);
                    ExternalRadii[i, j] += Random.Range(-10f, 10f);
                }
            }
            for (int i = 0; i < numParticles; i++)
            {
                positions[i] = new Vector3(positions[i].x + Random.Range(-5, 5), positions[i].y + Random.Range(-5, 5));
                if (Random.Range(0, 100) < 10)
                {  // 10% of the time a particle changes type
                    Particle p = Swarm[i];
                    p.Type = 1 + Random.Range(0, _numTypes);
                }
            } // Could also mutate the number of particles in the cell
        }

        public void Tick()
        {
            foreach (Particle p in Swarm)
            { // for each particle in this cell
                p.ApplyInternalForces(this);
                p.ApplyExternalForces(this);
                p.ApplyFoodForces(this);
            }

            Energy -= 1; // cells lose one energy/timestep - should be a variable. Or dependent on forces generated
        }
    }
}