using UnityEngine;
namespace TacticalFPS {
public sealed class GameHud : MonoBehaviour {
 public GameDirector Director;
 void OnGUI(){if(!Director || !Director.Player)return;
 float sx=Screen.width/1280f,sy=Screen.height/720f;GUI.matrix=Matrix4x4.Scale(new Vector3(sx,sy,1));
 GUI.Box(new Rect(20,20,330,85),"TACTICAL FPS / OPERATION RIDGELINE");
 GUI.Label(new Rect(35,50,310,50),Director.Mission.Phase==MissionPhase.FindIntel?"Find intel: north courtyard. Press E nearby.":"Return south to extraction. Hold safe for 5 sec.");
 GUI.Box(new Rect(1040,20,220,90),Director.Weapons.WeaponName);
 GUI.Label(new Rect(1070,55,180,45),Director.Weapons.Ammo.Magazine+" / "+Director.Weapons.Ammo.Reserve+(Director.Weapons.Ammo.Reloading?" RELOADING":""));
 GUI.Label(new Rect(20,650,650,45),"HEALTH "+Mathf.CeilToInt(Director.Player.Health.Current)+"   STAMINA "+Mathf.CeilToInt(Director.Player.Stamina)+"   MEDKITS "+Director.Medkits);
 GUI.Label(new Rect(632,350,25,25),"+");
 GUI.Box(new Rect(1080,530,180,170),"TACTICAL MAP");
 Dot(Director.Player.transform.position,Color.cyan);
 foreach(var e in Director.Enemies)if(e && !e.Health.Dead)Dot(e.transform.position,Color.red);
 Dot(Director.Mission.Phase==MissionPhase.FindIntel?Director.Intel:Director.Extraction,Color.yellow);
 if(Director.Mission.Phase==MissionPhase.Extract)GUI.Label(new Rect(500,600,300,35),"EXTRACTION "+Director.Mission.ExtractionProgress.ToString("0.0")+" / 5.0");
 if(Director.Paused || Director.Mission.Phase==MissionPhase.Won || Director.Mission.Phase==MissionPhase.Lost){GUI.Box(new Rect(390,220,500,280),Director.Paused?"PAUSED":Director.Mission.Phase.ToString());GUI.Label(new Rect(420,260,440,100),"WASD move / Mouse look / Shift sprint / C crouch\nLeft click fire / Right click aim / R reload\n1-3 weapons / E intel / H medkit / Esc pause");if(GUI.Button(new Rect(500,420,280,45),"Restart mission"))Director.Restart();}
 }
 void Dot(Vector3 p,Color c){GUI.color=c;GUI.DrawTexture(new Rect(1166+p.x*2.3f,615-p.z*2,5,5),Texture2D.whiteTexture);GUI.color=Color.white;}
}}
