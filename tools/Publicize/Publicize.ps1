param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$cecilPath = Join-Path $scriptDir "Mono.Cecil.dll"

if (-not (Test-Path $InputPath)) {
    Write-Host "ERROR: Input assembly not found at $InputPath"
    exit 1
}

if (-not (Test-Path $cecilPath)) {
    Write-Host "ERROR: Mono.Cecil.dll not found at $cecilPath"
    exit 1
}

Add-Type -Path $cecilPath

$resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
$inputDir = Split-Path -Parent $InputPath
$resolver.AddSearchDirectory($inputDir)
$readerParams = New-Object Mono.Cecil.ReaderParameters
$readerParams.AssemblyResolver = $resolver
$asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($InputPath, $readerParams)

function PublicizeType($type) {
    if ($type.IsNestedAndPrivate -or $type.IsNestedAssembly -or $type.IsNestedFamilyOrAssembly) {
        $type.IsNestedPublic = $true
    }
    elseif ($type.IsNestedFamily -or $type.IsNestedPrivate) {
        $type.IsNestedPublic = $true
    }
    elseif (-not $type.IsPublic -and -not $type.IsNestedPublic) {
        if ($type.IsNested) {
            $type.IsNestedPublic = $true
        }
        else {
            $type.IsPublic = $true
        }
    }

    foreach ($field in $type.Fields) {
        if ($field.IsPrivate -or $field.IsFamily -or $field.IsFamilyOrAssembly -or $field.IsAssembly) {
            $field.IsPublic = $true
        }
    }

    foreach ($method in $type.Methods) {
        if ($method.IsPrivate -or $method.IsFamily -or $method.IsFamilyOrAssembly -or $method.IsAssembly) {
            $method.IsPublic = $true
        }
    }

    foreach ($property in $type.Properties) {
        if ($property.GetMethod -ne $null) {
            if ($property.GetMethod.IsPrivate -or $property.GetMethod.IsFamily -or $property.GetMethod.IsFamilyOrAssembly -or $property.GetMethod.IsAssembly) {
                $property.GetMethod.IsPublic = $true
            }
        }
        if ($property.SetMethod -ne $null) {
            if ($property.SetMethod.IsPrivate -or $property.SetMethod.IsFamily -or $property.SetMethod.IsFamilyOrAssembly -or $property.SetMethod.IsAssembly) {
                $property.SetMethod.IsPublic = $true
            }
        }
    }

    foreach ($nested in $type.NestedTypes) {
        PublicizeType $nested
    }
}

foreach ($type in $asm.MainModule.Types) {
    PublicizeType $type
}

$outDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

$asm.Write($OutputPath)
Write-Host "Publicized assembly written to $OutputPath"
