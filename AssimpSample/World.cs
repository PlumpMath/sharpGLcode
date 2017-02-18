
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL.SceneGraph.Assets;
using SharpGL;
using System.Collections.Generic;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using SharpGL.SceneGraph.Cameras;
using System.Linq;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        public float CustomProp = 0;

        private LookAtCamera lookAtCam;
        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        public float ScaleVozilo = 1;
        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 7000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        public float xTranslate = 0;
        public float yTranslate = 0;
        private float howNear = 0;

        private enum TextureObjects { Beton = 0, Cigla , Trava};
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        /// <summary>
        ///	 Identifikatori OpenGL tekstura
        /// </summary>
        private uint[] m_textures = null;

        /// <summary>
        ///	 Putanje do slika koje se koriste za teksture
        /// </summary>
        private string[] m_textureFiles = { "teksture/beton.jpg", "teksture/brick.jpg", "teksture/grass.png", "..//..//images//door.jpg" };

        public AnimationState StanjeAnimacije = AnimationState.STOPED;

        public enum AnimationState
        {
           STOPED, POCETAK , CEKA_RAMPU, NASTAVLJA_POSLE_RAMPE, RIKVERC_NA_ISTOVAR,  NAPRED_POSLE_ISTOVARA, NASTAVLJA_U_NEPOZNATO
        }

        private double KamionX = 0;
        private double KamionZ = 0;
        private float RampaPodignuta = 0; // 0 do 1
        private bool KaminOkrenut = false;
        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set {
                if(value < 60 && value > -60)
                    m_xRotation = value;
            }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        public float HowNearCamera
        {
            get { return howNear; }
            set {
                if (value < 1.5)
                    howNear = value;
            }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public float VisinaRampe { get; set; } = 0.1f;

        public float RreflBoja { get; set; } = 0.1f;
        public float GreflBoja { get; set; } = 0.1f;
        public float BreflBoja { get; set; } = 0.1f;

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            Console.WriteLine(sceneFileName);
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;

            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

       

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            gl.ClearColor(1,1,1, 1.0f);
     //       gl.Color(1f, 0f, 0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_CULL_FACE);

            SetupLighting(gl);
            SetupTextures(gl);



            m_scene.LoadScene();
            m_scene.Initialize();

        }

        private void SetupTextures(OpenGL gl)
        {
      
       //     gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_DECAL);

            // Ucitaj slike i kreiraj teksture
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
        
                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);		
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);    
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

                gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

                image.UnlockBits(imageData);
                image.Dispose();
            }
        }

        private void SetupLighting(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_LIGHTING | OpenGL.GL_LIGHT0);
            float[] global_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);
          
            float[] light0ambient = new float[] { 0.5f, 0.5f, 0.1f, 1.0f };
            float[] light0diffuse = new float[] { 1f, 1f, 0, 1.0f };
            float[] light0specular = new float[] { 1,1,0, 1.0f };

            float[] light1ambient = new float[] { RreflBoja, GreflBoja, BreflBoja, 1.0f };
            float[] light1diffuse = new float[] { 0f, 0f, 1f, 1.0f };
            float[] light1specular = new float[] { 0.9f, 0.9f, 0.9f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, new float[] { 3, 10, -1, 0 });
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
        //    gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, light1specular);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 40.0f);
       //     gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_EXPONENT, 5.0f);


            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHT1);

            #region testlight
            float[] light3pos = new float[] { 0.0f, 10.0f, -10.0f, 1.0f };
            float[] light3ambient = new float[] { 0.4f, 0.4f, 0.4f, 1.0f };
            float[] light3diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] light3specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT3, OpenGL.GL_POSITION, light3pos);
            gl.Light(OpenGL.GL_LIGHT3, OpenGL.GL_AMBIENT, light3ambient);
            gl.Light(OpenGL.GL_LIGHT3, OpenGL.GL_DIFFUSE, light3diffuse);
            gl.Light(OpenGL.GL_LIGHT3, OpenGL.GL_SPECULAR, light3specular);
            //      gl.Enable(OpenGL.GL_LIGHT3);


            #endregion

            // Definisemo belu spekularnu komponentu materijala sa jakim odsjajem
            //     gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, light0specular);
            //     gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, 128.0f);
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            gl.Enable(OpenGL.GL_NORMALIZE);
            gl.ShadeModel(OpenGL.GL_SMOOTH);


        }
        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>

        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.LoadIdentity();
            resetMaterial(gl);

           

            gl.LookAt(-(5 - howNear), 1 - howNear/5, 0, 0, 0, 0, 0, 10, 0);

            gl.Rotate(RotationX, RotationY, 0);

            //   gl.Translate(0.0f, 1, -4);
            //  lookAtCam.Project(gl);
            /*
            #region testSpehres
            gl.PushMatrix();
            {
                gl.Translate(2, 1, 0.3);
                gl.Scale(0.2, 0.2, 0.2);
                Sphere sp = new Sphere();
                sp.Slices = 200;
                sp.Stacks = 200;
                sp.Material = new SharpGL.SceneGraph.Assets.Material();
                sp.Material.Specular = Color.White;
                sp.Material.Bind(gl);
                sp.CreateInContext(gl);
                sp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            }
            gl.PopMatrix();

            gl.PushMatrix();
            {
                gl.Translate(2, 0.2, 0.3);
                gl.Scale(0.2, 0.2, 0.2);
                Sphere sp = new Sphere();
                sp.Slices = 200;
                sp.Stacks = 200;
                sp.Material = new SharpGL.SceneGraph.Assets.Material();
                sp.Material.Specular = Color.White;
                sp.Material.Bind(gl);
                sp.CreateInContext(gl);
                sp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            }
            gl.PopMatrix();

            gl.PushMatrix();
            {
                gl.Translate(1, 0.1, 0.2);
                gl.Scale(0.2, 0.2, 0.2);
                Sphere sp = new Sphere();
                sp.Slices = 200;
                sp.Stacks = 200;
                sp.Material = new SharpGL.SceneGraph.Assets.Material();
                sp.Material.Specular = Color.White;
                sp.Material.Bind(gl);
                sp.CreateInContext(gl);
                sp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            }
            gl.PopMatrix();


            gl.PushMatrix();
            {
                gl.Translate(2, 0, 0.6);
                gl.Scale(0.2, 0.2, 0.2);
                Sphere sp = new Sphere();
                sp.Slices = 200;
                sp.Stacks = 200;
                sp.Material = new SharpGL.SceneGraph.Assets.Material();
                sp.Material.Specular = Color.White;
                sp.Material.Bind(gl);
                sp.CreateInContext(gl);
                sp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            }
            gl.PopMatrix();

            gl.PushMatrix();
            {
                gl.Translate(2, 0, 2);
                gl.Scale(0.2, 0.2, 0.2);
                Sphere sp = new Sphere();
                sp.Slices = 200;
                sp.Stacks = 200;
                sp.Material = new SharpGL.SceneGraph.Assets.Material();
                sp.Material.Specular = Color.White;
                sp.Material.Bind(gl);
                sp.CreateInContext(gl);
                sp.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            }
            gl.PopMatrix();
            #endregion
            */
            gl.PushMatrix();
            {
                gl.Translate(1, 0.6, 1);
                gl.Rotate(CustomProp, 0, 0);
                drawPlavuSvetlost(gl);
            }
            gl.PopMatrix();

      

            gl.PushMatrix();
            {
                gl.Translate(0.0f, -1 + 1, -1 + 4);
                drawPut(gl, 0.4f, new List<float[]>() { new float[] { -3 , 0.001f, 0 }, new float[] { 2, 0.001f, 0 }, new float[] {2 ,0.001f,-3 } });
            }
            gl.PopMatrix();

            gl.PushMatrix();
            {
                gl.Translate(0.0f, 0, 0);
                drawPodlogu(gl, 4);
            }
            gl.PopMatrix();


            gl.PushMatrix();
            {
                //truckX += 0.04;
                gl.Translate(0, -1 + 1, -2 + 4);
                drawZidoveDeponije(gl,1,0.3);
            }
            gl.PopMatrix();

            gl.PushMatrix();
            {
                //truckX += 0.04;
                gl.Translate(1.7,0.01,1.5);
                drawMestoTovara(gl,1.2f);
            }
            gl.PopMatrix();
            
            gl.PushMatrix();
            {
                //truckX += 0.04;
                gl.Translate(1, -1 + 1, -1 + 4);
                gl.Rotate(0,90,0);          
                drawRampu(gl, 0.5, VisinaRampe, 0,0 + RampaPodignuta);

                if (StanjeAnimacije == AnimationState.STOPED) RampaPodignuta = 0;
                if (StanjeAnimacije == AnimationState.CEKA_RAMPU)
                {
                    RampaPodignuta += 0.01f;
                    if (RampaPodignuta > 0.5)
                        StanjeAnimacije = AnimationState.NASTAVLJA_POSLE_RAMPE;
                }
                if (StanjeAnimacije == AnimationState.NASTAVLJA_POSLE_RAMPE) {
                    if (RampaPodignuta > 0)
                        RampaPodignuta -= 0.01f;
                }

            }
            gl.PopMatrix();



            gl.PushMatrix();
            {
                gl.Translate(3, -0.99 + 1, -3 + 4);
                drawGradiliste(gl,1.5,0.5);
            }
            gl.PopMatrix();

            gl.PushMatrix();
            {
                //truckX += 0.04;
                gl.Translate(-1.5 + KamionX, -1 + 1, -1 + KamionZ + 4);
                gl.Rotate(0, 90 + (KaminOkrenut? 90 : 0), 0);
                drawVozilo(gl, 0.03 * ScaleVozilo, 0);

               
                if (StanjeAnimacije == AnimationState.POCETAK)
                {
                    KamionX += 0.02;
                    if (KamionX > 2)
                        StanjeAnimacije = AnimationState.CEKA_RAMPU;
                }
                if (StanjeAnimacije == AnimationState.NASTAVLJA_POSLE_RAMPE)
                {
                    if (KamionX < 3.4)
                        KamionX += 0.02;
                    else
                    {
                        if(KamionZ > -2)
                        {
                            KamionZ -= 0.02;
                            KaminOkrenut = true;
                        }else
                        {
                            KaminOkrenut = false;
                            StanjeAnimacije = AnimationState.RIKVERC_NA_ISTOVAR;                        
                        }
                    }
                  
                }
                if (StanjeAnimacije == AnimationState.RIKVERC_NA_ISTOVAR)
                {
                    if (KamionX > 3.0)
                        KamionX -= 0.02;
                    else
                        StanjeAnimacije = AnimationState.NAPRED_POSLE_ISTOVARA;
                }
                if (StanjeAnimacije == AnimationState.NAPRED_POSLE_ISTOVARA)
                {
                    if (KamionX < 3.4)
                        KamionX += 0.02;
                    else
                        StanjeAnimacije = AnimationState.NASTAVLJA_U_NEPOZNATO;
                }
                if (StanjeAnimacije == AnimationState.NASTAVLJA_U_NEPOZNATO)
                {
                    KaminOkrenut = true;
                    if (KamionZ > -3)
                        KamionZ -= 0.02;
                    else
                        StanjeAnimacije = AnimationState.STOPED;
                }
                if (StanjeAnimacije == AnimationState.STOPED) { KamionX = KamionZ = 0; KaminOkrenut = false; }

            }
            gl.PopMatrix();

            gl.PushMatrix();
            {
    //            drawText(gl);
            }
            gl.PopMatrix();
            // Oznaci kraj iscrtavanja
            gl.Flush();
        }


        private void drawPlavuSvetlost(OpenGL gl)
        {
            float[] light1ambient = new float[] { RreflBoja, GreflBoja, BreflBoja, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, light1ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, new float[] {0,0,0, 1 });
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, new float[] { 0, -1 , 0 });
        }


        private void drawGradiliste(OpenGL gl, double width, double height)
        {
            resetMaterial(gl);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Color(1.0, 1.0, 1.0, 1.0);

                 gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Beton]);

            gl.Begin(OpenGL.GL_QUADS);
        //    gl.Color(0.8, 0.8, 0.8);
            gl.TexCoord(0,0);
            gl.Vertex(-width/2, 0, -width/2);
            gl.TexCoord(0, width/height);
            gl.Vertex(-width/2, 0, width/2);
            gl.TexCoord(1, width / height);
            gl.Vertex(width/2, 0, width/2);
            gl.TexCoord(1,0);
            gl.Vertex(width/2, 0, -width/2);
            gl.End();


            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Cigla]);

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-width / 2, 0, -width / 2);
            gl.TexCoord(0.0f, width/height);
            gl.Vertex(-width / 2, height, -width / 2);
            gl.TexCoord(1, width / height);
            gl.Vertex(width / 2, height, -width / 2);
            gl.TexCoord(1, 0);
            gl.Vertex(width / 2, 0, -width / 2);
            gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-width / 2, 0, width / 2);
            gl.TexCoord(0.0f, width / height);
            gl.Vertex(-width / 2, height, width / 2);
            gl.TexCoord(1, width / height);
            gl.Vertex(width / 2, height, width / 2);
            gl.TexCoord(1, 0);
            gl.Vertex(width / 2, 0, width / 2);
            gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(width / 2, 0, -width / 2);
            gl.TexCoord(0.0f, width / height);
            gl.Vertex(width / 2, height, -width / 2);
            gl.TexCoord(1, width / height);
            gl.Vertex(width / 2, height, width / 2);
            gl.TexCoord(1, 0);
            gl.Vertex(width / 2, 0, width / 2);
            gl.End();

            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_CULL_FACE);

            gl.Color(1f, 1f, 1f, 1f);
        }

        private void drawText(OpenGL gl)
        {
            /*  gl.DrawText3D("Verdana", 14f, 1f, 0.1f, "");
              Console.WriteLine(m_width);
              gl.DrawText(60, 105, 0.0f, 1.0f, 0.0f, "Verdana", 14, "Predmet: Racunarska grafika");
              gl.DrawText(10, 90, 0.0f, 1.0f, 0.0f, "Verdana", 14, "Sk.god: 2016/17.");
              gl.DrawText(10, 75, 0.0f, 1.0f, 0.0f, "Verdana", 14, "Ime: Jan");
              gl.DrawText(10, 60, 0.0f, 1.0f, 0.0f, "Verdana", 14, "Prezime: Varga");
              gl.DrawText(10, 45, 0.0f, 1.0f, 0.0f, "Verdana", 14, "Sifra zad: 5.2");*/

            
            gl.Ortho2D(m_width - 180, m_width, 70, 10);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            int ulevo = 230;
            gl.DrawText3D("Verdana", 14f, 1f, 0.1f, "");
            gl.DrawText(m_width - ulevo, 70, 255, 0, 0, "Verdana", 14, "Predmet:Racunarska grafika");
            gl.DrawText(m_width - ulevo, 55, 255, 0, 0, "Verdana", 14, "Skolska god.:2016/17.");
            gl.DrawText(m_width - ulevo, 40, 255, 0, 0, "Verdana", 14, "Ime:Jan");
            gl.DrawText(m_width - ulevo, 25, 255, 0, 0, "Verdana", 14, "Prezime:Varga");
            gl.DrawText(m_width - ulevo, 10, 255, 0, 0, "Verdana", 14, "Sifra zadatka:5.2");

            gl.Perspective(50f, (double)m_width / m_height, 1.0f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Color(1f, 1f, 1f, 1f);
        }

        /// <summary>
        ///     Rampa se crta tako da trenutna pozicija je sredina rampe
        /// </summary>
        /// <param name="gl"></param>
        /// <param name="sirina"></param>
        /// <param name="visina"></param>
        /// <param name="rotateRampu">
        ///     u pocetnom trenutku rampa je paralelna sa pogledom
        /// </param>
        private void drawRampu(OpenGL gl, double sirina, double visina, int rotateRampu, float podignuta)
        {
            resetMaterial(gl);
            gl.Translate(-sirina/2,0,0);
            gl.Rotate(0,rotateRampu,0);

            gl.Color(0.3,0.3,0.3);
            //levi stubic
            gl.PushMatrix();
            {
     
                gl.Scale(0.02, visina, 0.02);          
                Cube cube = new Cube();
                cube.Material = new SharpGL.SceneGraph.Assets.Material();
                cube.Material.Ambient = Color.Blue;
                cube.Material.Diffuse = Color.White;
                cube.Material.Specular = Color.White;
                cube.Render(gl, RenderMode.Render);
            }
            gl.PopMatrix();

            //desni stubic
            gl.PushMatrix();
            {
                gl.Translate(sirina, 0, 0);
                gl.Scale(0.02, visina, 0.02);

                Cube cube = new Cube();
                cube.Render(gl, RenderMode.Render);
            }
            gl.PopMatrix();

            gl.PushMatrix();
            {
                gl.Translate(0, visina, 0);
                gl.Rotate(0, 90, 0);
                gl.Rotate(-90 * podignuta,1,0,0);
                Cylinder cil = new Cylinder();
           //     Axies ax = new Axies();
           //     ax.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Design);
                cil.BaseRadius = 0.01;
                cil.TopRadius = 0.01;
                cil.Height = sirina;
                cil.CreateInContext(gl);
                cil.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            }
            gl.PopMatrix();

            gl.Color(1f, 1f, 1f, 1f);
        }
            

        private void drawZidoveDeponije(OpenGL gl, double width, double hight)
        {
            resetMaterial(gl);
            gl.Color(0.5,0.5,0.5);
            //leva strana
            gl.PushMatrix();
            {
                gl.Translate(-width / 2,0,0);
                gl.Scale(0.05, hight, width/2);              
                Cube cube = new Cube();
                cube.Render(gl, RenderMode.Render);
            }
            gl.PopMatrix();

            //desna strana
            gl.PushMatrix();
            {
                gl.Translate(width / 2, 0, 0);
                gl.Scale(0.05, hight, width/2);

                Cube cube = new Cube();
                cube.Render(gl, RenderMode.Render);
            }
            gl.PopMatrix();

            //gore strana
            gl.PushMatrix();
            {
                gl.Rotate(0, 90, 0);
                gl.Translate(width / 2, 0, 0);
                gl.Scale(0.05, hight, width/2);
       

                Cube cube = new Cube();
                cube.Render(gl, RenderMode.Render);
            }
            gl.PopMatrix();

            gl.Color(1,1,1,1);
        }


        private void drawVozilo(OpenGL gl, double scale, int angleToMakeNormal)
        {
            resetMaterial(gl);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.PushMatrix();
            gl.Rotate(0,angleToMakeNormal,0);
            gl.Scale(scale, scale, scale);
            m_scene.Draw();
            gl.PopMatrix();
            gl.Color(1.0, 1.0, 1.0, 1.0);
            gl.Disable(OpenGL.GL_TEXTURE_2D);


            gl.Enable(OpenGL.GL_COLOR_MATERIAL); // jer assimp napravi metez
            gl.Color(1f, 1f, 1f, 1f);
        }

        private void drawPodlogu(OpenGL gl,double width)
        {
            //    gl.PushMatrix();

            //      gl.Material(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT, new float[] { 0.1f,0.1f,0.1f,1 });
            //      gl.Material(OpenGL.GL_FRONT, OpenGL.GL_DIFFUSE, new float[] { 0.4f, 0.4f, 0.4f, 1 });
            //      gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, new float[] { 0.9f, 0.9f, 0.9f, 1 });
            resetMaterial(gl);
            gl.Color(1.0, 1.0, 1.0, 1.0);
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Trava]);
            gl.Begin(OpenGL.GL_QUADS);
                gl.Normal(0.0f, 1f, 0.0f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-width, 0, -width);
            gl.TexCoord(0,width);
            gl.Vertex(-width, 0, width);
            gl.TexCoord(width, width);
            gl.Vertex(width, 0, width);
            gl.TexCoord(width, 0);
            gl.Vertex(width, 0, -width); 
            gl.End();
            //    gl.PopMatrix();

            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Color(1f, 1f, 1f, 1f);
        }

        private void drawMestoTovara(OpenGL gl, float sirina)
        {
            resetMaterial(gl);
            gl.Color(Color.Gray);
        /*    gl.Material(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT, new float[] { 0f, 0.1f, 0, 1 });
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_DIFFUSE, new float[] { 0f, 1, 0, 1 });
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, new float[] { 1, 1, 1, 1 });
*/

            var slices = 100;

            foreach(var row in Enumerable.Range(0,slices))
                foreach (var col in Enumerable.Range(0, slices))
                {
                    var x1 = -sirina  + sirina * col / slices;
                    var x2 = -sirina  + sirina * col / slices + sirina / slices;
                    var z1 = -sirina  + sirina * row / slices;
                    var z2 = -sirina  + sirina * row / slices + sirina / slices;
                    gl.Begin(OpenGL.GL_QUADS);
                    gl.Normal(0, 1, 0);
                    gl.Vertex(x1, 0,z1);
                    gl.Vertex(x1, 0, z2);
                    gl.Vertex(x2, 0, z2);
                    gl.Vertex(x2, 0, z1);
                    gl.End();
                }

       /*     gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0.0f, 1f, 0.0f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-1, 0, -1);
            gl.TexCoord(0.0f, 1f);
            gl.Vertex(-1, 0, 1);
            gl.TexCoord(1f, 1f);
            gl.Vertex(1, 0, 1);
            gl.TexCoord(1f, 0.0f);
            gl.Vertex(1, 0, -1);
            gl.End();*/
        }

        private void drawPut(OpenGL gl, float sirina, List<float[]> tackePuta)
        {
            resetMaterial(gl);
            gl.Color(0.5, 0.5, 0.5, 1.0);
            gl.Enable(OpenGL.GL_TEXTURE_2D);          
            //   gl.PushMatrix();
            for (int i = 0; i < tackePuta.Count - 1; i++)
            {
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Beton]);
                gl.Begin(OpenGL.GL_QUADS);           
                gl.Normal(0.0f, 1f, 0.0f);

                var tacka1 = tackePuta[i];
                var tacka2 = tackePuta[i+1];


                var v1 = new Vector(tacka1[0],tacka1[2]);
                var v2 = new Vector(tacka2[0], tacka2[2]);

                var duzina = (v2 - v1).Length;

                var direction = v2 - v1;
                direction.Normalize();
                var rotated1 = new Vector(-direction.Y,direction.X);
                var rotated2 = new Vector(direction.Y,-direction.X);
                rotated1 *= sirina / 2;
                rotated2 *= sirina / 2;
                var translated11 = v1 + rotated1;
                var translated12 = v1 + rotated2;
                var translated21 = v2 + rotated1;
                var translated22 = v2 + rotated2;


              //  gl.Color(0.5, 0.5, 0.5);

                gl.TexCoord(0.0f, 0.0f);
                gl.Vertex(translated21.X, tacka2[1], translated21.Y);
                gl.TexCoord(1.0f, 0.0f);
                gl.Vertex(translated22.X, tacka2[1], translated22.Y);
                gl.TexCoord(1.0f, duzina/sirina);
                gl.Vertex(translated12.X, tacka1[1], translated12.Y);
                gl.TexCoord(0.0f, duzina/sirina);
                gl.Vertex(translated11.X, tacka1[1], translated11.Y);

                gl.End();

            }
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            //     gl.PopMatrix();
            gl.Color(1f, 1f, 1f, 1f);
        }

        public void resetMaterial(OpenGL gl)
        {
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT, new float[] { 0.2f, 0.2f, 0.2f, 1 });
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_DIFFUSE, new float[] { 0.8f, 0.8f, 0.8f, 1 });
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, new float[] { 0f, 0.0f, 0.0f, 1 });
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_EMISSION, new float[] { 0f, 0.0f, 0.0f, 1 });
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, 0);
        }

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50f, (double)width / height, 1, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }



        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
