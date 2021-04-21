using System;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using Veldrid.SPIRV;
using System.Linq;

namespace Tilegam.Client
{
    public static class ShaderHelper
    {
        public static ShaderSet LoadSPIRV(
            GraphicsDevice gd,
            ResourceFactory factory,
            ShaderSetKey key)
        {
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            CrossCompileOptions options = GetOptions(gd, key.Specializations);

            Dictionary<ShaderStages, int> indices = new();
            byte[][] bytecodes = new byte[key.Paths.Length][];
            for (int i = 0; i < bytecodes.Length; i++)
            {
                string path = key.Paths[i];
                ShaderStages stage = key.Stages[i];
                bytecodes[i] = LoadBytecode(GraphicsBackend.Vulkan, path, stage);
                indices[stage] = i;
            }

            int vsIndex = indices[ShaderStages.Vertex];
            int fsIndex = indices[ShaderStages.Fragment];

            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, bytecodes[vsIndex], "main", debug),
                new ShaderDescription(ShaderStages.Fragment, bytecodes[fsIndex], "main", debug),
                options);

            shaders[vsIndex].Name = key.Paths[vsIndex] + "-Vertex";
            shaders[fsIndex].Name = key.Paths[fsIndex] + "-Fragment";

            return new ShaderSet(key, shaders, options.Specializations);
        }

        private static CrossCompileOptions GetOptions(
            GraphicsDevice gd, ReadOnlySpan<SpecializationConstant> specializations)
        {
            SpecializationConstant[] specArray = GetExtendedSpecializations(gd, specializations);

            bool fixClipZ =
                (gd.BackendType == GraphicsBackend.OpenGL ||
                gd.BackendType == GraphicsBackend.OpenGLES)
                && !gd.IsDepthRangeZeroToOne;

            bool invertY = false;

            return new CrossCompileOptions(fixClipZ, invertY, specArray);
        }

        public const int SpecId_IsClipSpaceYInverted = 100;
        public const int SpecId_TextureCoordinatesInvertedY = 101;
        public const int SpecId_IsDepthRangeZeroToOne = 102;
        public const int SpecId_SwapchainIsSrgb = 103;

        public static SpecializationConstant[] GetExtendedSpecializations(
            GraphicsDevice gd, ReadOnlySpan<SpecializationConstant> specializations)
        {
            List<SpecializationConstant> specs = new(specializations.Length + 4);
            HashSet<uint> usedConstants = new(specializations.Length);

            foreach (SpecializationConstant spec in specializations)
            {
                if (!usedConstants.Add(spec.ID))
                {
                    throw new ArgumentException(
                        $"Provided constants share the same ID ({spec.ID}).",
                        nameof(specializations));
                }
                specs.Add(spec);
            }

            if (!usedConstants.Contains(SpecId_IsClipSpaceYInverted))
            {
                specs.Add(new SpecializationConstant(SpecId_IsClipSpaceYInverted, gd.IsClipSpaceYInverted));
            }

            if (!usedConstants.Contains(SpecId_TextureCoordinatesInvertedY))
            {
                bool glOrGles = gd.BackendType == GraphicsBackend.OpenGL || gd.BackendType == GraphicsBackend.OpenGLES;
                specs.Add(new SpecializationConstant(SpecId_TextureCoordinatesInvertedY, glOrGles));
            }

            if (!usedConstants.Contains(SpecId_IsDepthRangeZeroToOne))
            {
                specs.Add(new SpecializationConstant(SpecId_IsDepthRangeZeroToOne, gd.IsDepthRangeZeroToOne));
            }

            if (!usedConstants.Contains(SpecId_SwapchainIsSrgb))
            {
                PixelFormat swapchainFormat = gd.MainSwapchain.Framebuffer.OutputDescription.ColorAttachments[0].Format;
                bool swapchainIsSrgb =
                    swapchainFormat == PixelFormat.B8_G8_R8_A8_UNorm_SRgb ||
                    swapchainFormat == PixelFormat.R8_G8_B8_A8_UNorm_SRgb;
                specs.Add(new SpecializationConstant(SpecId_SwapchainIsSrgb, swapchainIsSrgb));
            }

            return specs.ToArray();
        }

        public static byte[] LoadBytecode(GraphicsBackend backend, string shaderPath, ShaderStages stage)
        {
            string stageExt = stage == ShaderStages.Vertex ? "vert" : "frag";
            string path = shaderPath + "." + stageExt;
            string? bytecodePath = null;

            if (backend == GraphicsBackend.Vulkan ||
                backend == GraphicsBackend.Direct3D11)
            {
                string bytecodeExtension = GetBytecodeExtension(backend);
                bytecodePath = path + bytecodeExtension;

                if (File.Exists(bytecodePath))
                    return File.ReadAllBytes(bytecodePath);
            }

            string extension = GetSourceExtension(backend);
            string sourcePath = path + extension;

            if (File.Exists(sourcePath))
                return File.ReadAllBytes(sourcePath);

            throw new FileNotFoundException("Missing \"" + bytecodePath + "\" or \"" + sourcePath + "\"");
        }

        private static string GetBytecodeExtension(GraphicsBackend backend)
        {
            return backend switch
            {
                GraphicsBackend.Direct3D11 => ".hlsl.bytes",
                GraphicsBackend.Vulkan => ".spv",
                GraphicsBackend.OpenGL => throw new InvalidOperationException("OpenGL and OpenGLES do not support shader bytecode."),
                _ => throw new InvalidOperationException("Invalid Graphics backend: " + backend),
            };
        }

        private static string GetSourceExtension(GraphicsBackend backend)
        {
            return backend switch
            {
                GraphicsBackend.Direct3D11 => ".hlsl",
                GraphicsBackend.Vulkan => ".450.glsl",
                GraphicsBackend.OpenGL => ".330.glsl",
                GraphicsBackend.OpenGLES => ".300.glsles",
                GraphicsBackend.Metal => ".metallib",
                _ => throw new InvalidOperationException("Invalid Graphics backend: " + backend),
            };
        }
    }
}
