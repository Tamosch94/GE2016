﻿using System;
using System.Collections.Generic;
using System.Linq;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Math.Core;
using Fusee.Serialization;
using Fusee.Xene;
using static Fusee.Engine.Core.Input;

namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        

        private Mesh _mesh;
        private const string _vertexShader = @"
            attribute vec3 fuVertex;
            attribute vec3 fuNormal;
            uniform mat4 FUSEE_MVP;
            uniform mat4 FUSEE_MV;
            varying vec3 modelpos;
            varying vec3 normal;
            void main()
            {
                modelpos = fuVertex;
                normal = normalize(mat3(FUSEE_MV) * fuNormal);
                gl_Position = FUSEE_MVP * vec4(fuVertex, 1.0);
            }";

        private const string _pixelShader = @"
            #ifdef GL_ES
                precision highp float;
            #endif
            varying vec3 modelpos;
            varying vec3 normal;
            uniform vec3 albedo;

            void main()
            {
                float intensity = dot(normal, vec3(0, 0, -1));
                gl_FragColor = vec4(intensity * albedo, 1);
            }";

        private float _alpha;
        private float _beta;

        private float _yawCube1;
        private float _pitchCube1;
        private float _yawCube2;
        private float _pitchCube2;
        private IShaderParam _albedoParam;
        private SceneOb _root;
        private SceneOb _rightArmMesh;
        private SceneOb _leftArmMesh;
        private float _armPitchRight;
        private float _armPitchLeft;
        private SceneOb _bodyMesh;

        void RenderSceneOb(SceneOb so, float4x4 modelView)
        {
            modelView = modelView * ModelXForm(so.Pos, so.Rot, so.Pivot) * float4x4.CreateScale(so.Scale);
            if (so.Mesh != null)
            {
                RC.ModelView = modelView * float4x4.CreateScale(so.ModelScale);
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
        }
        // helper method that allows us to load a Mesh with a single method call.
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

        public static SceneOb FindSceneOb(SceneOb so, string name)
        {
            if (so.Children != null)
            {

                foreach (var child in so.Children)
                {
                    if (child.nameOfMesh != null && child.nameOfMesh.Equals(name))
                        return child;

                    var grandChild = FindSceneOb(child, name);

                    if (grandChild != null)
                        return grandChild;
                }
            }
            return null; // not found
        
    }
        // Init is called on startup. 
        public override void Init()
        {
            var shader = RC.CreateShader(_vertexShader, _pixelShader);
            RC.SetShader(shader);
            _albedoParam = RC.GetShaderParam(shader, "albedo");

            // Load some meshes
            Mesh cone = LoadMesh("Cone.fus");
            Mesh cube = LoadMesh("Cube.fus");
            Mesh cylinder = LoadMesh("Cylinder.fus");
            Mesh pyramid = LoadMesh("Pyramid.fus");
            Mesh sphere = LoadMesh("Sphere.fus");

            // Setup a list of objects
            _root = new SceneOb
            {
                Children = new List<SceneOb>(new[]
                {
                    // Body
                    new SceneOb { Mesh = cube,     Pos = new float3(0, 2.75f, 0),     ModelScale = new float3(0.5f, 1, 0.25f), nameOfMesh = "bodyMesh",},
                    
                    // rightLeg
                    new SceneOb
                    {
                        Mesh = cylinder, Pos = new float3(-0.25f, 1, 0),    ModelScale = new float3(0.15f, 1, 0.15f), nameOfMesh = "rightLegMesh",   
                        Children = new List<SceneOb>(new[]
                        {
                            new SceneOb { Mesh = pyramid, Pos = new float3(0, -1f, -0.1f),    ModelScale = new float3(0.25f, 0.1f, 0.6f), Albedo  = new float3(0.2f, 0.2f, 0.2f), nameOfMesh = "rightFootMesh" },
                        })
                    },

                    //leftLeg
                    new SceneOb
                    {
                        Mesh = cylinder, Pos = new float3( 0.25f, 1, 0),    ModelScale = new float3(0.15f, 1, 0.15f), nameOfMesh = "leftLegMesh",
                        Children = new List<SceneOb>(new[]
                        {
                            //leftFoot
                            new SceneOb
                            {
                                Mesh = pyramid, Pos = new float3(0, -1, -0.1f),    ModelScale = new float3(0.25f, 0.1f, 0.6f), Albedo  = new float3(0.2f, 0.2f, 0.2f), nameOfMesh = "leftFootMesh"
                            },
                        })
                    },
                   // rightShoulder
                    new SceneOb
                    {
                        Mesh = sphere,   Pos = new float3(-0.75f, 3.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), nameOfMesh = "righthoulderMesh",
                         Children = new List<SceneOb>(new[]
                        {
                            //rightArm
                            new SceneOb
                            {
                                Mesh = cylinder, Pos = new float3(0, -1f, 0), ModelScale = new float3(0.15f, 1, 0.15f), Pivot = new float3(0, 1f, 0), nameOfMesh = "rightArmMesh",
                                 Children = new List<SceneOb>(new[]
                                {
                                     //rightelbow
                                    new SceneOb
                                    {
                                        Mesh = sphere,   Pos = new float3(0, 0, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), Albedo  = new float3(0.2f, 0.5f, 0.2f),
                                         Children = new List<SceneOb>(new[]
                                         {
                                             //rightHand
                                             new SceneOb
                                             {
                                                Mesh = sphere, Pos = new float3( 0, -1f, 0), ModelScale = new float3(0.25f, 0.5f, 0.25f), nameOfMesh = "rightHandMesh",

                                             }

                                         }),

                                    }
                                }),
                                

                            },
                         })
                    },

                    //leftShoulder
                    new SceneOb
                    {
                        Mesh = sphere,   Pos = new float3( 0.75f, 3.5f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), nameOfMesh = "leftShoulderMesh",
                         Children = new List<SceneOb>(new[]
                        {
                            //leftArm
                            new SceneOb
                            {
                                Mesh = cylinder, Pos = new float3( 0, -1f, 0), ModelScale = new float3(0.15f, 1, 0.15f), Pivot = new float3(0, 1f, 0), nameOfMesh = "leftArmMesh",
                                Children = new List<SceneOb>(new[]
                                {    
                                    //leftElbow
                                    new SceneOb
                                    {
                                        Mesh = sphere,   Pos = new float3( 0, 0f, 0), ModelScale = new float3(0.25f, 0.25f, 0.25f), Albedo  = new float3(0.2f, 0.5f, 0.2f),
                                            Children = new List<SceneOb>(new[]
                                        {
                                            //leftHand
                                            new SceneOb
                                            {
                                                Mesh = sphere, Pos = new float3( 0, -1f, 0), ModelScale = new float3(0.25f, 0.5f, 0.25f), nameOfMesh = "leftHandMesh",


                                            }
                                        }),
                                    },
                                }),

                            },

                         })

                    },                    
                    // Head
                    new SceneOb { Mesh = sphere,   Pos = new float3(0, 4.2f, 0),      ModelScale = new float3(0.35f, 0.5f, 0.35f),  },
                })
            };


            // Set the clear color for the backbuffer
            RC.ClearColor = new float4(1, 1, 1, 1);

            // look for _root children name if no errors occur = success
           _leftArmMesh = FindSceneOb(_root, "leftArmMesh");
           _rightArmMesh = FindSceneOb(_root, "rightArmMesh");

        }

        static float4x4 ModelXForm(float3 pos, float3 rot, float3 pivot)
        {
            return float4x4.CreateTranslation(pos + pivot)
                   *float4x4.CreateRotationY(rot.y)
                   *float4x4.CreateRotationX(rot.x)
                   *float4x4.CreateRotationZ(rot.z)
                   *float4x4.CreateTranslation(-pivot);
        }

        
        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity + Touch.GetVelocity(TouchPoints.Touchpoint_0);
            if (Mouse.LeftButton || Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                _alpha -= speed.x*0.0001f;
                _beta  -= speed.y*0.0001f;
            }

            _leftArmMesh.Rot.x = _armPitchRight;
            _rightArmMesh.Rot.x = _armPitchLeft;

            // rotation controls for 
            _armPitchLeft += Keyboard.ADAxis * 0.1f;
            _armPitchRight += Keyboard.LeftRightAxis*0.1f;
            // Setup matrices
            var aspectRatio = Width / (float)Height;
            RC.Projection = float4x4.CreatePerspectiveFieldOfView(3.141592f * 0.25f, aspectRatio, 0.01f, 20);
            float4x4 view = float4x4.CreateTranslation(0, 0, 8) * float4x4.CreateRotationY(_alpha) * float4x4.CreateRotationX(_beta) * float4x4.CreateTranslation(0, -2f, 0);

            RenderSceneOb(_root, view);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered farame) on the front buffer.
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