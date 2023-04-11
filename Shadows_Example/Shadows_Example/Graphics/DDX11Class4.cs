using DSharpDXRastertek.Tut41.System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;

namespace DSharpDXRastertek.Tut41.Graphics
{
    public class DDX11                  // 276 lines
    {
        // Properties.
        private bool VerticalSyncEnabled { get; set; }
        public int VideoCardMemory { get; private set; }
        public string VideoCardDescription { get; private set; }
        private SwapChain SwapChain { get; set; }
        public SharpDX.Direct3D11.Device Device { get; private set; }
        public DeviceContext DeviceContext { get; private set; }
        private RenderTargetView RenderTargetView { get; set; }
        private Texture2D DepthStencilBuffer { get; set; }
        public DepthStencilView DepthStencilView { get; set; }
        private RasterizerState RasterState { get; set; }
        public Matrix ProjectionMatrix { get; private set; }
        public Matrix WorldMatrix { get; private set; }
        public ViewportF ViewPort { get; set; }

        // Constructor
        public DDX11() { }

        public bool Initialize(DSystemConfiguration configuration, IntPtr windowHandle)
        {
            try
            {
                #region Environment Configuration
                // Store the vsync setting.
                VerticalSyncEnabled = DSystemConfiguration.VerticalSyncEnabled;

                // Create a DirectX graphics interface factory.
                var factory = new Factory1();

                // Use the factory to create an adapter for the primary graphics interface (video card).
                var adapter = factory.GetAdapter1(0);

                // Get the primary adapter output (monitor).
                var monitor = adapter.GetOutput(0);

                // Get modes that fit the DXGI_FORMAT_R8G8B8A8_UNORM display format for the adapter output (monitor).
                var modes = monitor.GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);

                // Now go through all the display modes and find the one that matches the screen width and height.
                // When a match is found store the the refresh rate for that monitor, if vertical sync is enabled. 
                // Otherwise we use maximum refresh rate.
                var rational = new Rational(0, 1);
                if (VerticalSyncEnabled)
                {
                    foreach (var mode in modes)
                    {
                        if (mode.Width == configuration.Width && mode.Height == configuration.Height)
                        {
                            rational = new Rational(mode.RefreshRate.Numerator, mode.RefreshRate.Denominator);
                            break;
                        }
                    }
                }

                // Get the adapter (video card) description.
                var adapterDescription = adapter.Description;

                // Store the dedicated video card memory in megabytes.
                VideoCardMemory = adapterDescription.DedicatedVideoMemory >> 10 >> 10;

                // Convert the name of the video card to a character array and store it.
                VideoCardDescription = adapterDescription.Description.Trim('\0');

                // Release the adapter output.
                monitor.Dispose();
                // Release the adapter.
                adapter.Dispose();
                // Release the factory.
                factory.Dispose();
                #endregion

                #region Initialize swap chain and d3d device
                // Initialize the swap chain description.
                var swapChainDesc = new SwapChainDescription()
                {
                    // Set to a single back buffer.
                    BufferCount = 1,
                    // Set the width and height of the back buffer.
                    ModeDescription = new ModeDescription(configuration.Width, configuration.Height, rational, Format.R8G8B8A8_UNorm) { Scaling = DisplayModeScaling.Unspecified, ScanlineOrdering = DisplayModeScanlineOrder.Unspecified },
                    // Set the usage of the back buffer.
                    Usage = Usage.RenderTargetOutput,
                    // Set the handle for the window to render to.
                    OutputHandle = windowHandle,
                    // Turn multisampling off.
                    SampleDescription = new SampleDescription(1, 0),
                    // Set to full screen or windowed mode.
                    IsWindowed = !DSystemConfiguration.FullScreen,
                    // Don't set the advanced flags.
                    Flags = SwapChainFlags.None,
                    // Discard the back buffer content after presenting.
                    SwapEffect = SwapEffect.Discard
                };

                // Create the swap chain, Direct3D device, and Direct3D device context.
                SharpDX.Direct3D11.Device device;
                SwapChain swapChain;
                SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out device, out swapChain);

                Device = device;
                SwapChain = swapChain;
                DeviceContext = device.ImmediateContext;
                #endregion

                #region Initialize buffers
                // Get the pointer to the back buffer.
                var backBuffer = Texture2D.FromSwapChain<Texture2D>(SwapChain, 0);

                // Create the render target view with the back buffer pointer.
                RenderTargetView = new RenderTargetView(device, backBuffer);

                // Release pointer to the back buffer as we no longer need it.
                backBuffer.Dispose();

                // Initialize and set up the description of the depth buffer.
                var depthBufferDesc = new Texture2DDescription()
                {
                    Width = configuration.Width,
                    Height = configuration.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.D24_UNorm_S8_UInt,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };

                // Create the texture for the depth buffer using the filled out description.
                DepthStencilBuffer = new Texture2D(device, depthBufferDesc);
                #endregion

                #region Initialize Depth Enabled Stencil
                #endregion

                #region Initialize Output Merger
                // Initialize and set up the depth stencil view.
                var depthStencilViewDesc = new DepthStencilViewDescription()
                {
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0
                    }
                };

                // Create the depth stencil view.
                DepthStencilView = new DepthStencilView(Device, DepthStencilBuffer, depthStencilViewDesc);

                // Bind the render target view and depth stencil buffer to the output render pipeline.
                DeviceContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);
                #endregion

                #region Initialize Raster State
                // Setup the raster description which will determine how and what polygon will be drawn.
                var rasterDesc = new RasterizerStateDescription()
                {
                    IsAntialiasedLineEnabled = false,
                    CullMode = CullMode.Back,
                    DepthBias = 0,
                    DepthBiasClamp = 0.0f,
                    IsDepthClipEnabled = true,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = 0.0f
                };

                // Create the rasterizer state from the description we just filled out.
                RasterState = new RasterizerState(Device, rasterDesc);
                #endregion

                #region Initialize Rasterizer
                // Now set the rasterizer state.
                DeviceContext.Rasterizer.State = RasterState;

                // Setup and create the viewport for rendering.
                ViewPort = new ViewportF()
                {
                    Width = configuration.Width,
                    Height = configuration.Height,
                    MinDepth = 0.0f,
                    MaxDepth = 1.0f,
                    X = 0.0f,
                    Y = 0.0f
                };
                DeviceContext.Rasterizer.SetViewport(ViewPort);
                #endregion

                #region Initialize matrices
                // Setup and create the projection matrix.
                ProjectionMatrix = Matrix.PerspectiveFovLH((float)(Math.PI / 4), ((float)configuration.Width / configuration.Height), DSystemConfiguration.ScreenNear, DSystemConfiguration.ScreenDepth);

                // Initialize the world matrix to the identity matrix.
                WorldMatrix = Matrix.Identity;
                #endregion

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void ShutDown()
        {
            // Before shutting down set to windowed mode or when you release the swap chain it will throw an exception.
            SwapChain?.SetFullscreenState(false, null);

            // Dispose of all objects.
            RasterState?.Dispose();
            RasterState = null;
            DepthStencilView?.Dispose();
            DepthStencilView = null;
            DepthStencilBuffer?.Dispose();
            DepthStencilBuffer = null;
            RenderTargetView?.Dispose();
            RenderTargetView = null;
            DeviceContext?.Dispose();
            DeviceContext = null;
            Device?.Dispose();
            Device = null;
            SwapChain?.Dispose();
            SwapChain = null;
        }
        public void BeginScene(float red, float green, float blue, float alpha)
        {
            BeginScene(new Color4(red, green, blue, alpha));
        }
        public void BeginScene(Color4 color)
        {

            // Clear the back buffer.
            DeviceContext.ClearRenderTargetView(RenderTargetView, color);
            // Clear the depth buffer.
            DeviceContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }
        public void EndScene()
        {
            // Present the back buffer to the screen since rendering is complete.
            if (VerticalSyncEnabled)
            {
                // Lock to screen refresh rate.
                SwapChain.Present(1, PresentFlags.None);
            }
            else
            {
                // Present as fast as possible.
                SwapChain.Present(0, PresentFlags.None);
            }
        }
        public void SetBackBufferRenderTarget()
        {
            // Bind the render target view and depth stencil buffer to the output render pipeline.
            DeviceContext.OutputMerger.SetRenderTargets(DepthStencilView, RenderTargetView);
        }
        public void ResetViewPort()
        {
            // Set the viewport.
            DeviceContext.Rasterizer.SetViewport(ViewPort);
        }
    }
}