﻿using DSharpDXRastertek.Tut40.Graphics;
using DSharpDXRastertek.Tut40.Input;
using SharpDX.Windows;
using System.Drawing;
using System.Windows.Forms;

namespace DSharpDXRastertek.Tut40.System
{
    public class DSystem                    // 172 lines
    {
        // Properties
        private RenderForm RenderForm { get; set; }
        public DSystemConfiguration Configuration { get; private set; }
        public DInput Input { get; private set; }
        public DGraphics Graphics { get; private set; }
        public DTimer Timer { get; private set; }
        public DPosition Position { get; private set; }

        // Constructor
        public DSystem() { }

        public static void StartRenderForm(string title, int width, int height, bool vSync, bool fullScreen = true, int testTimeSeconds = 0)
        {
            DSystem system = new DSystem();
            system.Initialize(title, width, height, vSync, fullScreen, testTimeSeconds);
            system.RunRenderForm();
        }

        // Methods
        public virtual bool Initialize(string title, int width, int height, bool vSync, bool fullScreen, int testTimeSeconds)
        {
            bool result = false;

            if (Configuration == null)
                Configuration = new DSystemConfiguration(title, width, height, fullScreen, vSync);

            // Initialize Window.
            InitializeWindows(title);

            if (Input == null)
            {
                Input = new DInput();
                if (!Input.Initialize(Configuration, RenderForm.Handle))
                    return false;
            }
            if (Graphics == null)
            {
                Graphics = new DGraphics();
                result = Graphics.Initialize(Configuration, RenderForm.Handle);
            }


            // Create and initialize Timer.
            Timer = new DTimer();
            if (!Timer.Initialize())
            {
                MessageBox.Show("Could not initialize Timer object", "Error", MessageBoxButtons.OK);
                return false;
            }

            // Create the position object.
            Position = new DPosition();

            // Set the initial position of the viewer to the same as the initial camera position.
            SharpDX.Vector3 camPos = Graphics.Camera.GetPosition();
            Position.SetPosition(camPos.X, camPos.Y, camPos.Z);

            return result;
        }
        private void InitializeWindows(string title)
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;

            // Initialize Window.
            RenderForm = new RenderForm(title)
            {
                ClientSize = new Size(Configuration.Width, Configuration.Height),
                FormBorderStyle = DSystemConfiguration.BorderStyle
            };

            // The form must be showing in order for the handle to be used in Input and Graphics objects.
            RenderForm.Show();
            RenderForm.Location = new Point((width / 2) - (Configuration.Width / 2), (height / 2) - (Configuration.Height / 2));
        }
        private void RunRenderForm()
        {
            RenderLoop.Run(RenderForm, () =>
            {
                if (!Frame())
                    ShutDown();
            });
        }
        public bool Frame()
        {
            // Read the user input.
            if (!Input.Frame() || Input.IsEscapePressed())
                return false;

            // Update the system stats.
            Timer.Frame2();
            

            // Do the frame input processing.
            if (!HandleInput(Timer.FrameTime))
                return false;

            // Get the view point position/rotation.
            // Do the frame processing for the graphics object.
            if (!Graphics.Frame(Position.PositionX, Position.PositionY, Position.PositionZ, Position.RotationX, Position.RotationY, Position.RotationZ))
                return false;

            return true;
        }
        private bool HandleInput(float frameTime)
        {
            // Set the frame time for calculating the updated position.
            Position.FrameTime = frameTime;

            // Handle the input
            bool keydown = Input.IsLeftArrowPressed();
            Position.TurnLeft(keydown);
            keydown = Input.IsRightArrowPressed();
            Position.TurnRight(keydown);
            keydown = Input.IsUpArrowPressed();
            Position.MoveForward(keydown);
            keydown = Input.IsDownArrowPressed();
            Position.MoveBackward(keydown);
            keydown = Input.IsPageUpPressed();
            Position.LookUp(keydown);
            keydown = Input.IsPageDownPressed();
            Position.LookDown(keydown);

            return true;
        }
        public void ShutDown()
        {
            ShutdownWindows();

            // Release the position object.
            Position = null;
            // Release the Timer object
            Timer = null;

            // Release graphics and related objects.
            Graphics?.Shutdown();
            Graphics = null;
            // Release DriectInput related object.
            Input?.Shutdown();
            Input = null;
            Configuration = null;
        }
        private void ShutdownWindows()
        {
            RenderForm?.Dispose();
            RenderForm = null;
        }
    }
}