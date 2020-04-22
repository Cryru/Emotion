# Emotion XML Spec

Emotion uses a custom XML serializer/deserializer which supports features not supported by the default .Net serializer. Despite this it is still compatible with it and can deserialize files produced by it.
This document outlines the format specification of the Emotion XML format, a lot of which is synonymous with the default XML spec.

The code for this parser can be found in Emotion/Standard/XML

# Structure

The file will always begin with a "<?xml" tag to allow for guessing of string encoding and compliance with the XML spec. Unlike other serializers you won't find any namespace or schema declarations here. Following is a serialized representation
of the object with the tag name being the type name.

Example:
```
<?xml>
<TypeName>
</TypeName>
```

# Type Names

Whenever types are referenced by an Emotion XML they will be serialized as the non-assembly qualified name of the type. Arrays are prepended with "ArrayOf" and generic types are prepended with "TypeOf" with the generic arguments stuck together.
Note that array-like types such as lists will not contain the "ArrayOf" prefix.

Example:
```
List<string> -> ListOfString
GenericType<int, bool> -> GenericTypeOfInt32Boolean
string[] -> ArrayOfString
```

Type names are expected to be of opaque types.

# Opaque Types

Some types are not acknowledged in the XML structure. They are qualified as "non-opaque". One such type is "Nullable". These types are serialized as if they are the underlaying opaque type, but when missing are deserialized as the non-opaque default.

Example:
```
int = 0 -> not serialized
int = 1 -> <Int32>1</Int32>
int? = null -> not serialized
int? = 0 -> <Int32></Int32>
int? = 1 -> <Int32>1</Int32>
```

An exception is made within arrays as the number of items in them must be preserved. In them "null" values are serialized with an immediately closed tag ```<tag/>``` containing a ```xsi:nil="true"``` attribute for .Net XML compatibility. This attribute is not required by Emotion, it will assume all such immediately closed tags are null.

Example:
```
int?[] = 1, null, 0, 2

    <Int32>1</Int32>
    <Int32 xsi:nil="true" />
    <Int32></Int32>
    <Int32>2</Int32>
```

# Primitive Types, Strings, and Enums

Primitive types are serialized between two tags with no new lines (unless contained in a string). "<" characters in strings are sanitized as "\&lt;".

Enums are serialized as their string values.

Default values are not serialized.

Example:
```
<Boolean>True</Boolean>
```

# Complex Types

Complex types are types containing fields of other types. The base class of a complex type is serialized first, followed by its own types. Default fields are not serialized. If the complex type is a value type its default value will also be considered.

Example:
```
<Transform>
 <Position>
  <X>100</X>
  <Y>200</Y>
  <Z>300</Z>
 </Position>
 <Size>
  <X>400</X>
  <Y>500</Y>
 </Size>
</Transform>
```

One feature supported by the Emotion XML format is derived types. A type's field could be defined as "MyType" but the instance could contain a type which inherits from "MyType". This will be preserved by placing a "type" attribute containing the assembly qualified name of the inherited type. The containing value will be that of the derived type.

Example:
```
<TransformLink>
 <Position>
  <X>100</X>
  <Y>200</Y>
  <Z>300</Z>
 </Position>
 <Size>
  <X>400</X>
  <Y>500</Y>
 </Size>
 <Left type="Tests.Classes.XMLTest+TransformDerived">
  <Size>
   <Y>1100</Y>
  </Size>
  <CoolStuff>True</CoolStuff>
 </Left>
</TransformLink>
```

Recursive references will not be serialized, such as when a type's field contains a reference to itself.

# Arrays (and Array-like)

Array-likes are types which inherit from IEnumerable. They are serialized as a list of tags containing the element type.

Example:
```
float[] = 1, 2, 3

    <Single>1</Single>
    <Single>2</Single>
    <Single>3</Single>
```

Default values are serialized as empty tags. Check the "Opaque Types" section for interactions with opaque types. Derived types are supported.

Emotion XML also supports the Dictionary type. It is treated as an array of the KeyValuePair generic type.

# Goals

The Emotion XML format forgoes many XML features and makes bold assumptions with the goal to be performant and to support many features. As such the produced documents in some cases may yield different results when deserialized with another parser. Additionally the format (and the optional things within it) are there to allow for smaller file sizes.

# Optional Things

A lot of things defined by the XML format are optional in the Emotion XML format but are there by default for compatibility sake.

- Closing tags matching the opening tag.
    
    The format does not allow for mixing tags, therefore any tag starting with "</" will be matched as a closing tag
    of the previously opened one.
- New lines (outside of strings)
    
    New lines within complex type fields are optional.

- Indentation

    The indenting character can be anything (except "<"), it could even be omitted.

- Any attributes other than "type"

    This is the only attribute which the deserializer will consume.