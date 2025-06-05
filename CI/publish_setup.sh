#!/bin/bash

echo 'Preparing to publish UPM package'

PKG_ROOT=$1

# Update 'Samples' directory since it should not be seen by the main package
if [ -f $PKG_ROOT/Samples.meta ]; then 
  rm $PKG_ROOT/Samples.meta && echo 'Removed Samples.meta'
fi

if [ -d $PKG_ROOT/Samples ]; then 
  mv $PKG_ROOT/Samples $PKG_ROOT/Samples~ && echo 'Renamed Samples folder to Samples~'
fi
