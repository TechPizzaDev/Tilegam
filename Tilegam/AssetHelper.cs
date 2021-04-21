using System;
using System.IO;
using System.Linq;

namespace Tilegam.Client
{
    internal static class AssetHelper
    {
        private static readonly string _assetRoot;
        private static readonly string _shaderRoot;

        static AssetHelper()
        {
            _assetRoot = Path.Combine(AppContext.BaseDirectory, "Assets");
            _shaderRoot = Path.Combine(_assetRoot, "Shaders");
        }

        public static string GetPath(string assetPath)
        {
            return Path.Combine(_assetRoot, assetPath);
        }

        public static string GetPath(params string[] paths)
        {
            return Path.Combine(paths.Prepend(_assetRoot).ToArray());
        }

        public static string GetShaderPath(string path)
        {
            return Path.Combine(_shaderRoot, path);
        }

        public static string GetShaderPath(params string[] paths)
        {
            return Path.Combine(paths.Prepend(_shaderRoot).ToArray());
        }
    }
}