using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

//  AsciiFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [ShaderName("Hidden/VolFx/Ascii")]
    public class AsciiPass : VolFxProc.Pass
    {
        private static readonly int s_GradData   = Shader.PropertyToID("_GradData");
        private static readonly int s_GradTex    = Shader.PropertyToID("_GradTex");
        private static readonly int s_AsciiColor = Shader.PropertyToID("_AsciiColor");
        private static readonly int s_ImageColor = Shader.PropertyToID("_ImageColor");
        private static readonly int s_NoiseTex   = Shader.PropertyToID("_NoiseTex");
        private static readonly int s_NoiseMad   = Shader.PropertyToID("_NoiseMad");
        private static readonly int s_PaletteLut = Shader.PropertyToID("_PaletteTex");
        private static readonly int s_LutWeight  = Shader.PropertyToID("_LutWeight");
        
        [Range(0, 1)]
        [Tooltip("Screen noise scale")]
        public  float    _noiseScale = .5f;
        
        [Header("Default volume overrides")]
        [Tooltip("Default sign texture")]
        public Texture2D _gradient;
        [Tooltip("Default palette texture")]
        public Optional<Texture2D> _palette = new Optional<Texture2D>(null, false);
        [Range(1, 7)]
        [Tooltip("Height of the animated gradient in signs(triggered by screen noise)")]
        public  int      _depth = 1;
        [Range(0, 1)]
        [Tooltip("Default Image scale")]
        public float     _scale = 1f;
        [Tooltip("Scale interpolation curve, used for smooth transition")]
        [CurveRange(0, 0, 1, .5f)]
        public AnimationCurve _scaleLerp = new AnimationCurve(new Keyframe[]
                                                            {
                                                                new Keyframe(0, 0, .0369f, 0.0369f, .0f, .8459f),
                                                                new Keyframe(1, .5f, 56.8515f, 56.8515f, .0104f, .0f)
                                                            });
        [Range(0, 120)]
        [Tooltip("Default Signs anime frame rate")]
        public  int   _frameRate;
        [Tooltip("Default Signs color")]
        public  Color _ascii = Color.white;
        [Tooltip("Default Image color")]
        public  Color _image = Color.black;
        
        private int       _frame;
        private Texture2D _noiseTex;
        private Vector4   _noiseMad = new Vector4(1, 1, 0, 0);
        
        private Dictionary<Texture2D, Texture2D> _paletteCache = new Dictionary<Texture2D, Texture2D>();

        // =======================================================================
        public override void Init()
        {
            _frame = 0;
            _paletteCache.Clear();
        }
        
        public override bool Validate(Material mat)
        {
            var settings = Stack.GetComponent<AsciiVol>();

            if (settings.IsActive() == false)
                return false;
            
            // access the palette
            var palette = settings.m_Palette.value as Texture2D;
            if (palette == null)
                palette = _palette.GetValueOrDefault();
            
            Texture2D paletteLut = null;
            if (palette != null && _paletteCache.TryGetValue(palette, out paletteLut) == false)
            {
                paletteLut = LutGenerator.Generate(palette);
                _paletteCache.Add(palette, paletteLut);
            }
            var usePalette = palette != null && settings.m_Impact.value > 0f;
            validatePalette(usePalette);
            if (usePalette)
            {
                mat.SetTexture(s_PaletteLut, paletteLut);
                mat.SetFloat(s_LutWeight, settings.m_Impact.value);
            }
            
            var fps = settings.m_Fps.overrideState ? settings.m_Fps.value : _frameRate;
            var curFrame = Mathf.FloorToInt(Time.unscaledTime / (1f / fps));
            var nextFrame = _frame != curFrame;
            if (nextFrame)
                _frame = curFrame;
            
            var scale = _scaleLerp.Evaluate(settings.m_Scale.overrideState ? settings.m_Scale.value : _scale);
            
            var grad = settings.m_Gradient.value;
            if (grad == null)
                grad = _gradient;
            var depth = settings.m_Depth.overrideState ? settings.m_Depth.value : _depth;
            
            mat.SetTexture(s_GradTex, grad);
            
            var signHeight = grad.height / depth;
            var gradWidth = (float)(grad.width / signHeight);
            var gradHeight = (float)depth;
            mat.SetVector(s_GradData, new Vector4(Screen.width * scale, Screen.height * scale, 1f / gradWidth, 1f / gradHeight));

            var asciiCol = settings.m_Ascii.overrideState ? settings.m_Ascii.value : _ascii;
            var imageCol = settings.m_Image.overrideState ? settings.m_Image.value : _image;
            
            mat.SetColor(s_AsciiColor, asciiCol);
            mat.SetColor(s_ImageColor, imageCol);
            
            _validateNoise();
            mat.SetTexture(s_NoiseTex, _noiseTex);
            
            if (nextFrame)
            {
                _noiseMad.z = Random.value;
                _noiseMad.w = Random.value;
            }
            _noiseMad.x = _noiseScale;
            _noiseMad.y = _noiseScale;
            
            mat.SetVector(s_NoiseMad, _noiseMad);
            
            return true;
            
            // -----------------------------------------------------------------------
            void validatePalette(bool isOn)
            {
                if (mat.IsKeywordEnabled("USE_PALETTE") == isOn)
                    return;
                
                if (isOn) mat.EnableKeyword("USE_PALETTE");
                else      mat.DisableKeyword("USE_PALETTE");
            }
            
            void _validateNoise()
            {
                var width  = Screen.width / 2;
                var height = Screen.height / 2;
                
                if (_noiseTex == null || _noiseTex.width != width || _noiseTex.height != height)
                {
                    _noiseTex            = new Texture2D(width, height);
                    _noiseTex.filterMode = FilterMode.Bilinear;
                    _noiseTex.wrapMode   = TextureWrapMode.Repeat;
                    
                    var pix = new Color[_noiseTex.width * _noiseTex.height];
                    for (var n = 0; n < _noiseTex.width * _noiseTex.height; n++)
                    {
                        var val = Random.value;
                        pix[n] = new Color(val, val, val, 1f);
                    }

                    _noiseTex.SetPixels(pix);
                    _noiseTex.Apply();
                }
            }
        }

        protected override bool _editorValidate => _gradient == null || (_palette != null && _palette.Value == null);
        protected override void _editorSetup(string folder, string asset)
        {
#if UNITY_EDITOR
            var sep = Path.DirectorySeparatorChar;
            
            if (_gradient == null)
                _gradient = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}{sep}Data{sep}Gradient{sep}ascii-gradient-a-1.png");
            
            if (_palette.Value == null)
                _palette.Value = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}{sep}Data{sep}Palette{sep}ascii-one-bit-bw-1x.png");
#endif
        }
        // =======================================================================
        
    public static class LutGenerator
    {
        private static Texture2D _lut16;
        private static Texture2D _lut32;
        private static Texture2D _lut64;

        // =======================================================================
        [Serializable]
        public enum LutSize
        {
            x16,
            x32,
            x64
        }

        [Serializable]
        public enum Gamma
        {
            rec601,
            rec709,
            rec2100,
            average,
        }
        
        // =======================================================================
        public static Texture2D Generate(Texture2D _palette, LutSize lutSize = LutSize.x16, Gamma gamma = Gamma.rec601)
        {
            var clean  = _getLut(lutSize);
            var lut    = clean.GetPixels();
            var colors = _palette.GetPixels();
            
            var _lutPalette = new Texture2D(clean.width, clean.height, TextureFormat.ARGB32, false);

            // grade colors from lut to palette by rgb 
            var palette = lut.Select(lutColor => colors.Select(gradeColor => (grade: compare(lutColor, gradeColor), color: gradeColor)).OrderBy(n => n.grade).First())
                            .Select(n => n.color)
                            .ToArray();
            
            _lutPalette.SetPixels(palette);
            _lutPalette.filterMode = FilterMode.Point;
            _lutPalette.wrapMode   = TextureWrapMode.Clamp;
            _lutPalette.Apply();
            
            return _lutPalette;

            // -----------------------------------------------------------------------
            float compare(Color a, Color b)
            {
                // compare colors by grayscale distance
                var weight = gamma switch
                {
                    Gamma.rec601  => new Vector3(0.299f, 0.587f, 0.114f),
                    Gamma.rec709  => new Vector3(0.2126f, 0.7152f, 0.0722f),
                    Gamma.rec2100 => new Vector3(0.2627f, 0.6780f, 0.0593f),
                    Gamma.average => new Vector3(0.33333f, 0.33333f, 0.33333f),
                    _             => throw new ArgumentOutOfRangeException()
                };

                // var c = a.ToVector3().Mul(weight) - b.ToVector3().Mul(weight);
                var c = new Vector3(a.r * weight.x, a.g * weight.y, a.b * weight.z) - new Vector3(b.r * weight.x, b.g * weight.y, b.b * weight.z);
                
                return c.magnitude;
            }
        }

        // =======================================================================
        internal static int _getLutSize(LutSize lutSize)
        {
            return lutSize switch
            {
                LutSize.x16 => 16,
                LutSize.x32 => 32,
                LutSize.x64 => 64,
                _           => throw new ArgumentOutOfRangeException()
            };
        }
        
        internal static Texture2D _getLut(LutSize lutSize)
        {
            var size = _getLutSize(lutSize);
            var _lut = lutSize switch
            {
                LutSize.x16 => _lut16,
                LutSize.x32 => _lut32,
                LutSize.x64 => _lut64,
                _           => throw new ArgumentOutOfRangeException(nameof(lutSize), lutSize, null)
            };
            
            if (_lut != null && _lut.height == size)
                 return _lut;
            
            _lut            = new Texture2D(size * size, size, TextureFormat.RGBA32, 0, false);
            _lut.filterMode = FilterMode.Point;

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size * size; x++)
                _lut.SetPixel(x, y, _lutAt(x, y));
            
            _lut.Apply();
            return _lut;

            // -----------------------------------------------------------------------
            Color _lutAt(int x, int y)
            {
                return new Color((x % size) / (size - 1f), y / (size - 1f), Mathf.FloorToInt(x / (float)size) * (1f / (size - 1f)), 1f);
            }
        }
    }
    }
}
