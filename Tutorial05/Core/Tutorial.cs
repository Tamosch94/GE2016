using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{
    class Renderer : SceneVisitor
    {
        public RenderContext RC;
        public IShaderParam AlbedoParam;
        public IShaderParam ShininessParam;
        public IShaderParam ShininessGainParam;
        //add specularint and speccolor
        public IShaderParam SpecularIntensityParam;
        public IShaderParam SpecularColorParam;

        public float4x4 View;
        private Dictionary<MeshComponent, Mesh> _meshes = new Dictionary<MeshComponent, Mesh>();
        private CollapsingStateStack<float4x4> _model = new CollapsingStateStack<float4x4>();

        public Renderer(RenderContext rc)
        {
            RC = rc;
            // Initialize the shader(s)
            var vertsh = AssetStorage.Get<string>("VertexShader.vert");
            var pixsh = AssetStorage.Get<string>("PixelShader.frag");
            var shader = RC.CreateShader(vertsh, pixsh);
            RC.SetShader(shader);
            AlbedoParam = RC.GetShaderParam(shader, "albedo");
            ShininessParam = RC.GetShaderParam(shader, "shininess");
            ShininessGainParam = RC.GetShaderParam(shader, "shineGain");
            SpecularIntensityParam = RC.GetShaderParam(shader, "specIntensity");
            SpecularColorParam = RC.GetShaderParam(shader, "specColor");


        }
        private Mesh LookupMesh(MeshComponent mc)
        {
            Mesh mesh;
            if (!_meshes.TryGetValue(mc, out mesh))
            {
                mesh = new Mesh
                {
                    Vertices = mc.Vertices,
                    Normals = mc.Normals,
                    Triangles = mc.Triangles
                };
                _meshes[mc] = mesh;
            }
            return mesh;
        }

        protected override void InitState()
        {
            _model.Clear();
            _model.Tos = float4x4.Identity;
        }

        protected override void PushState()
        {
            _model.Push();
        }

        protected override void PopState()
        {
            _model.Pop();
            RC.ModelView = View * _model.Tos;
        }

        [VisitMethod]
        void OnMesh(MeshComponent mesh)
        {
            RC.Render(LookupMesh(mesh));
        }

        [VisitMethod]
        void OnMaterial(MaterialComponent material)
        {
            RC.SetShaderParam(AlbedoParam, material.Diffuse.Color);
            RC.SetShaderParam(ShininessParam, material.Specular.Shininess);
            RC.SetShaderParam(SpecularIntensityParam, material.Specular.Intensity);
            RC.SetShaderParam(SpecularColorParam, material.Specular.Color);
        }

        [VisitMethod]
        void OnTransform(TransformComponent xform)
        {
            _model.Tos *= xform.Matrix();
            RC.ModelView = View * _model.Tos;
        }
    }

    //BEGINNING OF TUTORIAL CLASS
    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private Mesh _mesh;

        private IShaderParam _albedoParam;
        private float _alpha = 0.001f;
        private float _beta;

        private SceneOb _root;

        private SceneContainer _wuggy;
        private Renderer _renderer;

        //reference model component
        private TransformComponent _wheelBigL;
        private TransformComponent _wheelBigR;
        private TransformComponent _smallWheelR;
        private TransformComponent _smallWheelL;
        private TransformComponent _wuggyMesh;
        private TransformComponent _neckHiAndCamMount;


        //reference shininess
        private float _shininess;

        public static Mesh LoadMesh(string assetName)
        {
            SceneContainer sc = AssetStorage.Get<SceneContainer>(assetName);
            MeshComponent mc = sc.Children.FindComponents<MeshComponent>(c => true).First();
            return new Mesh
            {
                Vertices = mc.Vertices,
                Normals = mc.Normals,
                Triangles = mc.Triangles
            };
        }

        // Init is called on startup. 
        public override void Init()
        {
            //load contents of wuggy.fus  into the wuggy fiels and create an instance of renderer
            _wuggy = AssetStorage.Get<SceneContainer>("wuggy.fus");
            _wheelBigL = _wuggy.Children.FindNodes(n => n.Name == "WheelBigL").First().GetTransform();
            _wheelBigR = _wuggy.Children.FindNodes(n => n.Name == "WheelBigR").First().GetTransform();
            _smallWheelR = _wuggy.Children.FindNodes(n => n.Name == "WheelSmallR").First().GetTransform();
            _smallWheelL = _wuggy.Children.FindNodes(n => n.Name == "WheelSmallL").First().GetTransform();
            _wuggyMesh = _wuggy.Children.FindNodes(n => n.Name == "Wuggy").First().GetTransform();
            _neckHiAndCamMount = _wuggy.Children.FindNodes(n => n.Name == "NeckHi").First().GetTransform();
            _renderer = new Renderer(RC);

            // Initialize the shader(s)
   
     

            //initialize renderer properties


            // Load some meshes
            Mesh cube = LoadMesh("Cube.fus");
            Mesh cylinder = LoadMesh("Cylinder.fus");
            Mesh sphere = LoadMesh("Sphere.fus");

            /*// Setup a list of objects
            _root = new SceneOb { 
                Children = new List<SceneOb>(new []
                {
                    // Body
                    new SceneOb { Mesh = cube,     Pos = new float3(0, 2.75f, 0),     ModelScale = new float3(0.5f, 1, 0.25f),      Albedo = new float3(0.2f, 0.6f, 0.3f) },
                    // Legs
                    new SceneOb { Mesh = cylinder, Pos = new float3(-0.25f, 1, 0),    ModelScale = new float3(0.15f, 1, 0.15f),     },
                    new SceneOb { Mesh = cylinder, Pos = new float3( 0.25f, 1, 0),    ModelScale = new float3(0.15f, 1, 0.15f),     },
                    // Shoulders
                    new SceneOb { Mesh = sphere,   Pos = new float3(-0.75f, 3.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), },
                    new SceneOb { Mesh = sphere,   Pos = new float3( 0.75f, 3.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), },
                    // Arms
                    new SceneOb { Mesh = cylinder, Pos = new float3(-0.75f, 2.5f, 0), ModelScale = new float3(0.15f, 1, 0.15f),     },
                    new SceneOb { Mesh = cylinder, Pos = new float3( 0.75f, 2.5f, 0), ModelScale = new float3(0.15f, 1, 0.15f),     },
                    // Head
                    new SceneOb
                    {
                        Mesh = sphere,   Pos = new float3(0, 4.2f, 0),      ModelScale = new float3(0.35f, 0.5f, 0.35f),  
                        Albedo = new float3(0.9f, 0.6f, 0.5f)
                    },
                })};*/

            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);
        }

        static float4x4 ModelXForm(float3 pos, float3 rot, float3 pivot)
        {
            return float4x4.CreateTranslation(pos + pivot)
                   *float4x4.CreateRotationY(rot.y)
                   *float4x4.CreateRotationX(rot.x)
                   *float4x4.CreateRotationZ(rot.z)
                   *float4x4.CreateTranslation(-pivot);
        }

        /*void RenderSceneOb(SceneOb so, float4x4 modelView)
        {
            modelView = modelView * ModelXForm(so.Pos, so.Rot, so.Pivot) * float4x4.CreateScale(so.Scale);
            if (so.Mesh != null)
            {
                RC.ModelView = modelView*float4x4.CreateScale(so.ModelScale);
                RC.SetShaderParam(_albedoParam, so.Albedo);
                RC.Render(so.Mesh);
            }

            if (so.Children != null)
            {
                foreach (var child in so.Children)
                {
                    RenderSceneOb(child, modelView);
                }
            }
        }*/


        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            _renderer.Traverse(_wuggy.Children);

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x*0.0001f;
                _beta  -= speed.y*0.0001f;
            }

            //shininess
            if (Keyboard.GetKey(KeyCodes.H))
            {   
                //more shininess
                _shininess = _shininess + 0.1f;
            }
            else if (Keyboard.GetKey(KeyCodes.L) && _shininess > 0)
            {
                //lower shininess
                _shininess = _shininess - 0.1f;
            }
            
            //assign values to pixelshader var
            _renderer.RC.SetShaderParam(_renderer.ShininessGainParam, _shininess);

            _wheelBigL.Rotation += new float3(-0.05f * Keyboard.WSAxis, 0, 0);
            _wheelBigR.Rotation += new float3(-0.05f * Keyboard.WSAxis, 0, 0);
            _smallWheelL.Rotation += new float3(-0.05f * Keyboard.WSAxis, 0, 0);
            _smallWheelR.Rotation += new float3(-0.05f * Keyboard.WSAxis, 0, 0);
            _smallWheelL.Rotation.y = -0.3f * Keyboard.ADAxis;
            _smallWheelR.Rotation.y = -0.3f * Keyboard.ADAxis;
            _neckHiAndCamMount.Rotation += new float3(0 , 0.05f * Keyboard.LeftRightAxis, 0);
            _neckHiAndCamMount.Translation += new float3(0, 0.5f * Keyboard.UpDownAxis, 0);

            if (_neckHiAndCamMount.Translation.y > 25f)
            {
                _neckHiAndCamMount.Translation.y = 25f;
            }
            else if (_neckHiAndCamMount.Translation.y < -30.0f)
            {
                _neckHiAndCamMount.Translation.y = -30.0f;
            }

            //rotate wuggy
            float sin = 0.05f * (float)System.Math.Sin(_wuggyMesh.Rotation.y);
            float cos = 0.05f * (float)System.Math.Cos(_wuggyMesh.Rotation.y);

            // move wuggy
            _wuggyMesh.Translation += new float3(-sin * Keyboard.WSAxis, 0, -cos * Keyboard.WSAxis);
            _wuggyMesh.Rotation += new float3(0, 0.05f * Keyboard.ADAxis, 0);

            // Setup matrices
            var aspectRatio = Width / (float)Height;
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 0.01f, 20);
            float4x4 view = float4x4.CreateTranslation(0, 0, 5) * float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta) *
                                float4x4.CreateTranslation(0, -0.5f, 0);

            _renderer.View = view;
            _renderer.Traverse(_wuggy.Children);


            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }


        // Is called when the window was resized
        public override void Resize()
        {
            // Set the new rendering area to the entire new windows size
            RC.Viewport(0, 0, Width, Height);

            // Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width/(float) Height;

            // 0.25*PI Rad -> 45° Opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            // Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            // Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 1, 20000);
            RC.Projection = projection;
        }

    }
}