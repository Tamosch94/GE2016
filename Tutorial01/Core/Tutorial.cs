using System;
using System.Collections.Generic;
using System.Dynamic;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.GUI;
using Fusee.Math.Core;
using Fusee.Serialization;


namespace Fusee.Tutorial.Core
{
    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        // Create private Mesh field
        private Mesh _mesh;
        // Create another Mesh field
        private Mesh _mesh2;
        // create shader field
        private List<ShaderProgram> _shaderFieldList = new List<ShaderProgram>();

        // Shader Code: vertex and pixel Shaders
        private const string _vertexShader = @"
            attribute vec3 fuVertex;

            void main()
            {
                gl_Position = vec4(fuVertex, 1.0);
            }";

        private const string _pixelShader = @"
            #ifdef GL_ES
                precision highp float;
            #endif

            void main()
            {
                gl_FragColor = vec4(1, 0, 1, 1);
            }";

        //set another pixel shader w/ different color
        private const string _pixelShader2 = @"
            #ifdef GL_ES
                precision highp float;
            #endif

            void main()
            {
                gl_FragColor = vec4(0, 1, 0, 1);
            }";

        // Init is called on startup. 
        public override void Init()
        {

            // Set the clear color for the backbuffer to light green.
            RC.ClearColor = new float4(0.5f, 1, 0.7f, 1);
            
            // add shaders
            _shaderFieldList.Add(RC.CreateShader(_vertexShader, _pixelShader));
            _shaderFieldList.Add(RC.CreateShader(_vertexShader, _pixelShader2));

            // Initialize _mesh with a single triangle
            _mesh = new Mesh
            {
                Vertices = new[]
                {
                    new float3(-0.75f, 0.75f, 0),
                    new float3(0.75f, 0.75f, 0),
                    new float3(-0.75f, -0.4f, 0),
                    // add another vertex into vertices array
                    new float3(0.75f, -0.4f, 0),
                },

                Triangles = new ushort[] {
                    0, 2 , 1,
                    1, 2 , 3
                },
            };

            _mesh2 = new Mesh
            { 
                Vertices = new[]
                {
                        new float3(-1, 1f, 0),
                        new float3(1, 1, 0),
                        new float3(-1, -0.8f, 0),
                        // add another vertex into vertices array
                        new float3(1, -0.8f, 0),
                    },

                    Triangles = new ushort[] {
                        0, 2 , 1,
                        1, 2 , 3
                    },
                };
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Draw _mesh
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            // Configure Rendered mesh through shader
            RC.SetShader(_shaderFieldList[0]);
            RC.Render(_mesh);

            RC.SetShader(_shaderFieldList[1]);
            RC.Render(_mesh2);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rerndered farame) on the front buffer.
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