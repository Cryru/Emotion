# Configurator

The configurator is a class used to configure the Engine prior to setup, and can optionally be passed to Engine.Setup. If no configurator is passed, the default one will be created. Once the configurator is used it is locked and cannot be modified further, but is still accessible through the "Engine.Configuration" property.

The functions in the class can be chained as they always return the configurator, following the ASP.Net configuration pattern.
For information on supported settings, and the functions used to set them, refer to the Configurator class and its comments. 

## References

- [Configurator.cs]([CodeRoot]Common/Configurator.cs)