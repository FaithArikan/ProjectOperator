Shader "Custom/BlackBlurCensor_URP"
{
    Properties
    {
        _Color ("Tint Color", Color) = (0, 0, 0, 1)
        _BlurSize ("Blur Size", Range(0.0, 10.0)) = 2.0
        _Darkness ("Darkness Intensity", Range(0.0, 1.0)) = 0.8
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _BlurSize;
                float _Darkness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                
                // Calculate offset based on texel size
                // _CameraOpaqueTexture_TexelSize comes from DeclareOpaqueTexture.hlsl
                float2 texelSize = _CameraOpaqueTexture_TexelSize.xy;
                float2 offset = texelSize * _BlurSize;

                // Simple Box Blur (9 samples)
                half3 col = half3(0, 0, 0);
                
                // Center
                col += SampleSceneColor(screenUV);
                
                // Neighbors
                col += SampleSceneColor(screenUV + float2(-offset.x, -offset.y));
                col += SampleSceneColor(screenUV + float2(0, -offset.y));
                col += SampleSceneColor(screenUV + float2(offset.x, -offset.y));
                
                col += SampleSceneColor(screenUV + float2(-offset.x, 0));
                col += SampleSceneColor(screenUV + float2(offset.x, 0));
                
                col += SampleSceneColor(screenUV + float2(-offset.x, offset.y));
                col += SampleSceneColor(screenUV + float2(0, offset.y));
                col += SampleSceneColor(screenUV + float2(offset.x, offset.y));
                
                col /= 9.0;

                // Apply tint and darkness
                // We mix the blurred background with the tint color
                half3 finalRGB = lerp(col, _Color.rgb, _Darkness);
                
                // Output with alpha for transparency blending if needed, 
                // though usually for a censor bar we might want it fully opaque relative to the blur
                // effectively "painting" the blurred result.
                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
