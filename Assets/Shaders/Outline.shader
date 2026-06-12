Shader "Custom/Outline"
{
    Properties
    {
        [HDR] _OutlineColor ("Outline Color", Color) = (1, 0.5, 0, 1)
        _OutlineThickness ("Outline Thickness", Float) = 4.0
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent+100"
        }

        Pass
        {
            Name "Outline"
            Cull Back
            ZTest Always
            ZWrite Off
            Blend One One

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float  _OutlineThickness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS  = normalize(_WorldSpaceCameraPos - posInputs.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float fresnel = 1.0 - saturate(dot(normalize(input.viewDirWS), normalize(input.normalWS)));
                float rim = pow(fresnel, _OutlineThickness);
                return half4(_OutlineColor.rgb * rim, rim);
            }
            ENDHLSL
        }
    }
}
