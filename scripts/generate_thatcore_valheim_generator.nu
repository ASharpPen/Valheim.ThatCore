#!/usr/bin/env nu

# --------------------------------------------------------------------------
# Configuration
# --------------------------------------------------------------------------

# Generator project directory, relative to the repo root.
const GENERATOR_PROJECT = 'src/ThatCore.Valheim.Generator'

# Namespace the emitted constant classes live in.
const GENERATED_NAMESPACE = 'ThatCore.Valheim.Generator.Generated'

# Each source project, and whether its top-level types get internalized.
const SOURCE_PROJECTS = [
    { name: 'ThatCore', path: 'src/ThatCore', internalize: true }
    { name: 'ThatCore.Valheim', path: 'src/ThatCore.Valheim', internalize: true }
    { name: 'ThatCore.Shared', path: 'src/ThatCore.Shared', internalize: false }
]

# The csproj whose <Version> is used to set the generators.
const VERSION_SOURCE_PROJECT = 'src/ThatCore/ThatCore.csproj'

const EXCLUDED_DIRECTORIES = ['bin', 'obj', '.vs']
const EXCLUDED_FILE_SUFFIXES = ['.g.cs', '.generated.cs', '.Designer.cs']
const EXCLUDED_FILE_NAMES = ['AssemblyInfo.cs']

# Matches a top-level (column-zero) type declaration only. Nested types are
# indented and therefore skipped, which is intended: a public member or nested
# type inside an internal type is not externally visible.
# The trailing group replaces a lookahead, which the regex engine lacks; it is
# captured and re-emitted so the declaration itself is left untouched.
const TOP_LEVEL_PUBLIC_PATTERN = '(?m)^public(\s+(?:static\s+|sealed\s+|abstract\s+|partial\s+|unsafe\s+|readonly\s+|ref\s+)*(?:class|struct|interface|enum|record|delegate)\b)'

# --------------------------------------------------------------------------
# Helpers
# --------------------------------------------------------------------------

def get-project-version [csproj_path: string] {
    if not ($csproj_path | path exists) {
        error make { msg: $"Version source project not found: ($csproj_path)" }
    }

    let versions = (
        open $csproj_path
        | find -n <Version>
        | parse '{_}>{Value}<{_}'
        | get Value
    )

    if ($versions | is-empty) {
        error make { msg: $"No <Version> element found in ($csproj_path)" }
    }

    $versions | first
}

def get-source-files [
    project_directory: string
    excluded_directories: list<string>
    excluded_file_suffixes: list<string>
    excluded_file_names: list<string>
] {
    # glob treats a backslash as an escape, so patterns are always slash-based.
    let root = ($project_directory | str replace --all '\' '/')

    glob $"($root)/**/*.cs"
    | where { |file|
        let name = ($file | path basename)
        let segments = ($file | path relative-to $root | split row --regex '[\\/]')

        let in_excluded_dir = ($segments | any { |s| $s in $excluded_directories })
        let is_excluded_file = (
            ($name in $excluded_file_names) or
            ($excluded_file_suffixes | any { |s| $name | str ends-with $s })
        )

        (not $in_excluded_dir) and (not $is_excluded_file)
    }
    | sort --ignore-case
}

# Path-derived so that same-named files in different folders stay unique.
# src/ThatCore/Config/Toml/Writers/IntListWriter.cs
#     -> ThatCore.Config.Toml.Writers.IntListWriter.g.cs
def to-hint-name [project_name: string, relative_path: string] {
    let dotted = (
        $relative_path
        | str replace --regex '\.cs$' ''
        | str replace --all --regex '[\\/]' '.'
    )

    $"($project_name).($dotted).g.cs"
}

def to-identifier [hint_name: string] {
    let identifier = (
        $hint_name
        | str replace --regex '\.g\.cs$' ''
        | str replace --all --regex '[^A-Za-z0-9]' '_'
    )

    if ($identifier =~ '^[0-9]') { $"_($identifier)" } else { $identifier }
}

# Normalize to CRLF so the wrapper and the payload never mix line endings
# within one generated file (mixed endings make git diffs unreadable).
def normalize [] : string -> string {
    str replace --all "\r\n" "\n" | str replace --all "\n" "\r\n"
}

# Assembly-level attributes would be applied to the *consuming* assembly and
# collide with its own. Fail loudly rather than shipping them.
def assert-no-assembly-attributes [text: string, display_path: string] {
    if ($text =~ '(?m)^\s*\[\s*assembly\s*:') {
        error make { msg: $"Assembly-level attribute found in ($display_path). Remove it or exclude the file." }
    }
}

def update-generator-version [csproj_path: string, version: string] {
    let content = (open --raw $csproj_path | decode utf-8)

    if not ($content =~ '<Version>.*?</Version>') {
        return
    }

    let updated = ($content | str replace --all --regex '<Version>.*?</Version>' $"<Version>($version)</Version>")

    if $updated != $content {
        $updated | normalize | save --raw --force $csproj_path
        print $"  Generator version -> ($version)"
    }
}

def write-manifest [dir: string, manifest_files: list<string>] {
    let files = $manifest_files 
        | each { |i| $"     ctx.AddSource\(($i).HintName, ($i).Source\);" }
        | to text

    let content = ([
        '// <auto-generated>'
        "//    Produced by generate_thatcore_valheim_generator.nu"
        '// </auto-generated>'
        ''
        'using Microsoft.CodeAnalysis;'
        ''
        $"namespace ($GENERATED_NAMESPACE);"
        ''
        "internal static class SourceManifest"
        '{'
        '   public static void AddSourceTexts(IncrementalGeneratorPostInitializationContext ctx)'
        '   {'
        $files
        '   }'
        '}'
    ] | str join "\r\n")

    let file = $dir | path join "SourceManifest.g.cs"

    $content | normalize | save --raw --force $file
}

# --------------------------------------------------------------------------
# Main
# --------------------------------------------------------------------------

# Emits the core sources as C# string constants inside the generator project.
def main [
    --repo-root: string     # Repo root. Defaults to the parent of the folder holding this script.
    --verbose (-v)          # Print every file as it is emitted.
] {
    let repo_root = ($repo_root | default ($env.FILE_PWD | path dirname) | path expand)
    let generator_directory = ($repo_root | path join $GENERATOR_PROJECT | path expand --no-symlink)
    let generated_directory = ($generator_directory | path join 'Generated')

    if not ($generator_directory | path exists) {
        error make { msg: $"Generator project directory not found: ($generator_directory)" }
    }

    let source_version = (get-project-version ($repo_root | path join $VERSION_SOURCE_PROJECT))
    print $"Version: ($source_version)"

    # Initial cleanup
    rm --recursive --force $generated_directory
    mkdir $generated_directory

    # Run collection and source generation
    mut seen_hint_names = []
    mut file_count = 0
    mut manifest_files = [];

    for project in $SOURCE_PROJECTS {

        let project_directory = ($repo_root | path join $project.path | path expand --no-symlink)

        if not ($project_directory | path exists) {
            error make { msg: $"Project directory not found: ($project_directory)" }
        }

        print $"Scanning ($project.name) \(internalize: ($project.internalize))"

        let files = (get-source-files $project_directory $EXCLUDED_DIRECTORIES $EXCLUDED_FILE_SUFFIXES $EXCLUDED_FILE_NAMES)

        for file in $files {

            let relative_path = ($file | path relative-to ($project_directory | str replace --all '\' '/'))
            let display_path = ($project.name | path join $relative_path)

            let raw = (open --raw $file | decode utf-8)

            assert-no-assembly-attributes $raw $display_path

            # Order matters: rewrite modifiers on the real source, then escape.
            let rewrite_count = if $project.internalize {
                $raw | parse --regex $TOP_LEVEL_PUBLIC_PATTERN | length
            } else {
                0
            }

            let text = if $project.internalize {
                    $raw | str replace --all --regex $TOP_LEVEL_PUBLIC_PATTERN 'internal${1}'
                } else {
                    $raw
                }
                | normalize
                # Verbatim string literal: the only escape is doubling the quote. No indentation semantics, unlike a raw string literal.
                | str replace --all '"' '""';

            let hint_name = (to-hint-name $project.name $relative_path)

            if $hint_name in $seen_hint_names {
                error make { msg: $"Duplicate hint name '($hint_name)' produced by ($display_path)" }
            }
            $seen_hint_names = ($seen_hint_names | append $hint_name)

            let identifier = (to-identifier $hint_name)
            let output_path = ($generated_directory | path join $"($identifier).g.cs")

            let content = ([
                '// <auto-generated>'
                $"//    Produced by generate_thatcore_valheim_generator.nu from ($display_path | str replace --all --regex '[\\/]' '/')"
                '// </auto-generated>'
                ''
                $"namespace ($GENERATED_NAMESPACE);"
                ''
                $"internal static class ($identifier)"
                '{'
                $'    internal const string HintName = "($hint_name)";'
                ''
                $'    internal const string Source = @"($text)";'
                '}'
            ] | str join "\r\n")

            $content | normalize | save --raw --force $output_path

            $file_count = $file_count + 1

            if $verbose {
                let suffix = if $rewrite_count > 0 { $" \(($rewrite_count) internalized)" } else { '' }
                print $"  ($display_path)($suffix)"
            }

            $manifest_files = ($manifest_files | append $identifier);
        }
    }

    write-manifest $generated_directory $manifest_files

    # ----------------------------------------------------------------------
    # Version sync
    # ----------------------------------------------------------------------

    let generator_csproj = (glob $"($generator_directory | str replace --all '\' '/')/*.csproj" | first)

    if $generator_csproj != null {
        update-generator-version $generator_csproj $source_version
    } else {
        print $"Warning: No .csproj found in ($generator_directory); skipping version sync."
    }

    print $"Done. ($file_count) source files -> ($generated_directory)"
}
