using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//  AsciiFx © NullTale - https://twitter.com/NullTale/
namespace VolFx
{
    [Serializable, VolumeComponentMenu("VolFx/Ascii")]
    public sealed class AsciiVol : VolumeComponent, IPostProcessComponent
    {
        public ClampedFloatParameter m_Scale = new ClampedFloatParameter(1, 0, 1);
        public ColorParameter        m_Ascii = new ColorParameter(new Color(1, 1, 1, 0), false);
        public ColorParameter        m_Image = new ColorParameter(Color.white, false);

        public Texture2DParameter          m_Gradient = new Texture2DParameter(null, false);
        public NoInterpClampedIntParameter m_Depth    = new NoInterpClampedIntParameter(1, 1, 7);
        public ClampedIntParameter         m_Fps      = new ClampedIntParameter(0, 0, 120);
        public Texture2DParameter          m_Palette  = new Texture2DParameter(null, false);
        public ClampedFloatParameter       m_Impact   = new ClampedFloatParameter(0, 0, 1);
        
        // =======================================================================
        public bool IsActive() => active && (m_Scale.value < 1f || m_Impact.value > 0f);

        public bool IsTileCompatible() => false;
    }
}