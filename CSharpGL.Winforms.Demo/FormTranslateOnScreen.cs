﻿using CSharpGL.Maths;
using CSharpGL.Objects.Cameras;
using CSharpGL.Objects.Shaders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharpGL.Winforms.Demo
{
    public partial class FormTranslateOnScreen : Form
    {
        AxisElement element;

        ScientificCamera camera;

        SatelliteRotator satelliteRoration;

        public FormTranslateOnScreen()
        {
            InitializeComponent();

            if (CameraDictionary.Instance.ContainsKey("FormTranslateOnScreen"))
            {
                this.camera = CameraDictionary.Instance["FormTranslateOnScreen"];
            }
            else
            {
                this.camera = new ScientificCamera(CameraTypes.Ortho, this.glCanvas1.Width, this.glCanvas1.Height);
                CameraDictionary.Instance.Add("FormTranslateOnScreen", this.camera);
            }

            satelliteRoration = new SatelliteRotator(camera);

            var faceCount = 10;
            var radius = 0.1f;
            var height = 10f;
            element = new AxisElement(radius, height, faceCount);
            element.Initialize();

            element.BeforeRendering += element_BeforeRendering;
            element.AfterRendering += element_AfterRendering;

            this.glCanvas1.MouseWheel += glCanvas1_MouseWheel;
        }

        void element_AfterRendering(object sender, Objects.RenderEventArgs e)
        {
            element.shaderProgram.Unbind();
        }

        void element_BeforeRendering(object sender, Objects.RenderEventArgs e)
        {
            mat4 projectionMatrix = camera.GetProjectionMat4();

            mat4 viewMatrix = camera.GetViewMat4();

            mat4 modelMatrix = mat4.identity();

            ShaderProgram shaderProgram = element.shaderProgram;

            shaderProgram.Bind();

            shaderProgram.SetUniformMatrix4(CylinderVAOElement.strprojectionMatrix, projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(CylinderVAOElement.strviewMatrix, viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(CylinderVAOElement.strmodelMatrix, modelMatrix.to_array());
        }

        private void glCanvas1_MouseWheel(object sender, MouseEventArgs e)
        {
            this.camera.Scale(e.Delta);
        }

        private void FormTranslateOnScreen_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Use 'c' to switch camera types between perspective and ortho");

            // Init GL
            GL.ClearColor(0x87 / 255.0f, 0xce / 255.0f, 0xeb / 255.0f, 0xff / 255.0f);
            // first resize
            glCanvas_Resize(this.glCanvas1, e);
        }

        private void glCanvas1_OpenGLDraw(object sender, RenderEventArgs e)
        {
            //  Clear the color and depth buffer.
            GL.Clear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);

            element.Render(Objects.RenderModes.Render);
        }

        private void glCanvas_Resize(object sender, EventArgs e)
        {
            if (this.camera != null)
            {
                this.camera.Resize(this.glCanvas1.Width, this.glCanvas1.Height);
            }
            ////  Set the projection matrix.
            //GL.MatrixMode(GL.GL_PROJECTION);

            ////  Load the identity.
            //GL.LoadIdentity();

            ////  Create a perspective transformation.
            //GL.gluPerspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);

            ////  Use the 'look at' helper function to position and aim the camera.
            //GL.gluLookAt(-5, 5, -5, 0, 0, 0, 0, 1, 0);

            ////  Set the modelview matrix.
            //GL.MatrixMode(GL.GL_MODELVIEW);
        }


        private void glCanvas1_MouseDown(object sender, MouseEventArgs e)
        {
            satelliteRoration.SetBounds(this.glCanvas1.Width, this.glCanvas1.Height);
            satelliteRoration.MouseDown(e.X, e.Y);
            PrintCameraInfo();
        }

        private void glCanvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (satelliteRoration.mouseDownFlag)
            {
                satelliteRoration.MouseMove(e.X, e.Y);
                PrintCameraInfo();
            }
        }

        private void glCanvas1_MouseUp(object sender, MouseEventArgs e)
        {
            satelliteRoration.MouseUp(e.X, e.Y);
            PrintCameraInfo();
        }

        private void PrintCameraInfo()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Format("position:{0}", this.camera.Position));
            builder.Append(string.Format(" target:{0}", this.camera.Target));
            builder.Append(string.Format(" up:{0}", this.camera.UpVector));
            builder.Append(string.Format(" camera type: {0}", this.camera.CameraType));

            this.lblInfo.Text = builder.ToString();
        }

        private void glCanvas1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'c')
            {
                switch (this.camera.CameraType)
                {
                    case CameraTypes.Perspecitive:
                        this.camera.CameraType = CameraTypes.Ortho;
                        break;
                    case CameraTypes.Ortho:
                        this.camera.CameraType = CameraTypes.Perspecitive;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                PrintCameraInfo();
            }

        }

    }
}