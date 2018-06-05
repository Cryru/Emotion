#! /bin/bash

# Move to script's directory, otherwise all hell breaks loose with library loading.
cd "$(dirname "$0")"

# Override library path. Otherwise freetype cannot find libpng.
export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:./Libraries/x64/

./MacBundle