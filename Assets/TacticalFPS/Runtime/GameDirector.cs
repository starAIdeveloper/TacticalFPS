using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace TacticalFPS {
[RequireComponent(typeof(GameHud))]
public sealed class GameDirector : MonoBehaviour {
 public WorldBuilder World; public PlayerMotor Player; public WeaponController Weapons;
 public List<EnemyAgent> Enemies=new List<EnemyAgent>(); public MissionRules Mission=new MissionRules();
 public int Medkits=2; public bool Paused; public bool Active => !Paused && (Mission.Phase==MissionPhase.FindIntel || Mission.Phase==MissionPhase.Extract);
 public Vector3 Intel=new Vector3(0,0,28), Extraction=new Vector3(0,0,-26);
 public void Initialize(){ Mission.Start(); Cursor.lockState=CursorLockMode.Locked; Cursor.visible=false; }
 void Update(){
  if(Input.GetKeyDown(KeyCode.Escape)){Paused=!Paused;Time.timeScale=Paused?0:1;Cursor.lockState=Paused?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=Paused;}
  if(!Active)return;
  bool safe=true;foreach(var e in Enemies)if(e && !e.Health.Dead && Vector3.Distance(e.transform.position,Extraction)<12)safe=false;
  Mission.TickExtraction(Time.deltaTime,Vector3.Distance(Player.transform.position,Extraction)<3,safe);
  if(Mission.Phase==MissionPhase.Won)Unlock();
 }
 public void Interact(){if(Vector3.Distance(Player.transform.position,Intel)<3)Mission.CollectIntel();}
 public void FailMission(){Mission.Fail();Unlock();}
 void Unlock(){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
 public void Restart(){Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
 void OnDestroy(){Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
}}
