Shader "Unlit/ParticleURP"
{
    Properties {
        _BaseColor ("Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }
        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Particle {
                float2 position;
                float2 velocity;
                int type;
            };

            StructuredBuffer<Particle> particles;
            float4 _BaseColor;

            struct Attributes {
                uint vertexID : SV_VertexID;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN) {
                Varyings OUT;
                Particle p = particles[IN.vertexID];
                OUT.positionCS = TransformWorldToHClip(float3(p.position, 0));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
