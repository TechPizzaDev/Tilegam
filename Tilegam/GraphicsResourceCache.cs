using System;
using System.Collections.Generic;
using Veldrid;

namespace Tilegam.Client
{
    public class GraphicsResourceCache
    {
        private readonly Dictionary<GraphicsPipelineDescription, Pipeline> _pipelines = new();
        private readonly Dictionary<ResourceLayoutDescription, ResourceLayout> _layouts = new();
        private readonly Dictionary<ResourceSetDescription, ResourceSet> _resourceSets = new();

        private Texture? _pinkTex;

        public Pipeline GetPipeline(ResourceFactory factory, ref GraphicsPipelineDescription desc)
        {
            if (!_pipelines.TryGetValue(desc, out Pipeline? p))
            {
                p = factory.CreateGraphicsPipeline(ref desc);
                _pipelines.Add(desc, p);
            }
            return p;
        }

        public ResourceLayout GetResourceLayout(ResourceFactory factory, ResourceLayoutDescription desc)
        {
            if (!_layouts.TryGetValue(desc, out ResourceLayout? p))
            {
                p = factory.CreateResourceLayout(ref desc);
                _layouts.Add(desc, p);
            }
            return p;
        }

        public Texture GetPinkTexture(GraphicsDevice gd, ResourceFactory factory)
        {
            if (_pinkTex == null)
            {
                ReadOnlySpan<RgbaByte> pink = stackalloc RgbaByte[] { RgbaByte.Pink };
                _pinkTex = factory.CreateTexture(TextureDescription.Texture2D(
                    1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
                gd.UpdateTexture(_pinkTex, pink, 0, 0, 0, 1, 1, 1, 0, 0);
            }
            return _pinkTex;
        }

        public ResourceSet GetResourceSet(ResourceFactory factory, ResourceSetDescription description)
        {
            if (!_resourceSets.TryGetValue(description, out ResourceSet? ret))
            {
                ret = factory.CreateResourceSet(ref description);
                _resourceSets.Add(description, ret);
            }
            return ret;
        }

        public void DisposeResources()
        {
            foreach (KeyValuePair<GraphicsPipelineDescription, Pipeline> kvp in _pipelines)
            {
                kvp.Value.Dispose();
            }
            _pipelines.Clear();

            foreach (KeyValuePair<ResourceLayoutDescription, ResourceLayout> kvp in _layouts)
            {
                kvp.Value.Dispose();
            }
            _layouts.Clear();

            foreach (KeyValuePair<ResourceSetDescription, ResourceSet> kvp in _resourceSets)
            {
                kvp.Value.Dispose();
            }
            _resourceSets.Clear();

            _pinkTex?.Dispose();
            _pinkTex = null;
        }
    }
}
