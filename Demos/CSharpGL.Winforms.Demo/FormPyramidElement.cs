﻿using GLM;
using CSharpGL.Objects;
using CSharpGL.Objects.SceneElements;
using CSharpGL.Objects.Shaders;
using System.Windows.Forms;

namespace CSharpGL.Winforms.Demo
{
    public partial class FormPyramidElement : Form
    {
        PyramidElement pyramidElement;

        public mat4 projectionMatrix;
        public mat4 viewMatrix;
        public mat4 modelMatrix;
        private float rotation;

        public FormPyramidElement()
        {
            InitializeComponent();

            pyramidElement = new PyramidElement();
            pyramidElement.Initialize();

            pyramidElement.BeforeRendering += pyramidElement_BeforeRendering;
            pyramidElement.AfterRendering += pyramidElement_AfterRendering;

            //this.glCanvas1.MouseWheel += glCanvas1_MouseWheel;
            //this.glCanvas1.KeyPress += glCanvas1_KeyPress;
            //this.glCanvas1.MouseDown += glCanvas1_MouseDown;
            //this.glCanvas1.MouseMove += glCanvas1_MouseMove;
            //this.glCanvas1.MouseUp += glCanvas1_MouseUp;
            this.glCanvas1.OpenGLDraw += glCanvas1_OpenGLDraw;
            //this.glCanvas1.Resize += glCanvas1_Resize;
        }

        void pyramidElement_AfterRendering(object sender, Objects.RenderEventArgs e)
        {
            IMVP element = sender as IMVP;

            element.ResetShaderProgram();
        }

        void pyramidElement_BeforeRendering(object sender, Objects.RenderEventArgs e)
        {
            rotation += 3.0f;
            mat4 modelMatrix = glm.rotate(rotation, new vec3(0, 1, 0));

            const float distance = 0.7f;
            viewMatrix = glm.lookAt(new vec3(-distance, distance, -distance), new vec3(0, 0, 0), new vec3(0, -1, 0));

            int[] viewport = new int[4];
            GL.GetInteger(GetTarget.Viewport, viewport);
            projectionMatrix = glm.perspective(60.0f, (float)viewport[2] / (float)viewport[3], 0.01f, 100.0f);

            mat4 mvp = projectionMatrix * viewMatrix * modelMatrix;

            IMVP element = sender as IMVP;

            element.SetShaderProgram(mvp);
        }

        private void glCanvas1_OpenGLDraw(object sender, PaintEventArgs e)
        {
            GL.ClearColor(0x87 / 255.0f, 0xce / 255.0f, 0xeb / 255.0f, 0xff / 255.0f);
            GL.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

            var arg = new RenderEventArgs(RenderModes.Render, null);
            pyramidElement.Render(arg);
        }
    }
}
