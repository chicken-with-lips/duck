#!/usr/bin/env python3

# Modified from https://github.com/zorbathut/libgodot_example/blob/csharp/runit-cs.py

import subprocess
import os
import sys
import platform
import shutil
import glob

# Platform-specific settings
is_windows = platform.system() == "Windows"
if is_windows:
    godot_exe = os.path.abspath("External/Godot/bin/godot.windows.editor.dev.x86_64.executable.mono.exe")
    lib_path_var = "PATH"
    path_separator = ";"
else:
    godot_exe = os.path.abspath("External/Godot/bin/godot.linuxbsd.editor.dev.x86_64.executable.mono")
    lib_path_var = "LD_LIBRARY_PATH"
    path_separator = ":"

output_path = os.path.abspath("Build")

os.makedirs(output_path, exist_ok=True)

print("Building Godot executable with Mono support...")
# extra_suffix is just for compilation optimization, otherwise the binary and libgodot step on each other's feet and cause massively inflated iterative build times
# scu_build is just to make the build faster
subprocess.run(["scons", "module_mono_enabled=yes", "extra_suffix=executable", "dev_build=yes", "debug_symbols=yes", "scu_build=yes"], cwd="External/Godot", check=True)

print("Generating Mono glue files...")
subprocess.run(["bin/godot.linuxbsd.editor.dev.x86_64.executable.mono", "--headless", "--generate-mono-glue", "./modules/mono/glue"], cwd="External/Godot", check=True)

print("Making NuGet packages directory...")
os.makedirs("External/Godot/bin/GodotSharp/Tools/nupkgs", exist_ok=True)

print("Building C# assemblies and NuGet packages...")
subprocess.run([
        "python",
        "./modules/mono/build_scripts/build_assemblies.py",
        "--godot-output-dir", "./bin",
        "--no-deprecated"
    ], cwd="External/Godot", check=True)

print("Building Godot shared library with Mono support...")
subprocess.run(["scons", "module_mono_enabled=yes", "library_type=shared_library", "extra_suffix=shared_library", "dev_build=yes", "debug_symbols=yes", "scu_build=yes"], cwd="External/Godot", check=True)

shutil.copytree("External/Godot/bin/GodotSharp", output_path + "/GodotSharp")
shutil.copy2(godot_exe, output_path)

for file in glob.glob(r'External/Godot/bin/*.so'):
    shutil.copy(file, output_path)

print("Done!")