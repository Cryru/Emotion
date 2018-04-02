# Map

Things learned and notes.

### Render Caching

Caching your map renders to a render target is usually a good idea when dealing with segmented drawing like in the case of the map, as it allows you to easily apply cool effects like rotation and to prevent certain issues like tile seams showing. However I found that in the case of this tiled map it isn't the greatest of ideas as the animated tiles require frequent updates and the cost of switching render targets is too high. Rotation will have to be done on the camera or with some matrix, and seams will have to be dealt in another way.

### Object Wrapping

Early on in the development of Emotion I noticed that I had wrapped the TiledSharp object's properties way too much. As the project is compiled along with the engine and isn't a referenced library there is no need to do this.

In cases where classes expose objects defined in referenced libraries I do not think it is a good idea to make the user of your code access them directly, as in doing so you make them reference the other library as well. It should be kept optional. 

For instance Jint is a referenced library used by Emotion, and the ScriptingEngine class does not force you to import anything from the Jint namespace.