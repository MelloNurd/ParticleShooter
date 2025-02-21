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

        public int Id { get; set; }

        public float[,] InternalForces;
        public float[,] ExternalForces;
        public float[,] InternalMins;
        public float[,] ExternalMins;
        public float[,] InternalRadii;
        public float[,] ExternalRadii;

        public float MaxInternalRadii { get; set; } = 0;
        public float MaxExternalRadii { get; set; } = 0;

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
                    InternalMins[i, j] = Random.Range(.1f, 1f);
                    InternalRadii[i, j] = Random.Range(InternalMins[i, j] * 2, 2f); // minimum 'primary' force range must be twice repulsive range
                    ExternalForces[i, j] = Random.Range(-1f, 1f); // external forces could be attractive or repulsive
                    ExternalMins[i, j] = Random.Range(1f, 3f);
                    ExternalRadii[i, j] = Random.Range(ExternalMins[i, j] * 2f, 7f);

                    if (InternalRadii[i, j] > MaxInternalRadii)
                    {
                        MaxInternalRadii = InternalRadii[i, j];
                    }
                    if (ExternalRadii[i, j] > MaxExternalRadii)
                    {
                        MaxExternalRadii = ExternalRadii[i, j];
                    }
                }
            }
            
            for (int i = 0; i < numParticles; i++)
            {
                positions[i] = new Vector3(x + Random.Range(-0.5f, 0.5f), y + Random.Range(-0.5f, 0.5f));

                Particle newParticle = Instantiate(_particlePrefab, positions[i], Quaternion.identity, transform).GetComponent<Particle>();
                newParticle.Position = positions[i];
                newParticle.Type = 1 + Random.Range(0, _numTypes - 1);
                newParticle.ParentCluster = this;
                newParticle.GetComponent<SpriteRenderer>().color = Color.HSVToRGB((float)newParticle.Type / _numTypes, 1, 1); // color based on type
                Swarm.Add(newParticle);
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
            for (int i = 0; i < numParticles; i++)
            {
                Particle p = Swarm[i];
                //Particle temp = Instantiate(_particlePrefab, p.Position, Quaternion.identity, colony.transform).GetComponent<Particle>();
                p.Position = colony.Swarm[i].Position;
                p.transform.position = p.Position;
                p.Type = colony.Swarm[i].Type;
                p.GetComponent<SpriteRenderer>().color = Color.HSVToRGB((float)p.Type / _numTypes, 1, 1); // color based on type
                //Swarm.Add(p); // add to the new cell
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
                    InternalMins[i, j] += Random.Range(-0.5f, 0.5f);
                    InternalRadii[i, j] += Random.Range(-0.10f, 0.10f);
                    ExternalForces[i, j] += Random.Range(-0.1f, 0.1f);
                    ExternalMins[i, j] += Random.Range(-0.5f, 0.5f);
                    ExternalRadii[i, j] += Random.Range(-0.10f, 0.10f);
                }
            }
            for (int i = 0; i < numParticles; i++)
            {
                positions[i] = new Vector3(positions[i].x + Random.Range(-5, 5), positions[i].y + Random.Range(-5, 5));
                if (Random.Range(0, 100) < 10)
                {  // 10% of the time a particle changes type
                    Particle p = Swarm[i];
                    p.Type = 1 + Random.Range(0, _numTypes - 1);
                    p.GetComponent<SpriteRenderer>().color = Color.HSVToRGB((float)p.Type / _numTypes, 1, 1); // color based on type
                }
            } // Could also mutate the number of particles in the cell
        }

        public void UpdateCluster()
        {
            foreach (Particle p in Swarm)
            { // for each particle in this cell
                p.ApplyInternalForces(this);
                p.ApplyExternalForces(this);
                p.ApplyFoodForces(this);
            }

            AdjustCenter();

            Energy -= 1; // cells lose one energy/timestep - should be a variable. Or dependent on forces generated
        }

        public void AdjustCenter()
        {
            // Go through each particle in Swarm and calculate the average position, then set transform.position to that.
            Vector3 avg = Vector3.zero;
            foreach (Particle p in Swarm)
            {
                avg += p.Position;
            }
            avg /= Swarm.Count;
            transform.position = avg;
        }
    }
}