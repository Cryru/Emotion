#if !defined(CompatTextureIndex)

vec4 getTextureColor(int id, vec2 uvInput)
{
	vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);

	// Check if a texture is in use.
	if (id >= 0)
	{
		// Sample for the texture's color at the specified vertex UV.
		sampledColor = texture(textures[id], uvInput);
	}
	
	return sampledColor;
}

#else

vec4 getTextureColor(int value, vec2 uvInput)
{
	vec4 sampledColor = vec4(1.0, 1.0, 1.0, 1.0);

	if (value >= 0)
	{
		//TEXTURE_INDEX_UNWRAP
		sampledColor = texture(textures[value], uvInput);
		//END
	}
	
	return sampledColor;
}

#endif