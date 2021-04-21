using System;
using Veldrid;

namespace Tilegam.Client
{
    public class ShaderSetKey : IEquatable<ShaderSetKey>
    {
        public string[] Paths { get; }
        public ShaderStages[] Stages { get; }
        public SpecializationConstant[] Specializations { get; }

        public ShaderSetKey(string[] paths, ShaderStages[] stages, SpecializationConstant[]? specializations)
        {
            Paths = paths ?? throw new ArgumentNullException(nameof(paths));
            Stages = stages ?? throw new ArgumentNullException(nameof(stages));
            Specializations = specializations ?? Array.Empty<SpecializationConstant>();
        }

        public bool Equals(ShaderSetKey? other)
        {
            if (other == null)
                return false;

            return Paths.AsSpan().SequenceEqual(other.Paths.AsSpan())
                && Stages.AsSpan().SequenceEqual(other.Stages.AsSpan())
                && Specializations.AsSpan().SequenceEqual(other.Specializations.AsSpan());
        }

        public override bool Equals(object? obj)
        {
            return obj is ShaderSetKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();

            foreach (string path in Paths)
                hash.Add(path);

            foreach (ShaderStages stage in Stages)
                hash.Add(stage);

            foreach (SpecializationConstant specConst in Specializations)
                hash.Add(specConst);

            return hash.ToHashCode();
        }
    }
}
