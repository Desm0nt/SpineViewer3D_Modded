using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GlmNet;
using Microsoft.Win32;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;

namespace Volot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ViewWindow : Window
    {
        private StreamReader Reader { get; set; }
        private float corr = 0;
        float rotateX = 0, rotateY = 0;
        float Dstnc = -5.0f;
        float UpDn = 0.0f;
        float LfRt = 0.0f;
        float eyex = 0;
        float eyey = 0;
        float eyez = 2;
        float rx = 0.3f;
        float ry = 0.3f;
        float rz = 0f;
        private bool key = false;
        private bool kei = false;
        float prevX = 0, prevY = 0;
        private int vrcount;
        public string Tmphs { get; set; }
        string workpath = "";

        //  The projection, view and model matrices.
        mat4 projectionMatrix;
        mat4 viewMatrix;
        mat4 modelMatrix;
        mat4 modelMatrix2;
        mat4 modelviewMatrix = mat4.identity();
        private mat3 normalMatrix = mat3.identity();

        //  Constants that specify the attribute indexes.
        const uint attributeIndexPosition = 0;
        const uint attributeIndexColour = 1;
        const uint attributeIndexNormal = 2;

        //  The vertex buffer array which contains the vertex and colour buffers.
        VertexBufferArray vertexBufferArray;

        //  The shader program for our vertex and fragment shader.
        private ShaderProgram shaderProgram;


        public struct Facet
        {
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }
        public struct VertList
        {
            public List<RVertex> Vert { get; set; }
            public List<RNormal> Nrml { get; set; }
            public List<RColor> Colr { get; set; }
        }
        List<VertList> Vertlist = new List<VertList>();

        public ViewWindow(string value)
        {
            InitializeComponent();
            workpath = Environment.CurrentDirectory + "\\" + "images" + "\\" + value + "\\Calc_File_out2.stl";
        }

        #region Buttons
        private void Far_Click(object sender, RoutedEventArgs e)
        {
            Dstnc -= 5.0f;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Dstnc += 5.0f;
        }
        private void left_Click(object sender, RoutedEventArgs e)
        {
            LfRt -= 1.0f;
        }
        private void right_Click(object sender, RoutedEventArgs e)
        {
            LfRt += 1.0f;
        }
        private void left1_Click(object sender, RoutedEventArgs e)
        {
            rotateY -= 1.0f;
            modelMatrix = glm.rotate(modelviewMatrix, rotateY, new vec3(0, 0.1f, 0));
        }
        private void right1_Click(object sender, RoutedEventArgs e)
        {
            rotateY += 1.0f;
            modelMatrix = glm.rotate(modelviewMatrix, rotateY, new vec3(0, 0.1f, 0));
        }
        private void up_Click(object sender, RoutedEventArgs e)
        {
            UpDn += 1.0f;
        }
        private void down_Click(object sender, RoutedEventArgs e)
        {
            UpDn -= 1.0f;
        }

        private void btn5_Click(object sender, RoutedEventArgs e)
        {
            rotateX -= 5.0f;
        }
        #endregion

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            OpenGL gl = openGLControl.OpenGL;
            //  Now create the geometry for the square.
            CreateVerticesForSquare(gl);
            kei = true;
        }

        private void btn3_Click(object sender, RoutedEventArgs e)
        {
            OpenGL gl = openGLControl.OpenGL;
            try
            {
                string s = workpath;
                var r = new CustomStlReader();
                r.LoadStl(s);
                var vertx = r.ReadVertex();
                var normls = r.ReadNormals();
                var colrs = r.ReadColors();
                Vertlist.Add(new VertList { Vert = vertx, Nrml = normls, Colr = colrs });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            vrcount = 0;
            foreach (var vl in Vertlist)
            {
                vrcount = vrcount + vl.Vert.Count;

            }
            float[] tmpvrt = new float[(vrcount * 3)];
            float[] tmpclr = new float[(vrcount * 3)];
            float[] tmpnrm = new float[(vrcount * 3)];
            int stp = 0;

            foreach (var vl in Vertlist)
            {
                for (int i = 0; i < vl.Vert.Count; i++)
                {
                    tmpvrt[stp] = (float) vl.Vert[i].X;
                    tmpvrt[stp + 1] = (float) vl.Vert[i].Y;
                    tmpvrt[stp + 2] = (float)vl.Vert[i].Z; //задает точку 
                    tmpclr[stp] = vl.Colr[i].R;
                    tmpclr[stp + 1] = vl.Colr[i].G;
                    tmpclr[stp + 2] = vl.Colr[i].B; //задает цвет   
                    tmpnrm[stp] = (float)vl.Nrml[i].NX;
                    tmpnrm[stp + 1] = (float)vl.Nrml[i].NY;
                    tmpnrm[stp + 2] = (float)vl.Nrml[i].NZ; //задает нормали  
                    stp += 3;
                }

            }
            CreateVerticesForSquare2(gl, tmpvrt, tmpclr, tmpnrm);
        }
        /// <summary>
        /// Инициализация OpenGL.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = openGLControl.OpenGL;           
            gl.ClearColor(0.23529f, 0.23529f, 0.46666f, 0);


            //  Create the shader program.
            var vertexShaderSource = ManifestResourceLoader.LoadTextFile("Shader2.vert");
            var fragmentShaderSource = ManifestResourceLoader.LoadTextFile("Shader2.frag");
            shaderProgram = new ShaderProgram();
            shaderProgram.Create(gl, vertexShaderSource, fragmentShaderSource, null);
            shaderProgram.BindAttributeLocation(gl, attributeIndexPosition, "in_Position");
            shaderProgram.BindAttributeLocation(gl, attributeIndexColour, "in_Color");
            shaderProgram.BindAttributeLocation(gl, attributeIndexNormal, "in_Norm");
            shaderProgram.AssertValid(gl);

            //  Create a perspective projection matrix.
            const float rads = (60.0f / 360.0f) * (float)Math.PI * 2.0f;
            projectionMatrix = glm.perspective(rads, (float)gl.RenderContextProvider.Width /
                                                     (float)gl.RenderContextProvider.Height, 0.01f, 600.0f);

            //  Create a view matrix to move us back a bit.
            viewMatrix = glm.translate(new mat4(1.0f), new vec3(0.0f, 0.0f, -5.0f));

            //  Create a model matrix rotate model.
            modelMatrix = glm.rotate(mat4.identity(), 1.1f, new vec3(0, 1, 0));
        }

        /// <summary>
        /// Отрисовка OpenGL-графики в форме.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //  Get the OpenGL instance that's been passed to us.
            OpenGL gl = openGLControl.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);

            if (Vertlist.Count > 0 || kei == true)
            {
                //  Bind the shader, set the matrices.
                shaderProgram.Bind(gl);
                shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
                viewMatrix = glm.translate(new mat4(1.0f), new vec3(LfRt, UpDn, Dstnc));
                shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
                shaderProgram.SetUniformMatrix4(gl, "modelMatrix", modelMatrix.to_array());
                viewMatrix = glm.translate(new mat4(1.0f), new vec3(LfRt, UpDn, Dstnc));

                //  Create a model matrix to make the model a little bigger.
                //modelMatrix = glm.rotate(mat4.identity(), rotateX, new vec3(0.1f, 0, 0));
                //modelMatrix = glm.rotate(mat4.identity(), rotateY, new vec3(0, 0.1f, 0));
                //  Bind the out vertex array.
                vertexBufferArray.Bind(gl);

                //  Draw the square.
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, vrcount);

                //  Unbind our vertex array and shader.
                vertexBufferArray.Unbind(gl);
                shaderProgram.Unbind(gl);
            }
        }

        /// <summary>
        /// Установка камеры по-умолчанию.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            //  TODO: Set the projection matrix here.

            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //  Set the projection matrix.
            gl.MatrixMode(OpenGL.GL_PROJECTION);

            //  Load the identity.
            gl.LoadIdentity();

            //  Create a perspective transformation.
            gl.Perspective(45.0f, (float)gl.RenderContextProvider.Width /
                                  (float)gl.RenderContextProvider.Height,
                0.01f, 600.0f);

            ////  Use the 'eye at' helper function to position and aim the camera.
            gl.LookAt(0, 0, 10, 0, 0, 0, 0, 1, 0);

            //  Set the modelview matrix.
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }


        private void openGlControl_MouseDown(object sender, MouseEventArgs e)
        {
            key = true;
            prevX = Convert.ToSingle(e.GetPosition(this).X);
            prevY = Convert.ToSingle(e.GetPosition(this).Y);
        }

        private void openGlControl_MouseUp(object sender, MouseEventArgs e)
        {
            key = false;
        }

        private void openGlControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (key)
            {
                float dx = (prevX - Convert.ToSingle(e.GetPosition(this).X))/40;
                float dy = (prevY - Convert.ToSingle(e.GetPosition(this).Y))/40;
                prevX = Convert.ToSingle(e.GetPosition(this).X);
                prevY = Convert.ToSingle(e.GetPosition(this).Y);
                float angleX = dx * Convert.ToSingle(Math.PI) / 180;
                float angleY = dy * Convert.ToSingle(Math.PI) / 180;
                rotateY += dx;
                rotateX -= dy;
                modelMatrix = glm.rotate(modelviewMatrix, rotateY, new vec3(0, 0.1f, 0)) * glm.rotate(modelviewMatrix, rotateX, new vec3(0.1f, 0, 0));
                //rotatezX(-angleY / 2.2f);
                //rotateY(-angleX / 2.2f);
            }
        }

        private void CreateVerticesForSquare(OpenGL gl)
        {
            var vertices = new float[36];
            var colors = new float[36]; // Colors for our vertices

            colors[0] = 1.0f; colors[1] = 0.0f; colors[2] = 0.0f;
            vertices[0] = 0.0f; vertices[1] = 1.0f; vertices[2] = 0.0f;
            colors[3] = 0.0f; colors[4] = 1.0f; colors[5] = 0.0f;
            vertices[3] = -1.0f; vertices[4] = -1.0f; vertices[5] = 1.0f;
            colors[6] = 0.0f; colors[7] = 0.3f; colors[8] = 1.0f;
            vertices[6] = 1.0f; vertices[7] = -1.0f; vertices[8] = 1.0f;
            colors[9] = 1.0f; colors[10] = 0.3f; colors[11] = 0.5f;
            vertices[9] = 0.0f; vertices[10] = 1.0f; vertices[11] = 0.0f;
            colors[12] = 0.0f; colors[13] = 0.0f; colors[14] = 1.0f;
            vertices[12] = 1.0f; vertices[13] = -1.0f; vertices[14] = 1.0f;
            colors[15] = 0.3f; colors[16] = 1.0f; colors[17] = 1.0f;
            vertices[15] = 1.0f; vertices[16] = -1.0f; vertices[17] = -1.0f;
            colors[18] = 1.0f; colors[19] = 0.0f; colors[20] = 0.4f;
            vertices[18] = 0.0f; vertices[19] = 1.0f; vertices[20] = 0.0f;
            colors[21] = 0.0f; colors[22] = 1.0f; colors[23] = 0.0f;
            vertices[21] = 1.0f; vertices[22] = -1.0f; vertices[23] = -1.0f;
            colors[24] = 0.0f; colors[25] = 0.5f; colors[26] = 1.0f;
            vertices[24] = -1.0f; vertices[25] = -1.0f; vertices[26] = -1.0f;
            colors[27] = 1.0f; colors[28] = 0.0f; colors[29] = 0.0f;
            vertices[27] = 0.0f; vertices[28] = 1.0f; vertices[29] = 0.0f;
            colors[30] = 0.0f; colors[31] = 0.0f; colors[32] = 1.0f;
            vertices[30] = -1.0f; vertices[31] = -1.0f; vertices[32] = -1.0f;
            colors[33] = 0.0f; colors[34] = 1.0f; colors[35] = 0.0f;
            vertices[33] = -1.0f; vertices[34] = -1.0f; vertices[35] = 1.0f;

            //  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);

            //  Create a vertex buffer for the vertex data.
            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, colors, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(gl);
            colourDataBuffer.Bind(gl);
            colourDataBuffer.SetData(gl, 1, vertices, false, 3);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);
        }
        private void CreateVerticesForSquare2(OpenGL gl, float[] vert, float[] col, float[] nrm)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            string graphicsCard = string.Empty;
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["CurrentBitsPerPixel"] != null && obj["CurrentHorizontalResolution"] != null)
                {
                    graphicsCard = obj["Name"].ToString();
                }
            }

            string[] checkname1 = graphicsCard.Split(' ');
            List<string> checkname2 = new List<string>(checkname1);
            string match = checkname2.FirstOrDefault(element => element.Equals("nvidia",
                                     StringComparison.CurrentCultureIgnoreCase));
            var vertices = nrm;
            var colors = vert; // Colors for our vertices  
            var normals = col;


            if (match != null)
            {
                vertices = vert;
                colors = col; // Colors for our vertices  
                normals = nrm;
            }


            //  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(gl);
            vertexBufferArray.Bind(gl);

            //  Create a vertex buffer for the vertex data.
            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(gl);
            colourDataBuffer.Bind(gl);
            colourDataBuffer.SetData(gl, 1, colors, false, 3);

            var normalDataBuffer = new VertexBuffer();
            normalDataBuffer.Create(gl);
            normalDataBuffer.Bind(gl);
            normalDataBuffer.SetData(gl, 2, normals, false, 3);


            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(gl);
        }
    }
}
