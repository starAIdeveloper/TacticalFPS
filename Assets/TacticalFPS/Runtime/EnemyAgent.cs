using System.Collections.Generic;
using UnityEngine;
namespace TacticalFPS {
public sealed class EnemyAgent : MonoBehaviour {
 public GameDirector Director;public Transform LeftLeg,RightLeg;public int PatrolOffset;public Health Health{get;private set;}
 CharacterController body;List<Vector3> path=new List<Vector3>();int step,patrol;float repath,shot,vertical;
 void Awake(){Health=GetComponent<Health>();body=GetComponent<CharacterController>();}
 void Update(){if(!Director.Active || Health.Dead)return;
 var player=Director.Player;Vector3 eye=transform.position+Vector3.up*1.5f,target=player.transform.position+Vector3.up*1.3f;
 Vector3 delta=target-eye;bool visible=delta.magnitude<35 && !Physics.Linecast(eye,target,WorldBuilder.WorldMask);
 Vector3 goal=visible?player.transform.position:Director.World.PatrolPoints[(patrol+PatrolOffset)%Director.World.PatrolPoints.Count];
 if(!visible && Vector3.Distance(transform.position,goal)<2)patrol++;
 if(Time.time>=repath){path=Director.World.Path(transform.position,goal);step=0;repath=Time.time+1.2f;}
 Vector3 motion=Vector3.zero;
 if((!visible || delta.magnitude>12) && step<path.Count){Vector3 d=path[step]-transform.position;d.y=0;if(d.magnitude<.5f)step++;else motion=d.normalized*2.4f;}
 vertical=body.isGrounded?-2:Mathf.Max(-25,vertical-20*Time.deltaTime);body.Move((motion+Vector3.up*vertical)*Time.deltaTime);
 Vector3 face=visible?delta:motion;face.y=0;if(face.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(face),Time.deltaTime*6);
 if(visible && Time.time>=shot){shot=Time.time+1.1f;if(Physics.Raycast(eye,delta.normalized,out RaycastHit hit,35,WorldBuilder.WorldMask|WorldBuilder.ActorMask)){var h=hit.collider.GetComponentInParent<Health>();if(h)h.Damage(7,Team.Hostile);}}
 float angle=motion.sqrMagnitude>.1f?Mathf.Sin(Time.time*9)*22:0;LeftLeg.localRotation=Quaternion.Euler(angle,0,0);RightLeg.localRotation=Quaternion.Euler(-angle,0,0);
 }
}}
