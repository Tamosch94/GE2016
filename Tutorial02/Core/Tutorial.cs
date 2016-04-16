using System;
using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.GUI;
using Fusee.Math.Core;
using Fusee.Serialization;
using static Fusee.Engine.Core.Input;


namespace Fusee.Tutorial.Core
{

    [FuseeApplication(Name = "Tutorial Example", Description = "The official FUSEE Tutorial.")]
    public class Tutorial : RenderCanvas
    {
        private IShaderParam _alphaParam; // declare 2 fields for alpha and beta
        private IShaderParam _betaParam;
        private float _alpha, _beta; // create beta for rotation
        private IShaderParam _mouseParam; // setup mouse field
        private float2 _mousePosition; // create Mouse variables dor pixelshader, task: 

        private Mesh _mesh;
        private const string _vertexShader = @"
            attribute vec3 fuVertex;
            varying mat4 rotX;
            varying mat4 rotY;
            uniform float alpha;
            uniform float beta;
            // add varying value modelpos
            varying vec3 modelpos;
            // Pixelpos variable
            varying vec2 pixelPosition;
            varying vec4 position;



            void main()
            {
                modelpos = fuVertex;
                float s1 = sin(alpha);
                float c1 = cos(alpha);
                // Add rotation in 2ndaxis
                float s2 = sin(beta);
                float c2 = cos(beta);
                /* gl_Position = vec4( fuVertex.x * c - fuVertex.y * s,   // The transformed x coordinate
                                fuVertex.x * s + fuVertex.y * c,   // The transformed y coordinate
                                fuVertex.z,                        // z is unchanged
                                1.0);
                */

                rotX = mat4(1, 0, 0, 0,
                            0, c1, -s1, 0,
                            0, s1, c1, 0,
                            0, 0, 0, 1);
                
                rotY = mat4(c2, 0, s2, 0,
                            0, 1, 0, 0,
                            -s2, 0, c2, 0,
                            0, 0, 0, 1);
                gl_Position = rotX * rotY * vec4( fuVertex, 1.0);
            }";

        private const string _pixelShader = @"
            #ifdef GL_ES
                precision highp float;
            #endif
            varying vec3 modelpos;
            // add the variables you are going to use
            uniform vec2 mouse;
            varying float distance;
            varying vec4 position;
            

            void main()
            {
                float distance = distance(mouse, vec2(position.x, position.y));
                gl_FragColor = vec4(modelpos, 1) - vec4(distance, distance, distance, 1) + 1; // +1 increases intensity of color
            }";


        // Init is called on startup. 
        public override void Init()
        {
            _mesh = new Mesh
            {
                Vertices = new[]
            {
                new float3(-0.8f, -0.3333f, -0.47f), // Vertex 0
                new float3(0.8f, -0.3333f, -0.4714f),  // Vertex 1
                new float3(0, -0.3333f, 0.9428f),         // Vertex 2
                new float3(0, 1, 0),                      // Vertex 3

                //create additional vertex to build a more complex geometry

                new float3(-0.8f, -1,  0.47f),
                new float3(0, -1, 0.9428f),
                new float3(0.8f, -1, -0.47f),


            },
                Triangles = new ushort[]
                {
                    0, 2, 1,  // Triangle 0 "Bottom" facing towards negative y axis
                    0, 1, 3,  // Triangle 1 "Back side" facing towards negative z axis
                    1, 2, 3,  // Triangle 2 "Right side" facing towards positive x axis
                    2, 0, 3,  // Triangle 3 "Left side" facing towrads negative x axis
                    
                    // create a house
                    0, 4, 2,
                    2, 4, 5,
                    2, 5, 6,
                    1, 2, 6,
                },
            };

            var shader = RC.CreateShader(_vertexShader, _pixelShader);
            RC.SetShader(shader);
            _alphaParam = RC.GetShaderParam(shader, "alpha");
            _alpha = 0;
            _betaParam = RC.GetShaderParam(shader, "beta");
            _beta = 0;
            _mouseParam = RC.GetShaderParam(shader, "mouse");
            _mousePosition = new float2(1, 1);

            // Set the clear color for the backbuffer.
            RC.ClearColor = new float4(0.1f, 0.3f, 0.2f, 1);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            float2 speed = Mouse.Velocity; // create a 2-dimensional float named speed and capture the mouse speed
            // Mr Muellers Mouse- Formula
            float2 mousePos = new float2(((1.0f / (Width / 2.0f)) * Mouse.Position.x) - 1.0f, (((1.0f / (Height / 2.0f)) * Mouse.Position.y) - 1.0f) * -1.0f);

            //get mousepos variable and assign the mouse Position


            _mousePosition = mousePos;


            // create a mouse-on-click-interaction
            if (Mouse.LeftButton == true)
            {
                _alpha += speed.x * 0.0001f;
                RC.SetShaderParam(_alphaParam, _alpha);
                _beta += speed.x * 0.0001f;
                RC.SetShaderParam(_betaParam, _beta);
            }

            // navigate view with keyboard
            _alpha += Keyboard.LeftRightAxis * 0.4f;
            _beta += Keyboard.UpDownAxis * 0.4f;
            RC.SetShaderParam(_alphaParam, _alpha);
            RC.SetShaderParam(_betaParam, _beta);
            RC.SetShaderParam(_mouseParam, _mousePosition);


            RC.Render(_mesh);

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