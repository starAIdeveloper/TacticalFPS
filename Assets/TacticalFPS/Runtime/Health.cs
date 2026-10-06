using System;
using UnityEngine;

namespace TacticalFPS
{
    public sealed class Health : MonoBehaviour
    {
        public Team Team;
        public float Maximum = 100f;
        public float Current { get; private set; } = 100f;
        public float LastDamageTime { get; private set; } = -100f;
        public bool Dead => Current <= 0;
        public event Action<Health> Died;
        public void ResetHealth() { Current = Maximum; LastDamageTime = -100f; }
        public bool Damage(float amount, Team source)
        {
            if (Dead || source == Team || amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) return false;
            Current = Mathf.Max(0, Current - amount); LastDamageTime = Time.time;
            if (Dead) Died?.Invoke(this);
            return true;
        }
        public void Heal(float amount) { if (!Dead && amount > 0) Current = Mathf.Min(Maximum, Current + amount); }
    }
}
