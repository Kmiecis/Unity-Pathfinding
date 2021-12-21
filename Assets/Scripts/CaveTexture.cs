using Common;
using Common.Extensions;
using UnityEngine;

namespace Custom.CaveGeneration
{
    public class CaveTexture : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        protected MeshRenderer _renderer;

        private bool[,] _map;
        private Texture2D _texture;

        public bool[,] Map
        {
            set
            {
                _map = value;
                CheckTexture(value);
                RegenerateTexture(value);
            }
        }

        private void CheckTexture(bool[,] map)
        {
            if (map == null)
            {
                _texture?.Destroy();
                _texture = null;
                ApplyTextureToRenderer(null);
            }
            else
            {
                int width = map.GetWidth();
                var height = map.GetHeight();

                if (_texture == null)
                {
                    _texture = new Texture2D(width, height);
                    _texture.filterMode = FilterMode.Point;
                    ApplyTextureToRenderer(_texture);
                }
                else
                {
                    if (_texture.width != width || _texture.height != height)
                    {
                        _texture?.Destroy();
                        _texture = new Texture2D(width, height);
                        _texture.filterMode = FilterMode.Point;
                        ApplyTextureToRenderer(_texture);
                    }
                }
            }
        }

        private void ApplyTextureToRenderer(Texture2D texture)
        {
            if (_renderer != null)
            {
                var material = _renderer.sharedMaterial;
                if (material != null)
                {
                    material.SetTexture("_MainTex", texture);
                }
            }
        }

        private void RegenerateTexture(bool[,] map)
        {
            if (map != null)
            {
                var builder = Generate(map);
                builder.Overwrite(_texture);
            }
        }

        private ITexture2DBuilder Generate(bool[,] map)
        {
            int width = map.GetWidth();
            int height = map.GetHeight();

            var builder = new Texture2DBuilder(width, height);

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    builder[x, y] = map[x, y] ? Color.white : Color.black;
                }
            }

            return builder;
        }

        private void OnDestroy()
        {
            _texture?.Destroy();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RegenerateTexture(_map);
        }
#endif
    }
}
