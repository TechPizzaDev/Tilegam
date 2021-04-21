using System;
using Veldrid;

namespace Tilegam.Client
{
    public class ShaderSet : IEquatable<ShaderSet>, IDisposable
    {
        public ShaderSetKey Key { get; }
        public Shader[] Shaders { get; }

        /// <summary>
        /// Gets the backend-specific specialization constants.
        /// </summary>
        public SpecializationConstant[] Specializations { get; }

        public ShaderSet(ShaderSetKey key, Shader[] shaders, SpecializationConstant[] specializations)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Shaders = shaders ?? throw new ArgumentNullException(nameof(shaders));
            Specializations = specializations ?? throw new ArgumentNullException(nameof(specializations));
        }

        public ShaderSetDescription CreateDescription(params VertexLayoutDescription[] vertexLayouts)
        {
            return new ShaderSetDescription(vertexLayouts, Shaders, Specializations);
        }

        public bool Equals(ShaderSet? other)
        {
            if (other == null)
                return false;

            if (!Key.Equals(other.Key))
                return false;

            if (Shaders.Length != other.Shaders.Length)
                return false;

            for (int i = 0; i < Shaders.Length; i++)
            {
                if (Shaders[i] != other.Shaders[i])
                    return false;
            }
            return Specializations.AsSpan().SequenceEqual(other.Specializations.AsSpan());
        }

        public override bool Equals(object? obj)
        {
            return obj is ShaderSet other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (Shader shader in Shaders)
                hash.Add(shader);

            foreach (SpecializationConstant specConst in Specializations)
                hash.Add(specConst);

            return hash.ToHashCode();
        }

        public void Deconstruct(
            out Shader[] shaders, out SpecializationConstant[] specializations)
        {
            shaders = Shaders;
            specializations = Specializations;
        }

        public void Dispose()
        {
            foreach (Shader shader in Shaders)
                shader.Dispose();
        }
    }
}
