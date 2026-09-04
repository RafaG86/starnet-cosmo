"""
=============================================================================
STARNET COSMO - SCRAPER DE PÁGINAS AMARILLAS (DIRECTORIOS PÚBLICOS)
=============================================================================
Extrae ÚNICAMENTE datos reales y verificados de iglesias cristianas:
  - Nombre de la Iglesia
  - Ciudad y Departamento / Provincia
  - País
  - Dirección física real
  - Teléfono directo (con formato internacional para WhatsApp)
  - Correo electrónico (si está publicado, de lo contrario se deja en blanco)
  - Sitio web oficial (si está publicado, de lo contrario se deja en blanco)

* NO inventa nombres de pastores, ni dolores, ni herramientas, ni membresías.
* NO requiere API Key ni cuenta en Google Cloud ni tarjeta de crédito.
* Se integra directamente con la API REST de Starnet Cosmo (/api/leads/bulk).
=============================================================================
"""

import sys
import json
import time
import re
import argparse
import requests

# Catálogo de dominios de Páginas Amarillas por país
DIRECTORIOS_PAISES = {
    "Colombia": {
        "domain": "https://www.paginasamarillas.com.co",
        "prefix": "+57",
        "default_terms": ["iglesias-cristianas", "iglesias"]
    },
    "Argentina": {
        "domain": "https://www.paginasamarillas.com.ar",
        "prefix": "+54",
        "default_terms": ["iglesias-cristianas"]
    },
    "Peru": {
        "domain": "https://www.paginasamarillas.com.pe",
        "prefix": "+51",
        "default_terms": ["iglesias-cristianas"]
    },
    "Chile": {
        "domain": "https://www.amarillas.cl",
        "prefix": "+56",
        "default_terms": ["iglesias-cristianas"]
    },
    "El Salvador": {
        "domain": "https://www.paginasamarillas.com.sv",
        "prefix": "+503",
        "default_terms": ["iglesias-cristianas"]
    }
}

DEFAULT_API_URL = "http://localhost:5100/api/leads/bulk"

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
    "Accept": "application/json, text/plain, */*",
    "Accept-Language": "es-ES,es;q=0.9",
    "Referer": "https://www.paginasamarillas.com.co/"
}


def formatear_telefono(numero_raw, prefix_pais):
    """Limpia y asegura el formato de teléfono adecuado."""
    if not numero_raw:
        return ""
    
    # Extraer dígitos
    digitos = re.sub(r'[^\d+]', '', str(numero_raw).strip())
    if not digitos:
        return ""
        
    if digitos.startswith("+"):
        return digitos
        
    pref_limpio = prefix_pais.replace("+", "")
    if digitos.startswith(pref_limpio):
        return f"+{digitos}"
        
    return f"{prefix_pais}{digitos}"


def extraer_ciudad(main_address):
    """Extrae la ciudad más representativa de la estructura de dirección."""
    if not main_address:
        return "No especificada"
        
    locality_for_seo = main_address.get("localityForSEO")
    if locality_for_seo and locality_for_seo.strip():
        return locality_for_seo.strip().title()
        
    locality_to_show = main_address.get("localityToShow")
    if locality_to_show and locality_to_show.strip():
        parts = [p.strip() for p in locality_to_show.split("-") if p.strip()]
        if parts:
            return parts[0].title()
            
    address_loc = main_address.get("addressLocality")
    if address_loc and "|" in address_loc:
        parts = [p.strip() for p in address_loc.split("|") if p.strip()]
        if len(parts) > 1:
            return parts[1].title()
        elif parts:
            return parts[0].title()
            
    return "Colombia"


def extraer_email(contact_map):
    """Extrae email si existe, sino None."""
    if not contact_map or not isinstance(contact_map, dict):
        return None
        
    mails = contact_map.get("MAIL") or contact_map.get("EMAIL") or []
    if mails and isinstance(mails, list) and len(mails) > 0:
        email = mails[0].strip()
        if "@" in email:
            return email.lower()
    return None


def extraer_sitio_web(contact_map):
    """Extrae sitio web si existe, sino None."""
    if not contact_map or not isinstance(contact_map, dict):
        return None
        
    webs = contact_map.get("WEB") or contact_map.get("URL") or []
    if webs and isinstance(webs, list) and len(webs) > 0:
        url = webs[0].strip()
        if url.startswith("http://") or url.startswith("https://"):
            return url
        return f"https://{url}"
    return None


def buscar_iglesias_en_directorio(pais="Colombia", search_word="iglesias-cristianas", max_resultados=50):
    """
    Consulta la API interna del directorio y obtiene la lista de iglesias reales.
    """
    config = DIRECTORIOS_PAISES.get(pais)
    if not config:
        print(f"[!] País '{pais}' no configurado en directorios. Usando Colombia por defecto.")
        config = DIRECTORIOS_PAISES["Colombia"]
        pais = "Colombia"

    base_domain = config["domain"]
    phone_prefix = config["prefix"]
    api_endpoint = f"{base_domain}/api/advertisements"

    print(f"\n=======================================================")
    print(f"[*] Conectando con directorio de {pais}: {base_domain}")
    print(f"[*] Búsqueda: '{search_word}' | Meta máxima: {max_resultados} iglesias")
    print(f"=======================================================")

    iglesias_recolectadas = []
    page = 0
    page_size = 15

    while len(iglesias_recolectadas) < max_resultados:
        params = {
            "searchWord": search_word,
            "locationWord": "",
            "page": page,
            "size": page_size
        }

        try:
            resp = requests.get(api_endpoint, params=params, headers=HEADERS, timeout=15)
            if resp.status_code != 200:
                print(f"[!] Error HTTP {resp.status_code} en página {page}.")
                break

            data = resp.json()
            total_disponibles = data.get("total", 0)
            results = data.get("results", [])

            if not results:
                print(f"[*] No hay más resultados en la página {page}. Fin de la lista.")
                break

            if page == 0:
                print(f"[i] Total de registros reportados en directorio: {total_disponibles}")

            for item in results:
                nombre = (item.get("name") or "").strip()
                if not nombre:
                    continue

                main_addr = item.get("mainAddress") or {}
                street = (main_addr.get("streetName") or "").strip()
                ciudad = extraer_ciudad(main_addr)

                # Extraer teléfonos
                phones = main_addr.get("allPhones") or []
                telefono = ""
                if phones and isinstance(phones, list) and len(phones) > 0:
                    raw_num = phones[0].get("number") or phones[0].get("tel")
                    telefono = formatear_telefono(raw_num, phone_prefix)

                # Extraer email y sitio web (solo si existen)
                contact_map = item.get("contactMap") or {}
                email = extraer_email(contact_map)
                sitio_web = extraer_sitio_web(contact_map)

                # Construir DTO idéntico a LeadBulkItemDto
                # Nota: NO se inventa pastor, membresía, dolores ni herramientas.
                lead_dto = {
                    "NombreIglesia": nombre,
                    "Ciudad": ciudad,
                    "Pais": pais,
                    "Direccion": street if street else None,
                    "Telefono": telefono if telefono else None,
                    "Email": email,
                    "SitioWeb": sitio_web,
                    "NombreContacto": None,
                    "RedesSociales": None,
                    "CantidadMiembros": None,
                    "ProductoInteres": 1,  # 1 = EkklesiApp
                    "Origen": 2,          # 2 = GoogleMaps / Scraper web
                    "Notas": f"Capturado automáticamente de Directorio Público ({pais})"
                }

                iglesias_recolectadas.append(lead_dto)

                print(f"  [+] #{len(iglesias_recolectadas)}: {nombre} | {ciudad} | Tel: {telefono or 'Sin tel.'} | Web: {sitio_web or '-'}")

                if len(iglesias_recolectadas) >= max_resultados:
                    break

            page += 1
            time.sleep(0.6)  # Pausa respetuosa

        except Exception as e:
            print(f"[!] Excepción consultando página {page}: {e}")
            break

    print(f"\n[*] Total iglesias reales recolectadas: {len(iglesias_recolectadas)}")
    return iglesias_recolectadas


def enviar_a_starnet_cosmo(leads, api_url=DEFAULT_API_URL):
    """Envía el lote de leads a la base de datos de Starnet Cosmo."""
    if not leads:
        print("[!] No hay leads para enviar.")
        return

    print(f"\n[*] Enviando {len(leads)} leads a Starnet Cosmo ({api_url})...")
    try:
        resp = requests.post(api_url, json=leads, headers={"Content-Type": "application/json"}, timeout=30)
        if resp.status_code == 200:
            resultado = resp.json()
            print("\n=======================================================")
            print("  RESULTADO DE LA IMPORTACIÓN EN STARNET COSMO")
            print("=======================================================")
            print(f"  Total procesados:          {resultado.get('totalProcesados', 0)}")
            print(f"  Insertados exitosamente:   {resultado.get('totalInsertados', 0)}")
            print(f"  Duplicados omitidos:       {resultado.get('totalDuplicadosOmitidos', 0)}")
            print(f"  Fallidos:                  {resultado.get('totalFallidos', 0)}")
            print("=======================================================")
            for msg in resultado.get("mensajes", []):
                print(f"  -> {msg}")
        else:
            print(f"[!] Error del servidor ({resp.status_code}): {resp.text}")
    except Exception as e:
        print(f"[!] Error al conectar con Starnet Cosmo: {e}")
        print("    Asegúrate de que la aplicación esté corriendo en http://localhost:5100")


def main():
    parser = argparse.ArgumentParser(description="Scraper de Páginas Amarillas para Starnet Cosmo")
    parser.add_argument("--pais", default="Colombia", choices=list(DIRECTORIOS_PAISES.keys()), help="País a consultar")
    parser.add_argument("--query", default="iglesias-cristianas", help="Término de búsqueda")
    parser.add_argument("--limite", type=int, default=30, help="Cantidad máxima de iglesias a extraer")
    parser.add_argument("--api", default=DEFAULT_API_URL, help="URL de la API de Starnet Cosmo")
    parser.add_argument("--guardar-json", action="store_true", help="Guardar copia local en archivo JSON")

    args = parser.parse_args()

    leads = buscar_iglesias_en_directorio(pais=args.pais, search_word=args.query, max_resultados=args.limite)

    if args.guardar_json and leads:
        archivo = f"leads_{args.pais.lower()}_{int(time.time())}.json"
        with open(archivo, "w", encoding="utf-8") as f:
            json.dump(leads, f, indent=2, ensure_ascii=False)
        print(f"[i] Archivo guardado localmente: {archivo}")

    enviar_a_starnet_cosmo(leads, api_url=args.api)


if __name__ == "__main__":
    main()
