using NUnit.Framework;
namespace TacticalFPS.Tests {public class RulesTests {
[Test]public void ReloadConservesAmmo(){var a=new WeaponAmmo(3,2);a.Fire();a.Fire();a.BeginReload(1);a.Tick(1);Assert.AreEqual(3,a.Magazine);Assert.AreEqual(0,a.Reserve);}
[Test]public void CannotFireWhileReloading(){var a=new WeaponAmmo(3,3);a.Fire();a.BeginReload(1);Assert.IsFalse(a.Fire());}
[Test]public void CannotFireEmpty(){var a=new WeaponAmmo(1,0);Assert.IsTrue(a.Fire());Assert.IsFalse(a.Fire());}
[Test]public void ExtractionRequiresIntel(){var m=new MissionRules();m.Start();m.TickExtraction(8,true,true);Assert.AreEqual(MissionPhase.FindIntel,m.Phase);}
[Test]public void UnsafeExtractionResets(){var m=new MissionRules();m.Start();m.CollectIntel();m.TickExtraction(4,true,true);m.TickExtraction(1,true,false);Assert.AreEqual(0,m.ExtractionProgress);}
[Test]public void ExtractionWins(){var m=new MissionRules();m.Start();m.CollectIntel();m.TickExtraction(5,true,true);m.Fail();Assert.AreEqual(MissionPhase.Won,m.Phase);}
[Test]public void PathAvoidsWall(){var g=new GridPathfinder(3,3);g.SetBlocked(1,0,true);var p=g.FindPath(0,2);Assert.AreEqual(5,p.Count);Assert.IsFalse(p.Contains(1));}
[Test]public void SealedGoalHasNoPath(){var g=new GridPathfinder(3,1);g.SetBlocked(1,0,true);Assert.IsEmpty(g.FindPath(0,2));}
}}
