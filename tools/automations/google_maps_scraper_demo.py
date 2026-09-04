"""
STARNET COSMO - Scraper & Automation Integration Demo (Python)
---------------------------------------------------------------
Este script simula o ejecuta la extracción de prospectos de iglesias
(por ejemplo desde Google Maps, Facebook o directorios web) y los
inyecta automáticamente en tu base de datos local de STARNET COSMO.

Requisitos:
    pip install requests
"""

import requests
import json
import sys

API_URL = "http://localhost:5100/api/leads/bulk"

# Ejemplo de lote extraído por un scraper
leads_extraidos = [
    {
        "nombreIglesia": "Iglesia Comunidad de Fe Norte",
        "ciudad": "Bogotá",
        "direccion": "Calle 170 # 12-40",
        "telefono": "3109876543",
        "nombreContacto": "Pastor Daniel Ortiz",
        "sitioWeb": "https://comunidaddefenorte.org",
        "cantidadMiembros": 280,
        "productoInteres": 0,  # 0: EkklesiApp, 1: ChordSync, 2: SermonSync, 3: Suite
        "origen": 0,           # 0: GoogleMaps, 5: WebScraper
        "notas": "Extraído automáticamente de Google Maps. Teléfono verificado."
    },
    {
        "nombreIglesia": "Iglesia Bautista La Promesa",
        "ciudad": "Medellín",
        "direccion": "Circular 4ta # 73-10, Laureles",
        "telefono": "3154443322",
        "nombreContacto": "Pastor Juan Esteban",
        "cantidadMiembros": 150,
        "productoInteres": 1,  # Interés en música/alabanza -> ChordSync
        "origen": 0,
        "notas": "Fotos del templo muestran grupo de alabanza numeroso."
    },
    {
        "nombreIglesia": "Centro Cristiano Hosanna",
        "ciudad": "Cali",
        "direccion": "Carrera 1 # 45-20",
        "telefono": "3007778899",
        "nombreContacto": "Pastor Gabriel Restrepo",
        "cantidadMiembros": 420,
        "productoInteres": 3,  # Suite Completa
        "origen": 5,           # WebScraper
        "notas": "Manejan 3 servicios dominicales."
    }
]

def enviar_leads():
    print(f"🚀 Enviando {len(leads_extraidos)} prospectos a STARNET COSMO ({API_URL})...")
    try:
        response = requests.post(API_URL, json=leads_extraidos, timeout=10)
        if response.status_code == 200:
            data = response.json()
            print("✅ Ingesta completada con éxito:")
            print(f"   • Total procesados: {data.get('totalProcesados')}")
            print(f"   • Nuevos insertados: {data.get('totalInsertados')}")
            print(f"   • Duplicados omitidos: {data.get('totalDuplicadosOmitidos')}")
            for msg in data.get("mensajes", []):
                print(f"   • {msg}")
        else:
            print(f"❌ Error HTTP {response.status_code}: {response.text}")
    except requests.exceptions.ConnectionError:
        print("❌ No se pudo conectar con STARNET COSMO.")
        print("   Asegúrate de que la aplicación esté ejecutándose (http://localhost:5100).")

if __name__ == "__main__":
    enviar_leads()
