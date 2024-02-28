Shader "Hidden/VolFx/Ascii"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        ZClip false
            
        Pass    // 0
        {
            Name "Ascii"
            
            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_local USE_PALETTE _

            sampler2D _MainTex;
	        sampler2D _GradTex;
	        sampler2D _NoiseTex;
	        sampler2D _PaletteTex;
                        
            uniform float  _Dither;
            
            uniform float4 _DitherMad;
            uniform float4 _GradData;   // x, y - pixels, z - 1 / gradient lenght
            uniform float4 _AsciiColor;
            uniform float4 _ImageColor;
            uniform float4 _NoiseMad;
            uniform float  _LutWeight;

            struct vert_in
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct frag_in
            {
                float2 uv  : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            frag_in vert(vert_in v)
            {
                frag_in o;
                o.vertex = v.vertex;
                o.uv = v.uv - float2(.5, .5);
                return o;
            }
            
            half3 GetLinearToSRGB(half3 c)
            {
#if _USE_FAST_SRGB_LINEAR_CONVERSION
                return FastLinearToSRGB(c);
#else
                return LinearToSRGB(c);
#endif
            }

            half3 GetSRGBToLinear(half3 c)
            {
#if _USE_FAST_SRGB_LINEAR_CONVERSION
                return FastSRGBToLinear(c);
#else
                return SRGBToLinear(c);
#endif
            }
                        
            #define LUT_SIZE 16.
            #define LUT_SIZE_MINUS (16. - 1.)
            
            float3 lut_sample(in half3 col, const sampler2D tex)
            {
                //col *= .99;
#if !defined(UNITY_COLORSPACE_GAMMA)
                float3 uvw = GetLinearToSRGB(col);
#else
                float3 uvw = col;
#endif
                float2 uv;
                
                // get replacement color from the lut set
                uv.y = uvw.y * (LUT_SIZE_MINUS / LUT_SIZE) + .5 * (1. / LUT_SIZE);
                uv.x = uvw.x * (LUT_SIZE_MINUS / (LUT_SIZE * LUT_SIZE)) + .5 * (1. / (LUT_SIZE * LUT_SIZE)) + floor(uvw.z * LUT_SIZE) / LUT_SIZE;    

                float3 lutColor = tex2D(tex, uv).rgb;
                
#if !defined(UNITY_COLORSPACE_GAMMA)
                lutColor = float4(GetSRGBToLinear(lutColor.xyz), lutColor.w);
#endif

                return lutColor;
            }

            // sample single line gradient
            float4 grad_sample(in float2 uv, const in float val, const sampler2D tex)
            {
                // xy - pix * aspect, z - sample scale, w - sample count
                uv.x *= _GradData.z;
                uv.x += floor(val * _GradData.w) * _GradData.z;
                
                return tex2D(tex, uv);
            }
            
            float4 grad_sample(in float2 uv, const in float val, const in float ramp, const sampler2D tex)
            {
                // xy - pix * aspect, z - sample scale, w - sample count
                uv.x *= _GradData.z;
                uv.x += floor(val / _GradData.z) * _GradData.z;
                
                uv.y *= _GradData.w;
                uv.y += floor((ramp) / _GradData.w) * _GradData.w;
                
                return tex2D(tex, uv);
            }
            
            half luma(half3 rgb)
            {
                return dot(rgb, half3(.299, .585, .114));
            }
            
            half4 frag(frag_in i) : COLOR
            {
                float2 set = float2(i.uv.x * _GradData.x, i.uv.y * _GradData.y);
                float2 suv = frac(set - .5);
                float2 puv = float2(round(set.x) / _GradData.x + .5 , round(set.y) / _GradData.y + .5);
                
                half4  col      = saturate(tex2D(_MainTex, puv));
                float noise     = tex2D(_NoiseTex, mad(puv, _NoiseMad.xy, _NoiseMad.zw));
                float4 asciiCol = grad_sample(suv, pow(luma(col), .7), noise, _GradTex);
#ifdef USE_PALETTE
                col.rgb = lerp(col.rgb, lut_sample(col.rgb, _PaletteTex), _LutWeight);
#endif
                asciiCol.rgb *= lerp(col.rgb, _AsciiColor.rgb, _AsciiColor.a);
                asciiCol.a *= col.a;

                col *= _ImageColor;
                return lerp(col, asciiCol, asciiCol.a);
            }
            ENDHLSL
        }
    }
}