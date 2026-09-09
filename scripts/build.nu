#!/usr/bin/env nu

# --------------------------------------------------------------------------
# Configuration
# --------------------------------------------------------------------------

# Projects the version is stamped into, relative to the repo root.
const VERSIONED_PROJECTS = [
    'src/ThatCore/ThatCore.csproj'
    'src/ThatCore.Shared/ThatCore.Shared.csproj'
    'src/ThatCore.Valheim/ThatCore.Valheim.csproj'
    'src/ThatCore.Valheim.Generator/ThatCore.Valheim.Generator.csproj'
]

const VALHEIM_PROJECT = 'src/ThatCore.Valheim/ThatCore.Valheim.csproj'
const GENERATOR_PROJECT = 'src/ThatCore.Valheim.Generator/ThatCore.Valheim.Generator.csproj'

# ThatCore.Valheim references the other two, so its output folder holds all three.
const ASSEMBLY_OUTPUT = 'src/ThatCore.Valheim/bin/Release/net48'
const ASSEMBLIES = ['ThatCore.dll' 'ThatCore.Shared.dll' 'ThatCore.Valheim.dll']

const PACKAGE_OUTPUT = 'src/ThatCore.Valheim.Generator/bin/Release'

const RESULT_DIRECTORY = 'bin'

# --------------------------------------------------------------------------
# Helpers
# --------------------------------------------------------------------------

def set-project-version [csproj_path: string, version: string] {
    if not ($csproj_path | path exists) {
        error make { msg: $"Project not found: ($csproj_path)" }
    }

    let bytes = (open --raw $csproj_path | into binary)
    let has_bom = (($bytes | bytes at 0..<3) == 0x[EF BB BF])
    let text = ($bytes | decode utf-8)

    if not ($text =~ '<Version>.*?</Version>') {
        error make { msg: $"No <Version> element found in ($csproj_path)" }
    }

    let updated = ($text | str replace --all --regex '<Version>.*?</Version>' $"<Version>($version)</Version>")

    if $updated != $text {
        # decode strips the BOM; put it back so stamping leaves line 1 alone.
        let out = if $has_bom {
            0x[EF BB BF] ++ ($updated | into binary)
        } else {
            $updated | into binary
        }

        $out | save --raw --force $csproj_path
    }
}

def copy-artifact [source: string, target_directory: string] {
    if not ($source | path exists) {
        error make { msg: $"Expected build output not found: ($source)" }
    }

    cp $source $target_directory
    print $"  ($source | path basename)"
}

# --------------------------------------------------------------------------
# Main
# --------------------------------------------------------------------------

# Stamps the version, builds ThatCore.Valheim and the generator in Release, and
# collects the artifacts under result/ThatCore_v<version>.
def main [
    version: string     # Version to stamp into the projects, e.g. 1.0.5
] {
    if not ($version =~ '^\d+\.\d+\.\d+') {
        error make { msg: $"Version must start with <major>.<minor>.<patch>, got '($version)'" }
    }

    let repo_root = ($env.FILE_PWD | path dirname)
    let result_directory = ($repo_root | path join $RESULT_DIRECTORY $"ThatCore_v($version)")

    print $"Version: ($version)"

    for project in $VERSIONED_PROJECTS {
        set-project-version ($repo_root | path join $project) $version
    }

    print "Building ThatCore.Valheim (Release)"
    ^dotnet build ($repo_root | path join $VALHEIM_PROJECT) --configuration Release

    print "Generating generator sources"
    ^$nu.current-exe ($env.FILE_PWD | path join 'generate_thatcore_valheim_generator.nu')

    print "Building ThatCore.Valheim.Generator (Release)"
    ^dotnet build ($repo_root | path join $GENERATOR_PROJECT) --configuration Release

    print $"Collecting artifacts -> ($result_directory)"

    if ($result_directory | path exists) {
        rm --recursive --force $result_directory
    }
    mkdir $result_directory

    let assembly_directory = ($repo_root | path join $ASSEMBLY_OUTPUT)

    for assembly in $ASSEMBLIES {
        copy-artifact ($assembly_directory | path join $assembly) $result_directory
    }

    let package = (
        $repo_root
        | path join $PACKAGE_OUTPUT $"ThatCore.Valheim.Generator.($version).nupkg"
    )
    copy-artifact $package $result_directory

    print $"Done. ($result_directory)"
}
