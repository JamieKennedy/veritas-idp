[CmdletBinding()]
param(
    [switch]$Confirm
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:VeritasDatabaseVolumeNames = @(
    'veritas-postgres-data',
    'veritas_veritas-postgres-data'
)

function Invoke-Docker {
    param(
        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    $output = & docker @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Docker command failed: docker $($Arguments -join ' ')"
    }

    return @($output)
}

function Reset-DevDatabase {
    param(
        [switch]$Confirm
    )

    if (-not $Confirm) {
        throw 'Database reset is destructive. Re-run with -Confirm after stopping pnpm dev.'
    }

    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        throw 'Docker is required to reset the local development database.'
    }

    $existingVolumes = @(Invoke-Docker -Arguments @('volume', 'ls', '--format', '{{.Name}}'))
    $targetVolumes = @($existingVolumes | Where-Object { $_ -in $script:VeritasDatabaseVolumeNames })

    if ($targetVolumes.Count -eq 0) {
        Write-Host 'No Veritas development database volume exists. The database is already reset.'
        return
    }

    foreach ($volumeName in $targetVolumes) {
        $runningContainers = @(Invoke-Docker -Arguments @('ps', '--filter', "volume=$volumeName", '--format', '{{.ID}}'))

        if ($runningContainers.Count -gt 0) {
            throw "Volume '$volumeName' is used by a running container. Stop pnpm dev or Docker Compose, then retry."
        }

        $stoppedContainers = @(Invoke-Docker -Arguments @('ps', '-a', '--filter', "volume=$volumeName", '--format', '{{.ID}}'))
        foreach ($containerId in $stoppedContainers) {
            Invoke-Docker -Arguments @('rm', $containerId) | Out-Null
            Write-Host "Removed stopped container '$containerId' that used local development database volume '$volumeName'."
        }
    }

    foreach ($volumeName in $targetVolumes) {
        Invoke-Docker -Arguments @('volume', 'rm', $volumeName) | Out-Null
        Write-Host "Removed local development database volume '$volumeName'."
    }

    Write-Host 'Run pnpm dev to recreate the database and apply migrations.'
}

if ($MyInvocation.InvocationName -ne '.') {
    Reset-DevDatabase -Confirm:$Confirm
}
