using System.Collections.Generic;
using Veldrid;

namespace Tilegam.Client
{
    public class ShaderCache
    {
        private readonly Dictionary<ShaderSetKey, ShaderSet> _shaderSets = new();

        public ShaderSet GetShaders(
            GraphicsDevice gd,
            ResourceFactory factory,
            ShaderSetKey key)
        {
            if (!_shaderSets.TryGetValue(key, out ShaderSet? set))
            {
                set = ShaderHelper.LoadSPIRV(gd, factory, key);
                _shaderSets.Add(key, set);
            }
            return set;
        }

        public ShaderSet GetShaders(
            GraphicsDevice gd,
            ResourceFactory factory,
            string vertexShaderName,
            string fragmentShaderName,
            SpecializationConstant[]? specializations = default)
        {
            ShaderSetKey key = new(
                new[] { vertexShaderName, fragmentShaderName },
                new[] { ShaderStages.Vertex, ShaderStages.Fragment },
                specializations);
            return GetShaders(gd, factory, key);
        }

        public ShaderSet GetShaders(
            GraphicsDevice gd,
            ResourceFactory factory,
            string sharedVertexFragmentName,
            SpecializationConstant[]? specializations = default)
        {
            return GetShaders(gd, factory, sharedVertexFragmentName, sharedVertexFragmentName, specializations);
        }

        public bool DisposeShaderSet(ShaderSetKey key)
        {
            if (_shaderSets.Remove(key, out ShaderSet? set))
            {
                set.Dispose();
                return true;
            }
            return false;
        }

        public void DisposeResources()
        {
            foreach (KeyValuePair<ShaderSetKey, ShaderSet> kvp in _shaderSets)
            {
                kvp.Value.Dispose();
            }
            _shaderSets.Clear();
        }
    }
}
