using UnityEngine;
namespace TacticalFPS
{
    public sealed class HelicopterDecor : MonoBehaviour
    {
        public Transform Rotor;
        void Update()
        {
            if(Rotor)Rotor.Rotate(0,1400*Time.deltaTime,0,Space.Self);
            transform.position=new Vector3(Mathf.Sin(Time.time*.06f)*45,35+Mathf.Sin(Time.time*.4f),45);
            transform.rotation=Quaternion.Euler(0,Mathf.Cos(Time.time*.06f)>0?90:270,0);
        }
    }
}
