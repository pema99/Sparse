#!/bin/sh
cat build_list.txt | xargs -d '\n' fsharpc --nologo -a --out:bin/sparse.dll
fsharpc --nologo -r bin/sparse.dll example/Program.fs --out:bin/example.exe
