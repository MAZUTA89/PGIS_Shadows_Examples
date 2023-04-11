using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace DSharpDXRastertek.Tut42.Graphics.Models
{
    public class DOrthoWindow                   // 171 lines
    {
        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct DOrthoVertex
        {
            public Vector3 position;
            public Vector2 texture;
        }

        // Properties.
        public SharpDX.Direct3D11.Buffer VertexBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer IndexBuffer { get; set; }
        public int VertexCount { get; set; }
        public int IndexCount { get; private set; }
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }
     
        // Constructor
        public DOrthoWindow() { }

        // Methods
        public bool Initialize(SharpDX.Direct3D11.Device device, int screeenWidth, int screenHeight)
        {
            // Store the screen size.
            ScreenWidth = screeenWidth;
            ScreenHeight = screenHeight;

            // Initialize the vertex and index buffer.
            if (!InitializeBuffers(device, ScreenWidth, ScreenHeight))
                return false;

            return true;
        }
        public void Shutdown()
        {
            // Release the vertex and index buffers.
            ShutdownBuffers();
        }
        private void ShutdownBuffers()
        {
            // Return the index buffer.
            IndexBuffer?.Dispose();
            IndexBuffer = null;
            // Release the vertex buffer.
            VertexBuffer?.Dispose();
            VertexBuffer = null;
        }
        public bool Render(DeviceContext deviceContext)
        {
            // Put the vertex and index buffers on the graphics pipeline to prepare for drawings.
            RenderBuffers(deviceContext);

            return true;
        }
        private bool InitializeBuffers(SharpDX.Direct3D11.Device device, int windowWidth, int windowHeight)
        {
            float left, right, top, bottom;

            try
            {
                // Calculate the screen coordinates of the left side of the window.
                left = (float)((windowWidth / 2) * -1);
                // Calculate the screen coordinates of the right side of the window.
                right = left + (float)windowWidth;
                // Calculate the screen coordinates of the top of the window.
                top = (float)(windowHeight / 2);
                // Calculate the screen coordinates of the bottom of the window.
                bottom = top - (float)windowHeight;

                // Set the number of the vertices and indices in the vertex and index array, accordingly.
                VertexCount = 6;
                IndexCount = 6;

                // Create and load the vertex array.
                var vertices = new DOrthoVertex[] 
			    {
                     // Top left.
				    new DOrthoVertex()
				    {
					    position = new Vector3(left, top, 0),
					    texture = new Vector2(0, 0)
				    },
                    // Bottom right.
				    new DOrthoVertex()
				    {
					    position = new Vector3(right, bottom, 0),
					    texture = new Vector2(1, 1)
				    },
                    // Bottom left.
				    new DOrthoVertex()
				    {
					    position = new Vector3(left, bottom, 0),
					    texture = new Vector2(0, 1)
				    },
                    // Top left.
				    new DOrthoVertex()
				    {
					    position = new Vector3(left, top, 0),
					    texture = new Vector2(0, 0)
				    },
                     // Top right.
				    new DOrthoVertex()
				    {
					    position = new Vector3(right, top, 0),
					    texture = new Vector2(1, 0)
				    },
                    // Bottom right.
				    new DOrthoVertex()
				    {
					    position = new Vector3(right, bottom, 0),
					    texture = new Vector2(1, 1)
				    }
			    };

                // Create the index array.
                var indices = new int[IndexCount];

                // Load the index array with data.
                for (var i = 0; i < IndexCount; i++)
                    indices[i] = i;

                // Set up the description of the static vertex buffer.
                var vertexBuffer = new BufferDescription()
                {
                    Usage = ResourceUsage.Default, // ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DOrthoVertex>() * VertexCount,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None, // CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                // Create the vertex buffer.
                VertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, vertices, vertexBuffer);

                // Create the index buffer.
                IndexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, indices);

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void RenderBuffers(DeviceContext deviceContext)
        {
            // Set vertex buffer stride and offset.
            var stride = Utilities.SizeOf<DOrthoVertex>();
            var offset = 0;

            // Set the vertex buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, stride, offset));
            
            // Set the index buffer to active in the input assembler so it can be rendered.
            deviceContext.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
            
            // Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }
    }
}