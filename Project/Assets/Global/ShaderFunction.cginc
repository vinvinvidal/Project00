#ifndef EXAMPLE_INCLUDED
#define EXAMPLE_INCLUDED

	//InverseLerp‚ğè‘‚«‚Å‘‚¢‚½ŠÖ”
	float InverseLerp(float aRange, float bRange, float value)
	{
		float re;

		if (aRange != bRange)
		{
			re = (value - aRange) / (bRange - aRange);
		}

		return re;
	}

	//0`1‚ÉŠÛ‚ß‚Ä”½“]‚³‚¹‚éŠÖ”
	float ReverseSaturate(float value)
	{
		return (saturate(value) - 1) * -1;
	}

	// RGB->HSV•ÏŠ·
	float3 RGB2HSV(float3 rgb)
	{
		float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
		float4 p = rgb.g < rgb.b ? float4(rgb.bg, K.wz) : float4(rgb.gb, K.xy);
		float4 q = rgb.r < p.x ? float4(p.xyw, rgb.r) : float4(rgb.r, p.yzx);

		float d = q.x - min(q.w, q.y);
		float e = 1.0e-10;
		return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
	}

	// HSV->RGB•ÏŠ·
	float3 HSV2RGB(float3 hsv)
	{
		float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
		return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
	}

	//‹^——”¸»ŠÖ”
	float Random(float2 texCoord, int Seed)
	{
		return frac(sin(dot(texCoord.xy, float2(12.9898, 78.233)) + Seed) * 43758.5453);
	}

#endif