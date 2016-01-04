/// <class>TiltShiftEffect</class>

sampler2D inputSampler : register(S0);

/// <summary>The start of the line segment.</summary>
/// <defaultValue>0,0.8</defaultValue>
float2 Start : register(C0);

/// <summary>The end of the line segment.</summary>
/// <defaultValue>1,0.5</defaultValue>
float2 End : register(C1);

/// <summary>The maximum radius of the pyramid blur.</summary>
/// <minValue>0</minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>0.15</defaultValue>
float BlurRadius : register(C2);

/// <summary>The distance from the line at which the maximum blur radius is reached.</summary>
/// <minValue>0</minValue>
/// <maxValue>5</maxValue>
/// <defaultValue>0.5</defaultValue>
float GradientRadius : register(C3);

float random(float4 pos, float2 scale, float seed) 
{
    // use the fragment position for a different seed per-pixel
    return frac(sin(dot(pos.xy + seed, scale)) * 43758.5453 + seed);
}

float4 blur(float2 uv, float offset, float dx, float dy, float d, inout float totalWeight)
{
    float2 normal = normalize(float2(Start.y - End.y, End.x - Start.x));
    float radius = smoothstep(0.0, 1.0, abs(dot(uv - Start, normal)) / GradientRadius) * BlurRadius;

		float2 delta_temp = float2(dx / d, dy / d);
		
		float cs = 1;cos(offset / 10);
		float sn = 0;sin(offset);
		
		float2 delta = float2(delta_temp.x * cs - delta_temp.y * sn, delta_temp.x * sn + delta_temp.y * cs);
		
		float4 color = float4(0, 0, 0, 0);
		
		for (float t = -30.0; t <= 30.0; t++) 
    {
				float percent = (t + offset - 0.5) / 30.0;
				float weight = 1.0 - abs(percent);
				float2 normalizedUv = saturate(uv + delta * percent * radius);
				float4 sample = tex2D(inputSampler, normalizedUv);
				sample.rgb *= sample.a;
    	
				color += sample * weight;
				totalWeight += weight;
    }
    
    return color;
}

float random( float2 uv )
{
  // We need irrationals for pseudo randomness.
  // Most (all?) known transcendental numbers will (generally) work.
  const float2 r = float2(
    23.1406926327792690,  // e^pi (Gelfond's constant)
     2.6651441426902251); // 2^sqrt(2) (Gelfond?Schneider constant)
  return frac( cos( 123456789. % 1e-7 + 256. * dot(uv, r) ) );  
}

float4 main(float2 uv : TEXCOORD, float4 screenSpacePosition : SV_Position) : COLOR
{
		float dx = End.x - Start.x;
		float dy = End.y - Start.y;
		float d = sqrt(dx * dx + dy * dy);		

    // randomize the lookup values to hide the fixed number of samples
    float offset = random(screenSpacePosition, float2(12.9898, 78.233), 0.0);
    //float offset = random(uv);
    
    float4 color = float4(0, 0, 0, 0);
    float weight = 0;
    
    color += blur(uv, offset, dx, dy, d, weight);
    //color += blur(uv, offset, -dy, dx, d, weight);
    
    color /= weight;
    return color;
}