using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TacticalFPS
{
    public sealed class WorldBuilder : MonoBehaviour
    {
        public const int WorldLayer = 8, ActorLayer = 9;
        public const int WorldMask = 1 << WorldLayer, ActorMask = 1 << ActorLayer;
        public bool Night;
        [Range(1,8)] public int EnemyCount = 8;
        public List<Vector3> CoverPoints = new List<Vector3>();
        public List<Vector3> PatrolPoints = new List<Vector3>();
        public GameDirector Director { get; private set; }
        public GridPathfinder Navigation { get; private set; }
        const float Cell = 1.5f;
        const int Width = 40, Depth = 48;
        Material concrete, metal, timber, ground, olive, glass;
        readonly List<Object> owned = new List<Object>();

        void Awake()
        {
            Random.InitState(73);
            BuildMaterials(); BuildEnvironment();
            Physics.SyncTransforms(); BakeNavigation();
            Director = new GameObject("Mission Director").AddComponent<GameDirector>(); Director.World = this;
            SpawnPlayer(); SpawnEnemies();
            Director.GetComponent<GameHud>().Director = Director;
            Director.Initialize();
        }
        Material Material(string name, Color color, float roughness = .2f, bool texture = false)
        {
            Shader shader = Shader.Find("Standard");
            if (!shader) throw new System.InvalidOperationException("Built-in Standard shader missing. Use the Built-in render pipeline.");
            var m = new Material(shader) { name = name, color = color }; m.SetFloat("_Glossiness", roughness);
            owned.Add(m);
            if (texture)
            {
                var image = new Texture2D(64,64,TextureFormat.RGB24,true) { name = name + " procedural texture", wrapMode = TextureWrapMode.Repeat };
                var noise = new System.Random(73); var pixels = new Color[64*64];
                for (int y=0;y<64;y++) for (int x=0;x<64;x++)
                {
                    float value = .76f + (float)noise.NextDouble() * .24f;
                    if (name.Contains("concrete") && (y%16==0 || (x+(y/16%2)*16)%32==0)) value *= .72f;
                    pixels[y*64+x]=Color.white*value;
                }
                image.SetPixels(pixels); image.Apply(); m.mainTexture=image; m.mainTextureScale=new Vector2(3,3); owned.Add(image);
            }
            return m;
        }
        void BuildMaterials()
        {
            concrete=Material("weathered concrete",new Color(.57f,.53f,.45f),.12f,true);
            metal=Material("painted steel",new Color(.19f,.23f,.25f),.35f);
            timber=Material("crate timber",new Color(.43f,.31f,.18f),.08f,true);
            ground=Material("gravel",new Color(.31f,.32f,.29f),.05f,true);
            olive=Material("uniform",new Color(.23f,.29f,.19f),.1f);
            glass=Material("dark window",new Color(.13f,.21f,.25f),.65f);
        }
        public static GameObject Primitive(PrimitiveType type,string name,Transform parent,Vector3 position,Vector3 scale,Material material,bool collider=false)
        {
            GameObject go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);
            go.transform.localPosition=position;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=material;
            if (!collider) { var c=go.GetComponent<Collider>(); c.enabled=false; Destroy(c); }
            else go.layer=WorldLayer;
            return go;
        }
        GameObject Box(string name,Vector3 position,Vector3 scale,Material material,bool collider=true)
        { return Primitive(PrimitiveType.Cube,name,transform,position,scale,material,collider); }
        void BuildEnvironment()
        {
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=Night ? new Color(.22f,.28f,.39f) : new Color(.55f,.58f,.61f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=75;RenderSettings.fogEndDistance=250;
            RenderSettings.fogColor=Night ? new Color(.06f,.1f,.17f) : new Color(.58f,.63f,.68f);
            var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=Night?.2f:1.3f;
            sun.color=Night?new Color(.4f,.56f,.85f):new Color(1,.84f,.64f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(32,-40,0);sun.transform.SetParent(transform);
            Box("Ground",new Vector3(0,-.2f,0),new Vector3(140,.4f,140),ground);
            Box("West perimeter",new Vector3(-29,2,0),new Vector3(.6f,4,69),concrete);
            Box("East perimeter",new Vector3(29,2,0),new Vector3(.6f,4,69),concrete);
            Box("North perimeter",new Vector3(0,2,34),new Vector3(58,4,.6f),concrete);
            Box("South perimeter",new Vector3(0,2,-34),new Vector3(58,4,.6f),concrete);
            foreach (Vector3 p in new[]{new Vector3(-19,0,-9),new Vector3(19,0,-9),new Vector3(-19,0,18),new Vector3(19,0,22)}) Building(p);
            for (int i=0;i<12;i++)
            {
                float x=i%2==0?-6f:7f,z=-17+i*3.7f;
                Vector3 p=new Vector3(x,.65f,z);Box("Concrete cover",p,new Vector3(3,1.3f,1),concrete);
                CoverPoints.Add(p+new Vector3(0,-.65f,1.35f));CoverPoints.Add(p+new Vector3(0,-.65f,-1.35f));
                if (i%3==0) Box("Supply crate",new Vector3(x+3.5f,.65f,z+1.5f),new Vector3(1.3f,1.3f,1.3f),timber);
            }
            Vector3 tower=new Vector3(-10,0,29);
            foreach(float x in new[]{-1.7f,1.7f}) foreach(float z in new[]{-1.7f,1.7f}) Box("Watchtower support",tower+new Vector3(x,4,z),new Vector3(.25f,8,.25f),metal);
            Box("Watchtower platform",tower+Vector3.up*8,new Vector3(4.5f,.25f,4.5f),timber);
            Box("Watchtower roof",tower+Vector3.up*10.5f,new Vector3(5,.2f,5),metal);
            foreach(float x in new[]{-1.9f,1.9f}) Box("Tower rail",tower+new Vector3(x,8.8f,0),new Vector3(.12f,1.4f,4),metal);
            foreach(Vector3 p in new[]{new Vector3(-23,0,-24),new Vector3(23,0,5),new Vector3(0,0,30)})
            {
                Box("Lamp post",p+Vector3.up*5,new Vector3(.16f,10,.16f),metal);
                var lamp=new GameObject("Floodlight").AddComponent<Light>();lamp.transform.SetParent(transform);lamp.transform.position=p+Vector3.up*9;lamp.type=LightType.Point;
                lamp.range=23;lamp.intensity=Night?3.5f:.2f;lamp.color=new Color(1,.85f,.63f);
            }
            for(int i=0;i<14;i++) Mountain(i);
            var helicopter=new GameObject("Ambient helicopter");helicopter.transform.SetParent(transform);helicopter.transform.position=new Vector3(0,35,45);
            Primitive(PrimitiveType.Capsule,"Fuselage",helicopter.transform,Vector3.zero,new Vector3(2,2,5),metal);
            Primitive(PrimitiveType.Cube,"Tail",helicopter.transform,new Vector3(0,.2f,-5),new Vector3(.45f,.45f,6),metal);
            var rotor=Primitive(PrimitiveType.Cube,"Rotor",helicopter.transform,new Vector3(0,1.7f,0),new Vector3(13,.08f,.22f),metal);
            var decor=helicopter.AddComponent<HelicopterDecor>();decor.Rotor=rotor.transform;
            PatrolPoints.AddRange(new[]{new Vector3(-10,0,-12),new Vector3(10,0,-6),new Vector3(-11,0,10),new Vector3(11,0,24),new Vector3(0,0,28)});
        }
        void Building(Vector3 p)
        {
            Box("Building foundation",p,new Vector3(12,.2f,9),concrete);
            for(int level=0;level<2;level++)
            {
                float y=level*3.2f;
                Box("Left wing wall",p+new Vector3(-6,y+1.5f,0),new Vector3(.4f,3,9),concrete);
                Box("Right wing wall",p+new Vector3(6,y+1.5f,0),new Vector3(.4f,3,9),concrete);
                for(int face=-1;face<=1;face+=2)
                {
                    for(int x=-5;x<=5;x+=5)Box("Facade pier",p+new Vector3(x,y+1.5f,face*4.5f),new Vector3(1.4f,3,.35f),concrete);
                    Box("Facade lintel",p+new Vector3(0,y+2.9f,face*4.5f),new Vector3(12,.3f,.4f),concrete);
                    if(level==1)Box("Upper sill",p+new Vector3(0,y+.5f,face*4.5f),new Vector3(12,1,.35f),concrete);
                }
                if(level==1) Box("Upper floor",p+new Vector3(0,3.15f,0),new Vector3(12,.22f,9),concrete);
            }
            Box("Roof slab",p+Vector3.up*6.5f,new Vector3(12.5f,.3f,9.5f),concrete);
            Box("Utility box",p+new Vector3(3,7.2f,0),new Vector3(2,1.2f,2),metal);
            Box("Training banner",p+new Vector3(-4,4.5f,-4.75f),new Vector3(1.2f,2.1f,.04f),olive,false);
        }
        void Mountain(int index)
        {
            float a=index*Mathf.PI*2/14, radius=100;
            Vector3 p=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
            var go=new GameObject("Mountain silhouette");go.transform.SetParent(transform);go.transform.position=p;
            var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-22,0,-20),new Vector3(22,0,-20),new Vector3(22,0,20),new Vector3(-22,0,20),new Vector3(0,27+index%4*8,0)};
            mesh.triangles=new[]{0,4,1,1,4,2,2,4,3,3,4,0};mesh.RecalculateNormals();owned.Add(mesh);
            go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=ground;
        }
        void BakeNavigation()
        {
            Navigation=new GridPathfinder(Width,Depth);
            for(int z=0;z<Depth;z++)for(int x=0;x<Width;x++)
            {
                Vector3 p=CellPosition(Navigation.Index(x,z));
                Navigation.SetBlocked(x,z,Physics.CheckCapsule(p+Vector3.up*.5f,p+Vector3.up*1.5f,.4f,WorldMask));
            }
        }
        public Vector3 CellPosition(int index) { return new Vector3(-30+(index%Width+.5f)*Cell,0,-36+(index/Width+.5f)*Cell); }
        int NearestCell(Vector3 position)
        {
            int x=Mathf.Clamp(Mathf.FloorToInt((position.x+30)/Cell),0,Width-1),z=Mathf.Clamp(Mathf.FloorToInt((position.z+36)/Cell),0,Depth-1);
            int target=Navigation.Index(x,z),best=-1;float distance=float.MaxValue;
            for(int dz=-3;dz<=3;dz++)for(int dx=-3;dx<=3;dx++)
            {
                if(x+dx<0||x+dx>=Width||z+dz<0||z+dz>=Depth)continue;
                int i=Navigation.Index(x+dx,z+dz);if(Navigation.IsBlocked(i))continue;
                float d=(CellPosition(i)-position).sqrMagnitude;if(d<distance){distance=d;best=i;}
            }
            return best;
        }
        public List<Vector3> Path(Vector3 from,Vector3 to)
        {
            var path=new List<Vector3>();
            foreach(int i in Navigation.FindPath(NearestCell(from),NearestCell(to)))path.Add(CellPosition(i));
            return path;
        }
        void SpawnPlayer()
        {
            var root=new GameObject("Player");root.layer=ActorLayer;root.transform.position=new Vector3(0,.15f,-26);
            var health=root.AddComponent<Health>();health.Team=Team.Player;health.ResetHealth();
            var cc=root.AddComponent<CharacterController>();cc.height=1.85f;cc.radius=.32f;cc.center=Vector3.up*.925f;cc.stepOffset=.35f;
            var cameraObject=new GameObject("Player Camera");cameraObject.tag="MainCamera";cameraObject.transform.SetParent(root.transform,false);cameraObject.transform.localPosition=Vector3.up*1.65f;
            var camera=cameraObject.AddComponent<Camera>();camera.nearClipPlane=.04f;camera.farClipPlane=350;camera.fieldOfView=75;camera.backgroundColor=RenderSettings.fogColor;camera.clearFlags=CameraClearFlags.SolidColor;
            cameraObject.AddComponent<AudioListener>();
            var motor=root.AddComponent<PlayerMotor>();motor.View=camera;motor.Director=Director;
            var weapons=root.AddComponent<WeaponController>();weapons.Motor=motor;weapons.Director=Director;
            Director.Player=motor;Director.Weapons=weapons;
            health.Died+=h=>Director.FailMission();
        }
        void SpawnEnemies()
        {
            Vector3[] points={new Vector3(-10,0,8),new Vector3(11,0,12),new Vector3(-4,0,26),new Vector3(6,0,29),new Vector3(-23,0,4),new Vector3(23,0,4),new Vector3(-15,0,27),new Vector3(14,0,16)};
            for(int i=0;i<Mathf.Clamp(EnemyCount,1,8);i++)
            {
                var root=new GameObject("Hostile "+(i+1));root.layer=ActorLayer;root.transform.position=points[i]+Vector3.up*.2f;
                var cc=root.AddComponent<CharacterController>();cc.height=1.65f;cc.radius=.32f;cc.center=Vector3.up*.825f;cc.stepOffset=.35f;
                var health=root.AddComponent<Health>();health.Team=Team.Hostile;health.ResetHealth();
                var torso=Primitive(PrimitiveType.Capsule,"Armored torso",root.transform,new Vector3(0,1.05f,0),new Vector3(.65f,.45f,.4f),olive);
                var head=Primitive(PrimitiveType.Sphere,"Helmet",root.transform,new Vector3(0,1.78f,0),new Vector3(.5f,.5f,.5f),metal,true);head.layer=ActorLayer;head.AddComponent<HitZone>().Head=true;
                var left=Primitive(PrimitiveType.Cube,"Left leg",root.transform,new Vector3(-.18f,.38f,0),new Vector3(.21f,.7f,.23f),olive);
                var right=Primitive(PrimitiveType.Cube,"Right leg",root.transform,new Vector3(.18f,.38f,0),new Vector3(.21f,.7f,.23f),olive);
                Primitive(PrimitiveType.Cube,"Held rifle",root.transform,new Vector3(.22f,1.1f,.4f),new Vector3(.12f,.14f,.7f),metal);
                var agent=root.AddComponent<EnemyAgent>();agent.Director=Director;agent.LeftLeg=left.transform;agent.RightLeg=right.transform;agent.PatrolOffset=i;
                Director.Enemies.Add(agent);
            }
        }
        void OnDestroy() { foreach(var asset in owned) if(asset) Destroy(asset); }
    }
}
