using System;
using System.Collections.Generic;

namespace TacticalFPS
{
    public enum Team { Player, Hostile }
    public enum MissionPhase { Briefing, FindIntel, Extract, Won, Lost }

    public sealed class WeaponAmmo
    {
        public int Capacity { get; }
        public int Magazine { get; private set; }
        public int Reserve { get; private set; }
        public bool Reloading { get; private set; }
        public float Remaining { get; private set; }
        public WeaponAmmo(int capacity, int reserve)
        {
            if (capacity <= 0 || reserve < 0) throw new ArgumentOutOfRangeException();
            Capacity = capacity; Magazine = capacity; Reserve = reserve;
        }
        public bool Fire()
        {
            if (Reloading || Magazine <= 0) return false;
            Magazine--; return true;
        }
        public bool BeginReload(float seconds)
        {
            if (Reloading || Magazine == Capacity || Reserve == 0) return false;
            if (seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) throw new ArgumentOutOfRangeException();
            Reloading = true; Remaining = seconds; return true;
        }
        public void Tick(float delta)
        {
            if (delta < 0 || float.IsNaN(delta) || float.IsInfinity(delta)) throw new ArgumentOutOfRangeException();
            if (!Reloading) return;
            Remaining -= delta;
            if (Remaining > 0) return;
            int amount = Math.Min(Capacity - Magazine, Reserve);
            Magazine += amount; Reserve -= amount; Reloading = false; Remaining = 0;
        }
        public void CancelReload() { Reloading = false; Remaining = 0; }
        public void AddReserve(int amount) { if (amount > 0) Reserve = Math.Min(999, Reserve + amount); }
    }

    public sealed class MissionRules
    {
        public MissionPhase Phase { get; private set; } = MissionPhase.Briefing;
        public float ExtractionProgress { get; private set; }
        public void Start() { if (Phase == MissionPhase.Briefing) Phase = MissionPhase.FindIntel; }
        public bool CollectIntel()
        {
            if (Phase != MissionPhase.FindIntel) return false;
            Phase = MissionPhase.Extract; return true;
        }
        public void TickExtraction(float delta, bool inside, bool safe)
        {
            if (Phase != MissionPhase.Extract) return;
            if (delta < 0 || float.IsNaN(delta) || float.IsInfinity(delta)) throw new ArgumentOutOfRangeException();
            ExtractionProgress = inside && safe ? Math.Min(5f, ExtractionProgress + delta) : 0;
            if (ExtractionProgress >= 5f) Phase = MissionPhase.Won;
        }
        public void Fail() { if (Phase != MissionPhase.Won) Phase = MissionPhase.Lost; }
    }

    // Four-neighbour A*: no corner cutting, deterministic ties, explicit expansion budget.
    public sealed class GridPathfinder
    {
        public int Width { get; }
        public int Height { get; }
        readonly bool[] blocked;
        public GridPathfinder(int width, int height)
        {
            if (width < 1 || height < 1 || width * height > 10000) throw new ArgumentOutOfRangeException();
            Width = width; Height = height; blocked = new bool[width * height];
        }
        public bool IsBlocked(int index) { return index < 0 || index >= blocked.Length || blocked[index]; }
        public void SetBlocked(int x, int y, bool value) { blocked[y * Width + x] = value; }
        public int Index(int x, int y) { return y * Width + x; }
        int Heuristic(int a, int b) { return Math.Abs(a % Width - b % Width) + Math.Abs(a / Width - b / Width); }
        public List<int> FindPath(int start, int goal, int budget = 2400)
        {
            if (IsBlocked(start) || IsBlocked(goal) || budget <= 0) return new List<int>();
            int[] parent = new int[blocked.Length], cost = new int[blocked.Length];
            bool[] closed = new bool[blocked.Length];
            for (int i = 0; i < parent.Length; i++) { parent[i] = -1; cost[i] = int.MaxValue; }
            var open = new List<int> { start }; cost[start] = 0;
            while (open.Count > 0 && budget-- > 0)
            {
                int best = 0;
                for (int i = 1; i < open.Count; i++)
                    if (cost[open[i]] + Heuristic(open[i], goal) < cost[open[best]] + Heuristic(open[best], goal)) best = i;
                int current = open[best]; open.RemoveAt(best);
                if (current == goal)
                {
                    var result = new List<int>();
                    for (int at = goal; at != -1; at = parent[at]) result.Add(at);
                    result.Reverse(); return result;
                }
                closed[current] = true;
                int x = current % Width, y = current / Width;
                int[] next = { x > 0 ? current - 1 : -1, x < Width - 1 ? current + 1 : -1,
                    y > 0 ? current - Width : -1, y < Height - 1 ? current + Width : -1 };
                foreach (int n in next)
                {
                    if (IsBlocked(n) || closed[n] || cost[current] + 1 >= cost[n]) continue;
                    parent[n] = current; cost[n] = cost[current] + 1;
                    if (!open.Contains(n)) open.Add(n);
                }
            }
            return new List<int>();
        }
    }
}
