using DSharpDXRastertek.Tut42.Graphics.Camera;
using DSharpDXRastertek.Tut42.Graphics.Data;
using DSharpDXRastertek.Tut42.Graphics.Models;
using DSharpDXRastertek.Tut42.Graphics.Shaders;
using DSharpDXRastertek.Tut42.System;
using SharpDX;
using System;
using System.Windows.Forms;

namespace DSharpDXRastertek.Tut42.Graphics
{
    public class DGraphics                  // 676 lines
    {
        // Properties
        private DDX11 D3D { get; set; }
        public DCamera Camera { get; set; }

        #region Data
        private DLight Light { get; set; }
        private DRenderTexture RenderTexture { get; set; }
        private DRenderTexture BlackWhiteRenderTexture { get; set; }
        private DRenderTexture DownSampleTexure { get; set; }
        private DRenderTexture HorizontalBlurTexture { get; set; }
        private DRenderTexture VerticalBlurTexture { get; set; }
        private DRenderTexture UpSampleTexure { get; set; }
        #endregion

        #region Models
        private DModel CubeModel { get; set; }
        private DModel GroundModel { get; set; }
        private DModel SphereModel { get; set; }
        #endregion

        #region Shaders
        public DDepthShader DepthShader { get; set; }
        public DShadowShader ShadowShader { get; set; }
        public DOrthoWindow SmallWindow { get; set; }
        public DOrthoWindow FullScreenWindow { get; set; }
        public DTextureShader TextureShader { get; set; }
        public DHorizontalBlurShader HorizontalBlurShader { get; set; }
        public DVerticalBlurShader VerticalBlurShader { get; set; }
        public DSoftShadowShader SoftShadowShader { get; set; }
        #endregion     

        #region Variables
        private float lightPositionX;
        #endregion

        // Construtor
        public DGraphics() { }

        // Methods.
        public bool Initialize(DSystemConfiguration configuration, IntPtr windowHandle)
        {
            try
            {
                #region Initialize System
                // Create the Direct3D object.
                D3D = new DDX11();
                
                // Initialize the Direct3D object.
                if (!D3D.Initialize(configuration, windowHandle))
                    return false;
                #endregion

                #region Initialize Camera
                // Create the camera object
                Camera = new DCamera();

                // Set the initial position of the camera.
                Camera.SetPosition(0.0f, 0.0f, -10.0f);
                Camera.RenderBaseViewMatrix();
                #endregion

                #region Initialize Models
                // Create the cube model object.
                CubeModel = new DModel();

                // Initialize the cube model object.
                if (!CubeModel.Initialize(D3D.Device, "cube.txt", "wall01.bmp"))
                    return false;

                // Set the position for the cube model.
                CubeModel.SetPosition(-2.0f, 2.0f, 0.0f);

                // Create the sphere model object.
                SphereModel = new DModel();

                // Initialize the sphere model object.
                if (!SphereModel.Initialize(D3D.Device, "sphere.txt", "ice01.bmp"))
                    return false;

                // Set the position for the sphere model.
                SphereModel.SetPosition(2.0f, 2.0f, 0.0f);

                // Create the ground model object.
                GroundModel = new DModel();

                // Initialize the ground model object.
                if (!GroundModel.Initialize(D3D.Device, "plane01.txt", "metal001.bmp"))
                    return false;

                // Set the position for the ground model.
                GroundModel.SetPosition(0.0f, 1.0f, 0.0f);
                #endregion

                #region Data variables.
                // Create the light object.
                Light = new DLight();

                // Initialize the light object.
                Light.SetAmbientColor(0.15f, 0.15f, 0.15f, 1.0f);
                Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
                Light.SetLookAt(0.0f, 0.0f, 0.0f);
                Light.GenerateProjectionMatrix();

                // Create the render to texture object.
                RenderTexture = new DRenderTexture();

                // Initialize the render to texture object.
                if (!RenderTexture.Initialize(D3D.Device, 1024, 1024, DSystemConfiguration.ScreenDepth, DSystemConfiguration.ScreenNear))
                    return false;

                // Create the depth shader object.
                DepthShader = new DDepthShader();

                // Initialize the depth shader object.
                if (!DepthShader.Initialize(D3D.Device, windowHandle))
                    return false;

                // Create the black and white render to texture object.
                BlackWhiteRenderTexture = new DRenderTexture();

                // Initialize the black and white render to texture object.
                if (!BlackWhiteRenderTexture.Initialize(D3D.Device, 1024, 1024, DSystemConfiguration.ScreenDepth, DSystemConfiguration.ScreenNear))
                    return false;
                #endregion

                #region Initialize Shaders
                // Create the shadow shader object.
                ShadowShader = new DShadowShader();

                // Initialize the shadow shader object.
                if (!ShadowShader.Initialize(D3D.Device, windowHandle))
                    return false;

                // Set the size to sample down to.
                int downSampleWidth = 1024 / 2;
                int downSampleHeight = 1024 / 2;

                // Create the down sample render to texture object.
                DownSampleTexure = new DRenderTexture();

                // Initialize the down sample render to texture object.
                if (!DownSampleTexure.Initialize(D3D.Device, downSampleWidth, downSampleHeight, DSystemConfiguration.ScreenDepth, DSystemConfiguration.ScreenNear))
                    return false;

                // Create the small ortho window object.
                SmallWindow = new DOrthoWindow();

                // Initialize the small ortho window object.
                if (!SmallWindow.Initialize(D3D.Device, downSampleWidth, downSampleHeight))
                    return false;

                // Create the texture shader object.
                TextureShader = new DTextureShader();

                // Initialize the texture shader object.
                if (!TextureShader.Initialize(D3D.Device, windowHandle))
                    return false;

                // Create the horizontal blur render to texture object.
                HorizontalBlurTexture = new DRenderTexture();

                // Initialize the horizontal blur render to texture object.
                if (!HorizontalBlurTexture.Initialize(D3D.Device, downSampleWidth, downSampleHeight, DSystemConfiguration.ScreenDepth, 0.1f))
                    return false;

                // Create the horizontal blur shader object.
                HorizontalBlurShader = new DHorizontalBlurShader();

                // Initialize the horizontal blur shader object.
                if (!HorizontalBlurShader.Initialize(D3D.Device, windowHandle))
                    return false;

                // Create the vertical blur render to texture object.
                VerticalBlurTexture = new DRenderTexture();

                // Initialize the vertical blur render to texture object.
                if (!VerticalBlurTexture.Initialize(D3D.Device, downSampleWidth, downSampleHeight, DSystemConfiguration.ScreenDepth, 0.1f))
                    return false;

                // Create the vertical blur shader object.
                VerticalBlurShader = new DVerticalBlurShader();

                // Initialize the vertical blur shader object.
                if (!VerticalBlurShader.Initialize(D3D.Device, windowHandle))
                    return false;

                // Create the up sample render to texture object.
                UpSampleTexure = new DRenderTexture();

                // Initialize the up sample render to texture object.
                if (!UpSampleTexure.Initialize(D3D.Device, 1024, 1024, DSystemConfiguration.ScreenDepth, 0.1f))
                    return false;

                // Create the full screen ortho window object.
                FullScreenWindow = new DOrthoWindow();

                // Initialize the full screen ortho window object.
                if (!FullScreenWindow.Initialize(D3D.Device, 1024, 1024))
                    return false;

                // Create the soft shadow shader object.
                SoftShadowShader = new DSoftShadowShader();

                // Initialize the soft shadow shader object.
                if (!SoftShadowShader.Initialize(D3D.Device, windowHandle))
                    return false;
                #endregion
  
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not initialize Direct3D\nError is '" + ex.Message + "'");
                return false;
            }
        }
        public void Shutdown()
        {
            // Release the light object.
            Light = null;
            // Release the camera object.
            Camera = null;

            // Release the soft shadow shader object.
            SoftShadowShader?.ShutDown();
            SoftShadowShader = null;
            // Release the full screen ortho window object.
            FullScreenWindow?.Shutdown();
            FullScreenWindow = null;
            // Release the up sample render to texture object.
            UpSampleTexure?.Shutdown();
            UpSampleTexure = null;
            // Release the vertical blur shader object.
            VerticalBlurShader?.ShutDown();
            VerticalBlurShader = null;
            // Release the vertical blur render to texture object.
            VerticalBlurTexture?.Shutdown();
            VerticalBlurTexture = null;
            // Release the horizontal blur shader object.
            HorizontalBlurShader?.ShutDown();
            HorizontalBlurShader = null;
            // Release the horizontal blur render to texture object.
            HorizontalBlurTexture?.Shutdown();
            HorizontalBlurTexture = null;
            // Release the texture shader object.
            TextureShader?.ShutDown();
            TextureShader = null;
            // Release the small ortho window object.
            SmallWindow?.Shutdown();
            SmallWindow = null;
            // Release the down sample render to texture object.
            DownSampleTexure?.Shutdown();
            DownSampleTexure = null;
            // Release the shadow shader object.
            ShadowShader?.ShutDown();
            ShadowShader = null;
            // Release the black and white render to texture.
            BlackWhiteRenderTexture?.Shutdown();
            BlackWhiteRenderTexture = null;
            // Release the depth shader object.
            DepthShader?.ShutDown();
            DepthShader = null;
            /// Release the render to texture object.
            RenderTexture?.Shutdown();
            RenderTexture = null;
            // Release the ground model object.
            GroundModel?.Shutdown();
            GroundModel = null;
            // Release the sphere model object.
            SphereModel?.Shutdown();
            SphereModel = null;
            // Release the cube model object.
            CubeModel?.Shutdown();
            CubeModel = null;
            // Release the Direct3D object.
            D3D?.ShutDown();
            D3D = null;
        }
        public bool Frame(float positionX, float positionY, float positionZ, float rotationX, float rotationY, float rotationZ)
        {
            // Set the position of the camera.
            Camera.SetPosition(positionX, positionY, positionZ);
            Camera.SetRotation(rotationX, rotationY, rotationZ);

            // Cycle the position of the light each frame and reset to -5.0f when X is grwater then 5.0f.
            lightPositionX += 0.0005f;
            if (lightPositionX > 5.0f)
                lightPositionX = -5.0f;

            // Update the position of the light.
            Light.Position = new Vector3(lightPositionX, 8.0f, -5.0f);

            if (!Render())
                return false;

            return true;
        }
        public bool Render()
        {
            // First render the scene to a texture.
            if (!RenderSceneToTexture())
                return false;
            
            // Next render the shadowed scene in black and white.
            if (!RenderBlackAndWhiteShadows())
                return false;
            
            // Then down sample the black and white scene texture.
            if (!DownSampleTexture())
                return false;
            
            // Perform a horizontal blur on the down sampled texture.
            if (!RenderHorizontalBlurToTexture())
                return false;
            
            // Now perform a vertical blur on the texture.
            if (!RenderVerticalBlurToTexture())
                return false;
            
            // Finally up sample the final blurred render to texture that can now be used in the soft shadow shader.
            if (!UpSampleTexture())
                return false;

            // Clear the buffers to begin the scene.
            D3D.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the view matrix based on the camera's position.
            Camera.Render();

            // Get the world, view, and projection matrices from the camera and d3d objects.
            Matrix viewMatrix = Camera.ViewMatrix;
            Matrix worldMatrix = D3D.WorldMatrix;
            Matrix projectionMatrix = D3D.ProjectionMatrix;

            // Setup the translation matrix for the cube model.
            Vector3 cubePosition = CubeModel.GetPosition();
            Matrix.Translation(cubePosition.X, cubePosition.Y, cubePosition.Z, out worldMatrix);

            // Render the cube model using the soft shadow shader.
            CubeModel.Render(D3D.DeviceContext);
            if (!SoftShadowShader.Render(D3D.DeviceContext, CubeModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, CubeModel.Texture.TextureResource, UpSampleTexure.ShaderResourceView, Light.Position, Light.AmbientColor, Light.DiffuseColour))
                return false;

            // Reset the world matrix.
            worldMatrix = D3D.WorldMatrix;

            // Setup the translation matrix for the sphere model.
            Vector3 spherePosition = SphereModel.GetPosition();
            Matrix.Translation(spherePosition.X, spherePosition.Y, spherePosition.Z, out worldMatrix);

            // Render the sphere model using the soft shadow shader.
            SphereModel.Render(D3D.DeviceContext);
            if (!SoftShadowShader.Render(D3D.DeviceContext, SphereModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, SphereModel.Texture.TextureResource, UpSampleTexure.ShaderResourceView, Light.Position, Light.AmbientColor, Light.DiffuseColour))
                return false;

            // Reset the world matrix.
            worldMatrix = D3D.WorldMatrix;

            // Setup the translation matrix for the ground model.
            Vector3 groundPosition = GroundModel.GetPosition();
            Matrix.Translation(groundPosition.X, groundPosition.Y, groundPosition.Z, out worldMatrix);

            // Render the ground model using the shadow shader.
            GroundModel.Render(D3D.DeviceContext);
            if (!SoftShadowShader.Render(D3D.DeviceContext, GroundModel.IndexCount, worldMatrix, viewMatrix, projectionMatrix, GroundModel.Texture.TextureResource, UpSampleTexure.ShaderResourceView, Light.Position, Light.AmbientColor, Light.DiffuseColour))
                return false;

            // Present the rendered scene to the screen.
            D3D.EndScene();

            return true;
        }
        private bool UpSampleTexture()
        {
            // Set the render target to be the render to texture.
            UpSampleTexure.SetRenderTarget(D3D.DeviceContext);

            // Clear the render to texture.
            UpSampleTexure.ClearRenderTarget(D3D.DeviceContext, 0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the view matrix based on the camera's position.
            Camera.Render();
            
            // Get the world and view matrices from the camera and d3d objects.
            Matrix cameraViewMatrix = Camera.BaseViewMatrix;
            Matrix worldMatrix = D3D.WorldMatrix;

            // Get the ortho matrix from the render to texture since texture has different dimensions.
            Matrix orthoMatrix = UpSampleTexure.OrthoMatrix;

            // Turn off the Z buffer to begin all 2D rendering.
            D3D.TurnZBufferOff();

            // Put the full screen ortho window vertex and index buffers on the graphics pipeline to prepare them for drawing.
            if (!FullScreenWindow.Render(D3D.DeviceContext))
                return false;

            // Render the full screen ortho window using the texture shader and the small sized final blurred render to texture resource.
            if (!TextureShader.Render(D3D.DeviceContext, FullScreenWindow.IndexCount, worldMatrix, cameraViewMatrix, orthoMatrix,VerticalBlurTexture.ShaderResourceView))
                return false;

            // Turn the Z buffer back on now that all 2D rendering has completed.
            D3D.TurnZBufferOn();

            // Reset the render target back to the original back buffer and not the render to texture anymore.
            D3D.SetBackBufferRenderTarget();

            // Reset the viewport back to the original.
            D3D.ResetViewPort();

            return true;
        }
        private bool RenderVerticalBlurToTexture()
        {
            // Store the screen height in a float that will be used in the vertical blur shader.
            float screenSizeY = 1024.0f / 2;

            // Set the render target to be the render to texture.
            VerticalBlurTexture.SetRenderTarget(D3D.DeviceContext);

            // Clear the render to texture.
            VerticalBlurTexture.ClearRenderTarget(D3D.DeviceContext, 0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the view matrix based on the camera's position.
            Camera.Render();

            // Get the world and view matrices from the camera and d3d objects.
            Matrix cameraViewMatrix = Camera.BaseViewMatrix;
            Matrix worldMatrix = D3D.WorldMatrix;

            // Get the ortho matrix from the render to texture since texture has different dimensions.
            Matrix orthoMatrix = VerticalBlurTexture.OrthoMatrix;

            // Turn off the Z buffer to begin all 2D rendering.
            D3D.TurnZBufferOff();

            // Put the small ortho window vertex and index buffers on the graphics pipeline to prepare them for drawing.
            if (!SmallWindow.Render(D3D.DeviceContext))
                return false;

            // Render the small ortho window using the vertical blur shader and the horizontal blurred render to texture resource.
            if (!VerticalBlurShader.Render(D3D.DeviceContext, SmallWindow.IndexCount, worldMatrix, cameraViewMatrix, orthoMatrix, HorizontalBlurTexture.ShaderResourceView, screenSizeY))
                return false;

            // Turn the Z buffer back on now that all 2D rendering has completed.
            D3D.TurnZBufferOn();

            // Reset the render target back to the original back buffer and not the render to texture anymore.
            D3D.SetBackBufferRenderTarget();

            // Reset the viewport back to the original.
            D3D.ResetViewPort();

            return true;
        }
        private bool RenderHorizontalBlurToTexture()
        {
            // Store the screen width in a float that will be used in the horizontal blur shader.
            float screenSizeX = 1024.0f / 2;

            // Set the render target to be the render to texture.
            HorizontalBlurTexture.SetRenderTarget(D3D.DeviceContext);

            // Clear the render to texture.
            HorizontalBlurTexture.ClearRenderTarget(D3D.DeviceContext,  0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the view matrix based on the camera's position.
            Camera.Render();

            // Get the world and view matrices from the camera and d3d objects.
            Matrix cameraViewMatrix = Camera.BaseViewMatrix;
            Matrix worldMatrix = D3D.WorldMatrix;

            // Get the ortho matrix from the render to texture since texture has different dimensions.
            Matrix orthoMatrix = HorizontalBlurTexture.OrthoMatrix;

            // Turn off the Z buffer to begin all 2D rendering.
            D3D.TurnZBufferOff();

            // Put the small ortho window vertex and index buffers on the graphics pipeline to prepare them for drawing.
            if (!SmallWindow.Render(D3D.DeviceContext))
                return false;

            // Render the small ortho window using the horizontal blur shader and the down sampled render to texture resource.
            if (!HorizontalBlurShader.Render(D3D.DeviceContext, SmallWindow.IndexCount, worldMatrix, cameraViewMatrix, orthoMatrix, DownSampleTexure.ShaderResourceView, screenSizeX))
                return false;

            // Turn the Z buffer back on now that all 2D rendering has completed.
            D3D.TurnZBufferOn();

            // Reset the render target back to the original back buffer and not the render to texture anymore.
            D3D.SetBackBufferRenderTarget();

            // Reset the viewport back to the original.
            D3D.ResetViewPort();

            return true;
        }
        private bool DownSampleTexture()
        {
            // Set the render target to be the render to texture.
            DownSampleTexure.SetRenderTarget(D3D.DeviceContext);

            // Clear the render to texture.
            DownSampleTexure.ClearRenderTarget(D3D.DeviceContext,  0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the view matrix based on the camera's position.
            Camera.Render();

            // Get the world and view matrices from the camera and d3d objects.
            Matrix cameraViewMatrix = Camera.BaseViewMatrix;
            Matrix worldMatrix = D3D.WorldMatrix;

            // Get the ortho matrix from the render to texture since texture has different dimensions being that it is smaller.
            Matrix orthoMatrix = DownSampleTexure.OrthoMatrix;

            // Turn off the Z buffer to begin all 2D rendering.
            D3D.TurnZBufferOff();

            // Put the small ortho window vertex and index buffers on the graphics pipeline to prepare them for drawing.
            if (!SmallWindow.Render(D3D.DeviceContext))
                return false;

            // Render the small ortho window using the texture shader and the render to texture of the scene as the texture resource.
            if (!TextureShader.Render(D3D.DeviceContext, SmallWindow.IndexCount, worldMatrix, cameraViewMatrix, orthoMatrix, BlackWhiteRenderTexture.ShaderResourceView))
                return false;

            // Turn the Z buffer back on now that all 2D rendering has completed.
            D3D.TurnZBufferOn();

            // Reset the render target back to the original back buffer and not the render to texture anymore.
            D3D.SetBackBufferRenderTarget();

            // Reset the viewport back to the original.
            D3D.ResetViewPort();

            return true;
        }
        private bool RenderBlackAndWhiteShadows()
        {
            // Set the render target to be the render to texture.
            BlackWhiteRenderTexture.SetRenderTarget(D3D.DeviceContext);

            // Clear the render to texture.
            BlackWhiteRenderTexture.ClearRenderTarget(D3D.DeviceContext, 0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the view matrix based on the camera's position.
            Camera.Render();

            // Generate the light view matrix based on the light's position.
            Light.GenerateViewMatrix();

            // Get the world, view, and projection matrices from the camera and d3d objects.
            Matrix cameraViewMatrix = Camera.ViewMatrix;
            Matrix worldMatrix = D3D.WorldMatrix;
            Matrix projectionMatrix = D3D.ProjectionMatrix;

            // Get the light's view and projection matrices from the light object.
            Matrix lightViewMatrix = Light.ViewMatrix;
            Matrix lightProjectionMatrix = Light.ProjectionMatrix;

            // Setup the translation matrix for the cube model.
            Vector3 cubePosition = CubeModel.GetPosition();
            Matrix.Translation(cubePosition.X, cubePosition.Y, cubePosition.Z, out worldMatrix);

            // Render the cube model using the shadow shader.
            CubeModel.Render(D3D.DeviceContext);
            if (!ShadowShader.Render(D3D.DeviceContext, CubeModel.IndexCount, worldMatrix, cameraViewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix, RenderTexture.ShaderResourceView, Light.Position))
                return false;

            // Reset the world matrix.
            worldMatrix = D3D.WorldMatrix;

            // Setup the translation matrix for the sphere model.
            Vector3 spherePosition = SphereModel.GetPosition();
            Matrix.Translation(spherePosition.X, spherePosition.Y, spherePosition.Z, out worldMatrix);

            // Render the sphere model using the shadow shader.
            SphereModel.Render(D3D.DeviceContext);
            if (!ShadowShader.Render(D3D.DeviceContext, SphereModel.IndexCount, worldMatrix, cameraViewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix, RenderTexture.ShaderResourceView, Light.Position))
                return false;

            // Reset the world matrix.
            worldMatrix = D3D.WorldMatrix;

            // Setup the translation matrix for the ground model.
            Vector3 groundPosition = GroundModel.GetPosition();
            Matrix.Translation(groundPosition.X, groundPosition.Y, groundPosition.Z, out worldMatrix);

            // Render the ground model using the shadow shader.
            GroundModel.Render(D3D.DeviceContext);
            if (!ShadowShader.Render(D3D.DeviceContext, SphereModel.IndexCount, worldMatrix, cameraViewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix, RenderTexture.ShaderResourceView, Light.Position))
                return false;

            // Reset the render target back to the original back buffer and not the render to texture anymore.
            D3D.SetBackBufferRenderTarget();

            // Reset the viewport back to the original.
            D3D.ResetViewPort();

            return true;
        }
        private bool RenderSceneToTexture()
        {
            // Set the render target to be the render to texture.
            RenderTexture.SetRenderTarget(D3D.DeviceContext);

            // Clear the render to texture.
            RenderTexture.ClearRenderTarget(D3D.DeviceContext, 0.0f, 0.0f, 0.0f, 1.0f);

            // Generate the light view matrix based on the light's position.
            Light.GenerateViewMatrix();

            // Get the world matrix from the d3d object.
            Matrix worldMareix = D3D.WorldMatrix;

            // Get the view and orthographic matrices from the light object.
            Matrix lightViewMatrix = Light.ViewMatrix;
            Matrix lightOrthoMatrix = Light.ProjectionMatrix;

            // Setup the translation matrix for the cube model.
            Vector3 cubePosition = CubeModel.GetPosition();
            Matrix.Translation(cubePosition.X, cubePosition.Y, cubePosition.Z, out worldMareix);

            // Render the cube model with the depth shader.
            CubeModel.Render(D3D.DeviceContext);
            if (!DepthShader.Render(D3D.DeviceContext, CubeModel.IndexCount, worldMareix, lightViewMatrix, lightOrthoMatrix))
                return false;

            // Reset the world matrix.
            worldMareix = D3D.WorldMatrix;

            // Setup the translation matrix for the sphere model.
            Vector3 spherePosition = SphereModel.GetPosition();
            Matrix.Translation(spherePosition.X, spherePosition.Y, spherePosition.Z, out worldMareix);

            // Render the sphere model with the depth shader.
            SphereModel.Render(D3D.DeviceContext);
            if (!DepthShader.Render(D3D.DeviceContext, SphereModel.IndexCount, worldMareix, lightViewMatrix, lightOrthoMatrix))
                return false;

            // Reset the world matrix.
            worldMareix = D3D.WorldMatrix;

            // Setup the translation matrix for the ground model.
            Vector3 groundPosition = GroundModel.GetPosition();
            Matrix.Translation(groundPosition.X, groundPosition.Y, groundPosition.Z, out worldMareix);

            // Render the ground model with the depth shader.
            GroundModel.Render(D3D.DeviceContext);
            if (!DepthShader.Render(D3D.DeviceContext, GroundModel.IndexCount, worldMareix, lightViewMatrix, lightOrthoMatrix))
                return false;

            // Reset the render target back to the original back buffer and not the render to texture anymore.
            D3D.SetBackBufferRenderTarget();

            // Reset the viewport back to the original.
            D3D.ResetViewPort();

            return true;
        }
    }
}