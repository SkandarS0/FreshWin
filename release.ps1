#Requires -Version 7
<#
.SYNOPSIS
  Release helper for FreshWin.
.DESCRIPTION
  main is protected, so a release takes two runs:
    ./release.ps1 0.1.0   bump the version on a release branch and open a PR
    ./release.ps1 -Tag    once the PR is merged, tag main and push the tag
  Pushing the tag starts release.yaml. The tag name is read from Cargo.toml,
  so it cannot disagree with the crate version.
  The release commit and the tag are always signed (git commit -S, git tag -s),
  using whatever signing key git is configured with (gpg or ssh).
#>
[CmdletBinding(DefaultParameterSetName = 'Prepare')]
param(
    [Parameter(Mandatory, Position = 0, ParameterSetName = 'Prepare')]
    [string]$Version,

    [Parameter(Mandatory, ParameterSetName = 'Tag')]
    [switch]$Tag
)

$ErrorActionPreference = 'Stop'
Set-Location (git rev-parse --show-toplevel)

$Manifest = Join-Path (Get-Location) 'Cargo.toml'
$VersionPattern = '(?ms)(^\[workspace\.package\](?:(?!^\[).)*?^version\s*=\s*")([^"]+)(")'

function Exec([scriptblock]$Command) {
    & $Command
    if ($LASTEXITCODE -ne 0) { throw "Failed: $Command" }
}

function Confirm-Action([string]$Question) {
    if ((Read-Host "$Question (y/N)") -ne 'y') { throw 'Aborted.' }
}

function Get-CrateVersion {
    $m = [regex]::Match((Get-Content $Manifest -Raw), $VersionPattern)
    if (-not $m.Success) { throw 'No version found under [workspace.package] in Cargo.toml.' }
    $m.Groups[2].Value
}

function Set-CrateVersion([string]$New) {
    $text = [regex]::Replace((Get-Content $Manifest -Raw), $VersionPattern, "`${1}$New`${3}")
    Set-Content $Manifest $text -NoNewline
    if ((Get-CrateVersion) -ne $New) { throw 'Failed to update the version in Cargo.toml.' }
}

# Clean tree, up-to-date main, nothing unpushed.
function Sync-Main {
    if (git status --porcelain) { throw 'Working tree is not clean.' }
    Exec { git fetch origin --tags --prune --quiet }
    Exec { git switch main --quiet }
    Exec { git pull --ff-only --quiet }
    if ((git rev-parse HEAD) -ne (git rev-parse origin/main)) {
        throw 'Local main has commits that are not on origin/main.'
    }
}

# Fail early if signing doesn't work (key, agent, passphrase) instead of halfway through.
# commit-tree writes an unreferenced commit object: no branch or tag is touched.
function Assert-CanSign {
    $null = git commit-tree -S 'HEAD^{tree}' -m 'signing check'
    if ($LASTEXITCODE -ne 0) {
        throw 'Signing failed. Check user.signingkey, gpg.format and your gpg or ssh agent.'
    }
}

# Run 1: bump the version on a release branch and open a PR.
function Start-Release([string]$New) {
    if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'GitHub CLI (gh) is required.' }

    if ($New.StartsWith('v')) { $New = $New.Substring(1) }
    $semver = '^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|[0-9A-Za-z-]*[A-Za-z-][0-9A-Za-z-]*)(?:\.(?:0|[1-9]\d*|[0-9A-Za-z-]*[A-Za-z-][0-9A-Za-z-]*))*))?$'
    if ($New -notmatch $semver) {
        throw "'$New' is not a valid semver version."
    }

    Sync-Main
    Assert-CanSign

    $current = Get-CrateVersion
    $tagName = "v$New"
    $branch = "release/$tagName"
    if ($New -eq $current) { throw "Version is already $current." }
    if (git tag -l $tagName) { throw "Tag $tagName already exists." }

    Confirm-Action "Release $current -> $New (PR from $branch)?"

    Exec { git switch -c $branch --quiet }
    Set-CrateVersion $New
    Exec { cargo update --workspace --quiet }   # refresh Cargo.lock without touching dependencies
    Exec { git add Cargo.toml Cargo.lock }
    Exec { git commit -S -m "chore: release $tagName" --quiet }
    Exec { git push -u origin $branch --quiet }
    Exec { gh pr create --base main --title "chore: release $tagName" --body "Bump version to $New." }
    Exec { git switch main --quiet }

    Write-Host 'Merge the PR, then run: ./release.ps1 -Tag'
}

# Run 2: tag the merged release commit on main and push the tag.
function Publish-Tag {
    Sync-Main
    Assert-CanSign

    $tagName = "v$(Get-CrateVersion)"
    if (git tag -l $tagName) { throw "Tag $tagName already exists." }

    # Only tag the release commit itself (squash merge may append " (#123)").
    $expected = [regex]::Escape("chore: release $tagName")
    $subject = git log -1 --format=%s
    if ($subject -notmatch "^$expected( \(#\d+\))?$") {
        throw "HEAD is not the $tagName release commit ('$subject'). Merge the release PR first."
    }

    Confirm-Action "Tag $(git rev-parse --short HEAD) as $tagName and push it (starts the release)?"

    Exec { git tag -s $tagName -m $tagName }
    Exec { git push origin $tagName }

    # The squash-merged release branch is never "merged" as far as git knows.
    if (git branch --list "release/$tagName") { Exec { git branch -D "release/$tagName" --quiet } }
}

if ($PSCmdlet.ParameterSetName -eq 'Tag') { Publish-Tag } else { Start-Release $Version }