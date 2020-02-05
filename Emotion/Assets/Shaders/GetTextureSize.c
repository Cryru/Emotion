#if !defined(CompatTextureIndex)

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

ivec2 getTextureSize(int value)
{
	ivec2 sampledSize = ivec2(1, 1);

	if (value >= 0)
	{
		//TEXTURE_INDEX_UNWRAP
		sampledSize = textureSize(textures[value], 0);
		//END
	}

	return sampledSize;
}

#endif