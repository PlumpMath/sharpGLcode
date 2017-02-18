using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;
using System.ComponentModel;

namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Atributi
        private float _scaleVozilo = 1;
        public float ScaleVozilo {
            get {
                return _scaleVozilo;
            }
            set {
                _scaleVozilo = value;
                m_world.ScaleVozilo = value;
                RaisePropertyChanged(nameof(ScaleVozilo));
            }
        }

        private float _visinaRampe = 0.1f;
        public float VisinaRampe
        {
            get
            {
                return _visinaRampe;
            }
            set
            {
                _visinaRampe = value;
                m_world.VisinaRampe = value;
                RaisePropertyChanged(nameof(VisinaRampe));
            }
        }

        public float R
        {
            get
            {
                return m_world.RreflBoja;
            }
            set
            {
                m_world.RreflBoja = value;
                RaisePropertyChanged(nameof(R));
            }
        }
        public float G
        {
            get
            {
                return m_world.GreflBoja;
            }
            set
            {
                m_world.GreflBoja = value;
                RaisePropertyChanged(nameof(G));
            }
        }
        public float B
        {
            get
            {
                return m_world.BreflBoja;
            }
            set
            {
                m_world.BreflBoja = value;
                RaisePropertyChanged(nameof(B));
            }
        }

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();
            DataContext = this;
            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Truck"), "Truck.obj", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int) ActualWidth, (int) ActualHeight);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (m_world.StanjeAnimacije != World.AnimationState.STOPED) return;
            switch (e.Key)
            {
                case Key.I: m_world.RotationX -= 5.0f; break;
                case Key.K: m_world.RotationX += 5.0f; break;
                case Key.J: m_world.RotationY -= 5.0f; break;
                case Key.L: m_world.RotationY += 5.0f; break;
                case Key.Add: m_world.HowNearCamera += 0.2f; break;
                case Key.Subtract: m_world.HowNearCamera -= 0.2f; break;
                case Key.P: m_world.StanjeAnimacije = World.AnimationState.POCETAK; break;
                case Key.Q: Environment.Exit(0); break;
                case Key.Z: m_world.CustomProp += 0.5f; break;
                case Key.X: m_world.CustomProp -= 0.5f; break;
            /*    case Key.F2:
                    OpenFileDialog opfModel = new OpenFileDialog();
                    bool result = (bool) opfModel.ShowDialog();
                    if (result)
                    {

                        try
                        {
                            World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                            m_world.Dispose();
                            m_world = newWorld;
                            m_world.Initialize(openGLControl.OpenGL);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK );
                        }
                    }
                    break;*/
            }
        }
    }
}
