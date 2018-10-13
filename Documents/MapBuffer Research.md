# GL Map Buffer Research for Emotion Engine

## 1. Can you map buffers from other threads?

Yes, the pointer returned from the mapping initiator can be accessed from multiple threads. However you would need to initiate locks as not to overwrite other threads mapping.

Mapping initiation and flushing are GL operations though and need to be performed on the GL thread.

Sources:

- https://stackoverflow.com/questions/35388919/updating-mapped-opengl-buffers-from-another-thread

## 2. Does flushing flush the whole buffer?

By default - yes.

In order to prevent this from happening the buffer must be in `GL_MAP_FLUSH_EXPLICIT_BIT` mode and flushing ranges must be specified with `glFlushMappedBufferRange` prior to flushing.

Sources:
- https://www.khronos.org/registry/OpenGL-Refpages/gl4/html/glMapBufferRange.xhtml

## 3. Benchmarks

Check the MappingBenchmarks branch for code. 

All measurements are in ticks done by the C# stopwatch class - **lower is better**. All measurements are the average of a **10,000** iterations and are JITed.

### glMapBuffer

The first resultset - `Full`, uses `GL_WRITE_BIT` and flushes the whole buffer irregardless of how many changes were made. The second resultset, `Explicit` uses `GL_MAP_FLUSH_EXPLICIT_BIT` and flushes only the part of the buffer which was modified.

| | Init | Mapping Max | Flush | Single Quad (Init+Map+Flush) | Explicit Single - Init | Explicit Single - Flush |
| :--- | :---: | :--: | :--: | :--: | :--: | :--: |
| MapBuffer Full | 2 | 7786 | 4 | 6 | - | - |
| MapBuffer Explicit | 2 | 7799 | 4 | 8 | 6 | 2 |

#### Conclusions

Initiating mapping is twice as fast as flushing. However it seems using the explicit flag slows mapping initiation and only provides performance boosts when flushing. The differences in the `Mapping Max` metric are chalked up to randomness.

### glBufferSubData

| | Array Init (C#) | Flush | Single Quad (Init+Map+Flush) |
| :--- | :---: | :--: | :--:|
| glBufferSubData Explicit | 17256 | 12429 | 95 |

#### Conclusions

The much lower speeds can be explained as the C# code being slower than the driver code which creates and copies the array when using `glMapBuffer`. All operations using this method require creating array rather than populating pointers from native code.

## 4. Emotion Engine Conclusions

Continue using glMapBuffer with no changes.