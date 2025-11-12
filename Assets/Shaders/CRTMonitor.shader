Shader "UI/CRTMonitor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // CRT Effects
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _ScanlineCount ("Scanline Count", Range(100, 1000)) = 500
        _ScanlineOffset ("Scanline Offset", Range(0, 1)) = 0
        _ScanlineSpeed ("Scanline Speed", Range(0, 1)) = 0.1

        _Curvature ("Screen Curvature", Range(0, 0.1)) = 0.02
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.05)) = 0.01
        _Vignette ("Vignette", Range(0, 1)) = 0.3

        _FlickerIntensity ("Flicker Intensity", Range(0, 0.2)) = 0.05
        _FlickerSpeed ("Flicker Speed", Range(0, 50)) = 10

        _Brightness ("Brightness", Range(0.5, 2)) = 1.0
        _Contrast ("Contrast", Range(0.5, 2)) = 1.0

        // Required for UI
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _MainTex_ST;

            float _ScanlineIntensity;
            float _ScanlineCount;
            float _ScanlineOffset;
            float _ScanlineSpeed;
            float _Curvature;
            float _ChromaticAberration;
            float _Vignette;
            float _FlickerIntensity;
            float _FlickerSpeed;
            float _Brightness;
            float _Contrast;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // CRT screen curvature distortion
            float2 CurveUV(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / _Curvature;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            // Vignette effect
            float Vignette(float2 uv)
            {
                uv *= 1.0 - uv.yx;
                float vig = uv.x * uv.y * 15.0;
                vig = pow(vig, _Vignette);
                return vig;
            }

            // Scanlines
            float Scanline(float2 uv)
            {
                float scanline = sin((uv.y + _ScanlineOffset) * _ScanlineCount) * 0.5 + 0.5;
                return lerp(1.0, scanline, _ScanlineIntensity);
            }

            // Screen flicker
            float Flicker()
            {
                float flicker = sin(_Time.y * _FlickerSpeed) * 0.5 + 0.5;
                flicker = lerp(1.0, flicker, _FlickerIntensity);
                return flicker;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // Apply screen curvature
                float2 uv = CurveUV(IN.texcoord);

                // Discard pixels outside curved screen
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    discard;

                // Chromatic aberration
                float2 uvR = uv + float2(_ChromaticAberration, 0);
                float2 uvG = uv;
                float2 uvB = uv - float2(_ChromaticAberration, 0);

                float r = tex2D(_MainTex, uvR).r;
                float g = tex2D(_MainTex, uvG).g;
                float b = tex2D(_MainTex, uvB).b;
                float a = tex2D(_MainTex, uvG).a;

                fixed4 color = fixed4(r, g, b, a);

                // Apply tint and vertex color
                color *= IN.color;

                // Apply scanlines
                float scanline = Scanline(uv);
                color.rgb *= scanline;

                // Apply vignette
                float vignette = Vignette(uv);
                color.rgb *= vignette;

                // Apply flicker
                float flicker = Flicker();
                color.rgb *= flicker;

                // Apply brightness and contrast
                color.rgb = ((color.rgb - 0.5) * _Contrast + 0.5) * _Brightness;

                // Add subtle phosphor glow
                float glow = 1.0 + sin(_Time.y * 20.0 + uv.y * 10.0) * 0.03;
                color.rgb *= glow;

                return color;
            }
            ENDCG
        }
    }
}
