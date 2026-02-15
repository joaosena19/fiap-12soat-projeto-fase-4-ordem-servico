# Script para executar testes BDD e gerar Living Documentation com Pickles
# Pré-requisito: dotnet tool install --global Pickles.CommandLine
#                dotnet add package XunitXml.TestLogger
# Uso: .\generate-bdd-report.ps1

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  BDD Living Documentation Tool" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = $PSScriptRoot
$outputPath = Join-Path $projectPath "TestResults"
$livingDocPath = Join-Path $outputPath "LivingDoc"
$featuresPath = Join-Path $projectPath "BDD\Features"
$xunitResultsFile = Join-Path $outputPath "BDD-XUnit.xml"

# Passo 1: Limpar resultados anteriores
Write-Host "[1/5] Limpando resultados anteriores..." -ForegroundColor Yellow
if (Test-Path $outputPath) {
    Remove-Item $outputPath -Recurse -Force -ErrorAction SilentlyContinue
}
New-Item -Path $outputPath -ItemType Directory -Force | Out-Null

# Passo 2: Compilar o projeto
Write-Host "[2/5] Compilando o projeto de testes..." -ForegroundColor Yellow
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Falha ao compilar o projeto!" -ForegroundColor Red
    exit 1
}

# Passo 3: Executar os testes BDD com logger xUnit2 XML
Write-Host "[3/5] Executando testes BDD..." -ForegroundColor Yellow
dotnet test --configuration Release --filter "FullyQualifiedName~BDD" --no-build --logger "xunit;LogFileName=BDD-XUnit.xml" --results-directory $outputPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "Alguns testes falharam, mas o relatorio sera gerado mesmo assim." -ForegroundColor Yellow
}

# Passo 4: Verificar se o arquivo de resultados foi gerado
Write-Host "[4/5] Verificando resultados..." -ForegroundColor Yellow
if (-not (Test-Path $xunitResultsFile)) {
    Write-Host "Arquivo de resultados xUnit nao encontrado: $xunitResultsFile" -ForegroundColor Red
    exit 1
}

# Passo 5: Gerar Living Documentation com Pickles
Write-Host "[5/5] Gerando Living Documentation com Pickles..." -ForegroundColor Yellow
pickles --feature-directory="$featuresPath" --output-directory="$livingDocPath" --test-results-format=xunit2 --link-results-file="$xunitResultsFile" --language=pt-BR --documentation-format=Html

$reportFile = Join-Path $livingDocPath "index.html"
if (Test-Path $reportFile) {
    Write-Host ""
    Write-Host "Relatorio BDD Living Doc gerado com sucesso!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Localizacao: $reportFile" -ForegroundColor Cyan
    Write-Host ""
    
    $openReport = Read-Host "Deseja abrir o relatorio no navegador? (S/N)"
    if ($openReport -eq "S" -or $openReport -eq "s") {
        Start-Process $reportFile
    }
} else {
    Write-Host "Falha ao gerar o relatorio!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Processo concluido!" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
