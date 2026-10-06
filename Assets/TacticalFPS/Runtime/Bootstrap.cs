using UnityEngine;
namespace TacticalFPS {public static class Bootstrap {
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Start(){if(!Object.FindFirstObjectByType<WorldBuilder>())new GameObject("Training District").AddComponent<WorldBuilder>();}
}}
