sampler2D input : register(s0);

/// <minValue>0/minValue>
/// <maxValue>0.2</maxValue>
/// <defaultValue>0.03</defaultValue>
float smoothness : register(C0);

/// <minValue>2/minValue>
/// <maxValue>20</maxValue>
/// <defaultValue>10</defaultValue>
float roundness : register(C1);

/// <minValue>0/minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>0.47</defaultValue>
float radius : register(C2);

/// <minValue>0.2/minValue>
/// <maxValue>5</maxValue>
/// <defaultValue>1</defaultValue>
float aspectRatio : register(C3);

float4 vignetteColor : register(C4);

float4 blend(float4 A, float4 B)
{
   float4 C;
   C.a = A.a + (1 - A.a) * B.a;
   C.rgb = (1 / C.a) * (A.a * A.rgb + (1 - A.a) * B.a * B.rgb);
   return C;
}

float4 main(float2 uv : TEXCOORD) : COLOR 
{
	float4 color = tex2D(input, uv);

  float2 dist = 0.5 - uv;
  float vignette  = pow(abs(dist.x), roundness) + pow(abs(dist.y * aspectRatio), roundness);
  float minIntensity = pow(abs(radius - smoothness), roundness);
  float maxIntensity = pow(abs(radius + smoothness), roundness);
  
  float4 tmpVignetteColor = vignetteColor;
  tmpVignetteColor.a *= smoothstep(minIntensity, maxIntensity, vignette);
  tmpVignetteColor.rgb *= tmpVignetteColor.a;
  
  return blend(tmpVignetteColor, color);
}