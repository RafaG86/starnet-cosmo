#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
STARNET COSMO - Extractor REAL de Iglesias (Google Places API New)
==================================================================
Busca iglesias REALES en Google Maps usando la Places API (New) y las
inyecta en tu base de datos local de STARNET COSMO.

Solo extrae datos públicos reales:
  - Nombre de la iglesia
  - Dirección completa
  - Teléfono (si existe)
  - Sitio web (si existe)
  - País y ciudad (de los componentes de dirección)

Campos que NO existen públicamente se dejan en BLANCO (null).
NO se inventan pastores, miembros, dolores ni herramientas.

Requisitos:
  1. API Key de Google Cloud con Places API (New) habilitada
  2. Variable de entorno: GOOGLE_PLACES_API_KEY
     O usar el parámetro: --api-key TU_CLAVE

Uso:
  python google_maps_scraper.py --pais "Colombia" --ciudad "Bogotá" --limite 20
  python google_maps_scraper.py --pais "México" --ciudad "Guadalajara" --limite 10
  python google_maps_scraper.py --listar-paises
  python google_maps_scraper.py --pais "Colombia" --ciudad "Bogotá" --query "iglesias bautistas"
"""

import argparse
import json
import os
import sys
import time
import urllib.request
import urllib.parse
import urllib.error

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
if hasattr(sys.stderr, 'reconfigure'):
    sys.stderr.reconfigure(encoding='utf-8', errors='replace')

# ─── Configuración ───────────────────────────────────────────────
STARNET_API = "http://localhost:5100/api/leads/bulk"
PLACES_API_URL = "https://places.googleapis.com/v1/places:searchText"

# Campos que pedimos a Google Places (solo los que necesitamos)
FIELD_MASK = ",".join([
    "places.id",
    "places.displayName",
    "places.formattedAddress",
    "places.nationalPhoneNumber",
    "places.internationalPhoneNumber",
    "places.websiteUri",
    "places.addressComponents",
    "places.googleMapsUri",
    "places.location",
])

# Catálogo de países hispanohablantes con principales ciudades
CATALOGO_PAISES = {
    "Colombia": {
        "codigo": "57",
        "ciudades": ["Bogotá", "Medellín", "Cali", "Barranquilla", "Bucaramanga", "Pereira", "Cartagena", "Manizales"]
    },
    "México": {
        "codigo": "52",
        "ciudades": ["Ciudad de México", "Guadalajara", "Monterrey", "Puebla", "Querétaro", "Tijuana", "León", "Mérida"]
    },
    "Argentina": {
        "codigo": "54",
        "ciudades": ["Buenos Aires", "Córdoba", "Rosario", "Mendoza", "La Plata", "Mar del Plata"]
    },
    "España": {
        "codigo": "34",
        "ciudades": ["Madrid", "Barcelona", "Valencia", "Sevilla", "Zaragoza", "Málaga"]
    },
    "Perú": {
        "codigo": "51",
        "ciudades": ["Lima", "Arequipa", "Trujillo", "Chiclayo", "Cusco", "Piura"]
    },
    "Chile": {
        "codigo": "56",
        "ciudades": ["Santiago", "Valparaíso", "Concepción", "La Serena", "Antofagasta"]
    },
    "Guatemala": {
        "codigo": "502",
        "ciudades": ["Ciudad de Guatemala", "Quetzaltenango", "Escuintla", "Antigua Guatemala"]
    },
    "Ecuador": {
        "codigo": "593",
        "ciudades": ["Quito", "Guayaquil", "Cuenca", "Santo Domingo", "Ambato"]
    },
    "Bolivia": {
        "codigo": "591",
        "ciudades": ["Santa Cruz", "La Paz", "Cochabamba", "Sucre", "Tarija"]
    },
    "República Dominicana": {
        "codigo": "1",
        "ciudades": ["Santo Domingo", "Santiago de los Caballeros", "La Romana", "San Pedro de Macorís"]
    },
    "Costa Rica": {
        "codigo": "506",
        "ciudades": ["San José", "Alajuela", "Heredia", "Cartago"]
    },
    "Panamá": {
        "codigo": "507",
        "ciudades": ["Ciudad de Panamá", "San Miguelito", "David", "Colón"]
    },
    "El Salvador": {
        "codigo": "503",
        "ciudades": ["San Salvador", "Santa Ana", "San Miguel", "Soyapango"]
    },
    "Honduras": {
        "codigo": "504",
        "ciudades": ["Tegucigalpa", "San Pedro Sula", "Choloma", "La Ceiba"]
    },
    "Paraguay": {
        "codigo": "595",
        "ciudades": ["Asunción", "Ciudad del Este", "San Lorenzo", "Luque"]
    },
    "Nicaragua": {
        "codigo": "505",
        "ciudades": ["Managua", "León", "Granada", "Masaya"]
    },
    "Uruguay": {
        "codigo": "598",
        "ciudades": ["Montevideo", "Salto", "Ciudad de la Costa", "Paysandú"]
    },
    "Puerto Rico": {
        "codigo": "1",
        "ciudades": ["San Juan", "Bayamón", "Ponce", "Carolina", "Caguas"]
    },
    "Venezuela": {
        "codigo": "58",
        "ciudades": ["Caracas", "Maracaibo", "Valencia", "Barquisimeto"]
    },
    "Estados Unidos": {
        "codigo": "1",
        "ciudades": ["Miami", "Houston", "Los Ángeles", "Orlando", "Nueva York", "Dallas"]
    }
}


def buscar_en_google_places(api_key: str, query: str, limite: int = 20) -> list:
    """
    Realiza búsqueda real en Google Places API (New) Text Search.
    Retorna lista de lugares con datos reales.
    Soporta paginación con nextPageToken para obtener más de 20 resultados.
    """
    todos_los_resultados = []
    page_token = None
    paginas_max = (limite // 20) + (1 if limite % 20 > 0 else 0)
    pagina_actual = 0

    while pagina_actual < paginas_max and len(todos_los_resultados) < limite:
        body = {
            "textQuery": query,
            "pageSize": min(20, limite - len(todos_los_resultados)),  # Máx 20 por request
            "languageCode": "es"
        }

        if page_token:
            body["pageToken"] = page_token

        data = json.dumps(body).encode("utf-8")

        req = urllib.request.Request(
            PLACES_API_URL,
            data=data,
            headers={
                "Content-Type": "application/json",
                "X-Goog-Api-Key": api_key,
                "X-Goog-FieldMask": FIELD_MASK
            },
            method="POST"
        )

        try:
            with urllib.request.urlopen(req, timeout=15) as response:
                result = json.loads(response.read().decode("utf-8"))
                places = result.get("places", [])
                todos_los_resultados.extend(places)

                page_token = result.get("nextPageToken")
                if not page_token:
                    break  # No hay más páginas

                # Google requiere una pausa breve antes de usar nextPageToken
                time.sleep(1.5)

        except urllib.error.HTTPError as e:
            error_body = e.read().decode("utf-8") if e.fp else ""
            print(f"\n❌ Error HTTP {e.code} de Google Places API:")
            try:
                err_json = json.loads(error_body)
                print(f"   Mensaje: {err_json.get('error', {}).get('message', error_body)}")
                print(f"   Status:  {err_json.get('error', {}).get('status', 'UNKNOWN')}")
            except json.JSONDecodeError:
                print(f"   {error_body[:300]}")

            if e.code == 403:
                print("\n💡 Verifica que:")
                print("   1. La API Key sea válida")
                print("   2. Places API (New) esté habilitada en Google Cloud Console")
                print("   3. La API Key tenga permisos para Places API")
            elif e.code == 429:
                print("\n💡 Has excedido el límite de solicitudes. Espera unos minutos.")
            return todos_los_resultados

        except urllib.error.URLError as e:
            print(f"\n❌ Error de conexión con Google Places API: {e}")
            return todos_los_resultados

        pagina_actual += 1

    return todos_los_resultados[:limite]


def extraer_componente_direccion(place: dict, tipo: str) -> str:
    """Extrae un componente específico de la dirección (country, locality, etc.)"""
    components = place.get("addressComponents", [])
    for comp in components:
        types = comp.get("types", [])
        if tipo in types:
            return comp.get("longText", "")
    return ""


def transformar_a_lead(place: dict, pais_buscado: str) -> dict:
    """
    Transforma un resultado de Google Places en un lead para STARNET COSMO.
    SOLO datos reales. Si un campo no existe, se envía como null.
    """
    # Nombre de la iglesia (obligatorio)
    nombre = place.get("displayName", {}).get("text", "")
    if not nombre:
        return None

    # Dirección completa
    direccion = place.get("formattedAddress", None)

    # Teléfono (preferir nacional, si no, internacional)
    telefono = place.get("nationalPhoneNumber", None)
    if not telefono:
        telefono = place.get("internationalPhoneNumber", None)

    # Sitio web
    sitio_web = place.get("websiteUri", None)

    # País y ciudad desde componentes de dirección
    pais = extraer_componente_direccion(place, "country") or pais_buscado
    ciudad = (
        extraer_componente_direccion(place, "locality") or
        extraer_componente_direccion(place, "administrative_area_level_2") or
        extraer_componente_direccion(place, "administrative_area_level_1") or
        None
    )

    # Google Maps URL directa
    maps_url = place.get("googleMapsUri", None)

    # Coordenadas (para referencia en notas)
    location = place.get("location", {})
    lat = location.get("latitude", None)
    lng = location.get("longitude", None)

    # Construir notas informativas (sin inventar nada)
    notas_parts = ["Extraído de Google Maps (datos reales)."]
    if maps_url:
        notas_parts.append(f"Maps: {maps_url}")
    if lat and lng:
        notas_parts.append(f"Coords: {lat},{lng}")

    return {
        "nombreIglesia": nombre,
        "pais": pais,
        "ciudad": ciudad,
        "direccion": direccion,
        "telefono": telefono,
        "email": None,           # No disponible en Google Places → en blanco
        "sitioWeb": sitio_web,
        "nombreContacto": None,  # No disponible → en blanco (se prospecta por mensaje)
        "cantidadMiembros": None, # No disponible → en blanco (se prospecta por mensaje)
        "productoInteres": 0,    # EkklesiApp por defecto
        "origen": 0,             # GoogleMaps
        "notas": " | ".join(notas_parts)
    }


def enviar_a_starnet_cosmo(leads: list) -> tuple:
    """Envía los leads reales a la API local de STARNET COSMO."""
    data = json.dumps(leads).encode("utf-8")
    req = urllib.request.Request(
        STARNET_API,
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST"
    )

    try:
        with urllib.request.urlopen(req, timeout=12) as response:
            if response.status == 200:
                return True, json.loads(response.read().decode("utf-8"))
            return False, f"Status HTTP: {response.status}"
    except urllib.error.URLError as e:
        return False, str(e)


def main():
    parser = argparse.ArgumentParser(
        description="STARNET COSMO - Extractor REAL de Iglesias (Google Places API)",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Ejemplos:
  python google_maps_scraper.py --pais "Colombia" --ciudad "Bogotá" --limite 20
  python google_maps_scraper.py --pais "México" --ciudad "Guadalajara" --limite 10
  python google_maps_scraper.py --pais "España" --ciudad "Madrid" --query "iglesias evangélicas"
  python google_maps_scraper.py --listar-paises
  python google_maps_scraper.py --pais "Colombia" --ciudad "Bogotá" --api-key MI_CLAVE
        """
    )
    parser.add_argument("--pais", default="Colombia",
                        help="País de búsqueda (ej. Colombia, México, España)")
    parser.add_argument("--ciudad", default="",
                        help="Ciudad específica (si no se indica, usa la capital del país)")
    parser.add_argument("--limite", type=int, default=20,
                        help="Cantidad máxima de iglesias a extraer (default: 20)")
    parser.add_argument("--query", default="",
                        help="Query personalizado (ej: 'iglesias bautistas', 'iglesias pentecostales')")
    parser.add_argument("--api-key", default="",
                        help="API Key de Google Places (alternativa a variable de entorno)")
    parser.add_argument("--listar-paises", action="store_true",
                        help="Muestra todos los países disponibles con sus ciudades")
    parser.add_argument("--solo-ver", action="store_true",
                        help="Solo muestra los resultados sin enviarlos a STARNET COSMO")

    args = parser.parse_args()

    # ─── Listar países ───
    if args.listar_paises:
        print("\n🌎 PAÍSES HISPANOHABLANTES DISPONIBLES:")
        print("=" * 55)
        for p, info in CATALOGO_PAISES.items():
            ciudades = ", ".join(info["ciudades"][:4])
            print(f"  • {p:25s} (+{info['codigo']:3s}) → {ciudades}...")
        print(f"\nTotal: {len(CATALOGO_PAISES)} países configurados.")
        return

    # ─── Obtener API Key ───
    api_key = args.api_key or os.environ.get("GOOGLE_PLACES_API_KEY", "")
    if not api_key:
        print("❌ ERROR: No se encontró API Key de Google Places.")
        print()
        print("   Opciones para configurarla:")
        print("   1. Variable de entorno:")
        print("      $env:GOOGLE_PLACES_API_KEY = 'tu-clave-aqui'")
        print()
        print("   2. Parámetro directo:")
        print("      python google_maps_scraper.py --api-key TU_CLAVE --pais Colombia --ciudad Bogotá")
        print()
        print("   📌 Obtén tu clave en: https://console.cloud.google.com/")
        print("      → APIs & Services → Credentials → Create Credentials → API Key")
        print("      → Habilita 'Places API (New)' en Library")
        sys.exit(1)

    # ─── Resolver país y ciudad ───
    pais_sel = None
    for p in CATALOGO_PAISES:
        if p.lower() == args.pais.lower():
            pais_sel = p
            break

    if not pais_sel:
        print(f"⚠️  País '{args.pais}' no está en el catálogo, pero se usará como texto de búsqueda.")
        pais_sel = args.pais

    ciudad_sel = args.ciudad
    if not ciudad_sel and pais_sel in CATALOGO_PAISES:
        ciudad_sel = CATALOGO_PAISES[pais_sel]["ciudades"][0]
    elif not ciudad_sel:
        ciudad_sel = "Capital"

    # ─── Construir query de búsqueda ───
    if args.query:
        search_query = f"{args.query} en {ciudad_sel}, {pais_sel}"
    else:
        search_query = f"iglesias cristianas en {ciudad_sel}, {pais_sel}"

    print()
    print("=" * 60)
    print("🚀 STARNET COSMO - EXTRACTOR DE IGLESIAS REALES")
    print("=" * 60)
    print(f"   País:     {pais_sel}")
    print(f"   Ciudad:   {ciudad_sel}")
    print(f"   Query:    {search_query}")
    print(f"   Límite:   {args.limite} iglesias máximo")
    print(f"   API Key:  {api_key[:8]}...{api_key[-4:]}")
    print("=" * 60)
    print()

    # ─── Buscar en Google Places ───
    print(f"🔍 Buscando en Google Maps: \"{search_query}\"...")
    places = buscar_en_google_places(api_key, search_query, args.limite)

    if not places:
        print("⚠️  No se encontraron resultados para esta búsqueda.")
        print("   Intenta con otra ciudad o un query diferente.")
        return

    print(f"📍 Google Maps devolvió {len(places)} resultados.\n")

    # ─── Transformar a leads ───
    leads = []
    for i, place in enumerate(places, 1):
        lead = transformar_a_lead(place, pais_sel)
        if lead:
            leads.append(lead)
            # Mostrar datos reales encontrados
            nombre = lead["nombreIglesia"]
            tel = lead["telefono"] or "—"
            web = lead["sitioWeb"] or "—"
            ciudad = lead["ciudad"] or "—"
            dir_corta = (lead["direccion"] or "—")[:60]

            print(f"  {i:2d}. ⛪ {nombre}")
            print(f"      📍 {dir_corta}")
            print(f"      📞 {tel}  |  🌐 {web}")
            print(f"      🏙️  {ciudad}, {lead['pais']}")
            print()

    if not leads:
        print("⚠️  Ningún resultado pudo transformarse en lead válido.")
        return

    # Resumen de datos disponibles
    con_telefono = sum(1 for l in leads if l["telefono"])
    con_web = sum(1 for l in leads if l["sitioWeb"])
    print("─" * 60)
    print(f"📊 RESUMEN DE DATOS REALES ENCONTRADOS:")
    print(f"   Total iglesias:     {len(leads)}")
    print(f"   Con teléfono:       {con_telefono} ({con_telefono*100//len(leads)}%)")
    print(f"   Con sitio web:      {con_web} ({con_web*100//len(leads)}%)")
    print(f"   Con email:          0 (se prospecta por mensaje)")
    print(f"   Con pastor/contacto: 0 (se prospecta por mensaje)")
    print("─" * 60)

    if args.solo_ver:
        print("\n📋 Modo solo-ver activo. No se enviaron datos a STARNET COSMO.")
        return

    # ─── Enviar a STARNET COSMO ───
    print(f"\n📤 Enviando {len(leads)} iglesias REALES a STARNET COSMO ({STARNET_API})...\n")

    ok, resultado = enviar_a_starnet_cosmo(leads)
    if ok:
        print("✅ INGESTA COMPLETADA CON ÉXITO:")
        print(f"   • Total procesados:       {resultado.get('totalProcesados', 0)}")
        print(f"   • Nuevos insertados:      {resultado.get('totalInsertados', 0)}")
        print(f"   • Duplicados descartados: {resultado.get('totalDuplicadosOmitidos', 0)}")
        for msg in resultado.get("mensajes", []):
            print(f"   • {msg}")
        print(f"\n💡 Abre tu dashboard en http://localhost:5100 para ver los leads reales.")
        print(f"   Usa el botón de WhatsApp para contactar a las iglesias con teléfono.")
    else:
        print(f"❌ Error al conectar con STARNET COSMO:")
        print(f"   {resultado}")
        print(f"   Verifica que la app esté corriendo: dotnet run --project StarnetCosmo.App")


if __name__ == "__main__":
    main()
