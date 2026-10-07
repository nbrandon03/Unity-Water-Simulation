Shader "Custom/WaterShader"
{
    Properties
    {
        _ShallowColor ("Shallow Color", Color) = (0.18, 0.52, 0.62, 1)
        _DeepColor ("Deep Color", Color) = (0.02, 0.12, 0.22, 1)
        _Gloss ("Gloss", Range(0, 1)) = 0.92
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 0.8
        _SpecularPower ("Specular Power", Range(8, 256)) = 72
        _AmbientStrength ("Ambient Strength", Range(0, 2)) = 0.45
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _WaterLevel ("Water Level", Float) = 0
        _DepthFade ("Depth Fade", Float) = 1.8
        _FresnelPower ("Fresnel Power", Range(1, 8)) = 4.5
        _FresnelStrength ("Fresnel Strength", Range(0, 2)) = 1.0
        _CrestAmount ("Crest Amount", Range(0, 1)) = 0.35
        _DetailAmplitude ("Detail Amplitude", Range(0, 0.08)) = 0.01
        _DetailFrequency ("Detail Frequency", Float) = 2.0
        _DetailSpeed ("Detail Speed", Float) = 1.35
        _DetailShading ("Detail Shading", Range(0, 0.2)) = 0.05
        _CustomTime ("Custom Time", Float) = 0
        _WaveDir1 ("Wave Direction 1", Vector) = (1, 0, 0, 0)
        _Amplitude1 ("Amplitude 1", Float) = 0.5
        _Frequency1 ("Frequency 1", Float) = 0.5
        _Speed1 ("Speed 1", Float) = 1.0
        _Steepness1 ("Steepness 1", Float) = 1.0

        _WaveDir2 ("Wave Direction 2", Vector) = (0, 1, 0, 0)
        _Amplitude2 ("Amplitude 2", Float) = 0.3
        _Frequency2 ("Frequency 2", Float) = 0.8
        _Speed2 ("Speed 2", Float) = 1.2
        _Steepness2 ("Steepness 2", Float) = 0.8

        _WaveDir3 ("Wave Direction 3", Vector) = (1, 1, 0, 0)
        _Amplitude3 ("Amplitude 3", Float) = 0.4
        _Frequency3 ("Frequency 3", Float) = 0.6
        _Speed3 ("Speed 3", Float) = 0.8
        _Steepness3 ("Steepness 3", Float) = 0.7

        _WaveDir4 ("Wave Direction 4", Vector) = (-1, 1, 0, 0)
        _Amplitude4 ("Amplitude 4", Float) = 0.2
        _Frequency4 ("Frequency 4", Float) = 1.0
        _Speed4 ("Speed 4", Float) = 1.5
        _Steepness4 ("Steepness 4", Float) = 1.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Water fullforwardshadows vertex:vert addshadow
        #pragma target 3.0

        fixed4 _ShallowColor;
        fixed4 _DeepColor;
        half _Gloss;
        half _SpecularStrength;
        half _SpecularPower;
        half _AmbientStrength;
        half _Metallic;
        float _WaterLevel;
        float _DepthFade;
        float _FresnelPower;
        float _FresnelStrength;
        float _CrestAmount;
        float _DetailAmplitude;
        float _DetailFrequency;
        float _DetailSpeed;
        float _DetailShading;
        float _CustomTime;

        float4 _WaveDir1;
        float _Amplitude1;
        float _Frequency1;
        float _Speed1;
        float _Steepness1;

        float4 _WaveDir2;
        float _Amplitude2;
        float _Frequency2;
        float _Speed2;
        float _Steepness2;

        float4 _WaveDir3;
        float _Amplitude3;
        float _Frequency3;
        float _Speed3;
        float _Steepness3;

        float4 _WaveDir4;
        float _Amplitude4;
        float _Frequency4;
        float _Speed4;
        float _Steepness4;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
        };

        void ApplyWave(
            in float3 pos,
            in float2 dir,
            in float amplitude,
            in float frequency,
            in float speed,
            in float steepness,
            in float time,
            inout float3 displaced)
        {
            float phase = dot(dir, pos.xz) * frequency + time * speed;
            displaced.x += dir.x * (steepness * amplitude * cos(phase));
            displaced.z += dir.y * (steepness * amplitude * cos(phase));
            displaced.y += amplitude * sin(phase);
        }

        float3 GetDisplacedPosition(float3 pos, float time)
        {
            float3 displaced = pos;

            ApplyWave(pos, normalize(_WaveDir1.xy), _Amplitude1, _Frequency1, _Speed1, _Steepness1, time, displaced);
            ApplyWave(pos, normalize(_WaveDir2.xy), _Amplitude2, _Frequency2, _Speed2, _Steepness2, time, displaced);
            ApplyWave(pos, normalize(_WaveDir3.xy), _Amplitude3, _Frequency3, _Speed3, _Steepness3, time, displaced);
            ApplyWave(pos, normalize(_WaveDir4.xy), _Amplitude4, _Frequency4, _Speed4, _Steepness4, time, displaced);

            float detailA = sin(dot(pos.xz, float2(1.7, -1.3)) * _DetailFrequency + time * _DetailSpeed);
            float detailB = cos(dot(pos.xz, float2(-1.1, 2.0)) * (_DetailFrequency * 1.23) + time * (_DetailSpeed * 1.17));
            float detailWave = detailA * 0.6 + detailB * 0.4;
            displaced.y += detailWave * _DetailAmplitude;


            
            return displaced;
        }

        void vert(inout appdata_full v)
        {
            float time = _CustomTime;
            float3 pos = v.vertex.xyz;

            float3 center = GetDisplacedPosition(pos, time);

            float eps = 0.05;
            float3 sampleX = GetDisplacedPosition(pos + float3(eps, 0, 0), time);
            float3 sampleZ = GetDisplacedPosition(pos + float3(0, 0, eps), time);

            float3 tangentX = sampleX - center;
            float3 tangentZ = sampleZ - center;
            float3 normal = normalize(cross(tangentZ, tangentX));

            v.vertex.xyz = center;
            v.normal = normal;
        }

    inline half4 LightingWater(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
    {
        half3 N = normalize(s.Normal);
        half3 L = normalize(lightDir);
        half3 V = normalize(viewDir);
        half3 H = normalize(L + V);

        half ndl = saturate(dot(N, L));
        half spec = pow(saturate(dot(N, H)), max(8.0h, s.Gloss * _SpecularPower)) * s.Specular;

        half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * s.Albedo * _AmbientStrength;
        half3 diffuse = _LightColor0.rgb * s.Albedo * ndl * atten;
        half3 specular = _LightColor0.rgb * spec * atten;

        return half4(ambient + diffuse + specular, s.Alpha);
    }

    void surf (Input IN, inout SurfaceOutput o)
    {
        float3 N = normalize(IN.worldNormal);
        float3 V = normalize(IN.viewDir);

        float depth01 = saturate((_WaterLevel - IN.worldPos.y) / max(_DepthFade, 0.0001));
        float3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depth01);

        float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower) * _FresnelStrength;
        float crest = smoothstep(0.82, 0.96, 1.0 - saturate(N.y)) * _CrestAmount;
        float detailA = sin(dot(IN.worldPos.xz, float2(1.7, -1.3)) * _DetailFrequency + _CustomTime * _DetailSpeed);
        float detailB = cos(dot(IN.worldPos.xz, float2(-1.1, 2.0)) * (_DetailFrequency * 1.23) + _CustomTime * (_DetailSpeed * 1.17));
        float detailWave = detailA * 0.6 + detailB * 0.4;

        float3 grazingTint = float3(0.75, 0.88, 1.0) * fresnel;
        float3 crestFoam = float3(0.9, 0.95, 1.0) * crest;
        float3 detailTint = float3(0.02, 0.03, 0.04) * detailWave * _DetailShading;

        o.Albedo = saturate(waterColor + grazingTint + crestFoam + detailTint);
        o.Specular = saturate(_SpecularStrength * (0.75 + _Metallic * 0.25 + fresnel * 0.2 + abs(detailWave) * _DetailShading * 0.4));
        o.Gloss = saturate(_Gloss - crest * 0.35 + fresnel * 0.15 + detailWave * _DetailShading * 0.2);
        o.Alpha = lerp(_DeepColor.a, _ShallowColor.a, 1.0 - depth01);
    }
        ENDCG
    }

    FallBack "Diffuse"
}