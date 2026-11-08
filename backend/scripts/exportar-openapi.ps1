<#
.SYNOPSIS
    Exporta o documento OpenAPI gerado pela API em execução para contracts/openapi/pontovoz-v1.json.

.DESCRIPTION
    Compila a API, sobe uma instância local com o perfil "http" (Development, só em
    http://localhost:5080), baixa /openapi/v1.json e grava o arquivo em UTF-8 sem BOM. A
    instância é encerrada ao final, mesmo em caso de erro. O documento só contém os endpoints
    que existem no código; nada é escrito à mão.

    Uso, a partir da pasta backend:
        pwsh ./scripts/exportar-openapi.ps1
    (ou powershell -ExecutionPolicy Bypass -File .\scripts\exportar-openapi.ps1)
#>
[CmdletBinding()]
param(
    [int] $TempoLimiteSegundos = 60
)

$ErrorActionPreference = 'Stop'

$backend = Split-Path -Parent $PSScriptRoot
$projeto = Join-Path $backend 'src/PontoVoz.Api'
$destino = Join-Path (Split-Path -Parent $backend) 'contracts/openapi/pontovoz-v1.json'
$url = 'http://localhost:5080/openapi/v1.json'

if (Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction SilentlyContinue) {
    throw 'A porta 5080 já está em uso. Encerre a API que estiver rodando e tente de novo.'
}

dotnet build $projeto --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'A compilação falhou.' }

$api = Start-Process dotnet -ArgumentList @('run', '--project', $projeto, '--launch-profile', 'http', '--no-build') -PassThru -WindowStyle Hidden
try {
    $limite = (Get-Date).AddSeconds($TempoLimiteSegundos)
    $documento = $null
    while ($null -eq $documento) {
        try {
            $documento = (Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5).Content
        }
        catch {
            if ((Get-Date) -gt $limite) { throw "A API não respondeu em $url dentro de $TempoLimiteSegundos s." }
            Start-Sleep -Milliseconds 500
        }
    }

    # Garante que é JSON válido antes de gravar.
    $null = $documento | ConvertFrom-Json

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destino) | Out-Null
    [System.IO.File]::WriteAllText($destino, $documento.Replace("`r`n", "`n").TrimEnd() + "`n", [System.Text.UTF8Encoding]::new($false))
    Write-Host "Documento OpenAPI exportado para $destino"
}
finally {
    # dotnet run inicia o executável da API como processo filho; encerra os dois.
    Get-Process PontoVoz.Api -ErrorAction SilentlyContinue | Where-Object { $_.StartTime -ge $api.StartTime } | Stop-Process -Force
    if (-not $api.HasExited) { Stop-Process -Id $api.Id -Force }
}
