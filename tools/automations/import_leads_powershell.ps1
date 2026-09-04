# STARNET COSMO - Ingesta Masiva desde PowerShell
# ----------------------------------------------------
$apiUrl = "http://localhost:5100/api/leads/bulk"

$leads = @(
    @{
        nombreIglesia = "Iglesia Manantial de Bendición"
        ciudad = "Barranquilla"
        direccion = "Calle 72 # 50-20"
        telefono = "3128889911"
        nombreContacto = "Pastor Fernando Castro"
        cantidadMiembros = 190
        productoInteres = 0 # EkklesiApp
        origen = 5          # WebScraper
        notas = "Importado vía PowerShell Scraper Demo"
    },
    @{
        nombreIglesia = "Comunidad Cristiana Filadelfia"
        ciudad = "Bucaramanga"
        direccion = "Carrera 27 # 36-15"
        telefono = "3184445566"
        nombreContacto = "Pastora Marta Lucía"
        cantidadMiembros = 310
        productoInteres = 2 # SermonSync
        origen = 0          # GoogleMaps
        notas = "Enfoque en predicación y escuela bíblica"
    }
) | ConvertTo-Json -Depth 5

Write-Host "🚀 Enviando prospectos a STARNET COSMO en $apiUrl..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri $apiUrl -Method Post -Body $leads -ContentType "application/json"
    Write-Host "✅ Ingesta realizada con éxito:" -ForegroundColor Green
    Write-Host "   • Total procesados: $($response.totalProcesados)"
    Write-Host "   • Insertados: $($response.totalInsertados)"
    Write-Host "   • Duplicados omitidos: $($response.totalDuplicadosOmitidos)"
    foreach ($m in $response.mensajes) {
        Write-Host "   • $m" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Error al conectar con STARNET COSMO: $_" -ForegroundColor Red
    Write-Host "   Verifica que la app esté corriendo con: dotnet run --project StarnetCosmo.App" -ForegroundColor Yellow
}
