#!/bin/sh
cat build_list.txt | xargs -d '\n' fsharpc --nologo --out:bin/sparse.exe
