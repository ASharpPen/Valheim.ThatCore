#!/usr/bin/env nu

# Paths below are anchored to this script's directory, so the script can be run
# from anywhere.
let script_dir = $env.FILE_PWD

# Path to mod-profile dir (Eg., AppData\Roaming\r2modmanPlus-local\Valheim\profiles\my-profile)
$env.PROFILE_PATH = ''

# Path to Valheim\valheim_Data\Managed dir.
$env.VALHEIM_PATH = 'C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim_Data\Managed'

# Path to nstrip.exe dir
$env.NSTRIP_DIR = ($script_dir | path join ".." "nstrip" | path expand --no-symlink)

$env.OUTPUT = ($script_dir | path join ".." "libs" | path expand --no-symlink)

def strip [
    source: string
    target_dir: string
    --publicize
] {
    mut out = ($env.OUTPUT | path join $target_dir)

    if not ($out | path exists) {
        mkdir $out
    }

    if ($source | path type) == "file" {
        $out = ($out | path join ($source | path basename))
    }

    let nstrip = ($env.NSTRIP_DIR | path join "NStrip.exe")

    if $publicize {
        ^$nstrip -p -cg -d $env.VALHEIM_PATH $source $out
    } else {
        ^$nstrip -cg -d $env.VALHEIM_PATH $source $out
    }

    print $"-Source ($source) -TargetDir ($out)"
}

def copy-file [
    source: string
    target_dir: string
] {
    let out = ($env.OUTPUT | path join $target_dir)

    if not ($out | path exists) {
        mkdir $out
    }

    if ($source | path type) == "file" {
        cp $source $out
    }

    print $"-Source ($source) -TargetDir ($out)"
}

# NStrip
if not ($env.NSTRIP_DIR | path exists) {
    mkdir $env.NSTRIP_DIR
}

let nstrip_exe = ($env.NSTRIP_DIR | path join "NStrip.exe")

if not ($nstrip_exe | path exists) {
    http get "https://github.com/bbepis/NStrip/releases/download/v1.4.1/NStrip.exe"
    | save --raw --force $nstrip_exe
}

# BepInEx
copy-file ($env.PROFILE_PATH | path join "BepInEx" "core" "0Harmony.dll") "BepInEx"
copy-file ($env.PROFILE_PATH | path join "BepInEx" "core" "BepInEx.dll") "BepInEx"

# Valheim
strip ($env.VALHEIM_PATH | path join "assembly_valheim.dll") "Valheim" --publicize
strip ($env.VALHEIM_PATH | path join "assembly_utils.dll") "Valheim" --publicize

# Unity
copy-file ($env.VALHEIM_PATH | path join "UnityEngine.dll") "Unity"
copy-file ($env.VALHEIM_PATH | path join "UnityEngine.CoreModule.dll") "Unity"
copy-file ($env.VALHEIM_PATH | path join "UnityEngine.PhysicsModule.dll") "Unity"
copy-file ($env.VALHEIM_PATH | path join "UnityEngine.ImageConversionModule.dll") "Unity"
copy-file ($env.VALHEIM_PATH | path join "UnityEngine.UI.dll") "Unity"

print "Done"
