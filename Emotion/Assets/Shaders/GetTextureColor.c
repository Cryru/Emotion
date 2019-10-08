#if !defined(CompatTextureColor)

vec4 getTextureColor(int id, vec2 uvInput)
{
	vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);

	// Check if a texture is in use.
	if (id >= 0)
	{
		// Sample for the texture's color at the specified vertex UV and multiply it
		// by the tint.
		sampledColor = texture(textures[id], uvInput);
	}
	
	return sampledColor;
}

#else

vec4 getTextureColor(int id, vec2 uvInput)
{
	vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);

	// Check if a texture is in use.
	if (id >= 0)
	{
		// Sample for the texture's color at the specified vertex uvInput.
		if (id == 0)
		{
			sampledColor = texture(textures[0], uvInput);
		}
		else if (id == 1)
		{
			sampledColor = texture(textures[1], uvInput);
		}
		else if (id == 2)
		{
			sampledColor = texture(textures[2], uvInput);
		}
		else if (id == 3)
		{
			sampledColor = texture(textures[3], uvInput);
		}
		else if (id == 4)
		{
			sampledColor = texture(textures[4], uvInput);
		}
		else if (id == 5)
		{
			sampledColor = texture(textures[5], uvInput);
		}
		else if (id == 6)
		{
			sampledColor = texture(textures[6], uvInput);
		}
		else if (id == 7)
		{
			sampledColor = texture(textures[7], uvInput);
		}
		else if (id == 8)
		{
			sampledColor = texture(textures[8], uvInput);
		}
		else if (id == 9)
		{
			sampledColor = texture(textures[9], uvInput);
		}
		else if (id == 10)
		{
			sampledColor = texture(textures[10], uvInput);
		}
		else if (id == 11)
		{
			sampledColor = texture(textures[11], uvInput);
		}
		else if (id == 12)
		{
			sampledColor = texture(textures[12], uvInput);
		}
		else if (id == 13)
		{
			sampledColor = texture(textures[13], uvInput);
		}
		else if (id == 14)
		{
			sampledColor = texture(textures[14], uvInput);
		}
		else if (id == 15)
		{
			sampledColor = texture(textures[15], uvInput);
		}
		else
		{
			sampledColor = texture(textures[15], uvInput);
		}
	}

	return sampledColor;
}

#endif