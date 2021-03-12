using UnityEngine;
using Particle = UnityEngine.ParticleSystem.Particle;
using System.Collections.Generic;
 
public class ParticleLifetimeEvents : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;
    private Particle[] _particles;
    private float _shortestTimeAlive = float.MaxValue;
    private List<float> _aliveParticlesRemainingTime = new List<float>();
 
    private event System.Action<Particle> _particleWasBorn;
    public event System.Action<Particle> ParticleWasBorn
    {
        add { _particleWasBorn += value; }
        remove { _particleWasBorn -= value; }
    }
    private void RaiseParticleWasBorn(Particle particle) { if (_particleWasBorn != null) { _particleWasBorn(particle); } }
 
    private event System.Action _particleDied;
    public event System.Action ParticleDied
    {
        add { _particleDied += value; }
        remove { _particleDied -= value; }
    }
    private void RaiseParticleDied() { if (_particleDied != null) { _particleDied(); } }
 
    private void Awake()
    {
        _particles = new Particle[_particleSystem.main.maxParticles];
    }
 
    private void LateUpdate()
    {
        TryBroadcastParticleDeath();
 
        if (_particleSystem.particleCount == 0)
            return;
 
        var numParticlesAlive = _particleSystem.GetParticles(_particles);
 
        float youngestParticleTimeAlive = float.MaxValue;
        var youngestParticles = GetYoungestParticles(numParticlesAlive, _particles, ref youngestParticleTimeAlive);
        if (_shortestTimeAlive > youngestParticleTimeAlive)
        {
            for (int i = 0; i < youngestParticles.Length; i++)
            {
                RaiseParticleWasBorn(youngestParticles[i]);
                _aliveParticlesRemainingTime.Add(youngestParticles[i].remainingLifetime);
            }
        }
        _shortestTimeAlive = youngestParticleTimeAlive;
    }
 
    private void TryBroadcastParticleDeath()
    {
        for (int i = _aliveParticlesRemainingTime.Count - 1; i > -1; i--)
        {
            _aliveParticlesRemainingTime[i] -= Time.deltaTime;
            if (_aliveParticlesRemainingTime[i] <= 0)
            {
                _aliveParticlesRemainingTime.RemoveAt(i);
                RaiseParticleDied();
            }
        }
    }
 
    private Particle[] GetYoungestParticles(int numPartAlive, Particle[] particles, ref float youngestParticleTimeAlive)
    {
        var youngestParticles = new List<Particle>();
        for (int i = 0; i < numPartAlive; i++)
        {
            var timeAlive = particles[i].startLifetime - particles[i].remainingLifetime;
            if (timeAlive < youngestParticleTimeAlive)
            {
                youngestParticleTimeAlive = timeAlive;
            }
        }
        for (int i = 0; i < numPartAlive; i++)
        {
            var timeAlive = particles[i].startLifetime - particles[i].remainingLifetime;
            if (timeAlive == youngestParticleTimeAlive)
            {
                youngestParticles.Add(particles[i]);
            }
        }
        return youngestParticles.ToArray();
    }
 
    private void Reset()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }
}
 