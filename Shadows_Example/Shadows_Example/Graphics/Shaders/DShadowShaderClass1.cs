﻿using DSharpDXRastertek.Tut41.System;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DSharpDXRastertek.Tut41.Graphics.Shaders
{
    public class DShadowShader                  // 356 lines
    {
        // Structs
        [StructLayout(LayoutKind.Sequential)]
        internal struct DMatrixBufferType
        {
            internal Matrix world;
            internal Matrix view;
            internal Matrix projection;
            internal Matrix lightView;
            internal Matrix lightProjection;
            internal Matrix lightView2;
            internal Matrix lightProjection2;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct DLightBufferType
        {
            internal Vector4 ambientColor;
            internal Vector4 diffuseColor;
            internal Vector4 diffuseColor2;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct DLightBufferType2
        {
            internal Vector3 lightPosition;
            internal float padding1;
            internal Vector3 lightPosition2;
            internal float padding2;
        }

        // Properties
        public VertexShader VertexShader { get; set; }
        public PixelShader PixelShader { get; set; }
        public InputLayout Layout { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantMatrixBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantLightBuffer { get; set; }
        public SharpDX.Direct3D11.Buffer ConstantLightBuffer2 { get; set; }
        public SamplerState SamplerStateWrap { get; set; }
        public SamplerState SamplerStateClamp { get; set; }

        // Constructor
        public DShadowShader() { }

        // Methods
        public bool Initialize(Device device, IntPtr windowsHandler)
        {
            // Initialize the vertex and pixel shaders.
            return InitializeShader(device, windowsHandler, "shadow.vs", "shadow.ps");
        }
        private bool InitializeShader(Device device, IntPtr windowsHandler, string vsFileName, string psFileName)
        {
            try
            {
                // Setup full pathes
                vsFileName = DSystemConfiguration.ShaderFilePath + vsFileName;
                psFileName = DSystemConfiguration.ShaderFilePath + psFileName;

                // Compile the Vertex Shader & Pixel Shader code.
                ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(vsFileName, "ShadowVertexShader", DSystemConfiguration.VertexShaderProfile, ShaderFlags.None, EffectFlags.None);
                ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(psFileName, "ShadowPixelShader", DSystemConfiguration.PixelShaderProfile, ShaderFlags.None, EffectFlags.None);

                // Create the Vertex & Pixel Shaders from the buffer.
                VertexShader = new VertexShader(device, vertexShaderByteCode);
                PixelShader = new PixelShader(device, pixelShaderByteCode);

                // Now setup the layout of the data that goes into the shader.
                // This setup needs to match the VertexType structure in the Model and in the shader.
                InputElement[] inputElements = new InputElement[] 
                {
                    new InputElement()
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "TEXCOORD",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "NORMAL",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    }
                };

                // Create the vertex input the layout. Kin dof like a Vertex Declaration.
                Layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), inputElements);

                // Release the vertex and pixel shader buffers, since they are no longer needed.
                vertexShaderByteCode.Dispose();
                pixelShaderByteCode.Dispose();

                // Create a wrap texture sampler state description.
                SamplerStateDescription samplerDesc = new SamplerStateDescription()
                {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MipLodBias = 0,
                    MaximumAnisotropy = 1,
                    ComparisonFunction = Comparison.Always,
                    BorderColor = new Color4(0, 0, 0, 0),  // Black Border.
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue
                };

                // Create the texture sampler state.
                SamplerStateWrap = new SamplerState(device, samplerDesc);

                // Create a clamp texture sampler state description.
                samplerDesc.AddressU = TextureAddressMode.Clamp;
                samplerDesc.AddressV = TextureAddressMode.Clamp;
                samplerDesc.AddressW = TextureAddressMode.Clamp;

                // Create the texture sampler state.
                SamplerStateClamp = new SamplerState(device, samplerDesc);

                // Setup the description of the dynamic matrix constant buffer that is in the vertex shader.
                BufferDescription matrixBufferDescription = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic, // ResourceUsage.Default
                    SizeInBytes = Utilities.SizeOf<DMatrixBufferType>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write, // CpuAccessFlags.None
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                // Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
                ConstantMatrixBuffer = new SharpDX.Direct3D11.Buffer(device, matrixBufferDescription);

                // Setup the description of the light dynamic constant buffer that is in the pixel shader.
				var lightBufferDesc = new BufferDescription()
				{
					Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DLightBufferType>(),
					BindFlags = BindFlags.ConstantBuffer,
					CpuAccessFlags = CpuAccessFlags.Write,
					OptionFlags = ResourceOptionFlags.None,
					StructureByteStride = 0
				};

				// Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
                ConstantLightBuffer = new SharpDX.Direct3D11.Buffer(device, lightBufferDesc);

                // Setup the description of the light dynamic constant bufffer that is in the pixel shader.
                // Note that ByteWidth alwalys needs to be a multiple of the 16 if using D3D11_BIND_CONSTANT_BUFFER or CreateBuffer will fail.
                BufferDescription light2BufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<DLightBufferType2>(), // Must be divisable by 16 bytes, so this is equated to 32.
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                // Create the constant buffer pointer so we can access the vertex shader constant buffer from within this class.
                ConstantLightBuffer2 = new SharpDX.Direct3D11.Buffer(device, light2BufferDesc);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing shader. Error is " + ex.Message);
                return false;
            }
        }
        public void ShutDown()
        {
            // Shutdown the vertex and pixel shaders as well as the related objects.
            ShuddownShader();
        }
        private void ShuddownShader()
        {
            // Release the Light constant buffer.
            ConstantLightBuffer?.Dispose();
            ConstantLightBuffer = null;
            // Release the Light2 constant buffer.
            ConstantLightBuffer2?.Dispose();
            ConstantLightBuffer2 = null;
            // Release the matrix constant buffer.
            ConstantMatrixBuffer?.Dispose();
            ConstantMatrixBuffer = null;
            // Release the wrap sampler state.
            SamplerStateWrap?.Dispose();
            SamplerStateWrap = null;
            // Release the clamp sampler state.
            SamplerStateClamp?.Dispose();
            SamplerStateClamp = null;
            // Release the layout.
            Layout?.Dispose();
            Layout = null;
            // Release the pixel shader.
            PixelShader?.Dispose();
            PixelShader = null;
            // Release the vertex shader.
            VertexShader?.Dispose();
            VertexShader = null;
        }
        public bool Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, Matrix lightViewMatrix, Matrix lightProjectionMatrix, ShaderResourceView texture, ShaderResourceView depthMapTexture, Vector3 lightPosition, Vector4 ambientColor, Vector4 diffuseColor, Matrix lightViewMatrix2, Matrix lightProjectionMatrix2, ShaderResourceView depthMapTexture2, Vector3 lightPosition2, Vector4 diffuseColor2)
        {
            // Set the shader parameters that it will use for rendering.
            if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix, lightViewMatrix, lightProjectionMatrix, texture, depthMapTexture, lightPosition, ambientColor, diffuseColor, lightViewMatrix2, lightProjectionMatrix2, depthMapTexture2, lightPosition2, diffuseColor2))
                return false;

            // Now render the prepared buffers with the shader.
            RenderShader(deviceContext, indexCount);

            return true;
        }
        
        // Modified in Tutorial 10 with Specular color and Specual power as well as CameraPosition.
        private bool SetShaderParameters(DeviceContext deviceContext, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, Matrix lightViewMatrix, Matrix lightProjectionMatrix, ShaderResourceView texture, ShaderResourceView depthMapTexture, Vector3 lightPosition, Vector4 ambientColor, Vector4 diffuseColor, Matrix lightViewMatrix2, Matrix lightProjectionMatrix2, ShaderResourceView depthMapTexture2, Vector3 lightPosition2, Vector4 diffuseColor2) 
        {
            try
            {
                DataStream mappedResource;

                #region Constant Matrix Buffer
                // Transpose the matrices to prepare them for shader.
                worldMatrix.Transpose();
                viewMatrix.Transpose();
                projectionMatrix.Transpose();
                lightViewMatrix.Transpose();
                lightProjectionMatrix.Transpose();
                lightViewMatrix2.Transpose();
                lightProjectionMatrix2.Transpose();
                
                // Lock the constant buffer so it can be written to.
                deviceContext.MapSubresource(ConstantMatrixBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);

                //// Copy the passed in matrices into the constant buffer.
                DMatrixBufferType matrixBuffer = new DMatrixBufferType()
                {
                    world = worldMatrix,
                    view = viewMatrix,
                    projection = projectionMatrix,
                    lightView = lightViewMatrix,
                    lightProjection = lightProjectionMatrix,
                    lightView2 = lightViewMatrix2,
                    lightProjection2 = lightProjectionMatrix2
                };
                mappedResource.Write(matrixBuffer);

                // Unlock the constant buffer.
                deviceContext.UnmapSubresource(ConstantMatrixBuffer, 0);

                // Set the position of the constant buffer in the vertex shader.
                int bufferPositionNumber = 0;

                // Finally set the constant buffer in the vertex shader with the updated values.
                deviceContext.VertexShader.SetConstantBuffer(bufferPositionNumber, ConstantMatrixBuffer);

                // Set shader texture resource in the pixel shader.
                deviceContext.PixelShader.SetShaderResources(0, texture, depthMapTexture, depthMapTexture2);
                #endregion

                #region Constant Light Buffer
                // Lock the light constant buffer so it can be written to.
                deviceContext.MapSubresource(ConstantLightBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);

                // Copy the lighting variables into the constant buffer.
                var lightBuffer = new DLightBufferType()
                {
                     ambientColor = ambientColor,
                     diffuseColor = diffuseColor,
                     diffuseColor2 = diffuseColor2
                };
                mappedResource.Write(lightBuffer);

                // Unlock the constant buffer.
                deviceContext.UnmapSubresource(ConstantLightBuffer, 0);

                // Set the position of the light constant buffer in the pixel shader.
                bufferPositionNumber = 0;

                // Finally set the light constant buffer in the pixel shader with the updated values.
                deviceContext.PixelShader.SetConstantBuffer(bufferPositionNumber, ConstantLightBuffer);
                #endregion

                #region Constant Light Buffer2
                // Lock the second light constant buffer so it can be written to.
                deviceContext.MapSubresource(ConstantLightBuffer2, MapMode.WriteDiscard, MapFlags.None, out mappedResource);

                // Copy the lighting variables into the constant buffer.
                DLightBufferType2 lightBuffer2 = new DLightBufferType2()
                {
                     lightPosition = lightPosition,
                     padding1 = 0.0f,
                     lightPosition2 = lightPosition2,
                     padding2 = 0.0f
                };
                mappedResource.Write(lightBuffer2);

                // Unlock the constant buffer.
                deviceContext.UnmapSubresource(ConstantLightBuffer2, 0);

                // Set the position of the light constant buffer in the vertex shader.
                bufferPositionNumber = 1;

                // Finally set the light constant buffer in the pixel shader with the updated values.
                deviceContext.VertexShader.SetConstantBuffer(bufferPositionNumber, ConstantLightBuffer2);
                #endregion

                return true;
            }
            catch
            { 
                return false;
            }
        }
        private void RenderShader(DeviceContext deviceContext, int indexCount)
        {
            // Set the vertex input layout.
            deviceContext.InputAssembler.InputLayout = Layout;

            // Set the vertex and pixel shaders that will be used to render this triangle.
            deviceContext.VertexShader.Set(VertexShader);
            deviceContext.PixelShader.Set(PixelShader);

            // Set the sampler states in the pixel shader.
            deviceContext.PixelShader.SetSamplers(0, SamplerStateClamp, SamplerStateWrap);

            // Render the triangle.
            deviceContext.DrawIndexed(indexCount, 0, 0);
        }
    }
}