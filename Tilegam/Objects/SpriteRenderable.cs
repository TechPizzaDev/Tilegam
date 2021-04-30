using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Tilegam.Client;
using Veldrid;

namespace Tilegam.Objects
{
    public class SpriteRenderable : Renderable, IUpdateable
    {
        private GeometryBatch<VertexPositionTexture<RgbaByte>> batch;
        private ImageSharpTexture image0;

        private Pipeline batchPipeline;
        private ResourceSet matrixSet;
        private ResourceSet texSet0;

        private DeviceBuffer matrixBuffer;
        private Texture tex0;
        private Sampler sampler0;

        private bool needsbuild;

        private Vector2 kek;

        public override RenderPasses RenderPasses => RenderPasses.AlphaBlend;

        public SpriteRenderable()
        {
            uint quadCap = 1024 * 16;
            batch = new GeometryBatch<VertexPositionTexture<RgbaByte>>(6 * quadCap, 4 * quadCap);
            image0 = new ImageSharpTexture("Assets/Textures/DurrrSpaceShip.png", false);
        }

        public override void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            batch.CreateDeviceObjects(gd, cl, sc);

            tex0 = image0.CreateDeviceTexture(gd, gd.ResourceFactory);

            sampler0 = gd.PointSampler;

            SetupBatchPipeline(
                gd,
                gd.ResourceFactory,
                sc,
                sc.MainSceneFramebuffer.OutputDescription);

            needsbuild = true;
        }

        public override void DestroyDeviceObjects()
        {
            batch.DestroyDeviceObjects();
        }

        public override RenderOrderKey GetRenderOrderKey(Vector3 cameraPosition)
        {
            return new RenderOrderKey();
        }

        public override void Render(GraphicsDevice gd, CommandList cl, SceneContext sc, RenderPasses renderPass)
        {
            float width = sc.MainSceneFramebuffer.Width;
            float height = sc.MainSceneFramebuffer.Height;

            Matrices matrices;
            matrices.Projection = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
            matrices.View = Matrix4x4.CreateLookAt(new Vector3(0, 0, 0), new Vector3(0, 0, 0), Vector3.UnitY);
            matrices.World = Matrix4x4.Identity;
            gd.UpdateBuffer(matrixBuffer, 0, ref matrices);

            cl.SetPipeline(batchPipeline);
            cl.SetFramebuffer(sc.MainSceneFramebuffer);
            cl.SetFullViewport(0);
            cl.SetGraphicsResourceSet(0, matrixSet);
            cl.SetGraphicsResourceSet(1, texSet0);

            batch.Submit(cl);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public unsafe void Update(in FrameTime time)
        {
            float delta = time.DeltaSeconds;

            //if (!needsbuild)
            //{
            //    return;
            //}
            //needsbuild = false;

            batch.Begin();
            //
            //for (int y = 0; y < 256; y++)
            //{
            //    for (int x = 0; x < 256; x++)
            //    {
            //        var res = batch.ReserveQuadsUnsafe(1);
            //
            //        RgbaByte color = new RgbaByte((byte)x, (byte)((x + y) / 2), (byte)y, 255);
            //        SetQuad(res.Vertices, x + 100, y + 100, 0, 1, 1, 0, 0, 0, 0, color, color, color, color);
            //    }
            //}
            //
            //batch.End();
            
            //Span<VertexPositionColorTexture> vertices = stackalloc VertexPositionColorTexture[]
            //{
            //    new VertexPositionColorTexture
            //    {
            //        Position = new Vector3(-0.5f, -0.5f, 0)
            //    },
            //    new VertexPositionColorTexture
            //    {
            //        Position = new Vector3(-0.5f, 0.5f, 0)
            //    },
            //    new VertexPositionColorTexture
            //    {
            //        Position = new Vector3(0.5f, 0.5f, 0)
            //    },
            //    new VertexPositionColorTexture
            //    {
            //        Position = new Vector3(0.5f, -0.5f, 0)
            //    },
            //};
            //
            //for (int i = 0; i < vertices.Length; i++)
            //{
            //    vertices[i].Position *= 0.01f;
            //}

            //for (int j = 0; j < 1024 * 256; j++)
            //{
            //    float t = time.TotalSeconds + j * MathF.PI / 1024;
            //    //float s;
            //    //if(j < 2)
            //    //    s = (MathF.Cos(t) + 1) * 0.02f;
            //    //else
            //    //    s = (MathF.Sin(t) + 1) * 0.02f;
            //
            //    float x = MathF.Cos(t) * 0.01f;
            //    float y = MathF.Sin(t) * 0.01f;
            //
            //    vertices[0].Position.X = -x; // s; // * MathF.Cos(t * 1);
            //    vertices[0].Position.Y = -y; // s; // * MathF.Sin(t * 1);
            //                                 //  
            //    vertices[1].Position.X = -x; // s; // * MathF.Cos(t * MathF.PI * 1.3f);
            //    vertices[1].Position.Y = y; // s; // * MathF.Sin(t * MathF.PI * 1.3f);
            //                                // 
            //    vertices[2].Position.X = x; // s; // * MathF.Cos(t * MathF.PI * 2.6f);
            //    vertices[2].Position.Y = y; // s; // * MathF.Sin(t * MathF.PI * 2.6f);
            //                                //  
            //    vertices[3].Position.X = x; // s; // * MathF.Cos(t * MathF.PI * 3.9f);
            //    vertices[3].Position.Y = -y;// s; // * MathF.Sin(t * MathF.PI * 3.9f);
            //                             
            //    vertices[0].Color = new RgbaByte(255, 0, 0, 255);
            //    vertices[1].Color = new RgbaByte(0, 127, 0, 255);
            //    vertices[2].Color = new RgbaByte(0, 127, 0, 255);
            //    vertices[3].Color = new RgbaByte(0, 0, 255, 255);
            //    //for (int i = 0; i < vertices.Length; i++)
            //    //{
            //    //    vertices[i].Position.Y -= 0.5f;
            //    //}
            //    batch.AppendQuad(vertices[0], vertices[1], vertices[2], vertices[3]);
            //
            //    //vertices[0].Color = new RgbaByte(0, 0, 255, 255);
            //    ////for (int i = 0; i < vertices.Length; i++)
            //    ////{
            //    ////    vertices[i].Position.Y += 1f;
            //    ////}
            //    //batch.AppendQuad(vertices[0], vertices[1], vertices[2], vertices[3]);
            //}

            Vector2 mousePos = InputTracker.MousePosition;
            kek = Vector2.Lerp(kek, mousePos, delta * 15);

            float imgW = 1; // (ushort)image0.Width;
            float imgH = 1; // (ushort)image0.Height;
            float tt = time.TotalSeconds;

            int siz = 20;
            const int count = 1;
            for (int yy = 0; yy < 1080 / siz; yy++)
            {
                for (int xx = 0; xx < 1920 / siz; xx++)
                {
                    //float t = time.TotalSeconds + j * MathF.PI / 1024;

                    //float x = MathF.Cos(t) * 20f + 200;
                    //float y = MathF.Sin(t) * 20f + 200;
                    //float x = xx * 4 + 100;
                    //float y = yy * 4 + 100;
                    //float x = 480;
                    //float y = 330;
                    float x = 0;
                    float y = 0;
                    float w = 24;
                    float h = 24;

                    byte r = (byte)((MathF.Cos(tt) + 1) * 127.5f);
                    byte g = (byte)((MathF.Sin(tt) + 1) * 127.5f);

                    var reserve = batch.ReserveQuadsUnsafe(count);

                    for (int i = 0; i < count; i++)
                    {
                        VertexPositionTexture<RgbaByte>* ptr = reserve.Vertices + i * 4;

                        //SetQuad(ptr,
                        //    x, y, 0,
                        //    w, h,
                        //    0, 0,
                        //    imgW, imgH,
                        //    new RgbaByte(0, g, 0, 255),
                        //    new RgbaByte(0, g, 0, 255),
                        //    new RgbaByte(r, 0, 0, 255),
                        //    new RgbaByte(0, 0, 255, 255));

                        float qx = xx * siz;
                        float qy = yy * siz;
                        float angle = MathF.Atan2(qy - kek.Y, qx - kek.X) - MathF.PI / 2;

                        //float sin = MathF.Sin(angle);//(tt + i - 4.5f) * 3) * 1.5f;
                        //float cos = MathF.Cos(angle);//(tt + i - 4.5f) * 6) * 0.75f;

                        float sin = MathF.Sin((tt + MathF.Sin(yy * 0.1f) - 4.5f) * 3) * 1.5f;
                        float cos = MathF.Cos((tt + MathF.Cos(xx * 0.1f) - 4.5f) * 6) * 0.75f;

                        SetRotatedQuad(ptr,
                            qx,
                            qy,
                            0,
                            w / -2, h / -2,
                            w, h,
                            sin, cos,
                            0, 0,
                            imgW, imgH,
                            new RgbaByte(127, g, 127, 255),
                            new RgbaByte(127, g, 127, 255),
                            new RgbaByte(r, 127, 127, 255),
                            new RgbaByte(r, 127, 127, 255));
                    }
                }
            }

            batch.End();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetRotatedQuad<T>(
           VertexPositionTexture<T>* ptr,
           float x,
           float y,
           float z,
           float dx,
           float dy,
           float w,
           float h,
           float sin,
           float cos,
           float tlTexX,
           float tlTexY,
           float brTexX,
           float brTexY,
           T brData,
           T blData,
           T tlData,
           T trData)
           where T : unmanaged
        {
            SetQuad(
                ptr,
                brX: x + (dx + w) * cos - (dy + h) * sin,
                brY: y + (dx + w) * sin + (dy + h) * cos,
                blX: x + dx * cos - (dy + h) * sin,
                blY: y + dx * sin + (dy + h) * cos,
                tlX: x + dx * cos - dy * sin,
                tlY: y + dx * sin + dy * cos,
                trX: x + (dx + w) * cos - dy * sin,
                trY: y + (dx + w) * sin + dy * cos,
                z,
                tlTexX, tlTexY,
                brTexX, brTexY,
                brData,
                blData,
                tlData,
                trData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetQuad<T>(
            VertexPositionTexture<T>* ptr,
            float x,
            float y,
            float z,
            float w,
            float h,
            float tlTexX,
            float tlTexY,
            float brTexX,
            float brTexY,
            T brData,
            T blData,
            T tlData,
            T trData)
            where T : unmanaged
        {
            SetQuad(
                ptr,
                brX: x + w,
                brY: y + h,
                blX: x,
                blY: y + h,
                tlX: x,
                tlY: y,
                trX: x + w,
                trY: y,
                z,
                tlTexX, tlTexY,
                brTexX, brTexY,
                brData,
                blData,
                tlData,
                trData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetQuad<T>(
            VertexPositionTexture<T>* ptr,
            float brX,
            float brY,
            float blX,
            float blY,
            float tlX,
            float tlY,
            float trX,
            float trY,
            float z,
            float texTlX,
            float texTlY,
            float texBrX,
            float texBrY,
            T brData,
            T blData,
            T tlData,
            T trData)
            where T : unmanaged
        {
            // bottom-right
            ptr[0].Position.X = brX;
            ptr[0].Position.Y = brY;
            ptr[0].Position.Z = z;
            ptr[0].Texture.X = texBrX;
            ptr[0].Texture.Y = texBrY;
            ptr[0].Data = brData;

            // bottom-left
            ptr[1].Position.X = blX;
            ptr[1].Position.Y = blY;
            ptr[1].Position.Z = z;
            ptr[1].Texture.X = texTlX;
            ptr[1].Texture.Y = texBrY;
            ptr[1].Data = blData;

            // top-left
            ptr[2].Position.X = tlX;
            ptr[2].Position.Y = tlY;
            ptr[2].Position.Z = z;
            ptr[2].Texture.X = texTlX;
            ptr[2].Texture.Y = texTlY;
            ptr[2].Data = tlData;

            // top-right
            ptr[3].Position.X = trX;
            ptr[3].Position.Y = trY;
            ptr[3].Position.Z = z;
            ptr[3].Texture.X = texBrX;
            ptr[3].Texture.Y = texTlY;
            ptr[3].Data = trData;
        }

        public struct Matrices
        {
            public Matrix4x4 Projection;
            public Matrix4x4 View;
            public Matrix4x4 World;
        }

        public override void UpdatePerFrameResources(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
        }

        private void SetupBatchPipeline(
            GraphicsDevice device, ResourceFactory factory, SceneContext sc, OutputDescription outputs)
        {
            ResourceLayout matrixLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrix", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            ResourceLayout texLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("Texture0", ResourceKind.TextureReadOnly, ShaderStages.Vertex | ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler0", ResourceKind.Sampler, ShaderStages.Vertex | ShaderStages.Fragment)));

            void CreatePipeline()
            {
                ShaderSet shaderSet = sc.ShaderCache.GetShaders(
                    device, device.ResourceFactory, AssetHelper.GetShaderPath("GeometryBatch"));

                var depthStencilState = device.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyGreaterEqual
                    : DepthStencilStateDescription.DepthOnlyLessEqual;

                var rasterizerState = RasterizerStateDescription.Default;

                GraphicsPipelineDescription pd = new(
                    new BlendStateDescription(
                        RgbaFloat.Black,
                        BlendAttachmentDescription.AlphaBlend),
                    depthStencilState,
                    rasterizerState,
                    PrimitiveTopology.TriangleList,
                    shaderSet.CreateDescription(new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                        new VertexElementDescription("TexCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                        new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4))),
                    new ResourceLayout[] { matrixLayout, texLayout },
                    outputs);

                batchPipeline = factory.CreateGraphicsPipeline(ref pd);
            }

            CreatePipeline();

            matrixBuffer = factory.CreateBuffer(new BufferDescription(
                (uint)Unsafe.SizeOf<Matrices>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            matrixBuffer.Name = "what";

            matrixSet = factory.CreateResourceSet(new ResourceSetDescription(
                matrixLayout, matrixBuffer));

            texSet0 = factory.CreateResourceSet(new ResourceSetDescription(
                texLayout, tex0, sampler0));
        }
    }

    public struct UShort2
    {
        public ushort X;
        public ushort Y;

        public UShort2(ushort x, ushort y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionTexture<T>
        where T : unmanaged
    {
        public Vector3 Position;
        public Vector2 Texture;
        public T Data;
    }
}
