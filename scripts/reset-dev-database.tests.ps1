$scriptPath = Join-Path $PSScriptRoot 'reset-dev-database.ps1'

if (Test-Path $scriptPath) {
    . $scriptPath
}
else {
    function Invoke-Docker {
        param([string[]]$Arguments)
    }

    function Reset-DevDatabase {
        throw 'Reset-DevDatabase is not implemented.'
    }
}
Describe 'Reset-DevDatabase' {
    BeforeEach {
        Mock Get-Command { [pscustomobject]@{ Name = 'docker' } } -ParameterFilter { $Name -eq 'docker' }
        Mock Invoke-Docker { @() }
    }

    It 'requires explicit confirmation' {
        try {
            Reset-DevDatabase
            throw 'Expected Reset-DevDatabase to require confirmation.'
        }
        catch {
            $_.Exception.Message | Should BeLike '*-Confirm*'
        }

        Assert-MockCalled Invoke-Docker -Times 0 -Exactly
    }

    It 'does nothing when no recognised database volume exists' {
        Mock Invoke-Docker { @() } -ParameterFilter { ($Arguments -join '|') -eq 'volume|ls|--format|{{.Name}}' }

        Reset-DevDatabase -Confirm

        Assert-MockCalled Invoke-Docker -Times 1 -Exactly -ParameterFilter { ($Arguments -join '|') -eq 'volume|ls|--format|{{.Name}}' }
        Assert-MockCalled Invoke-Docker -Times 0 -Exactly -ParameterFilter { $Arguments[0] -eq 'volume' -and $Arguments[1] -eq 'rm' }
    }

    It 'refuses to reset while a container is running against the database volume' {
        Mock Invoke-Docker { @('veritas-postgres-data') } -ParameterFilter { ($Arguments -join '|') -eq 'volume|ls|--format|{{.Name}}' }
        Mock Invoke-Docker { @('container-id') } -ParameterFilter { ($Arguments -join '|') -eq 'ps|--filter|volume=veritas-postgres-data|--format|{{.ID}}' }

        try {
            Reset-DevDatabase -Confirm
            throw 'Expected Reset-DevDatabase to reject a running volume consumer.'
        }
        catch {
            $_.Exception.Message | Should BeLike '*running container*'
        }

        Assert-MockCalled Invoke-Docker -Times 0 -Exactly -ParameterFilter { ($Arguments -join '|') -eq 'volume|rm|veritas-postgres-data' }
    }

    It 'removes stopped consumers before deleting a recognised database volume' {
        Mock Invoke-Docker { @('veritas-postgres-data', 'unrelated-volume') } -ParameterFilter { ($Arguments -join '|') -eq 'volume|ls|--format|{{.Name}}' }
        Mock Invoke-Docker { @() } -ParameterFilter { ($Arguments -join '|') -eq 'ps|--filter|volume=veritas-postgres-data|--format|{{.ID}}' }
        Mock Invoke-Docker { @('container-id') } -ParameterFilter { ($Arguments -join '|') -eq 'ps|-a|--filter|volume=veritas-postgres-data|--format|{{.ID}}' }
        Mock Invoke-Docker { @('container-id') } -ParameterFilter { ($Arguments -join '|') -eq 'rm|container-id' }
        Mock Invoke-Docker { @('veritas-postgres-data') } -ParameterFilter { ($Arguments -join '|') -eq 'volume|rm|veritas-postgres-data' }

        Reset-DevDatabase -Confirm

        Assert-MockCalled Invoke-Docker -Times 1 -Exactly -ParameterFilter { ($Arguments -join '|') -eq 'rm|container-id' }
        Assert-MockCalled Invoke-Docker -Times 1 -Exactly -ParameterFilter { ($Arguments -join '|') -eq 'volume|rm|veritas-postgres-data' }
        Assert-MockCalled Invoke-Docker -Times 0 -Exactly -ParameterFilter { $Arguments -contains 'unrelated-volume' }
    }
}
