NNBT
====

NBT (Named Binary Tag) is a tag based binary format created by Markus Persson for storing Minecraft level data.  The structure of NBT is similar to JSON, and the format lends itself to many use cases outside of Minecraft.

NNBT is a second generation NBT library for .NET/Mono designed especially for these outside uses.  NNBT supports:
  * Reading and writing uncompressed NBT streams.
  * Diffing and merging NBT trees.
  * Serialization of classes to and from NBT stream.
  * Extensibility via new or custom tag types.
  
NNBT is a rewrite and extension of the NBT classes in [Substrate][1].  If your intention is to write Minecraft tooling that needs to access data in the traditional .dat/.mcr/.mca files, you should stick with the Substrate library.

[1]https://github.com/jaquadro/Substrate
