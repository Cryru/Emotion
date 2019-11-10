#if !defined(CompatTextureColor)

ivec2 getTextureSize(int id)
{
	ivec2 sampledSize = ivec2(1, 1);

	// Check if a texture is in use.
	if (id >= 0)
	{
		sampledSize = textureSize(textures[id], 0);
	}
	
	return sampledSize;
}

#else

ivec2 getTextureSize(int id)
{
	ivec2 sampledSize = ivec2(1, 1);

	// Check if a texture is in use.
	if (id >= 0)
	{
		if (id == 0)
		{
			sampledSize = textureSize(textures[0], 0);
		}
		else if (id == 1)
		{
			sampledSize = textureSize(textures[1], 0);
		}
		else if (id == 2)
		{
			sampledSize = textureSize(textures[2], 0);
		}
		else if (id == 3)
		{
			sampledSize = textureSize(textures[3], 0);
		}
		else if (id == 4)
		{
			sampledSize = textureSize(textures[4], 0);
		}
		else if (id == 5)
		{
			sampledSize = textureSize(textures[5], 0);
		}
		else if (id == 6)
		{
			sampledSize = textureSize(textures[6], 0);
		}
		else if (id == 7)
		{
			sampledSize = textureSize(textures[7], 0);
		}
		else if (id == 8)
		{
			sampledSize = textureSize(textures[8], 0);
		}
		else if (id == 9)
		{
			sampledSize = textureSize(textures[9], 0);
		}
		else if (id == 10)
		{
			sampledSize = textureSize(textures[10], 0);
		}
		else if (id == 11)
		{
			sampledSize = textureSize(textures[11], 0);
		}
		else if (id == 12)
		{
			sampledSize = textureSize(textures[12], 0);
		}
		else if (id == 13)
		{
			sampledSize = textureSize(textures[13], 0);
		}
		else if (id == 14)
		{
			sampledSize = textureSize(textures[14], 0);
		}
		else if (id == 15)
		{
			sampledSize = textureSize(textures[15], 0);
		}
		else
		{
			sampledSize = textureSize(textures[15], 0);
		}
	}

	return sampledSize;
}

#endif