git submodule update --init
git clean -xdff
git submodule foreach git clean -xdff
