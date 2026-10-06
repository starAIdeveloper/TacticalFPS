using UnityEngine;
namespace TacticalFPS {
public sealed class WeaponController : MonoBehaviour {
 public PlayerMotor Motor;public GameDirector Director;public int Selected;public WeaponAmmo Ammo=>ammo[Selected];
 readonly WeaponAmmo[] ammo={new WeaponAmmo(30,120),new WeaponAmmo(12,48),new WeaponAmmo(5,20)};
 public string WeaponName=>new[]{"CARBINE","SIDEARM","MARKSMAN"}[Selected];
 float nextShot;Transform model;Material material;
 void Start(){material=new Material(Shader.Find("Standard")){color=new Color(.12f,.15f,.17f)};
 model=new GameObject("Procedural weapon").transform;model.SetParent(Motor.View.transform,false);
 WorldBuilder.Primitive(PrimitiveType.Cube,"Receiver",model,new Vector3(.25f,-.23f,.4f),new Vector3(.1f,.14f,.4f),material);
 WorldBuilder.Primitive(PrimitiveType.Cube,"Barrel",model,new Vector3(.25f,-.2f,.75f),new Vector3(.045f,.045f,.45f),material);
 WorldBuilder.Primitive(PrimitiveType.Cube,"Optic",model,new Vector3(.25f,-.12f,.43f),new Vector3(.08f,.07f,.12f),material);}
 void Update(){if(!Director.Active)return;
 for(int i=0;i<3;i++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+i))){Ammo.CancelReload();Selected=i;}
 Ammo.Tick(Time.deltaTime);if(Input.GetKeyDown(KeyCode.R))Ammo.BeginReload(Selected==0?1.8f:1.3f);
 bool aim=Input.GetMouseButton(1);Motor.View.fieldOfView=Mathf.Lerp(Motor.View.fieldOfView,aim?Selected==2?30:55:75,Time.deltaTime*12);
 model.localPosition=Vector3.Lerp(model.localPosition,aim?new Vector3(-.25f,.08f,.05f):Vector3.zero,Time.deltaTime*12);
 if((Selected==0?Input.GetMouseButton(0):Input.GetMouseButtonDown(0)) && Time.time>=nextShot && Ammo.Fire()){
 nextShot=Time.time+(Selected==0?.12f:Selected==1?.25f:.8f);
 var ray=new Ray(Motor.View.transform.position,Motor.View.transform.forward);
 if(Physics.Raycast(ray,out RaycastHit hit,150,WorldBuilder.WorldMask|WorldBuilder.ActorMask)){
 var h=hit.collider.GetComponentInParent<Health>();if(h){var zone=hit.collider.GetComponent<HitZone>();h.Damage((Selected==2?70:Selected==1?22:28)*(zone && zone.Head?2:1),Team.Player);}}
 model.localPosition+=Vector3.back*.06f;
 }}
 void OnDestroy(){if(material)Destroy(material);}
}}
